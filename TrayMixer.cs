// TrayMixer — лёгкий микшер громкости в трее для Windows 11 (Mica/Acrylic, скруглённые углы).
// Окно рисуется вручную в одну поверхность с альфа-каналом — без WPF и дочерних контролов.
// Сборка: build.cmd (встроенный csc .NET Framework 4.x)
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
[assembly: System.Reflection.AssemblyVersion("3.0.2.0")]
[assembly: System.Reflection.AssemblyFileVersion("3.0.2.0")]

namespace TrayMixer
{
    // ───────────────────────── Тема ─────────────────────────
    class Theme
    {
        public bool Light;
        public Color Fg, Sub, Track, ThumbOuter, ThumbBorder, Hover, Sep, Accent, Badge, MediaBackground, Tint;
        public static readonly string FontName = Pick("Inter", "Segoe UI");
        public static readonly string FontSemi = Pick("Segoe UI Variable Text Semibold", "Segoe UI Semibold");
        public static readonly string GlyphName = Pick("Segoe Fluent Icons", "Segoe MDL2 Assets");
        const string Personalize = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

        static string Pick(string a, string b) { try { using (new FontFamily(a)) return a; } catch { return b; } }
        static Color C(uint argb) { return Color.FromArgb(unchecked((int)argb)); }

        public static Theme Load()
        {
            var t = new Theme { Light = RegInt(Personalize, "AppsUseLightTheme", 0) == 1 };
            if (t.Light)
            {
                t.Fg = C(0xE4000000); t.Sub = C(0x9E000000); t.Track = C(0x72000000); t.ThumbOuter = C(0xFFFFFFFF);
                t.ThumbBorder = C(0x29000000); t.Hover = C(0x0F000000); t.Sep = C(0x14000000); t.Badge = C(0xFFF3F3F3); t.MediaBackground = C(0x0C000000);
                t.Tint = C(0x88F9F9F9);
            }
            else
            {
                t.Fg = C(0xFFFFFFFF); t.Sub = C(0xC5FFFFFF); t.Track = C(0x8BFFFFFF); t.ThumbOuter = C(0xFF454545);
                t.ThumbBorder = C(0x18FFFFFF); t.Hover = C(0x15FFFFFF); t.Sep = C(0x18FFFFFF); t.Badge = C(0xFF2C2C2C); t.MediaBackground = C(0x40000000);
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
            return t;
        }

        public bool SameAs(Theme o) { return o.Light == Light && o.Accent == Accent; }
        public static bool TaskbarLight() { return RegInt(Personalize, "SystemUsesLightTheme", 0) == 1; }

        public static int RegInt(string path, string name, int def)
        {
            try { using (var k = Registry.CurrentUser.OpenSubKey(path)) { var v = k == null ? null : k.GetValue(name); return v is int ? (int)v : def; } }
            catch { return def; }
        }
    }

    static class Glyphs
    {
        public const string Speaker = "", Headphones = "", Mute = "", Bell = "", Monitor = "";
        public const string Sun = "", Moon = "", Dim = "";
        public const string Vol0 = "", Vol1 = "", Vol2 = "", Vol3 = "";
    }

    // ───────────────────────── Значок: три вертикальных фейдера ─────────────────────────
    static class Art
    {
        public static readonly Color Red = Color.FromArgb(255, 69, 58);

        // Логотип: три горизонтальных слайдера Windows 11 — красный уровень и круглая ручка.
        // c — цвет дорожки и ручек (белый или чёрный под тему панели задач), его прозрачность приглушает весь значок.
        // На малых размерах всё выровнено по пикселям, чтобы значок в трее был чётким.
        public static void DrawMixer(Graphics g, RectangleF r, Color c)
        {
            int n = (int)Math.Min(r.Width, r.Height);
            var oldSm = g.SmoothingMode; var oldPo = g.PixelOffsetMode;
            g.SmoothingMode = SmoothingMode.AntiAlias; g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            int th = Math.Max(2, (int)Math.Round(n * 0.07));          // толщина дорожки
            int kr = Math.Max(2, (int)Math.Round(n * 0.11));          // радиус ручки
            int pad = Math.Max(1, (int)Math.Round(n * 0.08));         // поля слева и справа
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
                    float ir = kr * 0.45f;                            // красная серединка ручки, если есть место
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

        // Создаёт .ico (16–256 px, PNG внутри) и превью 256 px
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
                            // Классический DIB: заголовок, BGRA снизу вверх, пустая AND-маска
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
                            // Превью — на чёрном фоне, сама иконка остаётся прозрачной
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

    // ───────────────────────── Гладкий текст ─────────────────────────
    // Текст рисуется в 3 раза крупнее с честной прозрачностью и плавно уменьшается:
    // сглаживание как у DirectWrite/macOS, без «пикселей» и поверх Mica.
    static class GdiText
    {
        const int K = 3;
        static readonly System.Collections.Generic.Dictionary<string, Font> Big = new System.Collections.Generic.Dictionary<string, Font>();

        static Font BigFont(Font f)
        {
            string key = f.Name + "|" + f.Size + "|" + (int)f.Style;
            Font b;
            if (!Big.TryGetValue(key, out b)) Big[key] = b = new Font(f.FontFamily, f.Size * K, f.Style, GraphicsUnit.Pixel);
            return b;
        }

        public static float MeasureWidth(Graphics g, string text, Font f)
        {
            using (var format = StringFormat.GenericTypographic)
                return (float)Math.Ceiling(g.MeasureString(text, BigFont(f), PointF.Empty, format).Width / K);
        }

        public static void Draw(Graphics g, string s, Font f, Color c, RectangleF rf, StringAlignment align, int digitWidth = 0, bool wrap = false)
        {
            if (string.IsNullOrEmpty(s)) return;
            var r = Rectangle.Round(rf);
            if (r.Width <= 0 || r.Height <= 0) return;
            using (var big = new Bitmap(r.Width * K, r.Height * K, PixelFormat.Format32bppPArgb))
            {
                using (var bg = Graphics.FromImage(big))
                using (var sf = new StringFormat(StringFormat.GenericTypographic)
                {
                    Alignment = align, LineAlignment = wrap ? StringAlignment.Near : StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter, FormatFlags = wrap ? 0 : StringFormatFlags.NoWrap
                })
                using (var b = new SolidBrush(c))
                {
                    bg.TextRenderingHint = TextRenderingHint.AntiAlias;
                    var font = BigFont(f);
                    if (digitWidth > 0)
                    {
                        sf.Alignment = StringAlignment.Center;
                        sf.Trimming = StringTrimming.None;
                        sf.FormatFlags |= StringFormatFlags.NoClip;
                        int cell = digitWidth * K;
                        int x = big.Width - s.Length * cell;
                        for (int i = 0; i < s.Length; i++)
                            bg.DrawString(s.Substring(i, 1), font, b, new RectangleF(x + i * cell, 0, cell, big.Height), sf);
                    }
                    else bg.DrawString(s, font, b, new RectangleF(0, 0, big.Width, big.Height), sf);
                }
                var oldI = g.InterpolationMode; var oldP = g.PixelOffsetMode;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic; g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawImage(big, r);
                g.InterpolationMode = oldI; g.PixelOffsetMode = oldP;
            }
        }
    }

    // ───────────────────────── Элементы окна ─────────────────────────
    enum Kind { Sep, Note, Row, Media }

    static class Loc
    {
        public static bool En = MixerPreferences.Language == "en";
        public static string Text(string ru, string en) { return En ? en : ru; }
        public static string BlueFilter { get { return Text("Фильтр синего света", "Blue light filter"); } }
        public static string Dimming { get { return Text("Затемнение", "Dimming"); } }
    }

    static class MixerPreferences
    {
        const string ConfigKey = @"Software\TrayMixer", NamesKey = ConfigKey + @"\SliderNames";
        static readonly Dictionary<string, string> names = LoadNames();

        public static string Language
        {
            get
            {
                try { using (var key = Registry.CurrentUser.OpenSubKey(ConfigKey)) return key == null ? null : key.GetValue("Language") as string; }
                catch (Exception e) { Debug.WriteLine(e); return null; }
            }
        }

        public static bool SaveLanguage(string language) { return Write(ConfigKey, "Language", language); }

        public static Keys OpenShortcut
        {
            get
            {
                try
                {
                    using (var key = Registry.CurrentUser.OpenSubKey(ConfigKey))
                    {
                        int packed;
                        if (key != null && int.TryParse(key.GetValue("OpenShortcut") as string, out packed))
                        {
                            var shortcut = (Keys)packed;
                            if (shortcut == Keys.None || MixerShortcut.IsValid(shortcut)) return shortcut;
                        }
                    }
                }
                catch (Exception e) { Debug.WriteLine(e); }
                return MixerShortcut.Default;
            }
        }

        public static bool SaveShortcut(Keys shortcut) { return Write(ConfigKey, "OpenShortcut", ((int)shortcut).ToString()); }

        static Dictionary<string, string> LoadNames()
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(NamesKey))
                    if (key != null)
                        foreach (string id in key.GetValueNames())
                        {
                            string value = key.GetValue(id) as string;
                            if (!string.IsNullOrWhiteSpace(value)) result[id] = value;
                        }
            }
            catch (Exception e) { Debug.WriteLine(e); }
            return result;
        }

        public static bool HasName(string id) { return names.ContainsKey(id); }

        public static string SliderName(string id, string original)
        {
            string name;
            return id != null && names.TryGetValue(id, out name) ? name : original;
        }

        public static bool SaveName(string id, string name)
        {
            if (!Write(NamesKey, id, name)) return false;
            if (name == null) names.Remove(id);
            else names[id] = name;
            return true;
        }

        static bool Write(string path, string name, string value)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(path))
                {
                    if (key == null) return false;
                    if (value == null) key.DeleteValue(name, false);
                    else key.SetValue(name, value, RegistryValueKind.String);
                }
                return true;
            }
            catch (Exception e) { Debug.WriteLine(e); return false; }
        }
    }

    class Item
    {
        public Kind K; public string Text, Key; public int Y, H;
        public string Glyph; public Bitmap Img; public bool IsDefault; public Action TitleClick;
        public Func<float> GetVol; public Action<float> SetVol;
        public Func<bool> GetMute; public Action<bool> SetMute;
        public int Value; public bool Muted;
        public string Label { get { return MixerPreferences.SliderName(Key, Text); } }

        public void Sync() { try { Value = (int)Math.Round(GetVol() * 100); Muted = GetMute != null && GetMute(); } catch { } }
    }

    // ───────────────────────── Всплывающее окно ─────────────────────────
    class Popup : Form
    {
        public const int Mica = 2, Acrylic = 3;
        const int W = 360;
        Theme T = Theme.Load();
        readonly float s;
        readonly List<Item> items = new List<Item>();
        readonly System.Windows.Forms.Timer sync = new System.Windows.Forms.Timer { Interval = 1000 };
        readonly ContextMenu rowMenu = new ContextMenu();
        readonly ToolTip mediaTip = new ToolTip();
        readonly WindowsMedia media = new WindowsMedia();
        readonly Stopwatch batteryAge = new Stopwatch();
        Dictionary<string, int> batteries = new Dictionary<string, int>();
        MediaTrack track;
        bool readingMedia, sendingMedia;
        int focusedMedia = -1, pressedMedia = -1;
        const int MediaPart = 4;
        string signature = "";
        List<Display> displayedMonitors;
        int contentHeight, scrollOffset;
        Item hoverItem, dragItem; int hoverPart; // 1 — иконка, 2 — название, 3 — слайдер, 4–6 — плеер
        IntPtr dib, memDC, oldBmp; Bitmap surface; int dibW, dibH;
        Font fText, fValue, fBattery, fGlyph, fMediaGlyph, fBadge;
        int backdrop;

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
            fBattery = new Font(Theme.FontName, S(12), GraphicsUnit.Pixel);
            fGlyph = new Font(Theme.GlyphName, S(17), GraphicsUnit.Pixel);
            fMediaGlyph = new Font(Theme.GlyphName, S(16), GraphicsUnit.Pixel);
            fBadge = new Font(Theme.GlyphName, S(7), GraphicsUnit.Pixel);
            sync.Tick += delegate { RefreshContent(false); RefreshMedia(); };
        }

        int S(float v) { return (int)Math.Round(v * s); }

        protected override CreateParams CreateParams { get { var cp = base.CreateParams; cp.ExStyle |= 0x80; return cp; } } // WS_EX_TOOLWINDOW

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x83 && m.WParam != IntPtr.Zero) { m.Result = IntPtr.Zero; return; } // WM_NCCALCSIZE: всё окно — клиентская область
            if (m.Msg == 0x84) { m.Result = (IntPtr)1; return; }                               // WM_NCHITTEST: HTCLIENT
            base.WndProc(ref m);
        }

        protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); ApplyBackdrop(); }

        public int Backdrop { get { return backdrop; } set { backdrop = value; ApplyBackdrop(); } }

        void ApplyBackdrop()
        {
            if (!IsHandleCreated) return;
            var m = new Native.MARGINS { L = -1, R = -1, T = -1, B = -1 };
            Native.DwmExtendFrameIntoClientArea(Handle, ref m);
            int dark = T.Light ? 0 : 1; Native.DwmSetWindowAttribute(Handle, 20, ref dark, 4);  // тёмная рамка
            int round = 2; Native.DwmSetWindowAttribute(Handle, 33, ref round, 4);                // скруглённые углы
            int bd = backdrop; Native.DwmSetWindowAttribute(Handle, 38, ref bd, 4);               // Mica / Acrylic
        }

        // ── Показ / скрытие ──
        public void ShowPopup()
        {
            var nt = Theme.Load();
            if (!nt.SameAs(T)) { T = nt; ApplyBackdrop(); }
            RefreshContent(true);
            Show(); Activate(); Native.SetForegroundWindow(Handle);
            if (!Visible) return;
            sync.Start();
            RefreshMedia();
            // Яркость читается в фоне: окно открывается сразу с прошлыми значениями
            Brightness.ScanAsync(() => { try { BeginInvoke((Action)(() => RefreshContent(false))); } catch { } });
        }

        public void HidePopup()
        {
            if (!Visible) return;
            Hide(); sync.Stop();
            items.Clear(); signature = ""; hoverItem = dragItem = null;
            track = null;
            focusedMedia = pressedMedia = -1;
            mediaTip.SetToolTip(this, null);
            batteryAge.Reset();
            displayedMonitors = null;
            scrollOffset = 0;
            Audio.ClearMetadata();
            FreeDib();
            hiddenFor.Restart();
        }

        protected override void OnDeactivate(EventArgs e) { base.OnDeactivate(e); HidePopup(); }
        protected override bool ProcessCmdKey(ref Message msg, Keys key)
        {
            if (key == Keys.Escape) { HidePopup(); return true; }
            if ((key == Keys.Tab || key == (Keys.Shift | Keys.Tab)) && track != null)
            {
                int step = key == Keys.Tab ? 1 : -1;
                int start = focusedMedia < 0 ? (step > 0 ? -1 : 3) : focusedMedia;
                for (int i = 1; i <= 3; i++)
                {
                    int candidate = (start + step * i + 3) % 3;
                    if (!track.CanExecute((MediaAction)candidate)) continue;
                    focusedMedia = candidate;
                    ScrollBy(-scrollOffset);
                    Invalidate();
                    return true;
                }
            }
            if ((key == Keys.Enter || key == Keys.Space) && focusedMedia >= 0)
            {
                ExecuteMedia(focusedMedia);
                return true;
            }
            if (key == Keys.PageUp || key == Keys.PageDown)
            {
                focusedMedia = -1;
                Invalidate();
                ScrollBy(key == Keys.PageUp ? -ClientSize.Height : ClientSize.Height);
                return true;
            }
            return base.ProcessCmdKey(ref msg, key);
        }

        async void RefreshMedia()
        {
            if (!Visible || readingMedia) return;
            readingMedia = true;
            MediaTrack next = null;
            try { next = await media.ReadAsync(); }
            catch (Exception e) { Debug.WriteLine(e); }
            finally { readingMedia = false; }
            if (IsDisposed || !Visible) return;
            track = next;
            if (focusedMedia >= 0 && !MediaEnabled(focusedMedia)) focusedMedia = -1;
            UpdateMediaTip();
            Invalidate();
        }

        bool MediaEnabled(int index) { return track != null && track.CanExecute((MediaAction)index); }

        async void ExecuteMedia(int index)
        {
            if (sendingMedia || !MediaEnabled(index)) return;
            sendingMedia = true;
            try { await track.Execute((MediaAction)index); }
            catch (Exception e) { Debug.WriteLine(e); }
            finally { sendingMedia = false; }
            if (!IsDisposed) RefreshMedia();
        }

        // ── Данные ──
        static string Sig(List<DeviceInfo> devs, List<AppInfo> apps)
        {
            var sb = new StringBuilder();
            foreach (var d in devs) sb.Append(d.Id).Append(d.IsDefault ? "*" : "").Append('\0');
            sb.Append('\0');
            foreach (var a in apps)
            {
                sb.Append(a.Key).Append('\0').Append(a.Name).Append('\0');
                foreach (var id in a.SessionIds) sb.Append(id).Append('\0');
                sb.Append('\0');
            }
            return sb.ToString();
        }

        bool SameMonitors(List<Display> disps)
        {
            if (displayedMonitors == disps) return true;
            if (displayedMonitors == null || displayedMonitors.Count != disps.Count) return false;
            for (int i = 0; i < disps.Count; i++) if (displayedMonitors[i] != disps[i]) return false;
            return true;
        }

        public void SyncValues()
        {
            if (!Visible) return;
            foreach (var it in items) if (it.K == Kind.Row && it != dragItem) it.Sync();
            Invalidate();
        }

        public void RefreshContent(bool force)
        {
            if (dragItem != null) return;
            List<DeviceInfo> devs; List<AppInfo> apps;
            try { devs = Audio.Devices(); apps = Audio.Apps(); }
            catch
            {
                devs = new List<DeviceInfo>();
                apps = new List<AppInfo>();
            }
            var disps = Brightness.Displays;
            string sig = Sig(devs, apps);
            if (!batteryAge.IsRunning || batteryAge.ElapsedMilliseconds >= 10000)
            {
                batteries = HeadsetBattery.Read(devs);
                batteryAge.Restart();
            }
            if (!force && sig == signature && SameMonitors(disps)) { SyncValues(); return; }
            signature = sig;
            displayedMonitors = disps;
            Build(devs, apps, disps);
        }

        void Build(List<DeviceInfo> devs, List<AppInfo> apps, List<Display> disps)
        {
            items.Clear(); hoverItem = null;
            items.Add(new Item { K = Kind.Media });
            foreach (var d in devs.FindAll(x => !Hidden.Is("dev:" + x.Id)))
            {
                var dev = d;
                items.Add(new Item
                {
                    K = Kind.Row, Text = d.Name, Key = "dev:" + d.Id, IsDefault = d.IsDefault,
                    Glyph = d.IsHeadphones ? Glyphs.Headphones : d.IsDisplay ? Glyphs.Monitor : Glyphs.Speaker,
                    TitleClick = () => { Audio.SetDefault(dev.Id); RefreshContent(true); },
                    GetVol = () => { float v; dev.Vol.GetMasterVolumeLevelScalar(out v); return v; },
                    SetVol = v => dev.Vol.SetMasterVolumeLevelScalar(v, ref Audio.Ctx),
                    GetMute = () => { bool m; dev.Vol.GetMute(out m); return m; },
                    SetMute = m => dev.Vol.SetMute(m, ref Audio.Ctx)
                });
            }
            if (devs.Count == 0) items.Add(new Item { K = Kind.Note, Text = Loc.Text("Нет устройств вывода", "No output devices") });
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
            if (apps.Count == 0) items.Add(new Item { K = Kind.Note, Text = Loc.Text("Сейчас ничего не воспроизводит звук", "No apps are playing audio") });
            // Яркость экранов: раздел появляется, только если есть экран с регулируемой яркостью
            var shown = disps.FindAll(x => !Hidden.Is("disp:" + x.Key));
            if (shown.Count > 0) items.Add(new Item { K = Kind.Sep });
            foreach (var d in shown)
            {
                var disp = d;
                items.Add(new Item
                {
                    K = Kind.Row, Text = d.Label, Key = "disp:" + d.Key, Glyph = Glyphs.Sun,
                    GetVol = () => disp.Percent / 100f,
                    SetVol = v => { var current = Brightness.Find(disp.Key); if (current != null) current.Set((int)Math.Round(v * 100)); }
                });
            }
            // Фильтр синего света и затемнение — для всех экранов сразу; клик по иконке включает и выключает
            bool blue = !Hidden.Is("blue:filter"), dim = !Hidden.Is("blue:dim");
            if (blue || dim) items.Add(new Item { K = Kind.Sep });
            if (blue)
                items.Add(new Item
                {
                    K = Kind.Row, Text = Loc.BlueFilter, Key = "blue:filter", Glyph = Glyphs.Moon,
                    GetVol = () => BlueLight.Strength / 100f, SetVol = v => BlueLight.SetStrength((int)Math.Round(v * 100)),
                    GetMute = () => !BlueLight.FilterOn, SetMute = m => BlueLight.SetFilterOn(!m)
                });
            if (dim)
                items.Add(new Item
                {
                    K = Kind.Row, Text = Loc.Dimming, Key = "blue:dim", Glyph = Glyphs.Dim,
                    GetVol = () => BlueLight.Dim / 100f, SetVol = v => BlueLight.SetDim((int)Math.Round(v * 100)),
                    GetMute = () => !BlueLight.DimOn, SetMute = m => BlueLight.SetDimOn(!m)
                });
            int y = S(6);
            foreach (var it in items)
            {
                it.Y = y;
                it.H = S(it.K == Kind.Media ? 63 : it.K == Kind.Sep ? 9 : it.K == Kind.Note ? 40 : 54);
                y += it.H;
                if (it.K == Kind.Row) it.Sync();
            }
            contentHeight = y + S(6);
            Place(S(W), contentHeight);
            ScrollBy(0);
            UpdateMediaTip();
            Invalidate();
        }

        void Place(int w, int h)
        {
            var scr = Screen.FromPoint(Cursor.Position);
            var wa = scr.WorkingArea; int m = S(12);
            w = Math.Min(w, wa.Width);
            h = Math.Min(h, Math.Max(1, wa.Height - 2 * m));
            int x = wa.Left > scr.Bounds.Left ? wa.Left + m : wa.Right - w - m;
            int y = wa.Top > scr.Bounds.Top ? wa.Top + m : wa.Bottom - h - m;
            SetBounds(Math.Max(wa.Left, Math.Min(x, wa.Right - w)), Math.Max(wa.Top, y), w, h);
        }

        void ScrollBy(int delta)
        {
            int next = Math.Max(0, Math.Min(scrollOffset + delta, contentHeight - ClientSize.Height));
            if (next == scrollOffset) return;
            scrollOffset = next;
            hoverItem = null;
            hoverPart = 0;
            UpdateMediaTip();
            Invalidate();
        }

        // ── Геометрия строки (компактная: иконка слева, название над слайдером) ──
        // Сетка: поля 16, иконки 16–36, контент (название и начало дорожки) с 50, числа прижаты к правому полю
        const int M = 16, L = 50, ValW = 40, Rad = 10, DigitWidth = 10;
        RectangleF IconRect(Item it) { return new RectangleF(S(10), it.Y + S(25.5f) - S(16), S(32), S(32)); }
        int RowRight { get { return Width - S(M); } }
        int BatteryWidth(int level) { return S(level >= 100 ? 61 : level >= 10 ? 54 : 47); }
        RectangleF TitleRect(Item it)
        {
            int level;
            int right = it.Key != null && batteries.TryGetValue(it.Key, out level)
                ? RowRight - BatteryWidth(level) - S(6) : RowRight;
            return new RectangleF(S(L), it.Y + S(5), right - S(L), S(20));
        }
        RectangleF MediaButton(Item it, int i) { return new RectangleF(Width - S(22 + 36 * (3 - i)), it.Y + S(10), S(34), S(34)); }
        RectangleF SliderRect(Item it) { return new RectangleF(S(L - Rad), it.Y + S(25), Width - S(M + ValW + 12 - Rad) - S(L - Rad), S(22)); }
        RectangleF ValueRect(int y, int h)
        {
            int width = 3 * S(DigitWidth);
            return new RectangleF(RowRight - width, y, width, h);
        }

        // ── Отрисовка в DIB с альфа-каналом (нужно для Mica) ──
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                sync.Dispose();
                rowMenu.Dispose();
                mediaTip.Dispose();
                FreeDib();
                fText.Dispose(); fValue.Dispose(); fBattery.Dispose(); fGlyph.Dispose(); fMediaGlyph.Dispose(); fBadge.Dispose();
                Audio.ClearMetadata();
            }
            base.Dispose(disposing);
        }

        void Render(Graphics g)
        {
            // Acrylic у DWM выходит светло-серым и плоским — тонируем, как флайауты Windows 11
            g.Clear(backdrop == Acrylic ? T.Tint : Color.Transparent);
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
                    if (it.K == Kind.Media) DrawMedia(g, it, sf);
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

        // Отладка: отрисовать окно поверх заданного фона без показа на экране
        public void RenderTo(string path, Color bg)
        {
            try { Brightness.Scan(); } catch { }
            RefreshContent(true);
            using (var bmp = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb))
            {
                using (var g = Graphics.FromImage(bmp)) Render(g);
                using (var outp = new Bitmap(Width, Height))
                {
                    using (var g = Graphics.FromImage(outp))
                    {
                        g.Clear(T.Light ? Color.FromArgb(243, 243, 243) : bg);
                        g.DrawImageUnscaled(bmp, 0, 0);
                    }
                    outp.Save(path);
                }
            }
        }

        static void Str(Graphics g, string s, Font f, Color c, RectangleF r, StringFormat sf) { GdiText.Draw(g, s, f, c, r, sf.Alignment); }

        void DrawMedia(Graphics g, Item it, StringFormat sf)
        {
            Fill(g, T.MediaBackground, Round(new RectangleF(S(8), it.Y, Width - S(16), S(54)), S(8)));
            string title = track == null ? (Loc.En ? "Nothing playing" : "Нет активного трека") : track.Title;
            if (string.IsNullOrWhiteSpace(title)) title = Loc.En ? "Media playback" : "Воспроизведение";
            string artist = track == null ? null : track.Artist;
            bool hasArtist = !string.IsNullOrWhiteSpace(artist);
            float textWidth = Math.Max(1, MediaButton(it, 0).X - S(28));
            Str(g, title, fText, track == null ? T.Sub : T.Fg,
                new RectangleF(S(20), it.Y + S(hasArtist ? 8 : 18), textWidth, S(18)), sf);
            if (hasArtist)
                Str(g, artist, fText, T.Sub, new RectangleF(S(20), it.Y + S(27), textWidth, S(18)), sf);

            string[] glyphs = { "\uE892", track != null && track.Playing ? "\uE769" : "\uE768", "\uE893" };
            using (var center = new StringFormat(sf) { Alignment = StringAlignment.Center })
                for (int i = 0; i < glyphs.Length; i++)
                {
                    var r = MediaButton(it, i);
                    bool enabled = MediaEnabled(i);
                    if (enabled && hoverItem == it && hoverPart == MediaPart + i) Fill(g, T.Hover, Round(r, S(6)));
                    if (enabled && focusedMedia == i)
                        using (var outline = Round(r, S(6)))
                        using (var pen = new Pen(T.Accent, Math.Max(1, S(1)))) g.DrawPath(pen, outline);
                    Str(g, glyphs[i], fMediaGlyph, enabled ? (i == 1 ? T.Accent : T.Fg) : T.Track, r, center);
                }
        }

        void DrawBattery(Graphics g, Item it, string label, RectangleF title)
        {
            int level;
            if (it.Key == null || !batteries.TryGetValue(it.Key, out level)) return;
            float labelWidth = Math.Min(title.Width, GdiText.MeasureWidth(g, label, fText));
            int height = S(18);
            var badge = new RectangleF(title.X + labelWidth + S(6), title.Y + (title.Height - height) / 2f, BatteryWidth(level), height);
            Fill(g, T.Hover, Round(badge, S(4)));
            float x = badge.X + S(5);
            int iconHeight = S(7);
            float y = badge.Y + (badge.Height - iconHeight) / 2;
            using (var pen = new Pen(T.Sub, Math.Max(1, S(1))))
                g.DrawRectangle(pen, x, y, S(11), iconHeight);
            using (var fill = new SolidBrush(T.Sub))
            {
                int fillHeight = S(3);
                float fillY = y + (iconHeight - fillHeight) / 2f;
                g.FillRectangle(fill, x + S(12), fillY, S(2), fillHeight);
                g.FillRectangle(fill, x + S(2), fillY, S(7) * level / 100f, fillHeight);
            }
            float textX = badge.X + S(22);
            GdiText.Draw(g, level + "%", fBattery, T.Sub,
                new RectangleF(textX, badge.Y, badge.Right - S(4) - textX, badge.Height), StringAlignment.Near);
        }

        void DrawRow(Graphics g, Item it, StringFormat sf)
        {
            var ir = IconRect(it);
            if (it.IsDefault) Fill(g, T.Accent, Pill(new RectangleF(S(4), ir.Y + ir.Height / 2 - S(7), S(3), S(14))));

            if (hoverItem == it && hoverPart == 1) Fill(g, T.Hover, Round(ir, S(5)));
            // Иконка всегда своя; при выключенном звуке — тусклая и со значком
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
            string label = it.Label;
            var title = TitleRect(it);
            Str(g, label, fText, tc, title, sf);
            DrawBattery(g, it, label, title);
            GdiText.Draw(g, it.Muted ? "—" : it.Value.ToString(), fValue, it.Muted ? T.Sub : T.Fg,
                ValueRect(it.Y + S(21), S(26)), StringAlignment.Far, it.Muted ? 0 : S(DigitWidth));

            // Слайдер в стиле Windows 11
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

        internal static GraphicsPath Round(RectangleF r, float rad)
        {
            var p = new GraphicsPath(); float d = rad * 2;
            p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure(); return p;
        }

        // ── Мышь ──
        Item RowAt(Point p)
        {
            p.Y += scrollOffset;
            foreach (var it in items)
            {
                if ((it.K == Kind.Row || it.K == Kind.Media) && p.Y >= it.Y && p.Y < it.Y + it.H) return it;
            }
            return null;
        }

        int PartAt(Item it, Point p)
        {
            if (it == null) return 0;
            p.Y += scrollOffset;
            if (it.K == Kind.Media)
            {
                for (int i = 0; i < 3; i++)
                    if (MediaEnabled(i) && MediaButton(it, i).Contains(p)) return MediaPart + i;
                return 0;
            }
            if (it.SetMute != null && IconRect(it).Contains(p)) return 1;
            if (it.TitleClick != null && !it.IsDefault)
            {
                var tr = TitleRect(it);
                float tw = Math.Min(tr.Width, TextRenderer.MeasureText(it.Label, fText).Width);
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
            it.Value = v;
            try
            {
                it.SetVol(v / 100f);
                if (it.Muted && v > 0) { it.SetMute(false); it.Muted = false; }
            }
            catch { }
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (dragItem != null) { SetFromX(dragItem, e.X); return; }
            var it = RowAt(e.Location); int part = PartAt(it, e.Location);
            if (it != hoverItem || part != hoverPart)
            {
                hoverItem = it; hoverPart = part;
                Cursor = part > 0 ? Cursors.Hand : Cursors.Default;
                UpdateMediaTip();
                Invalidate();
            }
        }

        void UpdateMediaTip()
        {
            string tip = null;
            int button = hoverPart - MediaPart;
            if (button >= 0 && MediaEnabled(button))
            {
                if (button == 0) tip = Loc.En ? "Previous track" : "Предыдущий трек";
                else if (button == 2) tip = Loc.En ? "Next track" : "Следующий трек";
                else if (track.Playing) tip = Loc.En ? "Pause" : "Пауза";
                else tip = Loc.En ? "Play" : "Воспроизвести";
            }
            if (mediaTip.GetToolTip(this) != (tip ?? "")) mediaTip.SetToolTip(this, tip);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (dragItem != null) return;
            hoverItem = null;
            hoverPart = 0;
            UpdateMediaTip();
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                pressedMedia = -1;
                if (focusedMedia >= 0) { focusedMedia = -1; Invalidate(); }
            }
            if (e.Button == MouseButtons.Left && e.X >= ClientSize.Width - S(8) && contentHeight > ClientSize.Height)
            {
                int target = (int)((long)e.Y * (contentHeight - ClientSize.Height) / ClientSize.Height);
                ScrollBy(target - scrollOffset);
                return;
            }
            var it = RowAt(e.Location); int part = PartAt(it, e.Location);
            if (e.Button == MouseButtons.Right && it != null && it.K == Kind.Row)
            {
                // Правый клик по строке — скрыть её (вернуть можно в меню значка в трее)
                var row = it;
                if (rowMenu.MenuItems.Count != 0)
                {
                    var previous = rowMenu.MenuItems[0];
                    rowMenu.MenuItems.RemoveAt(0);
                    previous.Dispose();
                }
                rowMenu.MenuItems.Add(new MenuItem(Loc.Text("Скрыть «", "Hide “") + row.Label.Replace("&", "&&") + Loc.Text("»", "”"),
                    delegate { Hidden.Hide(row.Key, row.Text); RefreshContent(true); }));
                rowMenu.Show(this, e.Location);
                return;
            }
            if (e.Button != MouseButtons.Left) return;
            pressedMedia = part >= MediaPart ? part - MediaPart : -1;
            if (pressedMedia >= 0) return;
            if (part == 1) { try { it.SetMute(!it.Muted); } catch { } it.Sync(); Invalidate(); }
            else if (part == 2) it.TitleClick();
            else if (part == 3) { dragItem = it; Capture = true; SetFromX(it, e.X); }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (dragItem != null) { dragItem = null; Capture = false; Invalidate(); }
            if (e.Button != MouseButtons.Left || pressedMedia < 0) return;
            int index = pressedMedia;
            pressedMedia = -1;
            if (PartAt(RowAt(e.Location), e.Location) == MediaPart + index) ExecuteMedia(index);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            var it = RowAt(e.Location);
            if ((ModifierKeys & Keys.Shift) != 0 || e.X < S(M) || e.X >= Width - S(M) || (it != null && it.K == Kind.Media))
            {
                if (focusedMedia >= 0) { focusedMedia = -1; Invalidate(); }
                ScrollBy(e.Delta > 0 ? -S(54) : S(54));
                return;
            }
            if (it != null && it.K == Kind.Row) Apply(it, it.Value + (e.Delta > 0 ? 2 : -2));
        }

    }

    // ───────────────────────── Скрытые строки (хранятся в реестре) ─────────────────────────
    static class Hidden
    {
        const string RegKey = @"Software\TrayMixer";
        public static readonly Dictionary<string, string> Items = Load(); // ключ → название

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
            catch (UnauthorizedAccessException) { }
            catch (SecurityException) { }
            catch (System.IO.IOException) { }
        }

        public static bool Is(string key) { return Items.ContainsKey(key); }
        public static void Hide(string key, string name) { Items[key] = name; Save(); }
        public static void Show(string key) { Items.Remove(key); Save(); }
        public static void ShowAll() { Items.Clear(); Save(); }
    }

    // ───────────────────────── Уведомления о звуке ─────────────────────────
    [ComVisible(true)]
    class Notifier : IMMNotificationClient, IAudioEndpointVolumeCallback
    {
        readonly Control ui; public Action VolumeChanged, DevicesChanged;
        public Notifier(Control ui) { this.ui = ui; }
        void Post(Action a) { try { if (a != null) ui.BeginInvoke(a); } catch { } }
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
    // the MenuItem model remains the single source of actions and checks.
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
        }
        int S(float v) { return (int)Math.Round(v * scale); }
        internal void RefreshItems()
        {
            theme = Theme.Load();
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
            menu.BackColor = theme.Light ? Color.FromArgb(243, 243, 243) : Color.FromArgb(32, 32, 32);
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
        internal sealed class FlyoutRenderer : ToolStripProfessionalRenderer
        {
            readonly Theme t; readonly float s;
            int S(float v) { return (int)Math.Round(v * s); }
            internal FlyoutRenderer(Theme t, float s) { this.t = t; this.s = s; RoundedEdges = false; }
            protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e) { e.Graphics.Clear(e.ToolStrip.BackColor); }
            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                using (var p = new Pen(t.Sep)) e.Graphics.DrawRectangle(p, 0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
            }
            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                var checkedItem = e.Item as ToolStripMenuItem;
                if (checkedItem != null && checkedItem.Checked)
                    using (var pen = new Pen(t.Accent, Math.Max(1, S(1.5f)))) {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        int y = e.Item.Height / 2;
                        e.Graphics.DrawLines(pen, new[] { new Point(S(9), y), new Point(S(12), y + S(3)), new Point(S(18), y - S(4)) });
                    }
                if (!e.Item.Selected || !e.Item.Enabled) return;
                var r = new RectangleF(0, 1, e.Item.Width, e.Item.Height - 2);
                using (var p = new GraphicsPath())
                using (var b = new SolidBrush(t.Hover))
                {
                    float d = S(8);
                    p.AddArc(r.Left, r.Top, d, d, 180, 90); p.AddArc(r.Right - d, r.Top, d, d, 270, 90);
                    p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.Left, r.Bottom - d, d, d, 90, 90); p.CloseFigure();
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
            protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e) { }
        }
    }

    static class MixerShortcut
    {
        public const Keys Default = Keys.Control | Keys.Alt | Keys.M;
        const Keys SupportedModifiers = Keys.Control | Keys.Alt | Keys.Shift;

        public static bool IsValid(Keys shortcut)
        {
            Keys key = shortcut & Keys.KeyCode;
            if ((shortcut & ~(Keys.KeyCode | SupportedModifiers)) != 0 ||
                (shortcut & (Keys.Control | Keys.Alt)) == 0 || key == Keys.F12) return false;
            return (key >= Keys.A && key <= Keys.Z) || (key >= Keys.D0 && key <= Keys.D9) ||
                (key >= Keys.F1 && key <= Keys.F24) || (key >= Keys.NumPad0 && key <= Keys.Divide) ||
                (key >= Keys.Space && key <= Keys.Down) || key == Keys.Insert || key == Keys.Delete;
        }

        public static uint Modifiers(Keys shortcut)
        {
            uint mods = 0x4000; // MOD_NOREPEAT
            if ((shortcut & Keys.Alt) != 0) mods |= 1;
            if ((shortcut & Keys.Control) != 0) mods |= 2;
            if ((shortcut & Keys.Shift) != 0) mods |= 4;
            return mods;
        }

        public static string Format(Keys shortcut)
        {
            return shortcut == Keys.None ? Loc.Text("Отключено", "Disabled") : new KeysConverter().ConvertToString(shortcut);
        }
    }

    sealed class TrayIcon : NativeWindow, IDisposable
    {
        const int Add = 0, Modify = 1, Delete = 2;
        const int CallbackMessage = 0x8001; // WM_APP + 1
        const int HotkeyMessage = 0x0312, FirstHotkeyId = 1, SecondHotkeyId = 2;
        static readonly int TaskbarCreated = RegisterWindowMessage("TaskbarCreated");
        readonly System.Windows.Forms.Timer retry = new System.Windows.Forms.Timer { Interval = 1000 };
        readonly Action<MouseButtons> click;
        Icon icon;
        string text;
        bool added, doubleClick;
        int hotkeyId;
        Keys registeredShortcut;
        public event Action OpenShortcutPressed;
        public bool HasOpenShortcut { get { return hotkeyId != 0; } }

        [DllImport("user32.dll")]
        static extern bool RegisterHotKey(IntPtr window, int id, uint modifiers, uint key);
        [DllImport("user32.dll")]
        static extern bool UnregisterHotKey(IntPtr window, int id);

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

        public string SetOpenShortcut(Keys shortcut, bool persist)
        {
            if (shortcut == registeredShortcut)
                return !persist || MixerPreferences.SaveShortcut(shortcut) ? null
                    : Loc.Text("Не удалось сохранить сочетание.", "Could not save the shortcut.");
            int nextId = hotkeyId == FirstHotkeyId ? SecondHotkeyId : FirstHotkeyId;
            if (shortcut != Keys.None && !RegisterHotKey(Handle, nextId, MixerShortcut.Modifiers(shortcut), (uint)(shortcut & Keys.KeyCode)))
                return Loc.Text("Сочетание занято или зарезервировано Windows.", "This shortcut is in use or reserved by Windows.");
            if (persist && !MixerPreferences.SaveShortcut(shortcut))
            {
                if (shortcut != Keys.None) UnregisterHotKey(Handle, nextId);
                return Loc.Text("Не удалось сохранить сочетание.", "Could not save the shortcut.");
            }
            if (hotkeyId != 0) UnregisterHotKey(Handle, hotkeyId);
            hotkeyId = shortcut == Keys.None ? 0 : nextId;
            registeredShortcut = shortcut;
            return null;
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
            if (m.Msg == HotkeyMessage && hotkeyId != 0 && m.WParam == (IntPtr)hotkeyId)
            {
                if (OpenShortcutPressed != null) OpenShortcutPressed();
                return;
            }
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
            if (hotkeyId != 0) UnregisterHotKey(Handle, hotkeyId);
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

    sealed class SettingsLabel : Label
    {
        public bool Wrap { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            GdiText.Draw(e.Graphics, Text, Font, ForeColor, ClientRectangle, StringAlignment.Near, wrap: Wrap);
        }
    }

    sealed class SettingsField : Control
    {
        readonly Theme theme;
        readonly float scale;
        readonly Color fill, border;
        readonly bool picker;
        bool expanded;

        public SettingsField(Theme theme, float scale, Color fill, Color border, bool picker)
        {
            this.theme = theme;
            this.scale = scale;
            this.fill = fill;
            this.border = border;
            this.picker = picker;
            AccessibleRole = picker ? AccessibleRole.PushButton : AccessibleRole.Text;
            TabStop = true;
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            GotFocus += delegate { Invalidate(); };
            LostFocus += delegate { Invalidate(); };
            TextChanged += delegate { Invalidate(); };
        }

        public bool Expanded
        {
            set { expanded = value; Invalidate(); }
        }

        int S(float value) { return (int)Math.Round(value * scale); }

        internal static void DrawFrame(Graphics graphics, Size size, int radius, Color fill, Color border)
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = Popup.Round(new RectangleF(0.5f, 0.5f, size.Width - 1, size.Height - 1), radius))
            using (var brush = new SolidBrush(fill))
            using (var pen = new Pen(border))
            {
                graphics.FillPath(brush, path);
                graphics.DrawPath(pen, path);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            DrawFrame(e.Graphics, ClientSize, S(7), fill, Focused || expanded ? theme.Accent : border);
            var textBounds = new Rectangle(S(10), 0, Width - S(picker ? 37 : 20), Height);
            GdiText.Draw(e.Graphics, Text, Font, ForeColor, textBounds, StringAlignment.Near);
            if (!picker) return;
            float x = Width - S(16), y = Height / 2f;
            float offset = expanded ? -S(2.5f) : S(2.5f);
            using (var pen = new Pen(ForeColor, Math.Max(1, scale * 1.2f)) { LineJoin = LineJoin.Round })
                e.Graphics.DrawLines(pen, new[] { new PointF(x - S(4), y - offset / 2), new PointF(x, y + offset), new PointF(x + S(4), y - offset / 2) });
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) Focus();
            base.OnMouseDown(e);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if (picker && (keyData == Keys.Down || keyData == Keys.Enter || keyData == Keys.Space || keyData == Keys.F4))
                return true;
            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (picker && (e.KeyData == Keys.Down || e.KeyData == Keys.Enter || e.KeyData == Keys.Space || e.KeyData == Keys.F4))
            {
                e.SuppressKeyPress = true;
                OnClick(EventArgs.Empty);
            }
            base.OnKeyDown(e);
        }
    }

    sealed class SliderNameDialog : Form
    {
        sealed class Choice
        {
            public readonly string Id, Original, Group;
            public Choice(string id, string original, string group) { Id = id; Original = original; Group = group; }
            public override string ToString() { return Group + " · " + Original; }
        }

        readonly List<Choice> choices = new List<Choice>();
        readonly ContextMenuStrip sliderMenu = new ContextMenuStrip();
        readonly SettingsField sliders;
        Choice selected;
        readonly TextBox name = new TextBox { MaxLength = 120, BorderStyle = BorderStyle.None };
        readonly Button save, reset;
        readonly Theme theme = Theme.Load();
        readonly float scale;
        readonly Color fieldColor, fieldBorder, muted;

        int S(int value) { return (int)Math.Round(value * scale); }

        public SliderNameDialog(float scale, Font font)
        {
            this.scale = scale;
            AutoScaleMode = AutoScaleMode.None;
            Font = font;
            Text = Loc.Text("Названия ползунков", "Slider names");
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(S(416), S(193));
            BackColor = theme.Light ? Color.FromArgb(243, 243, 243) : Color.FromArgb(32, 32, 32);
            ForeColor = theme.Light ? Color.FromArgb(27, 27, 27) : Color.FromArgb(244, 244, 244);
            fieldColor = theme.Light ? Color.White : Color.FromArgb(43, 43, 43);
            fieldBorder = theme.Light ? Color.FromArgb(193, 193, 193) : Color.FromArgb(75, 75, 75);
            muted = theme.Light ? Color.FromArgb(85, 85, 85) : Color.FromArgb(181, 181, 181);
            name.Font = font;
            name.BackColor = fieldColor;
            name.ForeColor = ForeColor;
            string sliderCaption = Loc.Text("Ползунок", "Slider");
            sliders = new SettingsField(theme, scale, fieldColor, fieldBorder, true) { TabIndex = 0 };
            sliderMenu.Font = font;
            sliderMenu.BackColor = BackColor;
            sliderMenu.ForeColor = ForeColor;
            sliderMenu.ShowImageMargin = sliderMenu.ShowCheckMargin = false;
            sliderMenu.Padding = new Padding(S(5));
            sliderMenu.Renderer = new TrayFlyout.FlyoutRenderer(theme, scale);
            sliderMenu.Opened += delegate {
                int round = 2, dark = theme.Light ? 0 : 1;
                Native.DwmSetWindowAttribute(sliderMenu.Handle, 33, ref round, 4);
                Native.DwmSetWindowAttribute(sliderMenu.Handle, 20, ref dark, 4);
                sliders.Expanded = true;
                if (selected != null) sliderMenu.Items[choices.IndexOf(selected)].Select();
            };
            sliderMenu.Closed += delegate { sliders.Expanded = false; };
            sliders.Click += delegate {
                sliderMenu.Show(sliders, new Point(0, sliders.Height + S(4)), ToolStripDropDownDirection.BelowRight);
            };
            name.AccessibleName = Loc.Text("Название", "Name");
            reset = new EditorButton(theme, scale, fieldColor, fieldBorder, muted, false) { Text = Loc.Text("Сбросить", "Reset"), TabIndex = 2 };
            save = new EditorButton(theme, scale, fieldColor, fieldBorder, muted, true) { Text = Loc.Text("Сохранить", "Save"), TabIndex = 3 };
            reset.SetBounds(S(18), S(143), S(96), S(32));
            save.SetBounds(S(286), S(143), S(112), S(32));
            AcceptButton = save;
            sliders.SetBounds(S(18), S(35), S(380), S(32));
            sliderMenu.MinimumSize = sliderMenu.MaximumSize = new Size(sliders.Width, 0);
            var nameField = new Panel { TabIndex = 1 };
            nameField.SetBounds(S(18), S(94), S(380), S(32));
            name.SetBounds(S(10), (nameField.Height - name.PreferredHeight) / 2, nameField.Width - S(20), name.PreferredHeight);
            nameField.Controls.Add(name);
            nameField.Paint += PaintNameField;
            nameField.MouseDown += delegate { name.Focus(); };
            name.GotFocus += delegate { nameField.Invalidate(); };
            name.LostFocus += delegate { nameField.Invalidate(); };
            var sliderLabel = new SettingsLabel { Text = sliderCaption, ForeColor = muted };
            sliderLabel.SetBounds(S(20), S(13), S(378), S(19));
            var nameLabel = new SettingsLabel { Text = name.AccessibleName, ForeColor = muted };
            nameLabel.SetBounds(S(20), S(73), S(378), S(19));
            Controls.AddRange(new Control[] { sliderLabel, sliders, nameLabel, nameField, reset, save });

            string volume = Loc.Text("Громкость", "Volume");
            string brightness = Loc.Text("Яркость", "Brightness");
            try
            {
                foreach (var device in Audio.Devices()) choices.Add(new Choice("dev:" + device.Id, device.Name, volume));
                foreach (var app in Audio.Apps()) choices.Add(new Choice("app:" + app.Key, app.Name, volume));
            }
            catch (Exception e) { Debug.WriteLine(e); }
            foreach (var display in Brightness.Displays) choices.Add(new Choice("disp:" + display.Key, display.Label, brightness));
            choices.Add(new Choice("blue:filter", Loc.BlueFilter, brightness));
            choices.Add(new Choice("blue:dim", Loc.Dimming, brightness));
            foreach (var choice in choices)
            {
                var item = new ToolStripMenuItem(choice.ToString()) {
                    AutoSize = false, Size = new Size(sliders.Width - sliderMenu.Padding.Horizontal - S(2), S(30))
                };
                item.Click += delegate { SelectChoice(choice); };
                sliderMenu.Items.Add(item);
            }

            name.TextChanged += delegate { UpdateActions(); };
            save.Click += delegate { StoreName(name.Text.Trim()); };
            reset.Click += delegate { StoreName(null); };
            SelectChoice(choices[0]);
        }

        void SelectChoice(Choice choice)
        {
            selected = choice;
            sliders.Text = choice.ToString();
            sliders.AccessibleName = Loc.Text("Ползунок", "Slider") + ": " + sliders.Text;
            for (int i = 0; i < choices.Count; i++)
                ((ToolStripMenuItem)sliderMenu.Items[i]).Checked = choices[i] == choice;
            LoadName();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) sliderMenu.Dispose();
            base.Dispose(disposing);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            int dark = theme.Light ? 0 : 1, round = 2;
            int caption = ColorTranslator.ToWin32(BackColor), title = ColorTranslator.ToWin32(ForeColor);
            Native.DwmSetWindowAttribute(Handle, 20, ref dark, 4);
            Native.DwmSetWindowAttribute(Handle, 33, ref round, 4);
            Native.DwmSetWindowAttribute(Handle, 35, ref caption, 4);
            Native.DwmSetWindowAttribute(Handle, 36, ref title, 4);
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Escape) { Close(); return true; }
            return base.ProcessDialogKey(keyData);
        }

        void PaintNameField(object sender, PaintEventArgs e)
        {
            var field = (Panel)sender;
            SettingsField.DrawFrame(e.Graphics, field.ClientSize, S(7), fieldColor, name.Focused ? theme.Accent : fieldBorder);
        }

        internal sealed class EditorButton : Button
        {
            readonly Theme theme;
            readonly float scale;
            readonly Color fieldColor, fieldBorder, muted;
            readonly bool primary;

            public EditorButton(Theme theme, float scale, Color fieldColor, Color fieldBorder, Color muted, bool primary)
            {
                this.theme = theme;
                this.scale = scale;
                this.fieldColor = fieldColor;
                this.fieldBorder = fieldBorder;
                this.muted = muted;
                this.primary = primary;
                FlatStyle = FlatStyle.Flat;
                SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                e.Graphics.Clear(BackColor);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Color fill = fieldColor;
                Color text = Enabled ? ForeColor : muted;
                if (Enabled && primary)
                {
                    fill = theme.Accent;
                    text = (fill.R * 299 + fill.G * 587 + fill.B * 114) >= 145000 ? Color.FromArgb(22, 22, 22) : Color.White;
                }
                var rect = new RectangleF(0.5f, 0.5f, Width - 1, Height - 1);
                using (var shape = Popup.Round(rect, (int)Math.Round(6 * scale)))
                {
                    using (var brush = new SolidBrush(fill)) e.Graphics.FillPath(brush, shape);
                    if (Enabled && ClientRectangle.Contains(PointToClient(MousePosition)))
                    {
                        int alpha = MouseButtons == MouseButtons.Left ? 35 : 17;
                        using (var hover = new SolidBrush(Color.FromArgb(alpha, text))) e.Graphics.FillPath(hover, shape);
                    }
                    if (!primary || !Enabled || Focused)
                        using (var border = new Pen(Focused ? theme.Accent : fieldBorder)) e.Graphics.DrawPath(border, shape);
                    if (Focused && primary && Enabled)
                    {
                        var focusRect = RectangleF.Inflate(rect, -2 * scale, -2 * scale);
                        using (var focus = Popup.Round(focusRect, 4 * scale))
                        using (var pen = new Pen(text)) e.Graphics.DrawPath(pen, focus);
                    }
                }
                GdiText.Draw(e.Graphics, Text, Font, text, ClientRectangle, StringAlignment.Center);
            }
        }

        void LoadName()
        {
            name.Text = MixerPreferences.SliderName(selected.Id, selected.Original);
            UpdateActions();
        }

        void UpdateActions()
        {
            string edited = name.Text.Trim();
            save.Enabled = edited.Length > 0 && edited != MixerPreferences.SliderName(selected.Id, selected.Original);
            reset.Enabled = MixerPreferences.HasName(selected.Id);
        }

        void StoreName(string edited)
        {
            if (MixerPreferences.SaveName(selected.Id, edited == selected.Original ? null : edited)) LoadName();
            else MessageBox.Show(this, Loc.Text("Не удалось сохранить название.", "Could not save the name."), Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    sealed class ShortcutDialog : Form
    {
        readonly Theme theme = Theme.Load();
        readonly SettingsField input;
        readonly SettingsLabel status = new SettingsLabel { Wrap = true };
        readonly Button save, disable;
        readonly Func<Keys, string> apply;
        readonly Keys original;
        readonly bool unavailable;
        Keys pending;

        public ShortcutDialog(float scale, Font font, Keys shortcut, bool unavailable, Func<Keys, string> apply)
        {
            this.apply = apply;
            this.unavailable = unavailable;
            pending = original = shortcut;
            Func<int, int> s = value => (int)Math.Round(value * scale);
            Text = Loc.Text("Горячая клавиша", "Keyboard shortcut");
            Font = font;
            AutoScaleMode = AutoScaleMode.None;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(s(396), s(174));
            BackColor = theme.Light ? Color.FromArgb(243, 243, 243) : Color.FromArgb(32, 32, 32);
            ForeColor = theme.Light ? Color.FromArgb(27, 27, 27) : Color.FromArgb(244, 244, 244);
            Color fieldColor = theme.Light ? Color.White : Color.FromArgb(43, 43, 43);
            Color borderColor = theme.Light ? Color.FromArgb(193, 193, 193) : Color.FromArgb(75, 75, 75);
            Color muted = theme.Light ? Color.FromArgb(85, 85, 85) : Color.FromArgb(181, 181, 181);
            var caption = new SettingsLabel { Text = Loc.Text("Нажмите сочетание с Ctrl или Alt", "Press a combination with Ctrl or Alt"), ForeColor = muted };
            caption.SetBounds(s(20), s(17), s(358), s(19));
            input = new SettingsField(theme, scale, fieldColor, borderColor, false)
                { AccessibleDescription = caption.Text, TabIndex = 0 };
            input.SetBounds(s(18), s(40), s(360), s(34));

            status.ForeColor = theme.Light ? Color.FromArgb(145, 40, 37) : Color.FromArgb(255, 170, 159);
            status.SetBounds(s(18), s(83), s(360), s(36));
            disable = new SliderNameDialog.EditorButton(theme, scale, fieldColor, borderColor, muted, false)
                { Text = Loc.Text("Отключить", "Disable"), TabIndex = 1 };
            save = new SliderNameDialog.EditorButton(theme, scale, fieldColor, borderColor, muted, true)
                { Text = Loc.Text("Сохранить", "Save"), TabIndex = 2 };
            disable.SetBounds(s(18), s(124), s(106), s(32));
            save.SetBounds(s(266), s(124), s(112), s(32));
            disable.Click += delegate { Apply(Keys.None); };
            save.Click += delegate { Apply(pending); };
            AcceptButton = save;
            Controls.AddRange(new Control[] { caption, input, status, disable, save });
            ActiveControl = input;
            UpdateChoice();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            int dark = theme.Light ? 0 : 1, round = 2;
            int caption = ColorTranslator.ToWin32(BackColor), text = ColorTranslator.ToWin32(ForeColor);
            Native.DwmSetWindowAttribute(Handle, 20, ref dark, 4);
            Native.DwmSetWindowAttribute(Handle, 33, ref round, 4);
            Native.DwmSetWindowAttribute(Handle, 35, ref caption, 4);
            Native.DwmSetWindowAttribute(Handle, 36, ref text, 4);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape) { Close(); return true; }
            if (input.Focused && (keyData & (Keys.Control | Keys.Alt)) != 0)
            {
                if (MixerShortcut.IsValid(keyData))
                {
                    pending = keyData;
                    UpdateChoice();
                }
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        void UpdateChoice()
        {
            input.Text = MixerShortcut.Format(pending);
            input.AccessibleName = Text + ": " + input.Text;
            disable.Enabled = original != Keys.None;
            save.Enabled = pending != original || unavailable;
            status.Text = pending == original && unavailable
                ? Loc.Text("Сочетание недоступно. Выберите другое.", "This shortcut is unavailable. Choose another.")
                : "";
        }

        void Apply(Keys shortcut)
        {
            var error = apply(shortcut);
            if (error == null) Close();
            else status.Text = error;
        }
    }

    // ───────────────────────── Трей ─────────────────────────
    class TrayApp : IDisposable
    {
        public const string StartupArgument = "--startup";
        const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run", AppName = "TrayMixer", CfgKey = @"Software\TrayMixer";
        readonly TrayIcon icon;
        readonly Popup popup;
        readonly TrayFlyout flyout;
        readonly ContextMenu trayMenu = new ContextMenu();
        readonly float scale;
        readonly Notifier notifier;
        readonly System.Windows.Forms.Timer audioRetry = new System.Windows.Forms.Timer { Interval = 1000 };
        IMMDeviceEnumerator audioEnumerator;
        IAudioEndpointVolume defVol;
        string lastGlyph; bool lastLight;
        bool settingsOpen;

        public TrayApp(float scale)
        {
            this.scale = scale;
            popup = new Popup(Theme.RegInt(CfgKey, "Backdrop", Popup.Mica), scale);
            popup.CreateControl(); GC.KeepAlive(popup.Handle); // окно нужно сразу: через него приходят уведомления

            try { Native.SetPreferredAppMode(1); Native.FlushMenuThemes(); } catch { } // тёмное контекстное меню
            flyout = new TrayFlyout(trayMenu, scale);
            icon = new TrayIcon(button => {
                if (button == MouseButtons.Right) { popup.HidePopup(); BuildTrayMenu(); flyout.ShowFlyout(); }
                else { flyout.Close(); OnTrayClick(button); }
            });
            icon.OpenShortcutPressed += OpenMixer;
            icon.SetOpenShortcut(MixerPreferences.OpenShortcut, false);

            notifier = new Notifier(popup)
            {
                VolumeChanged = () => { UpdateTray(); popup.SyncValues(); },
                DevicesChanged = () => { Rebind(); if (popup.Visible) popup.RefreshContent(true); }
            };
            UpdateTray();
            audioRetry.Tick += delegate { ConnectAudio(); };
            popup.BeginInvoke((Action)(() => {
                ConnectAudio();
                Brightness.ScanAsync(null);
                BlueLight.Start();
            }));
        }

        public void RequestOpen()
        {
            try { popup.BeginInvoke((Action)OpenMixer); }
            catch (InvalidOperationException) { } // The window is closing.
        }

        void OpenMixer()
        {
            if (settingsOpen) return;
            flyout.Close();
            if (popup.Visible) popup.Activate();
            else popup.ShowPopup();
        }

        public void Dispose()
        {
            audioRetry.Dispose();
            BlueLight.Reset();
            if (defVol != null) try { defVol.UnregisterControlChangeNotify(notifier); } catch { }
            if (audioEnumerator != null) try { audioEnumerator.UnregisterEndpointNotificationCallback(notifier); } catch { }
            icon.Dispose();
            flyout.Dispose();
            trayMenu.Dispose();
            popup.Dispose();
        }

        void BuildTrayMenu()
        {
            while (trayMenu.MenuItems.Count > 0)
            {
                var old = trayMenu.MenuItems[0];
                trayMenu.MenuItems.RemoveAt(0);
                old.Dispose();
            }
            var hidden = new MenuItem(Loc.Text("Скрытые", "Hidden"));
            FillHidden(hidden);
            var language = new MenuItem(Loc.Text("Язык", "Language"), new[]
            {
                new MenuItem("Русский", delegate { SetLanguage(false); }) { RadioCheck = true, Checked = !Loc.En },
                new MenuItem("English", delegate { SetLanguage(true); }) { RadioCheck = true, Checked = Loc.En }
            });
            var settings = new MenuItem(Loc.Text("Настройки", "Settings"), new[]
            {
                language,
                new MenuItem(Loc.Text("Названия ползунков…", "Slider names…"), delegate { popup.BeginInvoke((Action)ShowSliderNames); }),
                new MenuItem(Loc.Text("Горячая клавиша…", "Keyboard shortcut…"), delegate { popup.BeginInvoke((Action)ShowOpenShortcut); }),
                new MenuItem("-"),
                new MenuItem(Loc.Text("Фон: Mica", "Background: Mica"), delegate { SetBackdrop(Popup.Mica); }) { RadioCheck = true, Checked = popup.Backdrop == Popup.Mica },
                new MenuItem(Loc.Text("Фон: Acrylic", "Background: Acrylic"), delegate { SetBackdrop(Popup.Acrylic); }) { RadioCheck = true, Checked = popup.Backdrop == Popup.Acrylic },
                new MenuItem("-"),
                new MenuItem(Loc.Text("Запускать вместе с Windows", "Run at Windows startup"), delegate { ToggleAutostart(); }) { Checked = IsAutostart() }
            });
            trayMenu.MenuItems.AddRange(new[]
            {
                new MenuItem(Loc.Text("Параметры звука", "Sound settings"), delegate { Open("ms-settings:sound"); }),
                new MenuItem(Loc.Text("Микшер громкости Windows", "Windows volume mixer"), delegate { Open("ms-settings:apps-volume"); }),
                new MenuItem("-"), hidden, settings, new MenuItem("-"),
                new MenuItem(Loc.Text("Выход", "Exit"), delegate { Application.Exit(); })
            });
        }

        void SetLanguage(bool english)
        {
            if (Loc.En == english) return;
            if (!MixerPreferences.SaveLanguage(english ? "en" : "ru"))
            {
                MessageBox.Show(Loc.Text("Не удалось сохранить язык.", "Could not save the language."), AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Loc.En = english;
            UpdateTray();
        }

        void ShowSliderNames()
        {
            using (var font = new Font(Theme.FontName, (float)Math.Round(14 * scale), GraphicsUnit.Pixel))
            using (var dialog = new SliderNameDialog(scale, font))
                ShowSettingsDialog(dialog);
            if (popup.Visible) popup.RefreshContent(true);
        }

        void ShowOpenShortcut()
        {
            Keys shortcut = MixerPreferences.OpenShortcut;
            using (var font = new Font(Theme.FontName, (float)Math.Round(14 * scale), GraphicsUnit.Pixel))
            using (var dialog = new ShortcutDialog(scale, font, shortcut, shortcut != Keys.None && !icon.HasOpenShortcut,
                keys => icon.SetOpenShortcut(keys, true)))
                ShowSettingsDialog(dialog);
        }

        void ShowSettingsDialog(Form dialog)
        {
            if (settingsOpen) return;
            settingsOpen = true;
            try
            {
                flyout.Close();
                popup.HidePopup();
                dialog.Shown += delegate { Native.SetForegroundWindow(dialog.Handle); };
                dialog.ShowDialog();
            }
            finally { settingsOpen = false; }
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

        // Подписка на громкость текущего устройства по умолчанию
        void Rebind()
        {
            if (defVol != null) try { defVol.UnregisterControlChangeNotify(notifier); } catch { }
            defVol = null;
            try { var d = Audio.Default(); if (d != null) { defVol = Audio.EndpointVolume(d); defVol.RegisterControlChangeNotify(notifier); } } catch { }
            UpdateTray();
        }

        // Подменю «Скрытые»: клик возвращает строку в окно
        static void FillHidden(MenuItem menu)
        {
            if (Hidden.Items.Count == 0) { menu.MenuItems.Add(new MenuItem(Loc.Text("Нет скрытых", "No hidden sliders")) { Enabled = false }); return; }
            foreach (var kv in new Dictionary<string, string>(Hidden.Items))
            {
                string key = kv.Key;
                string name = MixerPreferences.SliderName(key, kv.Value);
                menu.MenuItems.Add(new MenuItem(Loc.Text("Показать «", "Show “") + name + Loc.Text("»", "”"), delegate { Hidden.Show(key); }));
            }
            menu.MenuItems.Add(new MenuItem("-"));
            menu.MenuItems.Add(new MenuItem(Loc.Text("Показать все", "Show all"), delegate { Hidden.ShowAll(); }));
        }

        void SetBackdrop(int b)
        {
            popup.Backdrop = b;
            try { using (var k = Registry.CurrentUser.CreateSubKey(CfgKey)) k.SetValue("Backdrop", b); }
            catch (UnauthorizedAccessException) { }
            catch (SecurityException) { }
            catch (System.IO.IOException) { }
        }

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
            string glyph = "off", text = Loc.Text("Нет устройства вывода", "No output device");
            try
            {
                if (defVol != null)
                {
                    float l; bool m; defVol.GetMasterVolumeLevelScalar(out l); defVol.GetMute(out m);
                    int p = (int)Math.Round(l * 100);
                    glyph = m ? "off" : "on";
                    text = Loc.Text("Громкость: ", "Volume: ") + (m ? Loc.Text("выкл.", "muted") : p + "%");
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
        }
    }

    static class Program
    {
        const string InstanceName = @"Local\TrayMixer_7c1f3e2a-5b0d-4f9e-9a61-2d8c4b7e1a90";

        [STAThread]
        static void Main(string[] args)
        {
            // Защита от подмены DLL: системные библиотеки грузятся только из System32, не из папки с exe
            try { Native.SetDefaultDllDirectories(0x800); } catch { }
            if (args.Length > 0 && args[0] == "--reset-gamma") { Environment.ExitCode = BlueLight.Reset() ? 0 : 1; return; }
            Native.SetProcessDPIAware();
            float scale; using (var g = Graphics.FromHwnd(IntPtr.Zero)) scale = g.DpiX / 96f;
            if (args.Length > 1 && args[0] == "--make-icon" && args[1].EndsWith(".ico", StringComparison.OrdinalIgnoreCase)) { Art.WriteIco(args[1]); return; }
            if (args.Length > 1 && args[0] == "--render" && args[1].EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                Loc.En = args.Length > 2 && args[2] == "en";
                using (var p = new Popup(Popup.Mica, scale)) p.RenderTo(args[1], Color.FromArgb(32, 32, 32));
                return;
            }
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
