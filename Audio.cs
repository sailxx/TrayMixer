// TrayMixer — работа с Core Audio (устройства, сессии приложений).
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;

namespace TrayMixer
{
    // ───────────────────────── Core Audio COM ─────────────────────────
    [StructLayout(LayoutKind.Sequential)]
    struct PROPERTYKEY { public Guid fmtid; public int pid; public PROPERTYKEY(Guid g, int p) { fmtid = g; pid = p; } }

    [StructLayout(LayoutKind.Explicit)]
    struct PROPVARIANT
    {
        [FieldOffset(0)] public ushort vt;
        [FieldOffset(8)] public IntPtr p;
        [FieldOffset(8)] public uint uintVal;
        [FieldOffset(16)] public IntPtr pad;
    }

    [ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")] class MMDeviceEnumeratorCo { }
    [ComImport, Guid("870af99c-171d-4f9e-af0d-e63df40c2bc9")] class PolicyConfigCo { }

    [ComImport, Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IMMDeviceEnumerator
    {
        void EnumAudioEndpoints(int dataFlow, int stateMask, out IMMDeviceCollection devices);
        [PreserveSig] int GetDefaultAudioEndpoint(int dataFlow, int role, out IMMDevice device);
        void GetDevice([MarshalAs(UnmanagedType.LPWStr)] string id, out IMMDevice device);
        void RegisterEndpointNotificationCallback(IMMNotificationClient client);
        void UnregisterEndpointNotificationCallback(IMMNotificationClient client);
    }

    // Уведомления Windows вместо постоянного опроса
    [ComImport, Guid("7991EEC9-7E89-4D85-8390-6C703CEC60C0"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IMMNotificationClient
    {
        [PreserveSig] int OnDeviceStateChanged([MarshalAs(UnmanagedType.LPWStr)] string id, int state);
        [PreserveSig] int OnDeviceAdded([MarshalAs(UnmanagedType.LPWStr)] string id);
        [PreserveSig] int OnDeviceRemoved([MarshalAs(UnmanagedType.LPWStr)] string id);
        [PreserveSig] int OnDefaultDeviceChanged(int flow, int role, [MarshalAs(UnmanagedType.LPWStr)] string id);
        [PreserveSig] int OnPropertyValueChanged([MarshalAs(UnmanagedType.LPWStr)] string id, PROPERTYKEY key);
    }

    [ComImport, Guid("657804FA-D6AD-4496-8A60-352752AF4F89"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IAudioEndpointVolumeCallback { [PreserveSig] int OnNotify(IntPtr data); }

    [ComImport, Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IMMDeviceCollection { void GetCount(out int n); void Item(int i, out IMMDevice d); }

    [ComImport, Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IMMDevice
    {
        void Activate(ref Guid iid, int ctx, IntPtr p, [MarshalAs(UnmanagedType.IUnknown)] out object o);
        void OpenPropertyStore(int access, out IPropertyStore ps);
        void GetId([MarshalAs(UnmanagedType.LPWStr)] out string id);
        void GetState(out int s);
    }

    [ComImport, Guid("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IPropertyStore
    {
        void GetCount(out int c);
        void GetAt(int i, out PROPERTYKEY k);
        void GetValue(ref PROPERTYKEY k, out PROPVARIANT v);
    }

    [ComImport, Guid("5CDF2C82-841E-4546-9722-0CF74078229A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IAudioEndpointVolume
    {
        void RegisterControlChangeNotify(IAudioEndpointVolumeCallback n);
        void UnregisterControlChangeNotify(IAudioEndpointVolumeCallback n);
        void GetChannelCount(out int c);
        void SetMasterVolumeLevel(float l, ref Guid ctx);
        void SetMasterVolumeLevelScalar(float l, ref Guid ctx);
        void GetMasterVolumeLevel(out float l);
        void GetMasterVolumeLevelScalar(out float l);
        void SetChannelVolumeLevel(int ch, float l, ref Guid ctx);
        void SetChannelVolumeLevelScalar(int ch, float l, ref Guid ctx);
        void GetChannelVolumeLevel(int ch, out float l);
        void GetChannelVolumeLevelScalar(int ch, out float l);
        void SetMute([MarshalAs(UnmanagedType.Bool)] bool m, ref Guid ctx);
        void GetMute([MarshalAs(UnmanagedType.Bool)] out bool m);
    }

    [ComImport, Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IAudioSessionManager2
    {
        void GetAudioSessionControl(IntPtr g, int f, out IntPtr c);
        void GetSimpleAudioVolume(IntPtr g, int f, out IntPtr v);
        void GetSessionEnumerator(out IAudioSessionEnumerator e);
    }

    [ComImport, Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IAudioSessionEnumerator { void GetCount(out int c); void GetSession(int i, out IAudioSessionControl2 s); }

    [ComImport, Guid("bfb7ff88-7239-4fc9-8fa2-07c950be9c6d"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IAudioSessionControl2
    {
        void GetState(out int s);
        void GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string n);
        void SetDisplayName([MarshalAs(UnmanagedType.LPWStr)] string n, ref Guid ctx);
        void GetIconPath([MarshalAs(UnmanagedType.LPWStr)] out string p);
        void SetIconPath([MarshalAs(UnmanagedType.LPWStr)] string p, ref Guid ctx);
        void GetGroupingParam(out Guid g);
        void SetGroupingParam(ref Guid g, ref Guid ctx);
        void RegisterAudioSessionNotification(IntPtr n);
        void UnregisterAudioSessionNotification(IntPtr n);
        void GetSessionIdentifier([MarshalAs(UnmanagedType.LPWStr)] out string s);
        void GetSessionInstanceIdentifier([MarshalAs(UnmanagedType.LPWStr)] out string s);
        void GetProcessId(out int pid);
        [PreserveSig] int IsSystemSoundsSession();
    }

    [ComImport, Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface ISimpleAudioVolume
    {
        void SetMasterVolume(float l, ref Guid ctx);
        void GetMasterVolume(out float l);
        void SetMute([MarshalAs(UnmanagedType.Bool)] bool m, ref Guid ctx);
        void GetMute([MarshalAs(UnmanagedType.Bool)] out bool m);
    }

    // Недокументированный интерфейс для смены устройства по умолчанию
    [ComImport, Guid("f8679f50-850a-41cf-9c72-430f290290c8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IPolicyConfig
    {
        [PreserveSig] int U1(); [PreserveSig] int U2(); [PreserveSig] int U3(); [PreserveSig] int U4(); [PreserveSig] int U5();
        [PreserveSig] int U6(); [PreserveSig] int U7(); [PreserveSig] int U8(); [PreserveSig] int U9(); [PreserveSig] int U10();
        [PreserveSig] int SetDefaultEndpoint([MarshalAs(UnmanagedType.LPWStr)] string id, int role);
    }

    static class Native
    {
        [DllImport("user32.dll")] public static extern bool DestroyIcon(IntPtr h);
        [DllImport("ole32.dll")] public static extern int PropVariantClear(ref PROPVARIANT pv);
        [DllImport("kernel32.dll")] public static extern IntPtr OpenProcess(int access, bool inherit, int pid);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] public static extern bool QueryFullProcessImageName(IntPtr h, int flags, StringBuilder sb, ref int size);
        [DllImport("kernel32.dll")] public static extern bool CloseHandle(IntPtr h);
        [DllImport("kernel32.dll")] public static extern bool SetProcessWorkingSetSize(IntPtr h, IntPtr min, IntPtr max);
        [DllImport("kernel32.dll")] public static extern IntPtr GetCurrentProcess();
        [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
        [DllImport("kernel32.dll")] public static extern bool SetDefaultDllDirectories(int flags);
        [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
        [DllImport("dwmapi.dll")] public static extern int DwmSetWindowAttribute(IntPtr h, int attr, ref int val, int size);
        [DllImport("dwmapi.dll")] public static extern int DwmExtendFrameIntoClientArea(IntPtr h, ref MARGINS m);
        [DllImport("gdi32.dll")] public static extern IntPtr CreateDIBSection(IntPtr hdc, ref BITMAPINFOHEADER bi, int usage, out IntPtr bits, IntPtr sec, int off);
        [DllImport("gdi32.dll")] public static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll")] public static extern IntPtr SelectObject(IntPtr hdc, IntPtr obj);
        [DllImport("gdi32.dll")] public static extern bool DeleteObject(IntPtr obj);
        [DllImport("gdi32.dll")] public static extern bool DeleteDC(IntPtr hdc);
        [DllImport("gdi32.dll")] public static extern bool BitBlt(IntPtr dst, int x, int y, int w, int h, IntPtr src, int sx, int sy, int rop);
        [DllImport("gdi32.dll")] public static extern bool GdiFlush();
        [DllImport("uxtheme.dll", EntryPoint = "#135")] public static extern int SetPreferredAppMode(int mode);
        [DllImport("uxtheme.dll", EntryPoint = "#136")] public static extern void FlushMenuThemes();

        [StructLayout(LayoutKind.Sequential)] public struct MARGINS { public int L, R, T, B; }
        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFOHEADER
        {
            public int biSize, biWidth, biHeight; public short biPlanes, biBitCount;
            public int biCompression, biSizeImage, biXPelsPerMeter, biYPelsPerMeter, biClrUsed, biClrImportant;
        }

        // Отдать неиспользуемую память системе
        public static void Trim()
        {
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            SetProcessWorkingSetSize(GetCurrentProcess(), (IntPtr)(-1), (IntPtr)(-1));
        }
    }

    // ───────────────────────── Модель ─────────────────────────
    class DeviceInfo { public string Id, Name; public bool IsDefault, IsHeadphones, IsDisplay; public IAudioEndpointVolume Vol; }
    class AppInfo { public string Key, Name; public Bitmap Img; public bool IsSystem; public List<ISimpleAudioVolume> Vols = new List<ISimpleAudioVolume>(); }

    static class Audio
    {
        public static readonly IMMDeviceEnumerator En = (IMMDeviceEnumerator)new MMDeviceEnumeratorCo();
        static Guid IID_EPV = typeof(IAudioEndpointVolume).GUID;
        static Guid IID_ASM2 = typeof(IAudioSessionManager2).GUID;
        static PROPERTYKEY PK_Name = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 14);
        static PROPERTYKEY PK_FormFactor = new PROPERTYKEY(new Guid("1da5d803-d492-4edd-8c23-e0c0ffee7f0e"), 0);
        static readonly Dictionary<string, string> NameCache = new Dictionary<string, string>();
        static readonly Dictionary<string, Bitmap> IconCache = new Dictionary<string, Bitmap>();
        public static Guid Ctx = Guid.Empty;

        public static IMMDevice Default()
        {
            IMMDevice d;
            return En.GetDefaultAudioEndpoint(0, 1, out d) == 0 ? d : null;
        }

        public static string DefaultId()
        {
            var d = Default(); if (d == null) return null;
            string id; d.GetId(out id); return id;
        }

        public static IAudioEndpointVolume EndpointVolume(IMMDevice d)
        {
            object o; d.Activate(ref IID_EPV, 23, IntPtr.Zero, out o);
            return (IAudioEndpointVolume)o;
        }

        public static List<DeviceInfo> Devices()
        {
            var list = new List<DeviceInfo>();
            string def = DefaultId();
            IMMDeviceCollection col;
            En.EnumAudioEndpoints(0, 1, out col);
            int n; col.GetCount(out n);
            for (int i = 0; i < n; i++)
            {
                try
                {
                    IMMDevice d; col.Item(i, out d);
                    var di = new DeviceInfo();
                    d.GetId(out di.Id);
                    IPropertyStore ps; d.OpenPropertyStore(0, out ps);
                    PROPVARIANT v;
                    ps.GetValue(ref PK_Name, out v);
                    di.Name = v.vt == 31 ? Marshal.PtrToStringUni(v.p) : "Устройство";
                    Native.PropVariantClear(ref v);
                    ps.GetValue(ref PK_FormFactor, out v);
                    di.IsHeadphones = v.vt == 19 && (v.uintVal == 3 || v.uintVal == 5);
                    di.IsDisplay = v.vt == 19 && v.uintVal == 9; // HDMI / DisplayPort
                    Native.PropVariantClear(ref v);
                    di.IsDefault = di.Id == def;
                    di.Vol = EndpointVolume(d);
                    list.Add(di);
                }
                catch { }
            }
            list.Sort((a, b) => a.IsDefault == b.IsDefault ? string.Compare(a.Name, b.Name) : (a.IsDefault ? -1 : 1));
            return list;
        }

        public static List<AppInfo> Apps()
        {
            var map = new Dictionary<string, AppInfo>();
            var order = new List<AppInfo>();
            var dev = Default(); if (dev == null) return order;
            object o; dev.Activate(ref IID_ASM2, 23, IntPtr.Zero, out o);
            var mgr = (IAudioSessionManager2)o;
            IAudioSessionEnumerator en; mgr.GetSessionEnumerator(out en);
            int n; en.GetCount(out n);
            for (int i = 0; i < n; i++)
            {
                try
                {
                    IAudioSessionControl2 s; en.GetSession(i, out s);
                    int state; s.GetState(out state);
                    if (state == 2) continue; // expired
                    bool sys = s.IsSystemSoundsSession() == 0;
                    int pid = 0; s.GetProcessId(out pid);
                    string path = sys ? null : ProcPath(pid);
                    if (!sys && path == null && !Alive(pid)) continue;
                    string key = sys ? "#sys" : (path ?? ("#pid" + pid));
                    AppInfo a;
                    if (!map.TryGetValue(key, out a))
                    {
                        a = new AppInfo { Key = key, IsSystem = sys };
                        if (sys) a.Name = "Системные звуки";
                        else
                        {
                            string dn; s.GetDisplayName(out dn);
                            a.Name = !string.IsNullOrEmpty(dn) && !dn.StartsWith("@") ? dn : NameFor(path, pid);
                            a.Img = IconFor(path);
                        }
                        map[key] = a; order.Add(a);
                    }
                    a.Vols.Add((ISimpleAudioVolume)s);
                }
                catch { }
            }
            order.Sort((x, y) => x.IsSystem == y.IsSystem ? string.Compare(x.Name, y.Name, StringComparison.CurrentCultureIgnoreCase) : (x.IsSystem ? 1 : -1));
            return order;
        }

        public static void SetDefault(string id)
        {
            try
            {
                var pc = (IPolicyConfig)new PolicyConfigCo();
                for (int r = 0; r < 3; r++) pc.SetDefaultEndpoint(id, r);
            }
            catch { }
        }

        static bool Alive(int pid) { try { Process.GetProcessById(pid); return true; } catch { return false; } }

        static string ProcPath(int pid)
        {
            if (pid <= 0) return null;
            IntPtr h = Native.OpenProcess(0x1000, false, pid);
            if (h == IntPtr.Zero) return null;
            try
            {
                var sb = new StringBuilder(1024); int sz = sb.Capacity;
                return Native.QueryFullProcessImageName(h, 0, sb, ref sz) ? sb.ToString() : null;
            }
            finally { Native.CloseHandle(h); }
        }

        static string NameFor(string path, int pid)
        {
            string name;
            if (path != null && NameCache.TryGetValue(path, out name)) return name;
            name = null;
            if (path != null)
            {
                try
                {
                    var fvi = FileVersionInfo.GetVersionInfo(path);
                    if (!string.IsNullOrWhiteSpace(fvi.FileDescription)) name = fvi.FileDescription.Trim();
                    else if (!string.IsNullOrWhiteSpace(fvi.ProductName)) name = fvi.ProductName.Trim();
                }
                catch { }
                if (name == null) name = System.IO.Path.GetFileNameWithoutExtension(path);
                NameCache[path] = name;
                return name;
            }
            try { return Process.GetProcessById(pid).ProcessName; } catch { return "Приложение"; }
        }

        static Bitmap IconFor(string path)
        {
            if (path == null) return null;
            Bitmap img;
            if (IconCache.TryGetValue(path, out img)) return img;
            try { using (var ic = Icon.ExtractAssociatedIcon(path)) img = ic.ToBitmap(); } catch { img = null; }
            IconCache[path] = img;
            return img;
        }
    }
}
