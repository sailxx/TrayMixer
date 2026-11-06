// TrayMixer: blue light filter and screen dimming through the video card gamma table (SetDeviceGammaRamp).
// Works on any monitor without DDC/CI. Windows refuses "too steep" tables by default,
// so when it refuses, the effect strength is reduced step by step to the largest value it accepts.
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;

namespace TrayMixer
{
    static class BlueLight
    {
        const string CfgKey = @"Software\TrayMixer";
        const int MinKelvin = 2000;            // color temperature at strength 100
        const double MaxDim = 0.8;             // dimming 100 leaves 20% brightness

        static volatile int strength = Clamp(Theme.RegInt(CfgKey, "BlueFilter", 0)), dim = Clamp(Theme.RegInt(CfgKey, "BlueDim", 0));
        static volatile bool filterOn = Theme.RegInt(CfgKey, "BlueFilterOn", 1) == 1, dimOn = Theme.RegInt(CfgKey, "BlueDimOn", 1) == 1;
        static readonly object io = new object(), q = new object();
        static readonly Dictionary<string, ushort[]> applied = new Dictionary<string, ushort[]>(); // what was applied to each screen
        static bool busy, pending, touched;
        static Timer guard;
        static volatile bool stopped;     // Reset was called: no more writes
        static bool dirtyFlag;            // mirrors the registry flag
        const string DirtyValue = "GammaApplied";

        public static int Strength { get { return strength; } }
        public static int Dim { get { return dim; } }
        public static bool FilterOn { get { return filterOn; } }
        public static bool DimOn { get { return dimOn; } }

        public static void SetStrength(int v) { strength = Clamp(v); Apply(); }
        public static void SetDim(int v) { dim = Clamp(v); Apply(); }
        public static void SetFilterOn(bool on) { filterOn = on; Apply(); }
        public static void SetDimOn(bool on) { dimOn = on; Apply(); }

        static int Clamp(int v) { return Math.Max(0, Math.Min(100, v)); }
        static bool Active { get { return (filterOn && strength > 0) || (dimOn && dim > 0); } }

        // Apply saved settings and make sure nobody resets the table (mode switch, wake from sleep, games)
        public static void Start()
        {
            SystemEvents.DisplaySettingsChanged += delegate { lock (io) applied.Clear(); if (Active) Apply(); };
            SystemEvents.PowerModeChanged += (s, e) => { if (e.Mode == PowerModes.Resume && Active) Apply(); };
            // Put the gamma back however the process ends (menu Exit, logoff, shutdown)
            AppDomain.CurrentDomain.ProcessExit += delegate { Reset(); };
            SystemEvents.SessionEnding += delegate { Reset(); };
            guard = new Timer(delegate { if (Active) try { Write(true); } catch { } }, null, 5000, 5000);
            if (Active) Apply();
            else if (Theme.RegInt(CfgKey, DirtyValue, 0) == 1)
                // The previous run was killed while the table was still changed: restore normal gamma once
                ThreadPool.QueueUserWorkItem(delegate { try { RestoreNormal(); } catch { } });
        }

        // Give the screens their normal gamma back (on exit)
        public static void Reset()
        {
            stopped = true;
            var g = guard; if (g != null) g.Dispose();
            RestoreNormal();
        }

        static void RestoreNormal()
        {
            lock (io)
            {
                if (touched || dirtyFlag || Theme.RegInt(CfgKey, DirtyValue, 0) == 1)
                    ForEachScreen((dc, name) => Native.SetDeviceGammaRamp(dc, Ramp(1, 1, 1)));
                touched = false; applied.Clear(); SetDirty(false);
            }
        }

        // Registry flag: the gamma table is currently changed by TrayMixer (survives a kill or crash)
        static void SetDirty(bool on)
        {
            try
            {
                using (var k = Registry.CurrentUser.CreateSubKey(CfgKey))
                    if (on) k.SetValue(DirtyValue, 1); else k.DeleteValue(DirtyValue, false);
            }
            catch { }
            dirtyFlag = on;
        }

        // Writing happens in the background; while it is busy only the latest value is kept
        static void Apply()
        {
            lock (q) { pending = true; if (busy) return; busy = true; }
            ThreadPool.QueueUserWorkItem(delegate
            {
                while (true)
                {
                    lock (q) { if (!pending) { busy = false; return; } pending = false; }
                    try { Write(false); Save(); } catch { }
                }
            });
        }

        static void Save()
        {
            using (var k = Registry.CurrentUser.CreateSubKey(CfgKey))
            {
                k.SetValue("BlueFilter", strength); k.SetValue("BlueDim", dim);
                k.SetValue("BlueFilterOn", filterOn ? 1 : 0); k.SetValue("BlueDimOn", dimOn ? 1 : 0);
            }
        }

        // check: only restore the table where it was replaced
        static void Write(bool check)
        {
            lock (io)
            {
                if (stopped) return;
                touched = true;
                double r, g, b; Target(out r, out g, out b);
                bool identity = r >= 1 && g >= 1 && b >= 1;
                if (!identity && !dirtyFlag) SetDirty(true);
                else if (identity && dirtyFlag) SetDirty(false);
                ForEachScreen((dc, name) =>
                {
                    ushort[] last;
                    if (check && applied.TryGetValue(name, out last))
                    {
                        var cur = new ushort[768];
                        if (!Native.GetDeviceGammaRamp(dc, cur) || Same(cur, last)) return;
                    }
                    // Full effect; if Windows refuses, the largest share it accepts
                    var ramp = Ramp(r, g, b);
                    if (!Native.SetDeviceGammaRamp(dc, ramp))
                    {
                        double lo = 0, hi = 1; ramp = null;
                        for (int i = 0; i < 7; i++)
                        {
                            double k = (lo + hi) / 2;
                            var t = Ramp(1 - (1 - r) * k, 1 - (1 - g) * k, 1 - (1 - b) * k);
                            if (Native.SetDeviceGammaRamp(dc, t)) { lo = k; ramp = t; } else hi = k;
                        }
                        if (ramp == null) { ramp = Ramp(1, 1, 1); Native.SetDeviceGammaRamp(dc, ramp); }
                    }
                    applied[name] = ramp;
                });
            }
        }

        // Channel multipliers: warm color temperature and overall dimming
        static void Target(out double r, out double g, out double b)
        {
            r = g = b = 1;
            if (filterOn && strength > 0)
            {
                double r0, g0, b0, r1, g1, b1;
                Kelvin(6500, out r0, out g0, out b0);
                Kelvin(6500 - (6500 - MinKelvin) * strength / 100.0, out r1, out g1, out b1);
                r = Math.Min(1, r1 / r0); g = Math.Min(1, g1 / g0); b = Math.Min(1, b1 / b0);
            }
            if (dimOn && dim > 0) { double m = 1 - MaxDim * dim / 100.0; r *= m; g *= m; b *= m; }
        }

        // Black body color (Tanner Helland approximation), 0-1 per channel
        static void Kelvin(double k, out double r, out double g, out double b)
        {
            double t = k / 100;
            r = t <= 66 ? 255 : 329.698727446 * Math.Pow(t - 60, -0.1332047592);
            g = t <= 66 ? 99.4708025861 * Math.Log(t) - 161.1195681661 : 288.1221695283 * Math.Pow(t - 60, -0.0755148492);
            b = t >= 66 ? 255 : t <= 19 ? 0 : 138.5177312231 * Math.Log(t - 10) - 305.0447927307;
            r = Math.Max(0, Math.Min(255, r)) / 255; g = Math.Max(0, Math.Min(255, g)) / 255; b = Math.Max(0, Math.Min(255, b)) / 255;
        }

        static ushort[] Ramp(double r, double g, double b)
        {
            var a = new ushort[768];
            for (int i = 0; i < 256; i++)
            {
                a[i] = (ushort)Math.Min(65535, Math.Round(i * 257 * r));
                a[256 + i] = (ushort)Math.Min(65535, Math.Round(i * 257 * g));
                a[512 + i] = (ushort)Math.Min(65535, Math.Round(i * 257 * b));
            }
            return a;
        }

        static bool Same(ushort[] a, ushort[] b)
        {
            for (int i = 0; i < a.Length; i++) if (Math.Abs(a[i] - b[i]) > 256) return false; // the driver may round slightly
            return true;
        }

        static void ForEachScreen(Action<IntPtr, string> f)
        {
            var names = new List<string>();
            Native.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMon, IntPtr hdc, IntPtr rc, IntPtr data) =>
            {
                var mi = new Native.MONITORINFOEX { cbSize = Marshal.SizeOf(typeof(Native.MONITORINFOEX)) };
                if (Native.GetMonitorInfo(hMon, ref mi)) names.Add(mi.szDevice);
                return true;
            }, IntPtr.Zero);
            foreach (var n in names)
            {
                IntPtr dc = Native.CreateDC("DISPLAY", n, null, IntPtr.Zero);
                if (dc == IntPtr.Zero) continue;
                try { f(dc, n); } catch { }
                finally { Native.DeleteDC(dc); }
            }
        }
    }

    static partial class Native
    {
        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)] public static extern IntPtr CreateDC(string driver, string device, string output, IntPtr init);
        [DllImport("gdi32.dll")] public static extern bool SetDeviceGammaRamp(IntPtr hdc, [In] ushort[] ramp);
        [DllImport("gdi32.dll")] public static extern bool GetDeviceGammaRamp(IntPtr hdc, [Out] ushort[] ramp);
    }
}
