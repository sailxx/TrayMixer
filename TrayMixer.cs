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
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

[assembly: System.Runtime.InteropServices.DefaultDllImportSearchPaths(System.Runtime.InteropServices.DllImportSearchPath.System32)]
[assembly: System.Reflection.AssemblyTitle("TrayMixer")]
[assembly: System.Reflection.AssemblyProduct("TrayMixer")]
[assembly: System.Reflection.AssemblyDescription("Lightweight Windows 11 tray volume mixer")]
[assembly: System.Reflection.AssemblyVersion("1.0.0.0")]
[assembly: System.Reflection.AssemblyFileVersion("1.0.0.0")]

namespace TrayMixer
{
    // ───────────────────────── Тема ─────────────────────────
    class Theme
    {
        public bool Light;
        public Color Fg, Sub, Track, ThumbOuter, ThumbBorder, Hover, Sep, Accent, Badge, Footer;
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
                t.ThumbBorder = C(0x29000000); t.Hover = C(0x0F000000); t.Sep = C(0x14000000); t.Badge = C(0xFFF3F3F3); t.Footer = C(0x0C000000);
            }
            else
            {
                t.Fg = C(0xFFFFFFFF); t.Sub = C(0xC5FFFFFF); t.Track = C(0x8BFFFFFF); t.ThumbOuter = C(0xFF454545);
                t.ThumbBorder = C(0x18FFFFFF); t.Hover = C(0x15FFFFFF); t.Sep = C(0x18FFFFFF); t.Badge = C(0xFF2C2C2C); t.Footer = C(0x40000000);
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
        public const string Speaker = "", Headphones = "", Mute = "", Bell = "", Monitor = "", Mixer = "";
        public const string Vol0 = "", Vol1 = "", Vol2 = "", Vol3 = "";
    }

    // ───────────────────────── Значок: три вертикальных фейдера ─────────────────────────
    static class Art
    {
        // Рисует символ микшера в квадрате r. На малых размерах всё выровнено по пикселям — без размытия.
        public static void DrawMixer(Graphics g, RectangleF r, Color c)
        {
            float n = Math.Min(r.Width, r.Height);
            var oldSm = g.SmoothingMode; var oldPo = g.PixelOffsetMode;
            g.SmoothingMode = SmoothingMode.AntiAlias; g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            float tw = Math.Max(2, (float)Math.Round(n * 0.09));            // толщина дорожки
            float cw = Math.Max(tw + 2, (float)Math.Round(n * 0.27));       // ширина ручки
            float ch = Math.Max(4, (float)Math.Round(n * 0.2));             // высота ручки
            float top = r.Y + (float)Math.Round(n * 0.08), bot = r.Y + n - (float)Math.Round(n * 0.08);
            float[] xs = { 0.19f, 0.5f, 0.81f }, lv = { 0.66f, 0.3f, 0.52f };
            using (var b = new SolidBrush(c))
                for (int i = 0; i < 3; i++)
                {
                    float cx = r.X + (float)Math.Round(n * xs[i] - tw / 2) + tw / 2;
                    g.FillRectangle(b, cx - tw / 2, top, tw, bot - top);
                    float ky = (float)Math.Round(top + (bot - top) * lv[i] - ch / 2);
                    using (var p = RoundRect(new RectangleF((float)Math.Round(cx - cw / 2), ky, cw, ch), Math.Max(1, ch * 0.3f)))
                        g.FillPath(b, p);
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
                    if (n == 256) bmp.Save(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(path)), "icon-preview.png"), ImageFormat.Png);
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

    // ───────────────────────── Текст через GDI с альфа-каналом ─────────────────────────
    // GDI+ рисует текст мыльно. Рисуем системным GDI (сглаживание в оттенках серого) белым по чёрному,
    // яркость пикселя превращаем в прозрачность и красим в нужный цвет — чёткий текст поверх Mica.
    static class GdiText
    {
        [StructLayout(LayoutKind.Sequential)] struct RECT { public int L, T, R, B; }
        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)] static extern IntPtr CreateFont(int h, int w, int esc, int orient, int weight, int italic, int underline, int strike, int charset, int outPrec, int clipPrec, int quality, int pitch, string face);
        [DllImport("gdi32.dll")] static extern int SetTextColor(IntPtr hdc, int c);
        [DllImport("gdi32.dll")] static extern int SetBkMode(IntPtr hdc, int m);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern int DrawText(IntPtr hdc, string s, int n, ref RECT r, int flags);

        static readonly Dictionary<string, IntPtr> Fonts = new Dictionary<string, IntPtr>();

        static IntPtr HFont(Font f)
        {
            string key = f.Name + "|" + f.Size + "|" + (f.Bold ? 1 : 0);
            IntPtr h;
            if (!Fonts.TryGetValue(key, out h))
                Fonts[key] = h = CreateFont(-(int)Math.Round(f.Size), 0, 0, 0, f.Bold ? 600 : 400, 0, 0, 0, 1, 0, 0, 4 /*ANTIALIASED_QUALITY*/, 0, f.Name);
            return h;
        }

        public static void Draw(Graphics g, string s, Font f, Color c, RectangleF rf, StringAlignment align)
        {
            if (string.IsNullOrEmpty(s)) return;
            var r = Rectangle.Round(rf);
            int w = Math.Max(1, r.Width), h = Math.Max(1, r.Height);
            var bi = new Native.BITMAPINFOHEADER { biSize = 40, biWidth = w, biHeight = -h, biPlanes = 1, biBitCount = 32 };
            IntPtr bits;
            IntPtr dib = Native.CreateDIBSection(IntPtr.Zero, ref bi, 0, out bits, IntPtr.Zero, 0);
            IntPtr dc = Native.CreateCompatibleDC(IntPtr.Zero);
            IntPtr oldB = Native.SelectObject(dc, dib), oldF = Native.SelectObject(dc, HFont(f));
            SetTextColor(dc, 0xFFFFFF); SetBkMode(dc, 1);
            var rc = new RECT { R = w, B = h };
            int flags = 0x20 | 0x4 | 0x800 | 0x8000 | (align == StringAlignment.Center ? 0x1 : align == StringAlignment.Far ? 0x2 : 0); // SINGLELINE|VCENTER|NOPREFIX|END_ELLIPSIS
            DrawText(dc, s, s.Length, ref rc, flags);
            Native.GdiFlush();
            var px = new int[w * h];
            Marshal.Copy(bits, px, 0, px.Length);
            Native.SelectObject(dc, oldF); Native.SelectObject(dc, oldB); Native.DeleteDC(dc); Native.DeleteObject(dib);

            int rgb = c.ToArgb() & 0xFFFFFF;
            for (int i = 0; i < px.Length; i++)
            {
                int cov = (px[i] >> 8) & 0xFF; // зелёный канал = покрытие пикселя
                px[i] = cov == 0 ? 0 : ((cov * c.A / 255) << 24) | rgb;
            }
            using (var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb))
            {
                var bd = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                Marshal.Copy(px, 0, bd.Scan0, px.Length);
                bmp.UnlockBits(bd);
                var oldI = g.InterpolationMode; g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(bmp, r.X, r.Y, w, h);
                g.InterpolationMode = oldI;
            }
        }
    }

    // ───────────────────────── Элементы окна ─────────────────────────
    enum Kind { Sep, Note, Row, Footer }

    class Item
    {
        public Kind K; public string Text, Key; public int Y, H;
        public string Glyph; public Bitmap Img; public bool IsDefault; public Action TitleClick;
        public Func<float> GetVol; public Action<float> SetVol;
        public Func<bool> GetMute; public Action<bool> SetMute;
        public int Value; public bool Muted;

        public void Sync() { try { Value = (int)Math.Round(GetVol() * 100); Muted = GetMute(); } catch { } }
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
        string signature = "";
        Item hoverItem, dragItem; int hoverPart; // 1 — иконка, 2 — название, 3 — слайдер, 4/5 — кнопки внизу
        IntPtr dib, memDC, oldBmp; Bitmap surface; int dibW, dibH;
        Font fText, fValue, fGlyph, fFootGlyph, fBadge;
        const string GearGlyph = "";
        static readonly string[] FooterUris = { "ms-settings:apps-volume", "ms-settings:sound" };
        int backdrop;

        public DateTime LastHide = DateTime.MinValue;

        public Popup(int backdrop, float scale)
        {
            this.backdrop = backdrop; s = scale;
            FormBorderStyle = FormBorderStyle.FixedSingle; ControlBox = false; MinimizeBox = MaximizeBox = false;
            Text = "TrayMixer"; ShowInTaskbar = false; TopMost = true; StartPosition = FormStartPosition.Manual;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);
            fText = new Font(Theme.FontName, S(14), GraphicsUnit.Pixel);
            fValue = new Font(Theme.FontName, S(16), GraphicsUnit.Pixel);
            fGlyph = new Font(Theme.GlyphName, S(17), GraphicsUnit.Pixel);
            fFootGlyph = new Font(Theme.GlyphName, S(16), GraphicsUnit.Pixel);
            fBadge = new Font(Theme.GlyphName, S(7), GraphicsUnit.Pixel);
            sync.Tick += delegate { RefreshContent(false); };
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
            sync.Start();
        }

        public void HidePopup()
        {
            if (!Visible) return;
            Hide(); sync.Stop();
            items.Clear(); signature = ""; hoverItem = dragItem = null;
            FreeDib();
            LastHide = DateTime.Now;
            Native.Trim();
        }

        protected override void OnDeactivate(EventArgs e) { base.OnDeactivate(e); HidePopup(); }
        protected override bool ProcessCmdKey(ref Message msg, Keys key) { if (key == Keys.Escape) { HidePopup(); return true; } return base.ProcessCmdKey(ref msg, key); }

        // ── Данные ──
        static string Sig(List<DeviceInfo> devs, List<AppInfo> apps)
        {
            var sb = new StringBuilder();
            foreach (var d in devs) sb.Append(d.Id).Append(d.IsDefault ? "*" : "").Append('|');
            sb.Append("||");
            foreach (var a in apps) sb.Append(a.Key).Append(':').Append(a.Vols.Count).Append('|');
            return sb.ToString();
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
            try { devs = Audio.Devices(); apps = Audio.Apps(); } catch { return; }
            string sig = Sig(devs, apps);
            if (!force && sig == signature) { SyncValues(); return; }
            signature = sig;
            Build(devs, apps);
        }

        void Build(List<DeviceInfo> devs, List<AppInfo> apps)
        {
            items.Clear(); hoverItem = null;
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
            if (devs.Count == 0) items.Add(new Item { K = Kind.Note, Text = "Нет устройств вывода" });
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
            if (apps.Count == 0) items.Add(new Item { K = Kind.Note, Text = "Сейчас ничего не воспроизводит звук" });
            items.Add(new Item { K = Kind.Footer, Text = "Громкость" });

            int y = S(6);
            foreach (var it in items)
            {
                if (it.K == Kind.Footer) y += S(4);
                it.Y = y;
                it.H = S(it.K == Kind.Sep ? 9 : it.K == Kind.Note ? 40 : it.K == Kind.Footer ? 46 : 54);
                y += it.H;
                if (it.K == Kind.Row) it.Sync();
            }
            Place(S(W), y);
            Invalidate();
        }

        void Place(int w, int h)
        {
            var scr = Screen.FromPoint(Cursor.Position);
            var wa = scr.WorkingArea; int m = S(12);
            int x = wa.Left > scr.Bounds.Left ? wa.Left + m : wa.Right - w - m;
            int y = wa.Top > scr.Bounds.Top ? wa.Top + m : wa.Bottom - h - m;
            SetBounds(x, Math.Max(wa.Top, y), w, h);
        }

        // ── Геометрия строки (компактная: иконка слева, название над слайдером) ──
        // Сетка: поля 16, иконки 16–36, контент (название и начало дорожки) с 50, числа прижаты к правому полю
        const int M = 16, L = 50, ValW = 40, Rad = 10;
        RectangleF IconRect(Item it) { return new RectangleF(S(10), it.Y + S(25.5f) - S(16), S(32), S(32)); }
        RectangleF TitleRect(Item it) { return new RectangleF(S(L), it.Y + S(5), Width - S(L) - S(M), S(20)); }
        RectangleF SliderRect(Item it) { return new RectangleF(S(L - Rad), it.Y + S(25), Width - S(M + ValW + 12 - Rad) - S(L - Rad), S(22)); }
        RectangleF PctRect(Item it) { return new RectangleF(Width - S(M + ValW), it.Y + S(21), S(ValW), S(26)); }
        RectangleF FooterBtn(Item it, int i) { return new RectangleF(Width - S(8) - S(32) * (FooterUris.Length - i), it.Y + (it.H - S(32)) / 2f, S(32), S(32)); }

        // ── Отрисовка в DIB с альфа-каналом (нужно для Mica) ──
        protected override void OnPaintBackground(PaintEventArgs e) { }

        protected override void OnPaint(PaintEventArgs e)
        {
            int w = ClientSize.Width, h = ClientSize.Height;
            if (w <= 0 || h <= 0) return;
            EnsureDib(w, h);
            using (var g = Graphics.FromImage(surface)) Render(g);
            Native.GdiFlush();
            IntPtr hdc = e.Graphics.GetHdc();
            Native.BitBlt(hdc, 0, 0, w, h, memDC, 0, 0, 0x00CC0020);
            e.Graphics.ReleaseHdc(hdc);
        }

        void EnsureDib(int w, int h)
        {
            if (dib != IntPtr.Zero && w == dibW && h == dibH) return;
            FreeDib();
            var bi = new Native.BITMAPINFOHEADER { biSize = 40, biWidth = w, biHeight = -h, biPlanes = 1, biBitCount = 32 };
            IntPtr bits;
            dib = Native.CreateDIBSection(IntPtr.Zero, ref bi, 0, out bits, IntPtr.Zero, 0);
            memDC = Native.CreateCompatibleDC(IntPtr.Zero);
            oldBmp = Native.SelectObject(memDC, dib);
            surface = new Bitmap(w, h, w * 4, PixelFormat.Format32bppPArgb, bits);
            dibW = w; dibH = h;
        }

        void FreeDib()
        {
            if (surface != null) { surface.Dispose(); surface = null; }
            if (memDC != IntPtr.Zero) { Native.SelectObject(memDC, oldBmp); Native.DeleteDC(memDC); memDC = IntPtr.Zero; }
            if (dib != IntPtr.Zero) { Native.DeleteObject(dib); dib = IntPtr.Zero; }
        }

        void Render(Graphics g)
        {
            g.Clear(Color.Transparent);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            using (var sf = new StringFormat(StringFormat.GenericTypographic) { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap })
            {
                foreach (var it in items)
                {
                    if (it.K == Kind.Footer) DrawFooter(g, it, sf);
                    else if (it.K == Kind.Note) Str(g, it.Text, fText, T.Sub, new RectangleF(S(M), it.Y, Width - S(2 * M), it.H), sf);
                    else if (it.K == Kind.Sep) using (var b = new SolidBrush(T.Sep)) g.FillRectangle(b, S(M), it.Y + it.H / 2, Width - S(2 * M), Math.Max(1, S(1)));
                    else DrawRow(g, it, sf);
                }
            }
        }

        // Отладка: отрисовать окно поверх заданного фона без показа на экране
        public void RenderTo(string path, Color bg)
        {
            RefreshContent(true);
            using (var bmp = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb))
            {
                using (var g = Graphics.FromImage(bmp)) Render(g);
                using (var outp = new Bitmap(Width, Height))
                {
                    using (var g = Graphics.FromImage(outp)) { g.Clear(bg); g.DrawImage(bmp, 0, 0); }
                    outp.Save(path);
                }
            }
        }

        static void Str(Graphics g, string s, Font f, Color c, RectangleF r, StringFormat sf) { GdiText.Draw(g, s, f, c, r, sf.Alignment); }

        // Нижняя панель: подпись и кнопки (микшер Windows, параметры звука)
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
            Str(g, it.Text, fText, tc, TitleRect(it), sf);
            using (var rf = new StringFormat(sf) { Alignment = StringAlignment.Far })
                Str(g, it.Muted ? "—" : it.Value.ToString(), fValue, it.Muted ? T.Sub : T.Fg, PctRect(it), rf);

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

        static GraphicsPath Round(RectangleF r, float rad)
        {
            var p = new GraphicsPath(); float d = rad * 2;
            p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure(); return p;
        }

        // ── Мышь ──
        Item RowAt(Point p) { foreach (var it in items) if ((it.K == Kind.Row || it.K == Kind.Footer) && p.Y >= it.Y && p.Y < it.Y + it.H) return it; return null; }

        int PartAt(Item it, Point p)
        {
            if (it == null) return 0;
            if (it.K == Kind.Footer)
            {
                for (int i = 0; i < FooterUris.Length; i++) if (FooterBtn(it, i).Contains(p)) return 4 + i;
                return 0;
            }
            if (IconRect(it).Contains(p)) return 1;
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
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e) { if (dragItem == null) { hoverItem = null; hoverPart = 0; Invalidate(); } }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            var it = RowAt(e.Location); int part = PartAt(it, e.Location);
            if (e.Button == MouseButtons.Right && it != null && it.K == Kind.Row)
            {
                // Правый клик по строке — скрыть её (вернуть можно в меню значка в трее)
                var row = it;
                new ContextMenu(new[] { new MenuItem("Скрыть «" + row.Text + "»", delegate { Hidden.Hide(row.Key, row.Text); RefreshContent(true); }) })
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

        protected override void OnMouseUp(MouseEventArgs e) { if (dragItem != null) { dragItem = null; Capture = false; Invalidate(); } }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            var it = RowAt(e.Location);
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
            using (var k = Registry.CurrentUser.CreateSubKey(RegKey)) k.SetValue("Hidden", l.ToArray(), RegistryValueKind.MultiString);
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
        public int OnDefaultDeviceChanged(int flow, int role, string id) { if (flow == 0 && role == 1) Post(DevicesChanged); return 0; }
        public int OnPropertyValueChanged(string id, PROPERTYKEY key) { return 0; }
        public int OnNotify(IntPtr data) { Post(VolumeChanged); return 0; }
    }

    // ───────────────────────── Трей ─────────────────────────
    class TrayApp
    {
        const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run", AppName = "TrayMixer", CfgKey = @"Software\TrayMixer";
        readonly NotifyIcon icon = new NotifyIcon();
        readonly Popup popup;
        readonly Notifier notifier;
        IAudioEndpointVolume defVol;
        string lastGlyph; bool lastLight;
        readonly MenuItem autoItem, micaItem, acrylicItem;

        public TrayApp(float scale)
        {
            popup = new Popup(Theme.RegInt(CfgKey, "Backdrop", Popup.Mica), scale);
            popup.CreateControl(); var h = popup.Handle;

            try { Native.SetPreferredAppMode(1); Native.FlushMenuThemes(); } catch { } // тёмное контекстное меню
            micaItem = new MenuItem("Фон: Mica", delegate { SetBackdrop(Popup.Mica); }) { RadioCheck = true };
            acrylicItem = new MenuItem("Фон: Acrylic", delegate { SetBackdrop(Popup.Acrylic); }) { RadioCheck = true };
            autoItem = new MenuItem("Запускать вместе с Windows", delegate { ToggleAutostart(); }) { Checked = IsAutostart() };
            var hiddenItem = new MenuItem("Скрытые", new[] { new MenuItem("-") });
            hiddenItem.Popup += delegate { FillHidden(hiddenItem); };
            icon.ContextMenu = new ContextMenu(new[]
            {
                new MenuItem("Параметры звука", delegate { Open("ms-settings:sound"); }),
                new MenuItem("Микшер Windows", delegate { Open("ms-settings:apps-volume"); }),
                new MenuItem("-"), hiddenItem,
                new MenuItem("-"), micaItem, acrylicItem,
                new MenuItem("-"), autoItem,
                new MenuItem("Выход", delegate { icon.Visible = false; Application.Exit(); })
            });
            icon.MouseClick += OnTrayClick;
            UpdateChecks();

            notifier = new Notifier(popup)
            {
                VolumeChanged = () => { UpdateTray(); popup.SyncValues(); },
                DevicesChanged = () => { Rebind(); if (popup.Visible) popup.RefreshContent(true); }
            };
            try { Audio.En.RegisterEndpointNotificationCallback(notifier); } catch { }
            Rebind();
            icon.Visible = true;
            Native.Trim();
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
            menu.MenuItems.Clear();
            if (Hidden.Items.Count == 0) { menu.MenuItems.Add(new MenuItem("Нет скрытых") { Enabled = false }); return; }
            foreach (var kv in new Dictionary<string, string>(Hidden.Items))
            {
                string key = kv.Key;
                menu.MenuItems.Add(new MenuItem("Показать «" + kv.Value + "»", delegate { Hidden.Show(key); }));
            }
            menu.MenuItems.Add(new MenuItem("-"));
            menu.MenuItems.Add(new MenuItem("Показать все", delegate { Hidden.ShowAll(); }));
        }

        void SetBackdrop(int b)
        {
            popup.Backdrop = b;
            using (var k = Registry.CurrentUser.CreateSubKey(CfgKey)) k.SetValue("Backdrop", b);
            UpdateChecks();
        }

        void UpdateChecks() { micaItem.Checked = popup.Backdrop == Popup.Mica; acrylicItem.Checked = popup.Backdrop == Popup.Acrylic; }

        void OnTrayClick(object s, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (popup.Visible) popup.HidePopup();
                else if ((DateTime.Now - popup.LastHide).TotalMilliseconds > 300) popup.ShowPopup();
            }
            else if (e.Button == MouseButtons.Middle && defVol != null)
            {
                try { bool m; defVol.GetMute(out m); defVol.SetMute(!m, ref Audio.Ctx); } catch { }
            }
        }

        void UpdateTray()
        {
            string glyph = "off", text = "Нет устройства вывода";
            try
            {
                if (defVol != null)
                {
                    float l; bool m; defVol.GetMasterVolumeLevelScalar(out l); defVol.GetMute(out m);
                    int p = (int)Math.Round(l * 100);
                    glyph = m ? "off" : "on";
                    text = "Громкость: " + (m ? "выкл." : p + "%");
                }
            }
            catch { }
            bool light = Theme.TaskbarLight();
            if (glyph != lastGlyph || light != lastLight)
            {
                var old = icon.Icon; icon.Icon = MakeIcon(glyph == "off", light); if (old != null) old.Dispose();
                lastGlyph = glyph; lastLight = light;
            }
            icon.Text = text;
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

        public static bool IsAutostart()
        {
            using (var k = Registry.CurrentUser.OpenSubKey(RunKey)) return k != null && k.GetValue(AppName) != null;
        }

        public static void SetAutostart(bool on)
        {
            using (var k = Registry.CurrentUser.CreateSubKey(RunKey))
            {
                if (on) k.SetValue(AppName, "\"" + Application.ExecutablePath + "\"");
                else k.DeleteValue(AppName, false);
            }
        }

        void ToggleAutostart() { SetAutostart(!IsAutostart()); autoItem.Checked = IsAutostart(); }
    }

    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Защита от подмены DLL: системные библиотеки грузятся только из System32, не из папки с exe
            try { Native.SetDefaultDllDirectories(0x800); } catch { }
            Native.SetProcessDPIAware();
            float scale; using (var g = Graphics.FromHwnd(IntPtr.Zero)) scale = g.DpiX / 96f;
            if (args.Length > 1 && args[0] == "--make-icon" && args[1].EndsWith(".ico", StringComparison.OrdinalIgnoreCase)) { Art.WriteIco(args[1]); return; }
            if (args.Length > 1 && args[0] == "--render" && args[1].EndsWith(".png", StringComparison.OrdinalIgnoreCase)) { var p = new Popup(Popup.Mica, scale); p.CreateControl(); p.RenderTo(args[1], Color.FromArgb(32, 32, 32)); return; }
            bool created;
            using (var mtx = new Mutex(true, @"Local\TrayMixer_7c1f3e2a-5b0d-4f9e-9a61-2d8c4b7e1a90", out created))
            {
                if (!created) return;
                // При первом запуске включаем автозапуск; путь обновляется, если exe перенесли
                using (var k = Registry.CurrentUser.CreateSubKey(@"Software\TrayMixer"))
                    if (k.GetValue("Initialized") == null) { TrayApp.SetAutostart(true); k.SetValue("Initialized", 1); }
                if (TrayApp.IsAutostart()) TrayApp.SetAutostart(true);
                new TrayApp(scale);
                Application.Run();
            }
        }
    }
}
