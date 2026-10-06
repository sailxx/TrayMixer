// TrayMixer — яркость экранов: внешние мониторы через DDC/CI, встроенный экран ноутбука через WMI.
// Обмен с монитором медленный (десятки миллисекунд), поэтому чтение и запись идут в фоне,
// а окно работает с последним известным значением.
using System;
using System.Collections.Generic;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;

namespace TrayMixer
{
    class Display
    {
        public string Key, Name;
        public bool Internal;
        internal IntPtr Phys;            // физический монитор DDC/CI (для внешних)
        int min, max = 100;
        volatile int cur;                // 0–100
        readonly object io = new object(), q = new object();
        int want = -1; bool busy;

        public int Percent { get { return cur; } }

        // Запись идёт в фоне; пока монитор занят, копится только последнее значение
        public void Set(int pct)
        {
            pct = Math.Max(0, Math.Min(100, pct));
            cur = pct;
            lock (q) { want = pct; if (busy) return; busy = true; }
            ThreadPool.QueueUserWorkItem(delegate
            {
                while (true)
                {
                    int v;
                    lock (q) { if (want < 0) { busy = false; return; } v = want; want = -1; }
                    Write(v);
                }
            });
        }

        void Write(int pct)
        {
            lock (io)
            {
                try
                {
                    if (Internal) Brightness.WmiSet(pct);
                    else if (Phys != IntPtr.Zero) Native.SetMonitorBrightness(Phys, (uint)(min + (max - min) * pct / 100));
                }
                catch { }
            }
        }

        // Прочитать значение с монитора; false — монитор не отвечает
        internal bool Read()
        {
            lock (io)
            {
                lock (q) if (busy) return true; // пользователь двигает слайдер — его значение важнее
                try
                {
                    if (Internal) { int v; if (!Brightness.WmiGet(out v)) return false; cur = v; return true; }
                    uint mn, c, mx;
                    if (Phys == IntPtr.Zero || !Native.GetMonitorBrightness(Phys, out mn, out c, out mx) || mx <= mn) return false;
                    min = (int)mn; max = (int)mx;
                    cur = (int)Math.Round(((int)c - min) * 100.0 / (max - min));
                    return true;
                }
                catch { return false; }
            }
        }

        internal void Close()
        {
            lock (io) { if (Phys != IntPtr.Zero) { Native.DestroyPhysicalMonitor(Phys); Phys = IntPtr.Zero; } }
        }
    }

    static class Brightness
    {
        static volatile List<Display> list = new List<Display>();
        static int scanning;
        static volatile bool dirty;

        static Brightness() { SystemEvents.DisplaySettingsChanged += delegate { dirty = true; }; }

        // Последний известный список экранов (не меняется после публикации)
        public static List<Display> Displays { get { return list; } }

        // Пересканировать экраны в фоне и по готовности вызвать done (из фонового потока)
        public static void ScanAsync(Action done)
        {
            if (Interlocked.Exchange(ref scanning, 1) == 1) return;
            ThreadPool.QueueUserWorkItem(delegate
            {
                try { Scan(); } catch { }
                finally { Interlocked.Exchange(ref scanning, 0); }
                if (done != null) done();
            });
        }

        public static void Scan()
        {
            var old = new Dictionary<string, Display>();
            bool reuse = !dirty; dirty = false;
            foreach (var d in list) old[d.Key] = d;
            var found = new List<Display>();
            var names = TargetNames();

            Native.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMon, IntPtr hdc, IntPtr rc, IntPtr data) =>
            {
                try
                {
                    var mi = new Native.MONITORINFOEX { cbSize = Marshal.SizeOf(typeof(Native.MONITORINFOEX)) };
                    if (!Native.GetMonitorInfo(hMon, ref mi)) return true;
                    Target t;
                    if (!names.TryGetValue(mi.szDevice, out t)) t = new Target { Path = mi.szDevice, Name = null };
                    string key = t.Path;
                    Display d;
                    if (reuse && old.TryGetValue(key, out d)) { old.Remove(key); found.Add(d); return true; }
                    d = new Display { Key = key, Name = t.Name, Internal = t.Internal };
                    if (!t.Internal)
                    {
                        uint n;
                        if (Native.GetNumberOfPhysicalMonitorsFromHMONITOR(hMon, out n) && n > 0)
                        {
                            var pm = new Native.PHYSICAL_MONITOR[n];
                            if (Native.GetPhysicalMonitorsFromHMONITOR(hMon, n, pm))
                            {
                                d.Phys = pm[0].h;
                                if (string.IsNullOrEmpty(d.Name) && !pm[0].desc.StartsWith("Generic")) d.Name = pm[0].desc;
                                for (int i = 1; i < n; i++) Native.DestroyPhysicalMonitor(pm[i].h);
                            }
                        }
                    }
                    found.Add(d);
                }
                catch { }
                return true;
            }, IntPtr.Zero);

            // Оставляем только экраны, которые отвечают; у знакомых прощаем разовый сбой
            var ok = new List<Display>();
            foreach (var d in found)
            {
                bool known = list.Contains(d);
                if (d.Read() || known) ok.Add(d); else d.Close();
            }
            int k = 0;
            foreach (var d in ok) if (string.IsNullOrEmpty(d.Name)) d.Name = d.Internal ? "Встроенный экран" : "Монитор " + (++k);
            ok.Sort((a, b) => a.Internal == b.Internal ? string.Compare(a.Name, b.Name, StringComparison.CurrentCultureIgnoreCase) : (a.Internal ? -1 : 1));
            list = ok;
            foreach (var d in old.Values) d.Close();
        }

        // ── Имена мониторов из EDID через DisplayConfig: «\\.\DISPLAY1» → «DELL U2720Q» ──
        struct Target { public string Path, Name; public bool Internal; }

        static Dictionary<string, Target> TargetNames()
        {
            var res = new Dictionary<string, Target>(StringComparer.OrdinalIgnoreCase);
            try
            {
                uint np, nm;
                if (Native.GetDisplayConfigBufferSizes(2, out np, out nm) != 0) return res; // QDC_ONLY_ACTIVE_PATHS
                var paths = new Native.DISPLAYCONFIG_PATH_INFO[np];
                IntPtr modes = Marshal.AllocHGlobal((int)Math.Max(1, nm) * 64);
                try { if (Native.QueryDisplayConfig(2, ref np, paths, ref nm, modes, IntPtr.Zero) != 0) return res; }
                finally { Marshal.FreeHGlobal(modes); }
                for (int i = 0; i < np; i++)
                {
                    var p = paths[i];
                    var src = new Native.DISPLAYCONFIG_SOURCE_DEVICE_NAME();
                    src.h.type = 1; src.h.size = Marshal.SizeOf(typeof(Native.DISPLAYCONFIG_SOURCE_DEVICE_NAME));
                    src.h.adapterId = p.sourceAdapter; src.h.id = p.sourceId;
                    if (Native.DisplayConfigGetDeviceInfo(ref src) != 0) continue;
                    var tgt = new Native.DISPLAYCONFIG_TARGET_DEVICE_NAME();
                    tgt.h.type = 2; tgt.h.size = Marshal.SizeOf(typeof(Native.DISPLAYCONFIG_TARGET_DEVICE_NAME));
                    tgt.h.adapterId = p.targetAdapter; tgt.h.id = p.targetId;
                    if (Native.DisplayConfigGetDeviceInfo(ref tgt) != 0) continue;
                    uint tech = (uint)tgt.outputTechnology;
                    res[src.viewGdiDeviceName] = new Target
                    {
                        Path = string.IsNullOrEmpty(tgt.monitorDevicePath) ? src.viewGdiDeviceName : tgt.monitorDevicePath,
                        Name = tgt.monitorFriendlyDeviceName,
                        Internal = tech == 0x80000000 || tech == 11 || tech == 13 // встроенный, eDP, UDI embedded
                    };
                }
            }
            catch { }
            return res;
        }

        // ── Встроенный экран: WMI ──
        internal static bool WmiGet(out int v)
        {
            v = 0;
            using (var s = new ManagementObjectSearcher(@"root\wmi", "SELECT CurrentBrightness FROM WmiMonitorBrightness WHERE Active=TRUE"))
            using (var r = s.Get())
                foreach (ManagementObject o in r) using (o) { v = Convert.ToInt32(o["CurrentBrightness"]); return true; }
            return false;
        }

        internal static void WmiSet(int v)
        {
            using (var c = new ManagementClass(@"root\wmi", "WmiMonitorBrightnessMethods", null))
            using (var r = c.GetInstances())
                foreach (ManagementObject o in r) using (o) o.InvokeMethod("WmiSetBrightness", new object[] { (uint)1, (byte)v });
        }
    }

    static partial class Native
    {
        public delegate bool MonitorEnumProc(IntPtr hMon, IntPtr hdc, IntPtr rc, IntPtr data);
        [DllImport("user32.dll")] public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr clip, MonitorEnumProc proc, IntPtr data);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] public static extern bool GetMonitorInfo(IntPtr hMon, ref MONITORINFOEX mi);
        [DllImport("dxva2.dll")] public static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMon, out uint n);
        [DllImport("dxva2.dll")] public static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMon, uint n, [Out] PHYSICAL_MONITOR[] arr);
        [DllImport("dxva2.dll")] public static extern bool DestroyPhysicalMonitor(IntPtr h);
        [DllImport("dxva2.dll")] public static extern bool GetMonitorBrightness(IntPtr h, out uint min, out uint cur, out uint max);
        [DllImport("dxva2.dll")] public static extern bool SetMonitorBrightness(IntPtr h, uint v);
        [DllImport("user32.dll")] public static extern int GetDisplayConfigBufferSizes(uint flags, out uint paths, out uint modes);
        [DllImport("user32.dll")] public static extern int QueryDisplayConfig(uint flags, ref uint np, [Out] DISPLAYCONFIG_PATH_INFO[] paths, ref uint nm, IntPtr modes, IntPtr topology);
        [DllImport("user32.dll")] public static extern int DisplayConfigGetDeviceInfo(ref DISPLAYCONFIG_SOURCE_DEVICE_NAME r);
        [DllImport("user32.dll")] public static extern int DisplayConfigGetDeviceInfo(ref DISPLAYCONFIG_TARGET_DEVICE_NAME r);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct MONITORINFOEX
        {
            public int cbSize; public int l, t, r, b, wl, wt, wr, wb; public int dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string szDevice;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct PHYSICAL_MONITOR { public IntPtr h; [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string desc; }

        [StructLayout(LayoutKind.Sequential)] public struct LUID { public uint lo; public int hi; }

        // DISPLAYCONFIG_PATH_INFO (72 байта), вложенные структуры развёрнуты
        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_PATH_INFO
        {
            public LUID sourceAdapter; public uint sourceId, sourceModeIdx, sourceFlags;
            public LUID targetAdapter; public uint targetId, targetModeIdx; public int outputTechnology, rotation, scaling;
            public uint refreshNum, refreshDen; public int scanLineOrdering, targetAvailable; public uint targetFlags;
            public uint flags;
        }

        [StructLayout(LayoutKind.Sequential)] public struct DISPLAYCONFIG_HEADER { public int type, size; public LUID adapterId; public uint id; }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DISPLAYCONFIG_SOURCE_DEVICE_NAME
        {
            public DISPLAYCONFIG_HEADER h;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string viewGdiDeviceName;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DISPLAYCONFIG_TARGET_DEVICE_NAME
        {
            public DISPLAYCONFIG_HEADER h;
            public uint flags; public int outputTechnology; public ushort edidManufactureId, edidProductCodeId; public uint connectorInstance;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)] public string monitorFriendlyDeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string monitorDevicePath;
        }
    }
}
