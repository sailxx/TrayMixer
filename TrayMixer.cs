// TrayMixer: lightweight tray volume mixer for Windows 11 (Mica/Acrylic, rounded corners).
// The window is drawn by hand onto one surface with an alpha channel, without WPF or child controls.
// Build: build.cmd (the csc compiler built into .NET Framework 4.x)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

[assembly: System.Runtime.InteropServices.DefaultDllImportSearchPaths(System.Runtime.InteropServices.DllImportSearchPath.System32)]
[assembly: System.Reflection.AssemblyTitle("TrayMixer")]
[assembly: System.Reflection.AssemblyProduct("TrayMixer")]
[assembly: System.Reflection.AssemblyDescription("Lightweight Windows 11 tray volume mixer")]
[assembly: System.Reflection.AssemblyVersion("3.0.1.0")]
[assembly: System.Reflection.AssemblyFileVersion("3.0.1.0")]

namespace TrayMixer
{
    // ───────────────────────── Theme ─────────────────────────
    class Theme
    {
        public bool Light;
        bool transparency;
        public Color Fg, Sub, Track, ThumbOuter, ThumbBorder, Hover, Sep, Accent, Badge, Footer, Tint;
        public static readonly string FontName = Pick("Inter", "Segoe UI");
        public static readonly string FontSemi = Pick("Segoe UI Variable Text Semibold", "Segoe UI Semibold");
        public static readonly string GlyphName = Pick("Segoe Fluent Icons", "Segoe MDL2 Assets");
        const string Personalize = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

        static string Pick(string a, string b) { try { using (new FontFamily(a)) return a; } catch { return b; } }
        static Color C(uint argb) { return Color.FromArgb(unchecked((int)argb)); }

        public static Theme Load()
        {
            // The charcoal surface keeps light controls in either Windows theme.
            var t = new Theme { Light = false, transparency = !SystemInformation.HighContrast && RegInt(Personalize, "EnableTransparency", 1) != 0 };
            if (t.Light)
            {
                t.Fg = C(0xFF111820); t.Sub = C(0xFF424B56); t.Track = C(0xB0000000); t.ThumbOuter = C(0xFFFFFFFF);
                t.ThumbBorder = C(0x29000000); t.Hover = C(0x0F000000); t.Sep = C(0x14000000); t.Badge = C(0xFFF3F3F3); t.Footer = C(0x0C000000);
                t.Tint = C(0x88F9F9F9);
            }
            else
            {
                t.Fg = C(0xFFFFFFFF); t.Sub = C(0xC5FFFFFF); t.Track = C(0x8BFFFFFF); t.ThumbOuter = C(0xFF454545);
                t.ThumbBorder = C(0x18FFFFFF); t.Hover = C(0x15FFFFFF); t.Sep = C(0x18FFFFFF); t.Badge = C(0xFF2C2C2C); t.Footer = C(0x40000000);
                t.Tint = C(0x9A202020);
            }
            t.Accent = t.Light ? Color.FromArgb(0, 95, 184) : Color.FromArgb(76, 194, 255);
            try
            {
                using (var k = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Accent"))
                {
                    var pal = k == null ? null : k.GetValue("AccentPalette") as byte[];
                    if (pal != null && pal.Length >= 32) { int i = (t.Light ? 4 : 1) * 4; t.Accent = Color.FromArgb(pal[i], pal[i + 1], pal[i + 2]); }
                }
            }
            catch { }
            // Keep secondary labels and controls readable on the brightest wallpaper.
            if (!t.Light) { t.Sub = Color.FromArgb(218, 226, 236); t.Track = Color.FromArgb(190, 202, 216); }
            if (SystemInformation.HighContrast)
            {
                t.Fg = t.Sub = SystemColors.WindowText; t.Accent = SystemColors.Highlight;
                t.Track = SystemColors.WindowText; t.Hover = SystemColors.Highlight;
                t.Badge = t.ThumbOuter = SystemColors.Window; t.Footer = SystemColors.Window;
            }
            return t;
        }

        public bool Transparency { get { return transparency; } }
        public Color Solid { get { return SystemInformation.HighContrast ? SystemColors.Window : Color.FromArgb(32, 32, 32); } }
        // Only about 10% wallpaper shows through; foreground elements remain fully opaque.
        public Color Glass { get { return Transparency ? Color.FromArgb(230, 32, 32, 32) : Solid; } }
        public Color Surface(bool material) { return material && Transparency ? Glass : Solid; }
        public bool SameAs(Theme o) { return o.Light == Light && o.Accent == Accent && o.Transparency == Transparency; }
        public static bool TaskbarLight() { return RegInt(Personalize, "SystemUsesLightTheme", 0) == 1; }

        public static int RegInt(string path, string name, int def)
        {
            try { using (var k = Registry.CurrentUser.OpenSubKey(path)) { var v = k == null ? null : k.GetValue(name); return v is int ? (int)v : def; } }
            catch { return def; }
        }
    }

    // Wallpaper-backed frost avoids the opaque ToolStrip/DWM composition path.
    // Only the public desktop wallpaper is read; never capture other windows.
    static class WallpaperGlass
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern bool SystemParametersInfo(int action, int param, StringBuilder value, int flags);
        static Bitmap wallpaper;
        static readonly object gate = new object();
        static readonly Dictionary<Size, Bitmap> blurredCache = new Dictionary<Size, Bitmap>();
        static readonly Dictionary<string, Bitmap> surfaces = new Dictionary<string, Bitmap>();
        internal static int SurfaceBuilds;
        static int refreshing;
        internal static int CacheBuilds;
        static string signature;
        internal static void RefreshAsync(Control owner)
        {
            if (!Theme.Load().Transparency) return;
            if (Interlocked.Exchange(ref refreshing, 1) != 0) return;
            ThreadPool.QueueUserWorkItem(delegate {
                try { Refresh(); }
                finally { Interlocked.Exchange(ref refreshing, 0); }
                try { if (!owner.IsDisposed && owner.IsHandleCreated) owner.BeginInvoke((Action)(() => owner.Invalidate(true))); } catch { }
            });
        }
        static void ClearCache()
        {
            foreach (var image in surfaces.Values) image.Dispose();
            surfaces.Clear();
            foreach (var image in blurredCache.Values) image.Dispose();
            blurredCache.Clear();
        }
        internal static string Status = "not loaded";
        internal static int Paints;
        internal static void Refresh()
        {
            try
            {
                var path = new StringBuilder(32768);
                if (!SystemParametersInfo(0x73, path.Capacity, path, 0)) return;
                string file = path.ToString();
                if (!System.IO.File.Exists(file))
                    file = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Microsoft\Windows\Themes\TranscodedWallpaper");
                string next = file + "|" + System.IO.File.GetLastWriteTimeUtc(file).Ticks;
                if (next == signature) return;
                Bitmap loaded = null;
                if (System.IO.File.Exists(file))
                    using (var image = Image.FromFile(file)) loaded = new Bitmap(image);
                lock (gate)
                {
                    if (wallpaper != null) wallpaper.Dispose();
                    ClearCache();
                    wallpaper = loaded; signature = next;
                    Status = loaded == null ? "wallpaper unavailable" : "wallpaper loaded " + loaded.Width + "x" + loaded.Height;
                }
            }
            catch { lock (gate) { if (wallpaper != null) wallpaper.Dispose(); wallpaper = null; signature = null; ClearCache(); } }
        }
        internal static void Paint(Graphics g, Rectangle client, Rectangle screenBounds, Rectangle windowBounds, Theme theme)
        {
            Paints++;
            if (client.Width <= 0 || client.Height <= 0) return;
            lock (gate)
            {
                // Cache the final crop and tint, not just the monitor-sized wallpaper.
                string key = client.Size + "|" + screenBounds + "|" + windowBounds + "|" + theme.Glass.ToArgb() + "|" + theme.Solid.ToArgb();
                Bitmap prepared;
                if (!surfaces.TryGetValue(key, out prepared))
                {
                    if (surfaces.Count >= 8) { foreach (var image in surfaces.Values) image.Dispose(); surfaces.Clear(); }
                    prepared = new Bitmap(client.Width, client.Height, PixelFormat.Format32bppPArgb);
                    using (var graphics = Graphics.FromImage(prepared)) PaintCore(graphics, new Rectangle(Point.Empty, client.Size), screenBounds, windowBounds, theme);
                    surfaces.Add(key, prepared); SurfaceBuilds++;
                }
                g.DrawImageUnscaled(prepared, client.Location);
            }
        }
        static void PaintCore(Graphics g, Rectangle client, Rectangle screenBounds, Rectangle windowBounds, Theme theme)
        {
            g.Clear(theme.Solid);
            if (!theme.Transparency || client.Width <= 0 || client.Height <= 0) return;
            // Near-clear frost preserves more wallpaper detail without weakening the contrast scrim.
            int w = Math.Max(1, screenBounds.Width / 2), h = Math.Max(1, screenBounds.Height / 2);
            lock (gate)
            {
                if (wallpaper == null) return;
                Bitmap blurred;
                var key = new Size(w, h);
                if (!blurredCache.TryGetValue(key, out blurred))
                {
                    if (blurredCache.Count >= 4) ClearCache();
                    blurred = new Bitmap(w, h);
                    using (var bg = Graphics.FromImage(blurred))
                    {
                        bg.Clear(Color.Black);
                        bg.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        float scale = Math.Max((float)w / wallpaper.Width, (float)h / wallpaper.Height);
                        float dw = wallpaper.Width * scale, dh = wallpaper.Height * scale;
                        bg.DrawImage(wallpaper, (w - dw) / 2, (h - dh) / 2, dw, dh);
                    }
                    blurredCache.Add(key, blurred); CacheBuilds++;
                }
                var old = g.InterpolationMode;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                var src = new RectangleF((windowBounds.Left - screenBounds.Left) * w / (float)screenBounds.Width,
                    (windowBounds.Top - screenBounds.Top) * h / (float)screenBounds.Height,
                    client.Width * w / (float)screenBounds.Width, client.Height * h / (float)screenBounds.Height);
                g.DrawImage(blurred, client, src, GraphicsUnit.Pixel);
                g.InterpolationMode = old;
            }
            using (var tint = new SolidBrush(theme.Glass)) g.FillRectangle(tint, client);
        }
    }

    static class Glyphs
    {
        public const string Speaker = "", Headphones = "", Mute = "", Bell = "", Monitor = "", Mixer = "";
        public const string Sun = "", Moon = "", Dim = "";
        public const string Vol0 = "", Vol1 = "", Vol2 = "", Vol3 = "";
    }

    // ───────────────────────── Icon: three sliders ─────────────────────────
    static class Art
    {
        public static readonly Color Red = Color.FromArgb(255, 69, 58);

        // Logo: three horizontal Windows 11 sliders with a red level and a round knob.
        // c is the color of the track and knobs (white or black to match the taskbar theme); its alpha dims the whole icon.
        // At small sizes everything is pixel-aligned so the tray icon stays sharp.
        public static void DrawMixer(Graphics g, RectangleF r, Color c)
        {
            int n = (int)Math.Min(r.Width, r.Height);
            var oldSm = g.SmoothingMode; var oldPo = g.PixelOffsetMode;
            g.SmoothingMode = SmoothingMode.AntiAlias; g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            int th = Math.Max(2, (int)Math.Round(n * 0.07));          // track thickness
            int kr = Math.Max(2, (int)Math.Round(n * 0.11));          // knob radius
            int pad = Math.Max(1, (int)Math.Round(n * 0.08));         // left and right padding
            float x0 = r.X + pad, x1 = r.X + n - pad;
            float[] rows = { 0.22f, 0.5f, 0.78f }, lv = { 0.72f, 0.3f, 0.55f };
            var red = Color.FromArgb(c.A, Red);
            using (var track = new SolidBrush(Color.FromArgb(c.A * 40 / 100, c)))
            using (var fill = new SolidBrush(red))
            using (var knob = new SolidBrush(c))
                for (int i = 0; i < 3; i++)
                {
                    float cy = r.Y + (float)Math.Round(n * rows[i]);
                    float kx = (float)Math.Round(x0 + kr + (x1 - x0 - 2 * kr) * lv[i]);
                    var tr = new RectangleF(x0, cy - th / 2f, x1 - x0, th);
                    using (var p = RoundRect(tr, th / 2f)) g.FillPath(track, p);
                    using (var p = RoundRect(new RectangleF(x0, tr.Y, kx - x0, th), th / 2f)) g.FillPath(fill, p);
                    g.FillEllipse(knob, kx - kr, cy - kr, kr * 2, kr * 2);
                    float ir = kr * 0.45f;                            // red center of the knob, if there is room
                    if (kr >= 4) g.FillEllipse(fill, kx - ir, cy - ir, ir * 2, ir * 2);
                }
            g.SmoothingMode = oldSm; g.PixelOffsetMode = oldPo;
        }

        static GraphicsPath RoundRect(RectangleF r, float rad)
        {
            var p = new GraphicsPath(); float d = rad * 2;
            p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure(); return p;
        }

        // Creates the .ico (16-256 px, PNG inside) and a 256 px preview
        public static void WriteIco(string path)
        {
            int[] sizes = { 16, 20, 24, 32, 40, 48, 64, 128, 256 };
            var pngs = new List<byte[]>();
            foreach (int n in sizes)
                using (var bmp = new Bitmap(n, n, PixelFormat.Format32bppArgb))
                {
                    using (var g = Graphics.FromImage(bmp)) DrawMixer(g, new RectangleF(0, 0, n, n), Color.White);
                    using (var ms = new System.IO.MemoryStream())
                    {
                        if (n >= 256) bmp.Save(ms, ImageFormat.Png);
                        else
                        {
                            // Classic DIB: header, BGRA bottom-up, empty AND mask
                            var bw = new System.IO.BinaryWriter(ms);
                            bw.Write(40); bw.Write(n); bw.Write(n * 2); bw.Write((short)1); bw.Write((short)32);
                            bw.Write(0); bw.Write(n * n * 4); bw.Write(0); bw.Write(0); bw.Write(0); bw.Write(0);
                            for (int y = n - 1; y >= 0; y--)
                                for (int x = 0; x < n; x++) { var c = bmp.GetPixel(x, y); bw.Write(c.B); bw.Write(c.G); bw.Write(c.R); bw.Write(c.A); }
                            int maskRow = ((n + 31) / 32) * 4;
                            bw.Write(new byte[maskRow * n]);
                            bw.Flush();
                        }
                        pngs.Add(ms.ToArray());
                    }
                    if (n == 256)
                        using (var prev = new Bitmap(n, n))
                        {
                            // The preview is on a black background; the icon itself stays transparent
                            using (var g = Graphics.FromImage(prev)) { g.Clear(Color.Black); g.DrawImage(bmp, 0, 0); }
                            prev.Save(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(path)), "icon-preview.png"), ImageFormat.Png);
                        }
                }
            using (var w = new System.IO.BinaryWriter(System.IO.File.Create(path)))
            {
                w.Write((short)0); w.Write((short)1); w.Write((short)sizes.Length);
                int off = 6 + 16 * sizes.Length;
                for (int i = 0; i < sizes.Length; i++)
                {
                    w.Write((byte)(sizes[i] % 256)); w.Write((byte)(sizes[i] % 256)); w.Write((byte)0); w.Write((byte)0);
                    w.Write((short)1); w.Write((short)32); w.Write(pngs[i].Length); w.Write(off);
                    off += pngs[i].Length;
                }
                foreach (var p in pngs) w.Write(p);
            }
        }
    }

    // ───────────────────────── Smooth text ─────────────────────────
    // The text is drawn 3 times larger with real transparency and scaled down smoothly:
    // smoothing like DirectWrite/macOS, no pixelation, and it works on top of Mica.
    static class GdiText
    {
        const int K = 3;
        static readonly Dictionary<string, Bitmap> Labels = new Dictionary<string, Bitmap>();
        internal static int Rasterizations;
        static readonly System.Collections.Generic.Dictionary<string, Font> Big = new System.Collections.Generic.Dictionary<string, Font>();

        static Font BigFont(Font f)
        {
            string key = f.Name + "|" + f.Size + "|" + (int)f.Style;
            Font b;
            if (!Big.TryGetValue(key, out b)) Big[key] = b = new Font(f.FontFamily, f.Size * K, f.Style, GraphicsUnit.Pixel);
            return b;
        }

        public static void Draw(Graphics g, string s, Font f, Color c, RectangleF rf, StringAlignment align)
        {
            if (string.IsNullOrEmpty(s)) return;
            var r = Rectangle.Round(rf);
            if (r.Width <= 0 || r.Height <= 0) return;
            string key = s + "|" + f.Name + "|" + f.Size + "|" + (int)f.Style + "|" + c.ToArgb() + "|" + r.Size + "|" + align;
            Bitmap label;
            if (Labels.TryGetValue(key, out label)) { g.DrawImageUnscaled(label, r.Location); return; }
            if (Labels.Count >= 384) { foreach (var cached in Labels.Values) cached.Dispose(); Labels.Clear(); }
            label = new Bitmap(r.Width, r.Height, PixelFormat.Format32bppPArgb);
            using (var big = new Bitmap(r.Width * K, r.Height * K, PixelFormat.Format32bppPArgb))
            {
                using (var bg = Graphics.FromImage(big))
                using (var sf = new StringFormat(StringFormat.GenericTypographic)
                {
                    Alignment = align, LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap
                })
                using (var b = new SolidBrush(c))
                {
                    bg.TextRenderingHint = TextRenderingHint.AntiAlias;
                    bg.DrawString(s, BigFont(f), b, new RectangleF(0, 0, big.Width, big.Height), sf);
                }
                using (var output = Graphics.FromImage(label))
                {
                    output.InterpolationMode = InterpolationMode.HighQualityBicubic; output.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    output.DrawImage(big, new Rectangle(Point.Empty, r.Size));
                }
            }
            Labels.Add(key, label); Rasterizations++;
            g.DrawImageUnscaled(label, r.Location);
        }
    }

    // ───────────────────────── Window items ─────────────────────────
    enum Kind { Sep, Note, Row, Footer }

    class Item
    {
        public Kind K; public string Text, Key; public int Y, H;
        public string Glyph; public Bitmap Img; public bool IsDefault; public Action TitleClick;
        public Func<float> GetVol; public Action<float> SetVol;
        public Func<bool> GetMute; public Action<bool> SetMute;
        public int Value; public bool Muted;

        public void Sync() { try { Value = (int)Math.Round(GetVol() * 100); Muted = GetMute != null && GetMute(); } catch { } }
    }

    // ───────────────────────── Popup window ─────────────────────────
    class Popup : Form
    {
        public const int Mica = 2, Acrylic = 3;
        const int W = 360;
        Theme T = Theme.Load();
        readonly float s;
        readonly List<Item> items = new List<Item>();
        int contentHeight, scrollOffset;
        readonly System.Windows.Forms.Timer sync = new System.Windows.Forms.Timer { Interval = 1000 };
        bool scanning;
        bool refreshAgain;
        bool rebuildAgain;
        Item hoverItem, dragItem; int hoverPart; // 1: icon, 2: title, 3: slider, 4/5: footer buttons
        IntPtr dib, memDC, oldBmp; Bitmap surface; int dibW, dibH;
        Font fText, fValue, fGlyph, fFootGlyph, fBadge;
        const string GearGlyph = "";
        static readonly string[] FooterUris = { "ms-settings:apps-volume", "ms-settings:sound" };
        int backdrop;
        bool material;
        int wheel, ticks;   // wheel: accumulated mouse wheel delta; ticks: timer ticks since the window opened

        readonly System.Windows.Forms.Timer appearance = new System.Windows.Forms.Timer { Interval = 15 };
        readonly Stopwatch appearanceClock = new Stopwatch();
        readonly System.Windows.Forms.Timer volumeWrite = new System.Windows.Forms.Timer { Interval = 16 };
        Item pendingVolume;
        void FlushVolume()
        {
            volumeWrite.Stop();
            var item = pendingVolume; pendingVolume = null;
            if (item == null) return;
            try {
                item.SetVol(item.Value / 100f);
                if (item.Muted && item.Value > 0 && item.SetMute != null) { item.SetMute(false); item.Muted = false; }
            } catch { }
        }
        Rectangle restingBounds;
        [DllImport("user32.dll", EntryPoint = "SystemParametersInfoW")]
        static extern bool GetAnimation(int action, int param, out int enabled, int flags);
        internal static bool MotionEnabled { get { int enabled; return !SystemInformation.HighContrast && GetAnimation(0x1042, 0, out enabled, 0) && enabled != 0; } }
        internal const int AppearanceDuration = 200;
        internal static double EaseOut(double progress) { double p = Math.Max(0, Math.Min(1, progress)); return 1 - Math.Pow(1 - p, 3); }
        readonly Stopwatch hiddenFor = new Stopwatch();
        public bool CanOpenFromTray { get { return !hiddenFor.IsRunning || hiddenFor.ElapsedMilliseconds > 300; } }

        public Popup(int backdrop, float scale)
        {
            this.backdrop = backdrop; s = scale;
            FormBorderStyle = FormBorderStyle.FixedSingle; ControlBox = false; MinimizeBox = MaximizeBox = false;
            base.Text = "TrayMixer"; ShowInTaskbar = false; TopMost = true; StartPosition = FormStartPosition.Manual;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);
            fText = new Font(Theme.FontName, S(14), GraphicsUnit.Pixel);
            fValue = new Font(Theme.FontName, S(16), GraphicsUnit.Pixel);
            fGlyph = new Font(Theme.GlyphName, S(17), GraphicsUnit.Pixel);
            fFootGlyph = new Font(Theme.GlyphName, S(16), GraphicsUnit.Pixel);
            fBadge = new Font(Theme.GlyphName, S(7), GraphicsUnit.Pixel);
            // Every tick only re-reads values; the full device and app enumeration runs on every third tick
            sync.Tick += delegate { if (++ticks % 3 == 0) RefreshContent(false); else SyncValues(); };
            volumeWrite.Tick += delegate { FlushVolume(); };
            appearance.Tick += delegate {
                double progress = appearanceClock.Elapsed.TotalMilliseconds / AppearanceDuration;
                var work = Screen.FromControl(this).WorkingArea;
                var next = new Point(restingBounds.X, Math.Min(work.Bottom - Height, restingBounds.Y + (int)Math.Round(S(12) * (1 - EaseOut(progress)))));
                if (Location != next) Location = next;
                if (progress >= 1) { appearance.Stop(); appearanceClock.Stop(); Bounds = restingBounds; RefreshContent(false); }
            };
        }

        int S(float v) { return (int)Math.Round(v * s); }

        protected override CreateParams CreateParams { get { var cp = base.CreateParams; cp.ExStyle |= 0x80; return cp; } } // WS_EX_TOOLWINDOW

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x83 && m.WParam != IntPtr.Zero) { m.Result = IntPtr.Zero; return; } // WM_NCCALCSIZE: the whole window is client area
            if (m.Msg == 0x84) { m.Result = (IntPtr)1; return; }                               // WM_NCHITTEST: HTCLIENT
            base.WndProc(ref m);
        }

        protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); ApplyBackdrop(); }

        public int Backdrop { get { return backdrop; } set { backdrop = value; ApplyBackdrop(); } }

        void ApplyBackdrop()
        {
            if (!IsHandleCreated) return;
            int extent = T.Transparency ? -1 : 0;
            var m = new Native.MARGINS { L = extent, R = extent, T = extent, B = extent };
            bool frame = Native.DwmExtendFrameIntoClientArea(Handle, ref m) == 0;
            int dark = T.Light ? 0 : 1; Native.DwmSetWindowAttribute(Handle, 20, ref dark, 4);  // dark frame
            int round = 2; Native.DwmSetWindowAttribute(Handle, 33, ref round, 4);                // rounded corners
            int bd = T.Transparency ? backdrop : 1;
            material = Native.DwmSetWindowAttribute(Handle, 38, ref bd, 4) == 0 && frame && T.Transparency;
            Invalidate();
        }

        // ── Show / hide ──
        public void ShowPopup()
        {
            var nt = Theme.Load();
            T = nt; WallpaperGlass.RefreshAsync(this); ApplyBackdrop();
            appearance.Stop();
            if (items.Count == 0) RefreshContent(true);
            else { SyncValues(); Place(S(W), contentHeight); }
            restingBounds = Bounds;
            bool animate = MotionEnabled;
            if (animate) Location = new Point(restingBounds.X, restingBounds.Y + S(12));
            Show(); Activate(); Native.SetForegroundWindow(Handle);
            if (animate) { appearanceClock.Restart(); appearance.Start(); }
            if (!animate) BeginInvoke((Action)(() => { if (Visible) RefreshContent(false); }));
            ticks = 0; sync.Start();
            // Brightness is read in the background: the window opens at once with the previous values
            Brightness.ScanAsync(() => { try { BeginInvoke((Action)(() => RefreshContent(false))); } catch { } });
        }

        public void HidePopup()
        {
            if (!Visible) return;
            appearance.Stop(); appearanceClock.Stop();
            FlushVolume(); Hide(); sync.Stop();
            Bounds = restingBounds;
            hoverItem = dragItem = null;
            scrollOffset = 0;
            hiddenFor.Restart();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                FlushVolume(); volumeWrite.Dispose();
                appearance.Stop(); appearance.Dispose(); sync.Stop(); sync.Dispose();
                FreeDib();
                fText.Dispose(); fValue.Dispose(); fGlyph.Dispose(); fFootGlyph.Dispose(); fBadge.Dispose();
            }
            base.Dispose(disposing);
        }
        protected override void OnDeactivate(EventArgs e) { base.OnDeactivate(e); HidePopup(); }
        protected override bool ProcessCmdKey(ref Message msg, Keys key)
        {
            if (key == Keys.Escape) { HidePopup(); return true; }
            if (key == Keys.PageUp || key == Keys.PageDown)
            {
                ScrollBy(key == Keys.PageUp ? -ClientSize.Height : ClientSize.Height);
                return true;
            }
            return base.ProcessCmdKey(ref msg, key);
        }

        // ── Data ──
        public void SyncValues()
        {
            if (!Visible || dragItem != null || pendingVolume != null || appearance.Enabled) return;
            bool changed = false;
            foreach (var it in items) if (it.K == Kind.Row && it != dragItem)
            {
                int value = it.Value; bool muted = it.Muted;
                it.Sync();
                if (value != it.Value || muted != it.Muted) changed = true;
            }
            if (changed) Invalidate();
        }

        public void RefreshContent(bool force)
        {
            if (dragItem != null || pendingVolume != null || appearance.Enabled) return;
            if (scanning) { refreshAgain = true; rebuildAgain |= force; return; }
            // The first population stays synchronous; later discovery runs on MTA workers.
            if (items.Count == 0) {
                try { Build(Audio.Devices(), Audio.Apps(), Brightness.Displays); }
                catch { Build(new List<DeviceInfo>(), new List<AppInfo>(), Brightness.Displays); }
                return;
            }
            scanning = true;
            ThreadPool.QueueUserWorkItem(delegate {
                List<DeviceInfo> devs = null; List<AppInfo> apps = null;
                try { devs = Audio.Devices(); apps = Audio.Apps(); } catch { }
                try { BeginInvoke((Action)(() => {
                    scanning = false;
                    if (devs != null && apps != null && Visible && dragItem == null && pendingVolume == null && !appearance.Enabled) {
                        // Always replace bindings: a same-count session replacement is still new COM state.
                        Build(devs, apps, Brightness.Displays);
                    }
                    bool again = refreshAgain, rebuild = rebuildAgain;
                    refreshAgain = rebuildAgain = false;
                    if (again && Visible) RefreshContent(rebuild);
                })); } catch { }
            });
        }

        void Build(List<DeviceInfo> devs, List<AppInfo> apps, List<Display> disps)
        {
            items.Clear(); hoverItem = null;
            foreach (var d in devs.FindAll(x => !Hidden.Is("dev:" + x.Id)))
            {
                var dev = d;
                items.Add(new Item
                {
                    K = Kind.Row, Text = d.Name, Key = "dev:" + d.Id, IsDefault = d.IsDefault,
                    Glyph = d.IsHeadphones ? Glyphs.Headphones : d.IsDisplay ? Glyphs.Monitor : Glyphs.Speaker,
                    TitleClick = () => { if (!Audio.SetDefault(dev.Id)) System.Media.SystemSounds.Beep.Play(); RefreshContent(true); },
                    GetVol = () => { float v; dev.Vol.GetMasterVolumeLevelScalar(out v); return v; },
                    SetVol = v => dev.Vol.SetMasterVolumeLevelScalar(v, ref Audio.Ctx),
                    GetMute = () => { bool m; dev.Vol.GetMute(out m); return m; },
                    SetMute = m => dev.Vol.SetMute(m, ref Audio.Ctx)
                });
            }
            if (devs.Count == 0) items.Add(new Item { K = Kind.Note, Text = "No output devices" });
            items.Add(new Item { K = Kind.Sep });
            foreach (var a in apps.FindAll(x => !Hidden.Is("app:" + x.Key)))
            {
                var app = a;
                items.Add(new Item
                {
                    K = Kind.Row, Text = a.Name, Key = "app:" + a.Key, Img = a.Img, Glyph = a.IsSystem ? Glyphs.Bell : Glyphs.Speaker,
                    GetVol = () => { float v; app.Vols[0].GetMasterVolume(out v); return v; },
                    SetVol = v => { foreach (var x in app.Vols) try { x.SetMasterVolume(v, ref Audio.Ctx); } catch { } },
                    GetMute = () => { bool m; app.Vols[0].GetMute(out m); return m; },
                    SetMute = m => { foreach (var x in app.Vols) try { x.SetMute(m, ref Audio.Ctx); } catch { } }
                });
            }
            if (apps.Count == 0) items.Add(new Item { K = Kind.Note, Text = "Nothing is playing sound right now" });
            // Screen brightness: the section appears only if some screen has adjustable brightness
            var shown = disps.FindAll(x => !Hidden.Is("disp:" + x.Key));
            if (shown.Count > 0) items.Add(new Item { K = Kind.Sep });
            foreach (var d in shown)
            {
                string dk = d.Key; // rows look the Display up by key, so a rescan cannot leave them with a closed one
                items.Add(new Item
                {
                    K = Kind.Row, Text = d.Name, Key = "disp:" + d.Key, Glyph = Glyphs.Sun,
                    GetVol = () => { var x = Brightness.Find(dk); return x == null ? 0f : x.Percent / 100f; },
                    SetVol = v => { var x = Brightness.Find(dk); if (x != null) x.Set((int)Math.Round(v * 100)); }
                });
            }
            // Blue light filter and dimming apply to all screens at once; clicking the icon switches them on and off
            bool blue = !Hidden.Is("blue:filter"), dim = !Hidden.Is("blue:dim");
            if (blue || dim) items.Add(new Item { K = Kind.Sep });
            if (blue)
                items.Add(new Item
                {
                    K = Kind.Row, Text = "Blue light filter", Key = "blue:filter", Glyph = Glyphs.Moon,
                    GetVol = () => BlueLight.Strength / 100f, SetVol = v => BlueLight.SetStrength((int)Math.Round(v * 100)),
                    GetMute = () => !BlueLight.FilterOn, SetMute = m => BlueLight.SetFilterOn(!m)
                });
            if (dim)
                items.Add(new Item
                {
                    K = Kind.Row, Text = "Dimming", Key = "blue:dim", Glyph = Glyphs.Dim,
                    GetVol = () => BlueLight.Dim / 100f, SetVol = v => BlueLight.SetDim((int)Math.Round(v * 100)),
                    GetMute = () => !BlueLight.DimOn, SetMute = m => BlueLight.SetDimOn(!m)
                });
            items.Add(new Item { K = Kind.Footer, Text = "Volume" });

            int y = S(6);
            foreach (var it in items)
            {
                if (it.K == Kind.Footer) y += S(4);
                it.Y = y;
                it.H = S(it.K == Kind.Sep ? 9 : it.K == Kind.Note ? 40 : it.K == Kind.Footer ? 46 : 54);
                y += it.H;
                if (it.K == Kind.Row) it.Sync();
            }
            contentHeight = y;
            var screen = Visible ? Screen.FromControl(this) : Screen.FromPoint(Cursor.Position);
            PlaceOnScreen(screen, S(W), contentHeight);
            Invalidate();
        }

        void Place(int w, int h)
        {
            PlaceOnScreen(Screen.FromPoint(Cursor.Position), w, h);
        }

        void PlaceOnScreen(Screen screen, int w, int h)
        {
            var area = screen.WorkingArea;
            var size = new Size(Math.Min(w, area.Width), Math.Min(h, Math.Max(1, area.Height - S(24))));
            restingBounds = FlyoutPlacement.Bounds(screen.Bounds, area, size, S(12));
            if (Bounds != restingBounds) Bounds = restingBounds;
            ScrollBy(0);
        }

        void ScrollBy(int delta)
        {
            int next = Math.Max(0, Math.Min(scrollOffset + delta, contentHeight - ClientSize.Height));
            if (next == scrollOffset) return;
            scrollOffset = next;
            hoverItem = null;
            hoverPart = 0;
            Invalidate();
        }

        // ── Row geometry (compact: icon on the left, title above the slider) ──
        // Grid: margins 16, icons 16-36, content (title and start of the track) from 50, numbers pinned to the right margin
        const int M = 16, L = 50, ValW = 40, Rad = 10;
        RectangleF IconRect(Item it) { return new RectangleF(S(10), it.Y + S(25.5f) - S(16), S(32), S(32)); }
        RectangleF TitleRect(Item it) { return new RectangleF(S(L), it.Y + S(5), Width - S(L) - S(M), S(20)); }
        RectangleF SliderRect(Item it) { return new RectangleF(S(L - Rad), it.Y + S(25), Width - S(M + ValW + 12 - Rad) - S(L - Rad), S(22)); }
        RectangleF PctRect(Item it) { return new RectangleF(Width - S(M + ValW), it.Y + S(21), S(ValW), S(26)); }
        RectangleF FooterBtn(Item it, int i) { return new RectangleF(Width - S(8) - S(32) * (FooterUris.Length - i), it.Y + (it.H - S(32)) / 2f, S(32), S(32)); }

        // ── Drawing into a DIB with an alpha channel (needed for Mica) ──
        protected override void OnPaintBackground(PaintEventArgs e) { }

        protected override void OnPaint(PaintEventArgs e)
        {
            int w = ClientSize.Width, h = ClientSize.Height;
            if (w <= 0 || h <= 0) return;
            if (!EnsureDib(w, h)) { Render(e.Graphics); return; }
            using (var g = Graphics.FromImage(surface)) Render(g);
            Native.GdiFlush();
            IntPtr hdc = e.Graphics.GetHdc();
            try { Native.BitBlt(hdc, 0, 0, w, h, memDC, 0, 0, 0x00CC0020); }
            finally { e.Graphics.ReleaseHdc(hdc); }
        }

        bool EnsureDib(int w, int h)
        {
            if (surface != null && w == dibW && h == dibH) return true;
            FreeDib();
            var bi = new Native.BITMAPINFOHEADER { biSize = 40, biWidth = w, biHeight = -h, biPlanes = 1, biBitCount = 32 };
            IntPtr bits;
            dib = Native.CreateDIBSection(IntPtr.Zero, ref bi, 0, out bits, IntPtr.Zero, 0);
            if (dib == IntPtr.Zero || bits == IntPtr.Zero) { FreeDib(); return false; }
            memDC = Native.CreateCompatibleDC(IntPtr.Zero);
            if (memDC == IntPtr.Zero) { FreeDib(); return false; }
            oldBmp = Native.SelectObject(memDC, dib);
            if (oldBmp == IntPtr.Zero || oldBmp == new IntPtr(-1)) { FreeDib(); return false; }
            try { surface = new Bitmap(w, h, w * 4, PixelFormat.Format32bppPArgb, bits); }
            catch (ArgumentException) { FreeDib(); return false; }
            catch (OutOfMemoryException) { FreeDib(); return false; }
            dibW = w; dibH = h;
            return true;
        }

        void FreeDib()
        {
            if (surface != null) { surface.Dispose(); surface = null; }
            if (memDC != IntPtr.Zero)
            {
                if (oldBmp != IntPtr.Zero && oldBmp != new IntPtr(-1)) Native.SelectObject(memDC, oldBmp);
                Native.DeleteDC(memDC); memDC = IntPtr.Zero; oldBmp = IntPtr.Zero;
            }
            if (dib != IntPtr.Zero) { Native.DeleteObject(dib); dib = IntPtr.Zero; }
        }

        void Render(Graphics g)
        {
            WallpaperGlass.Paint(g, ClientRectangle, Screen.FromControl(this).Bounds, appearance.Enabled ? restingBounds : Bounds, T);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            var drawingState = g.Save();
            g.TranslateTransform(0, -scrollOffset);
            using (var sf = new StringFormat(StringFormat.GenericTypographic) { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap })
            {
                foreach (var it in items)
                {
                    if (it.Y + it.H <= scrollOffset || it.Y >= scrollOffset + ClientSize.Height) continue;
                    if (it.K == Kind.Footer) DrawFooter(g, it, sf);
                    else if (it.K == Kind.Note) Str(g, it.Text, fText, T.Sub, new RectangleF(S(M), it.Y, Width - S(2 * M), it.H), sf);
                    else if (it.K == Kind.Sep) using (var b = new SolidBrush(T.Sep)) g.FillRectangle(b, S(M), it.Y + it.H / 2, Width - S(2 * M), Math.Max(1, S(1)));
                    else DrawRow(g, it, sf);
                }
            }
            g.Restore(drawingState);
            if (contentHeight > ClientSize.Height)
            {
                int thumb = Math.Min(ClientSize.Height, Math.Max(S(16), (int)((long)ClientSize.Height * ClientSize.Height / contentHeight)));
                int top = (int)((long)(ClientSize.Height - thumb) * scrollOffset / (contentHeight - ClientSize.Height));
                using (var brush = new SolidBrush(T.Sub)) g.FillRectangle(brush, Width - S(5), top, S(3), thumb);
            }
        }

        // Debug: render the window over the given background without showing it on screen
        public void RenderTo(string path, Color bg)
        {
            WallpaperGlass.Refresh();
            try { Brightness.Scan(); } catch { }
            RefreshContent(true);
            using (var bmp = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb))
            {
                using (var g = Graphics.FromImage(bmp)) Render(g);
                using (var outp = new Bitmap(Width, Height))
                {
                    using (var g = Graphics.FromImage(outp)) { g.Clear(T.Light ? Color.FromArgb(243, 243, 243) : bg); g.DrawImage(bmp, 0, 0); }
                    outp.Save(path);
                }
            }
        }

        static void Str(Graphics g, string s, Font f, Color c, RectangleF r, StringFormat sf) { GdiText.Draw(g, s, f, c, r, sf.Alignment); }

        // Footer: label and buttons (Windows volume mixer, sound settings)
        void DrawFooter(Graphics g, Item it, StringFormat sf)
        {
            using (var b = new SolidBrush(T.Footer)) g.FillRectangle(b, 0, it.Y, Width, Height - it.Y);
            using (var b = new SolidBrush(T.Sep)) g.FillRectangle(b, 0, it.Y, Width, Math.Max(1, S(1)));
            Str(g, it.Text, fText, T.Fg, new RectangleF(S(M), it.Y, Width / 2, it.H), sf);
            for (int i = 0; i < FooterUris.Length; i++)
            {
                var r = FooterBtn(it, i);
                if (hoverItem == it && hoverPart == 4 + i) Fill(g, T.Hover, Round(r, S(5)));
                if (i == 0) using (var c = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center }) Str(g, Glyphs.Mixer, fFootGlyph, T.Fg, r, c);
                else using (var c = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    Str(g, GearGlyph, fFootGlyph, T.Fg, r, c);
            }
        }

        void DrawRow(Graphics g, Item it, StringFormat sf)
        {
            var ir = IconRect(it);
            if (it.IsDefault) Fill(g, T.Accent, Pill(new RectangleF(S(4), ir.Y + ir.Height / 2 - S(7), S(3), S(14))));

            if (hoverItem == it && hoverPart == 1) Fill(g, T.Hover, Round(ir, S(5)));
            // The icon is always the row's own; when muted it is dimmed and gets a badge
            float sz = S(20);
            var r = new RectangleF((float)Math.Round(ir.X + (ir.Width - sz) / 2), (float)Math.Round(ir.Y + (ir.Height - sz) / 2), sz, sz);
            if (it.Img != null)
            {
                using (var ia = new ImageAttributes())
                {
                    ia.SetColorMatrix(new ColorMatrix { Matrix33 = it.Muted ? 0.35f : 1f });
                    g.DrawImage(it.Img, Rectangle.Round(r), 0, 0, it.Img.Width, it.Img.Height, GraphicsUnit.Pixel, ia);
                }
            }
            else
                using (var c = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    Str(g, it.Glyph, fGlyph, it.Muted ? Color.FromArgb(90, T.Fg) : T.Fg, new RectangleF(r.X - S(4), r.Y - S(4), r.Width + S(8), r.Height + S(8)), c);
            if (it.Muted)
            {
                var br = new RectangleF(r.Right - S(8), r.Bottom - S(8), S(13), S(13));
                using (var b = new SolidBrush(T.Badge)) g.FillEllipse(b, br);
                using (var c = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    Str(g, Glyphs.Mute, fBadge, T.Fg, br, c);
            }

            bool clickable = it.TitleClick != null && !it.IsDefault;
            Color tc = clickable && hoverItem == it && hoverPart == 2 ? T.Accent : T.Fg;
            Str(g, it.Text, fText, tc, TitleRect(it), sf);
            using (var rf = new StringFormat(sf) { Alignment = StringAlignment.Far })
                Str(g, it.Muted ? "—" : it.Value.ToString(), fValue, it.Muted ? T.Sub : T.Fg, PctRect(it), rf);

            // Windows 11 style slider
            var sr = SliderRect(it);
            float rad = S(10), th = Math.Max(2, S(4)), x0 = sr.X + rad, x1 = sr.Right - rad, scy = sr.Y + sr.Height / 2;
            float cx = x0 + (x1 - x0) * it.Value / 100f;
            Color fill = it.Muted ? T.Track : T.Accent;
            Fill(g, T.Track, Pill(new RectangleF(x0 - th / 2, scy - th / 2, x1 - x0 + th, th)));
            Fill(g, fill, Pill(new RectangleF(x0 - th / 2, scy - th / 2, cx - x0 + th, th)));
            float ro = rad - 0.5f, ri = S(dragItem == it ? 5 : hoverItem == it && hoverPart == 3 ? 7 : 6);
            using (var b = new SolidBrush(T.ThumbOuter)) g.FillEllipse(b, cx - ro, scy - ro, ro * 2, ro * 2);
            using (var p = new Pen(T.ThumbBorder, 1)) g.DrawEllipse(p, cx - ro, scy - ro, ro * 2, ro * 2);
            using (var b = new SolidBrush(fill)) g.FillEllipse(b, cx - ri, scy - ri, ri * 2, ri * 2);
        }

        static void Fill(Graphics g, Color c, GraphicsPath p) { using (p) using (var b = new SolidBrush(c)) g.FillPath(b, p); }

        static GraphicsPath Pill(RectangleF r) { return Round(r, Math.Min(r.Width, r.Height) / 2); }

        static GraphicsPath Round(RectangleF r, float rad)
        {
            var p = new GraphicsPath(); float d = rad * 2;
            p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure(); return p;
        }

        // ── Mouse ──
        Item RowAt(Point p)
        {
            p.Y += scrollOffset;
            foreach (var it in items)
            {
                if ((it.K == Kind.Row || it.K == Kind.Footer) && p.Y >= it.Y && p.Y < it.Y + it.H) return it;
            }
            return null;
        }

        int PartAt(Item it, Point p)
        {
            if (it == null) return 0;
            p.Y += scrollOffset;
            if (it.K == Kind.Footer)
            {
                for (int i = 0; i < FooterUris.Length; i++) if (FooterBtn(it, i).Contains(p)) return 4 + i;
                return 0;
            }
            if (it.SetMute != null && IconRect(it).Contains(p)) return 1;
            if (it.TitleClick != null && !it.IsDefault)
            {
                var tr = TitleRect(it);
                float tw = Math.Min(tr.Width, TextRenderer.MeasureText(it.Text, fText).Width);
                if (new RectangleF(tr.X, tr.Y, tw, tr.Height).Contains(p)) return 2;
            }
            var sr = SliderRect(it); sr.Inflate(0, S(4));
            return sr.Contains(p) ? 3 : 0;
        }

        void SetFromX(Item it, int x)
        {
            var sr = SliderRect(it); float rad = S(10);
            Apply(it, (int)Math.Round((x - sr.X - rad) / (sr.Width - 2 * rad) * 100));
        }

        void Apply(Item it, int v)
        {
            v = Math.Max(0, Math.Min(100, v));
            if (v == it.Value) return;
            if (pendingVolume != null && pendingVolume != it) FlushVolume();
            it.Value = v;
            pendingVolume = it;
            // Paint immediately; collapse redundant mouse samples to one write per frame.
            Invalidate();
            volumeWrite.Start();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (dragItem != null) { SetFromX(dragItem, e.X); return; }
            var it = RowAt(e.Location); int part = PartAt(it, e.Location);
            if (it != hoverItem || part != hoverPart)
            {
                hoverItem = it; hoverPart = part;
                Cursor = part > 0 ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e) { if (dragItem == null) { hoverItem = null; hoverPart = 0; Invalidate(); } }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            var it = RowAt(e.Location); int part = PartAt(it, e.Location);
            if (e.Button == MouseButtons.Right && it != null && it.K == Kind.Row)
            {
                // Right click on a row hides it (it can be brought back from the tray icon menu)
                var row = it;
                new ContextMenu(new[] { new MenuItem("Hide \"" + row.Text + "\"", delegate { Hidden.Hide(row.Key, row.Text); RefreshContent(true); }) })
                    .Show(this, e.Location);
                return;
            }
            if (e.Button != MouseButtons.Left) return;
            if (part == 1) { try { it.SetMute(!it.Muted); } catch { } it.Sync(); Invalidate(); }
            else if (part == 2) it.TitleClick();
            else if (part == 3) { dragItem = it; Capture = true; SetFromX(it, e.X); }
            else if (part >= 4)
            {
                HidePopup();
                try { Process.Start(new ProcessStartInfo(FooterUris[part - 4]) { UseShellExecute = true }); } catch { }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e) { if (dragItem != null) { FlushVolume(); dragItem = null; Capture = false; Invalidate(); } }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if ((ModifierKeys & Keys.Shift) != 0 || e.X < S(M) || e.X >= Width - S(M))
            {
                ScrollBy(e.Delta > 0 ? -S(54) : S(54));
                return;
            }
            var it = RowAt(e.Location);
            if (it == null || it.K != Kind.Row) return;
            // Touchpads send many small deltas: collect them and step once per full notch (120)
            wheel += e.Delta;
            int steps = wheel / 120;
            if (steps == 0) return;
            wheel -= steps * 120;
            Apply(it, it.Value + steps * 2);
        }
    }

    // ───────────────────────── Hidden rows (stored in the registry) ─────────────────────────
    static class Hidden
    {
        const string RegKey = @"Software\TrayMixer";
        public static readonly Dictionary<string, string> Items = Load(); // key -> title

        static Dictionary<string, string> Load()
        {
            var d = new Dictionary<string, string>();
            try
            {
                using (var k = Registry.CurrentUser.OpenSubKey(RegKey))
                {
                    var v = k == null ? null : k.GetValue("Hidden") as string[];
                    if (v != null) foreach (var s in v) { int i = s.IndexOf('|'); if (i > 0) d[s.Substring(0, i)] = s.Substring(i + 1); }
                }
            }
            catch { }
            return d;
        }

        static void Save()
        {
            var l = new List<string>();
            foreach (var kv in Items) l.Add(kv.Key + "|" + kv.Value);
            try { using (var k = Registry.CurrentUser.CreateSubKey(RegKey)) k.SetValue("Hidden", l.ToArray(), RegistryValueKind.MultiString); }
            catch { } // a registry failure must not crash the tray process
        }

        public static bool Is(string key) { return Items.ContainsKey(key); }
        public static void Hide(string key, string name) { Items[key] = name; Save(); }
        public static void Show(string key) { Items.Remove(key); Save(); }
        public static void ShowAll() { Items.Clear(); Save(); }
    }

    // ───────────────────────── Sound notifications ─────────────────────────
    [ComVisible(true)]
    class Notifier : IMMNotificationClient, IAudioEndpointVolumeCallback
    {
        readonly Control ui; public Action VolumeChanged, DevicesChanged;
        int volumePending, devicesPending;
        public Notifier(Control ui) { this.ui = ui; }
        void Post(Action a)
        {
            if (a == null) return;
            bool volume = a == VolumeChanged;
            if (volume ? Interlocked.Exchange(ref volumePending, 1) != 0 : Interlocked.Exchange(ref devicesPending, 1) != 0) return;
            try { ui.BeginInvoke((Action)(() => {
                if (volume) Interlocked.Exchange(ref volumePending, 0); else Interlocked.Exchange(ref devicesPending, 0);
                if (!ui.IsDisposed) a();
            })); } catch { if (volume) Interlocked.Exchange(ref volumePending, 0); else Interlocked.Exchange(ref devicesPending, 0); }
        }
        public int OnDeviceStateChanged(string id, int state) { Post(DevicesChanged); return 0; }
        public int OnDeviceAdded(string id) { Post(DevicesChanged); return 0; }
        public int OnDeviceRemoved(string id) { Post(DevicesChanged); return 0; }
        public int OnDefaultDeviceChanged(int flow, int role, string id) { if (flow == 0 && role == 1) { Post(DevicesChanged); } return 0; }
        public int OnPropertyValueChanged(string id, PROPERTYKEY key) { return 0; }
        public int OnNotify(IntPtr data) { Post(VolumeChanged); return 0; }
    }

    // Shared placement keeps both tray panels clear of the taskbar on every edge.
    static class FlyoutPlacement
    {
        internal static Rectangle Bounds(Rectangle screen, Rectangle work, Size size, int gap)
        {
            int x = work.Left > screen.Left ? work.Left + gap : work.Right - size.Width - gap;
            int y = work.Top > screen.Top ? work.Top + gap : work.Bottom - size.Height - gap;
            return new Rectangle(Math.Max(work.Left, x), Math.Max(work.Top, y), size.Width, size.Height);
        }
    }

    // ToolStrip supplies keyboard navigation, accessibility and submenu lifetime;
    // the existing MenuItem model remains the single source of actions and checks.
    sealed class TrayFlyout : ContextMenuStrip
    {
        readonly ContextMenu model;
        readonly float scale;
        Font menuFont;
        Theme theme;
        internal TrayFlyout(ContextMenu model, float scale)
        {
            this.model = model; this.scale = scale;
            AutoSize = true; ShowImageMargin = false; ShowCheckMargin = false;
            DropShadowEnabled = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer, false);
        }
        protected override void OnPaintBackground(PaintEventArgs e) { }
        protected override void OnPaint(PaintEventArgs e) { PaintMaterial(this, e, base.OnPaint); }

        sealed class MaterialMenu : ToolStripDropDownMenu
        {
            internal MaterialMenu() { SetStyle(ControlStyles.OptimizedDoubleBuffer, false); }
            protected override void OnPaintBackground(PaintEventArgs e) { }
            protected override void OnPaint(PaintEventArgs e) { PaintMaterial(this, e, base.OnPaint); }
        }

        static void PaintMaterial(ToolStrip menu, PaintEventArgs e, Action<PaintEventArgs> paint)
        {
            int w = menu.ClientSize.Width, h = menu.ClientSize.Height;
            if (w <= 0 || h <= 0) return;
            // Ordinary GDI buffers discard alpha. Present the whole menu through the
            // same premultiplied DIB path as the mixer, including text and highlights.
            var info = new Native.BITMAPINFOHEADER { biSize = 40, biWidth = w, biHeight = -h, biPlanes = 1, biBitCount = 32 };
            IntPtr bits;
            IntPtr dib = Native.CreateDIBSection(IntPtr.Zero, ref info, 0, out bits, IntPtr.Zero, 0);
            IntPtr dc = Native.CreateCompatibleDC(IntPtr.Zero);
            if (dib == IntPtr.Zero || dc == IntPtr.Zero)
            {
                if (dc != IntPtr.Zero) Native.DeleteDC(dc);
                if (dib != IntPtr.Zero) Native.DeleteObject(dib);
                paint(e); return;
            }
            IntPtr old = Native.SelectObject(dc, dib);
            try
            {
                using (var image = new Bitmap(w, h, w * 4, PixelFormat.Format32bppPArgb, bits))
                using (var graphics = Graphics.FromImage(image))
                using (var args = new PaintEventArgs(graphics, menu.ClientRectangle))
                {
                    WallpaperGlass.Paint(graphics, menu.ClientRectangle, Screen.FromControl(menu).Bounds, menu.Bounds, Theme.Load());
                    paint(args);
                }
                Native.GdiFlush();
                IntPtr target = e.Graphics.GetHdc();
                try { Native.BitBlt(target, 0, 0, w, h, dc, 0, 0, 0x00CC0020); }
                finally { e.Graphics.ReleaseHdc(target); }
            }
            finally
            {
                Native.SelectObject(dc, old); Native.DeleteDC(dc); Native.DeleteObject(dib);
            }
        }
        int S(float v) { return (int)Math.Round(v * scale); }
        internal void RefreshItems()
        {
            theme = Theme.Load();
            WallpaperGlass.RefreshAsync(this);
            ClearItems(Items);
            if (menuFont != null) menuFont.Dispose();
            menuFont = new Font(Theme.FontName, S(14), GraphicsUnit.Pixel);
            Style(this);
            Populate(Items, model.MenuItems);
        }
        void Style(ToolStripDropDown menu)
        {
            menu.Font = menuFont;
            menu.Padding = new Padding(S(6));
            menu.BackColor = theme.Solid;
            menu.ForeColor = theme.Fg;
            menu.Renderer = new FlyoutRenderer(theme, scale);
            menu.MinimumSize = new Size(S(256), 0);
            menu.Opened -= RoundWindow;
            menu.Opened += RoundWindow;
            var drop = menu as ToolStripDropDownMenu;
            if (drop != null) { drop.ShowImageMargin = false; drop.ShowCheckMargin = false; }
        }
        void RoundWindow(object sender, EventArgs e)
        {
            var menu = (ToolStripDropDown)sender;
            int round = 2, dark = theme.Light ? 0 : 1;
            Native.DwmSetWindowAttribute(menu.Handle, 33, ref round, 4);
            Native.DwmSetWindowAttribute(menu.Handle, 20, ref dark, 4);
            // Respect the Windows accessibility and transparency preferences.
            bool enabled = theme.Transparency;
            var margins = new Native.MARGINS { L = enabled ? -1 : 0, R = enabled ? -1 : 0, T = enabled ? -1 : 0, B = enabled ? -1 : 0 };
            int backdrop = enabled ? Popup.Mica : 1;
            bool frame = Native.DwmExtendFrameIntoClientArea(menu.Handle, ref margins) == 0;
            bool applied = Native.DwmSetWindowAttribute(menu.Handle, 38, ref backdrop, 4) == 0;
            ((FlyoutRenderer)menu.Renderer).Material = enabled && frame && applied;
            menu.Invalidate();
        }
        static void ClearItems(ToolStripItemCollection items)
        {
            while (items.Count > 0) { var item = items[0]; items.RemoveAt(0); item.Dispose(); }
        }
        void Populate(ToolStripItemCollection target, Menu.MenuItemCollection source)
        {
            foreach (MenuItem item in source)
            {
                if (!item.Visible) continue;
                if (item.Text == "-") { target.Add(new ToolStripSeparator { AutoSize = false, Height = S(9), Width = S(232) }); continue; }
                var action = item;
                var row = new ToolStripMenuItem(item.Text) {
                    Enabled = item.Enabled, Checked = item.Checked, CheckOnClick = false,
                    AutoSize = false, Size = new Size(Math.Max(S(244), Math.Min(S(420), TextRenderer.MeasureText(item.Text, menuFont).Width + S(60))), S(30)),
                    Padding = new Padding(S(26), 0, S(22), 0),
                    AccessibleName = item.Text
                };
                if (item.MenuItems.Count > 0)
                {
                    row.DropDown = new MaterialMenu();
                    Style(row.DropDown);
                    Populate(row.DropDownItems, item.MenuItems);
                    row.DropDownOpening += delegate {
                        ClearItems(row.DropDownItems);
                        Populate(row.DropDownItems, action.MenuItems);
                    };
                }
                else row.Click += delegate { action.PerformClick(); };
                target.Add(row);
                var owner = row.Owner as ToolStripDropDown;
                if (owner != null) owner.MinimumSize = new Size(Math.Max(owner.MinimumSize.Width, row.Width + S(12)), 0);
            }
        }
        internal void ShowFlyout()
        {
            RefreshItems();
            var screen = Screen.FromPoint(Cursor.Position);
            var bounds = FlyoutPlacement.Bounds(screen.Bounds, screen.WorkingArea, GetPreferredSize(Size.Empty), S(12));
            Show(bounds.Location);
            Native.SetForegroundWindow(Handle);
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing && menuFont != null) { menuFont.Dispose(); menuFont = null; }
        }
        sealed class FlyoutRenderer : ToolStripProfessionalRenderer
        {
            readonly Theme t; readonly float s;
            int S(float v) { return (int)Math.Round(v * s); }
            internal FlyoutRenderer(Theme t, float s) { this.t = t; this.s = s; RoundedEdges = false; }
            internal bool Material;
            protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
            {
                WallpaperGlass.Paint(e.Graphics, e.ToolStrip.ClientRectangle, Screen.FromControl(e.ToolStrip).Bounds, e.ToolStrip.Bounds, t);
            }
            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                using (var p = new Pen(t.Sep)) e.Graphics.DrawRectangle(p, 0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
            }
            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                var checkedItem = e.Item as ToolStripMenuItem;
                if (checkedItem != null && checkedItem.Checked)
                    using (var pen = new Pen(t.Accent, Math.Max(1, S(1.5f)))) {
                        int y = e.Item.Height / 2;
                        e.Graphics.DrawLines(pen, new[] { new Point(S(9), y), new Point(S(12), y + S(3)), new Point(S(18), y - S(4)) });
                    }
                if (!e.Item.Selected || !e.Item.Enabled) return;
                var r = new RectangleF(0, 1, e.Item.Width, e.Item.Height - 2);
                using (var p = new GraphicsPath())
                using (var b = new SolidBrush(t.Hover))
                {
                    float d = S(8);
                    p.AddArc(r.Left, r.Top, d, d, 180, 90); p.AddArc(r.Right-d, r.Top, d, d, 270, 90);
                    p.AddArc(r.Right-d, r.Bottom-d, d, d, 0, 90); p.AddArc(r.Left, r.Bottom-d, d, d, 90, 90); p.CloseFigure();
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias; e.Graphics.FillPath(b, p);
                }
            }
            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                GdiText.Draw(e.Graphics, e.Text, e.TextFont, e.Item.Enabled ? t.Fg : t.Sub,
                    new RectangleF(S(28), 0, e.Item.Width - S(52), e.Item.Height), StringAlignment.Near);
            }
            protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
            {
                using (var p = new Pen(t.Sep)) e.Graphics.DrawLine(p, S(8), e.Item.Height / 2, e.Item.Width - S(8), e.Item.Height / 2);
            }
            protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e) { e.ArrowColor = t.Sub; base.OnRenderArrow(e); }
            protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
            {
                using (var p = new Pen(t.Accent, Math.Max(1, S(1.5f)))) {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    int y = e.Item.Height / 2;
                    e.Graphics.DrawLines(p, new[] { new Point(S(9), y), new Point(S(12), y + S(3)), new Point(S(18), y - S(4)) });
                }
            }
        }
    }

    sealed class TrayIcon : NativeWindow, IDisposable
    {
        const int Add = 0, Modify = 1, Delete = 2;
        const int CallbackMessage = 0x8001; // WM_APP + 1
        static readonly int TaskbarCreated = RegisterWindowMessage("TaskbarCreated");
        readonly System.Windows.Forms.Timer retry = new System.Windows.Forms.Timer { Interval = 1000 };
        readonly Action<MouseButtons> click;
        Icon icon;
        string text;
        bool added, doubleClick;

        public TrayIcon(Action<MouseButtons> click)
        {
            this.click = click;
            CreateHandle(new CreateParams());
            retry.Tick += delegate { Publish(); };
        }

        public void Update(Icon replacement, string tooltip)
        {
            if (replacement != null)
            {
                if (icon != null) icon.Dispose();
                icon = replacement;
            }
            text = tooltip;
            Publish();
        }

        void Publish()
        {
            if (icon == null) return;
            var data = MakeData();
            // Explorer can reject registration while the notification area is starting.
            added = (added && Shell_NotifyIcon(Modify, ref data)) || Shell_NotifyIcon(Add, ref data);
            retry.Enabled = !added;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == TaskbarCreated && TaskbarCreated != 0)
            {
                Publish();
            }
            else if (m.Msg == CallbackMessage && m.WParam == (IntPtr)1)
            {
                switch (m.LParam.ToInt64())
                {
                    case 0x0202: MouseUp(MouseButtons.Left); break;   // WM_LBUTTONUP
                    case 0x0205: MouseUp(MouseButtons.Right); break;  // WM_RBUTTONUP
                    case 0x0208: MouseUp(MouseButtons.Middle); break; // WM_MBUTTONUP
                    case 0x0203: // WM_LBUTTONDBLCLK
                    case 0x0206: // WM_RBUTTONDBLCLK
                    case 0x0209: // WM_MBUTTONDBLCLK
                        doubleClick = true;
                        break;
                }
                return;
            }
            base.WndProc(ref m);
        }

        void MouseUp(MouseButtons button)
        {
            bool singleClick = !doubleClick;
            doubleClick = false;
            if (singleClick) click(button);
        }

        public void Dispose()
        {
            retry.Dispose();
            if (added)
            {
                var data = MakeData();
                Shell_NotifyIcon(Delete, ref data);
            }
            DestroyHandle();
            if (icon != null) icon.Dispose();
        }

        NotifyIconData MakeData()
        {
            return new NotifyIconData
            {
                Size = Marshal.SizeOf(typeof(NotifyIconData)), Window = Handle, Id = 1,
                Flags = 0x07, Callback = CallbackMessage, Icon = icon.Handle, Tip = text
            };
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int RegisterWindowMessage(string name);
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        static extern bool Shell_NotifyIcon(int message, ref NotifyIconData data);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct NotifyIconData
        {
            public int Size;
            public IntPtr Window;
            public int Id, Flags, Callback;
            public IntPtr Icon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string Tip;
            public int State, StateMask;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string Info;
            public int Version;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)] public string InfoTitle;
            public int InfoFlags;
            public Guid Guid;
            public IntPtr BalloonIcon;
        }
    }

    // ───────────────────────── Tray ─────────────────────────
    class TrayApp : IDisposable
    {
        public const string StartupArgument = "--startup";
        const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run", AppName = "TrayMixer", CfgKey = @"Software\TrayMixer";
        readonly TrayIcon icon;
        readonly TrayFlyout flyout;
        readonly System.Windows.Forms.Timer audioRetry = new System.Windows.Forms.Timer { Interval = 1000 };
        IMMDeviceEnumerator audioEnumerator;
        readonly Popup popup;
        readonly Notifier notifier;
        IAudioEndpointVolume defVol;
        string lastGlyph; bool lastLight;
        readonly MenuItem autoItem, micaItem, acrylicItem;

        public TrayApp(float scale)
        {
            popup = new Popup(Theme.RegInt(CfgKey, "Backdrop", Popup.Mica), scale);
            popup.CreateControl(); GC.KeepAlive(popup.Handle); // the window is needed right away: notifications arrive through it

            try { Native.SetPreferredAppMode(1); Native.FlushMenuThemes(); } catch { } // dark context menu
            var ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            string verStr = "TrayMixer " + ver.Major + "." + ver.Minor + "." + ver.Build;
            micaItem = new MenuItem("Background: Mica", delegate { SetBackdrop(Popup.Mica); }) { RadioCheck = true };
            acrylicItem = new MenuItem("Background: Acrylic", delegate { SetBackdrop(Popup.Acrylic); }) { RadioCheck = true };
            autoItem = new MenuItem("Run at Windows startup", delegate { ToggleAutostart(); }) { Checked = IsAutostart() };
            var hiddenItem = new MenuItem("Hidden", new[] { new MenuItem("-") });
            hiddenItem.Popup += delegate { FillHidden(hiddenItem); };
            var menuModel = CreateTrayMenu(verStr, hiddenItem, micaItem, acrylicItem, autoItem,
                () => Open("ms-settings:sound"), () => Open("ms-settings:apps-volume"),
                () => Application.Exit());
            flyout = new TrayFlyout(menuModel, scale);
            icon = new TrayIcon(button => {
                if (button == MouseButtons.Right) { popup.HidePopup(); FillHidden(hiddenItem); flyout.ShowFlyout(); }
                else { flyout.Close(); OnTrayClick(button); }
            });
            UpdateChecks();

            notifier = new Notifier(popup)
            {
                VolumeChanged = () => { UpdateTray(); popup.SyncValues(); },
                DevicesChanged = () => { Rebind(); if (popup.Visible) popup.RefreshContent(true); }
            };
            UpdateTray();
            audioRetry.Tick += delegate { ConnectAudio(); };
            WallpaperGlass.RefreshAsync(popup);
            popup.BeginInvoke((Action)(() => {
                ConnectAudio();
                Brightness.ScanAsync(null);
                BlueLight.Start();
            }));
        }

        public void RequestOpen()
        {
            try
            {
                popup.BeginInvoke((Action)delegate
                {
                    flyout.Close();
                    if (popup.Visible) popup.Activate();
                    else popup.ShowPopup();
                });
            }
            catch (InvalidOperationException) { } // The window is closing.
        }

        public void Dispose()
        {
            audioRetry.Dispose();
            if (defVol != null) try { defVol.UnregisterControlChangeNotify(notifier); } catch { }
            if (audioEnumerator != null) try { audioEnumerator.UnregisterEndpointNotificationCallback(notifier); } catch { }
            icon.Dispose();
            flyout.Dispose();
            popup.Dispose();
        }

        void ConnectAudio()
        {
            try
            {
                var enumerator = Audio.En;
                enumerator.RegisterEndpointNotificationCallback(notifier);
                audioEnumerator = enumerator;
            }
            catch
            {
                // Windows Audio may still be starting when the user signs in.
                audioRetry.Start();
                return;
            }
            audioRetry.Stop();
            Rebind();
        }

        // Keep frequent actions at the top level; less frequent preferences share one submenu.
        internal static ContextMenu CreateTrayMenu(string version, MenuItem hidden, MenuItem mica,
            MenuItem acrylic, MenuItem autostart, Action sound, Action mixer, Action exit)
        {
            return new ContextMenu(new[]
            {
                new MenuItem("Sound settings", delegate { sound(); }),
                new MenuItem("Windows volume mixer", delegate { mixer(); }),
                new MenuItem("-"),
                hidden,
                new MenuItem("Settings", new[] { mica, acrylic, new MenuItem("-"), autostart }),
                new MenuItem("-"),
                new MenuItem(version) { Enabled = false },
                new MenuItem("Exit", delegate { exit(); })
            });
        }

        // Subscribe to the volume of the current default device
        void Rebind()
        {
            if (defVol != null) try { defVol.UnregisterControlChangeNotify(notifier); } catch { }
            defVol = null;
            try { var d = Audio.Default(); if (d != null) { defVol = Audio.EndpointVolume(d); defVol.RegisterControlChangeNotify(notifier); } } catch { }
            UpdateTray();
        }

        // "Hidden" submenu: a click brings the row back into the window
        static void FillHidden(MenuItem menu)
        {
            menu.MenuItems.Clear();
            if (Hidden.Items.Count == 0) { menu.MenuItems.Add(new MenuItem("No hidden items") { Enabled = false }); return; }
            foreach (var kv in new Dictionary<string, string>(Hidden.Items))
            {
                string key = kv.Key;
                menu.MenuItems.Add(new MenuItem("Show \"" + kv.Value + "\"", delegate { Hidden.Show(key); }));
            }
            menu.MenuItems.Add(new MenuItem("-"));
            menu.MenuItems.Add(new MenuItem("Show all", delegate { Hidden.ShowAll(); }));
        }

        void SetBackdrop(int b)
        {
            popup.Backdrop = b;
            using (var k = Registry.CurrentUser.CreateSubKey(CfgKey)) k.SetValue("Backdrop", b);
            UpdateChecks();
        }

        void UpdateChecks() { micaItem.Checked = popup.Backdrop == Popup.Mica; acrylicItem.Checked = popup.Backdrop == Popup.Acrylic; }

        void OnTrayClick(MouseButtons button)
        {
            if (button == MouseButtons.Left)
            {
                if (popup.Visible) popup.HidePopup();
                else if (popup.CanOpenFromTray) popup.ShowPopup();
            }
            else if (button == MouseButtons.Middle && defVol != null)
            {
                try { bool m; defVol.GetMute(out m); defVol.SetMute(!m, ref Audio.Ctx); } catch { }
            }
        }

        void UpdateTray()
        {
            string glyph = "off", text = "No output device";
            try
            {
                if (defVol != null)
                {
                    float l; bool m; defVol.GetMasterVolumeLevelScalar(out l); defVol.GetMute(out m);
                    int p = (int)Math.Round(l * 100);
                    glyph = m ? "off" : "on";
                    text = "Volume: " + (m ? "muted" : p + "%");
                }
            }
            catch { }
            bool light = Theme.TaskbarLight();
            Icon replacement = null;
            if (glyph != lastGlyph || light != lastLight)
            {
                replacement = MakeIcon(glyph == "off", light);
                lastGlyph = glyph; lastLight = light;
            }
            icon.Update(replacement, text);
        }

        static Icon MakeIcon(bool dim, bool light)
        {
            int sz = SystemInformation.SmallIconSize.Width;
            using (var bmp = new Bitmap(sz, sz))
            {
                using (var g = Graphics.FromImage(bmp))
                    Art.DrawMixer(g, new RectangleF(0, 0, sz, sz), Color.FromArgb(dim ? 110 : 255, light ? Color.Black : Color.White));
                IntPtr h = bmp.GetHicon();
                var ic = (Icon)Icon.FromHandle(h).Clone(); Native.DestroyIcon(h); return ic;
            }
        }

        static void Open(string uri) { try { Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true }); } catch { } }

        static bool IsAutostart()
        {
            try { using (var k = Registry.CurrentUser.OpenSubKey(RunKey)) return k != null && k.GetValue(AppName) != null; }
            catch (UnauthorizedAccessException) { return false; }
            catch (SecurityException) { return false; }
            catch (System.IO.IOException) { return false; }
        }

        static string StartupCommand { get { return "\"" + Application.ExecutablePath + "\" " + StartupArgument; } }

        public static void InitializeAutostart()
        {
            try
            {
                using (var config = Registry.CurrentUser.CreateSubKey(CfgKey))
                using (var run = Registry.CurrentUser.OpenSubKey(RunKey))
                {
                    var existing = run == null ? null : run.GetValue(AppName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
                    bool firstRun = config.GetValue("Initialized") == null;
                    string legacyCommand = "\"" + Application.ExecutablePath + "\"";
                    if ((firstRun && existing == null) || string.Equals(existing as string, legacyCommand, StringComparison.OrdinalIgnoreCase))
                        SetAutostart(true);
                    if (firstRun) config.SetValue("Initialized", 1);
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (SecurityException) { }
            catch (System.IO.IOException) { }
        }

        static void SetAutostart(bool on)
        {
            using (var k = Registry.CurrentUser.CreateSubKey(RunKey))
            {
                if (on) k.SetValue(AppName, StartupCommand, RegistryValueKind.String);
                else k.DeleteValue(AppName, false);
            }
        }

        void ToggleAutostart()
        {
            try { SetAutostart(!IsAutostart()); }
            catch (UnauthorizedAccessException) { }
            catch (SecurityException) { }
            catch (System.IO.IOException) { }
            autoItem.Checked = IsAutostart();
        }
    }

    static class Program
    {
        const string InstanceName = @"Local\TrayMixer_7c1f3e2a-5b0d-4f9e-9a61-2d8c4b7e1a90";
        [STAThread]
        static void Main(string[] args)
        {
            // Protection against DLL planting: system libraries load only from System32, not from the exe folder
            try { Native.SetDefaultDllDirectories(0x800); } catch { }
            // Multicore JIT: later launches compile the startup methods on a background thread
            try { string dir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TrayMixer"); System.IO.Directory.CreateDirectory(dir); System.Runtime.ProfileOptimization.SetProfileRoot(dir); System.Runtime.ProfileOptimization.StartProfile("startup.profile"); } catch { }
            Native.SetProcessDPIAware();
            float scale; using (var g = Graphics.FromHwnd(IntPtr.Zero)) scale = g.DpiX / 96f;
            if (args.Length > 1 && args[0] == "--make-icon" && args[1].EndsWith(".ico", StringComparison.OrdinalIgnoreCase)) { Art.WriteIco(args[1]); return; }
            if (args.Length > 1 && args[0] == "--render" && args[1].EndsWith(".png", StringComparison.OrdinalIgnoreCase)) { var p = new Popup(Popup.Mica, scale); p.CreateControl(); p.RenderTo(args[1], Color.FromArgb(32, 32, 32)); return; }
            // Used by the uninstaller: put the screens back to normal gamma after the process was killed, then quit
            if (args.Length > 0 && args[0] == "--reset-gamma") { Environment.ExitCode = BlueLight.Reset() ? 0 : 1; return; }
            using (var identity = WindowsIdentity.GetCurrent()) Run(scale, args.Length > 0 && args[0] == TrayApp.StartupArgument, identity.User);
        }

        static void Run(float scale, bool startup, SecurityIdentifier user)
        {
            string name = InstanceName + "_" + user.Value;
            var mutexAcl = new MutexSecurity();
            mutexAcl.AddAccessRule(new MutexAccessRule(user, MutexRights.FullControl, AccessControlType.Allow));
            var eventAcl = new EventWaitHandleSecurity();
            eventAcl.AddAccessRule(new EventWaitHandleAccessRule(user, EventWaitHandleRights.FullControl, AccessControlType.Allow));
            bool created;
            using (var openRequest = new EventWaitHandle(false, EventResetMode.AutoReset, name + "_Open", out created, eventAcl))
            using (new Mutex(true, name, out created, mutexAcl))
            {
                if (!created)
                {
                    if (!startup) openRequest.Set();
                    return;
                }
                if (!startup) TrayApp.InitializeAutostart();
                using (var app = new TrayApp(scale))
                {
                    var wait = ThreadPool.RegisterWaitForSingleObject(openRequest,
                        delegate { app.RequestOpen(); }, null, Timeout.Infinite, false);
                    try { Application.Run(); }
                    finally { wait.Unregister(null); }
                }
            }
        }
    }
}
