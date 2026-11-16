using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TrayMixer;

internal static class TrayMenuTests
{
    [DllImport("user32.dll")] static extern IntPtr FindWindow(string cls, string title);
    [DllImport("user32.dll")] static extern bool EndMenu();
    [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr hwnd, out RECT rect);
    struct RECT { public int L, T, R, B; }
    [DllImport("dwmapi.dll")] static extern int DwmGetWindowAttribute(IntPtr hwnd, int attribute, out int value, int size);
    static void CheckMaterial(ToolStripDropDown menu)
    {
        int backdrop;
        bool enabled = Theme.Load().Transparency;
        int result = DwmGetWindowAttribute(menu.Handle, 38, out backdrop, 4);
        Check(result == 0 && backdrop == (enabled ? Popup.Mica : 1), "DWM backdrop on " + menu.GetType().Name);
        Console.WriteLine("PASS: DWM backdrop " + backdrop + " on " + menu.GetType().Name);
    }
    static double Luma(Color c)
    {
        double[] v = { c.R / 255.0, c.G / 255.0, c.B / 255.0 };
        for (int i = 0; i < 3; i++) v[i] = v[i] <= .04045 ? v[i] / 12.92 : Math.Pow((v[i] + .055) / 1.055, 2.4);
        return .2126 * v[0] + .7152 * v[1] + .0722 * v[2];
    }
    static double Contrast(Color a, Color b)
    {
        double x = Luma(a), y = Luma(b);
        return (Math.Max(x, y) + .05) / (Math.Min(x, y) + .05);
    }
    static void CheckContrast()
    {
        var t = Theme.Load();
        Check(t.Transparency == (!SystemInformation.HighContrast && Theme.RegInt(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency", 1) != 0), "Respect Windows transparency preference");
        Check(Contrast(t.Sub, t.Solid) >= 4.5, "Secondary text contrast on solid background");
        Check(t.Surface(false).A == 255 && t.Surface(true).A == (t.Transparency ? 230 : 255), "Slightly translucent background only");
        if (!SystemInformation.HighContrast)
            Check(!t.Light && t.Solid.ToArgb() == Color.FromArgb(32, 32, 32).ToArgb(), "Neutral charcoal in either Windows theme");
        using (var image = new Bitmap(40, 40))
        using (var graphics = Graphics.FromImage(image))
        {
            foreach (int offset in new[] { 0, 200 })
            {
                graphics.Clear(Color.Magenta);
                WallpaperGlass.Paint(graphics, new Rectangle(0, 0, 40, 40), new Rectangle(0, 0, 1920, 1080), new Rectangle(offset, offset, 40, 40), t);
                Check(image.GetPixel(0, 0).ToArgb() == t.Solid.ToArgb() && image.GetPixel(39, 39).ToArgb() == t.Solid.ToArgb(), "Missing wallpaper must use solid fallback");
            }
        }
        var field = typeof(WallpaperGlass).GetField("wallpaper", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        var clear = typeof(WallpaperGlass).GetMethod("ClearCache", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        Color first = Color.Empty;
        try
        {
            foreach (var background in new[] { Color.Black, Color.White })
            using (var wallpaper = new Bitmap(80, 80))
            using (var image = new Bitmap(40, 40))
            using (var graphics = Graphics.FromImage(image))
            {
                using (var g = Graphics.FromImage(wallpaper)) g.Clear(background);
                field.SetValue(null, wallpaper);
                clear.Invoke(null, null);
                WallpaperGlass.Paint(graphics, new Rectangle(0, 0, 40, 40), new Rectangle(0, 0, 80, 80), new Rectangle(0, 0, 40, 40), t);
                var pixel = image.GetPixel(20, 20);
                Check(pixel.A == 255 && Contrast(t.Sub, pixel) >= 4.5 && Contrast(t.Track, pixel) >= 4.5, "Readable labels and tracks on black/white wallpaper");
                if (!SystemInformation.HighContrast) Check(pixel.R >= 28 && pixel.R <= 55, "Background stays dark gray on extreme wallpapers");
                if (first.IsEmpty) first = pixel;
                else Check(t.Transparency ? pixel.ToArgb() != first.ToArgb() : pixel.ToArgb() == first.ToArgb(), "Wallpaper influence follows transparency preference");
                field.SetValue(null, null);
            }
        }
        finally { field.SetValue(null, null); clear.Invoke(null, null); }
        Console.WriteLine("PASS: subtle charcoal transparency, solid fallback, black/white wallpaper contrast >= 4.5:1");
    }
    static int clicks;
    static void Check(bool ok, string message) { if (!ok) throw new Exception(message); }
    [STAThread]
    static int Main(string[] args)
    {
        try
        {
            CheckContrast();
            Native.SetProcessDPIAware();
            try { Native.SetPreferredAppMode(1); Native.FlushMenuThemes(); } catch { }
            var mica = new MenuItem("Background: Mica", delegate { clicks++; }) { RadioCheck = true, Checked = true };
            var acrylic = new MenuItem("Background: Acrylic", delegate { clicks++; }) { RadioCheck = true };
            var startup = new MenuItem("Run at Windows startup", delegate { clicks++; }) { Checked = true };
            var hidden = new MenuItem("Hidden", new[] { new MenuItem("No hidden items") { Enabled = false } });
            using (var menu = TrayApp.CreateTrayMenu("TrayMixer 3.0.0", hidden, mica, acrylic, startup,
                () => clicks++, () => clicks++, () => clicks++))
            {
                Check(menu.MenuItems.Count == 8, "Expected 8 rows including separators");
                Check(menu.MenuItems[0].Text == "Sound settings", "Sound action first");
                Check(menu.MenuItems[1].Text == "Windows volume mixer", "Mixer action missing");
                Check(menu.MenuItems[3] == hidden, "Hidden submenu must be preserved");
                var settings = menu.MenuItems[4];
                Check(settings.Text == "Settings" && settings.MenuItems.Count == 4, "Settings grouping");
                Check(settings.MenuItems[0] == mica && settings.MenuItems[1] == acrylic && settings.MenuItems[3] == startup, "Existing preference handlers must be preserved");
                Check(mica.Checked && !acrylic.Checked && startup.Checked, "Preference state");
                Check(!menu.MenuItems[6].Enabled && menu.MenuItems[6].Text == "TrayMixer 3.0.0", "Version footer");
                menu.MenuItems[0].PerformClick(); menu.MenuItems[1].PerformClick(); menu.MenuItems[7].PerformClick();
                mica.PerformClick(); acrylic.PerformClick(); startup.PerformClick();
                Check(clicks == 6, "Action dispatch");
                Console.WriteLine("PASS: structure, preference state, preserved submenus, all 6 action handlers");
                if (args.Length > 0)
                {
                    using (var flyout = new TrayFlyout(menu, 1.25f))
                    using (var host = new Form { ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, FormBorderStyle = FormBorderStyle.None, Bounds = Screen.FromPoint(Cursor.Position).WorkingArea })
                    using (var timer = new Timer { Interval = 500 })
                    {
                        Exception captureError = null;
                        bool captured = false;
                        var captureClock = System.Diagnostics.Stopwatch.StartNew();
                        host.Paint += delegate(object sender, PaintEventArgs e) {
                            using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(host.ClientRectangle, Color.White, Color.Magenta, 0f))
                                e.Graphics.FillRectangle(brush, host.ClientRectangle);
                            for (int x = 0; x < host.Width; x += 80)
                                using (var brush = new SolidBrush(x % 160 == 0 ? Color.Cyan : Color.Yellow))
                                    e.Graphics.FillRectangle(brush, x, host.Height - 400, 40, 400);
                        };
                        host.Shown += delegate { host.Refresh(); flyout.ShowFlyout(); };
                        timer.Tick += delegate {
                            if (WallpaperGlass.Status == "not loaded" && captureClock.ElapsedMilliseconds < 5000) return;
                            timer.Stop();
                            try {
                                RECT r;
                                Check(WallpaperGlass.Paints > 0, "Wallpaper material must actually paint");
                                Console.WriteLine("PASS: wallpaper material painted; " + WallpaperGlass.Status);
                                CheckMaterial(flyout);
                                Check(GetWindowRect(flyout.Handle, out r), "Themed menu not visible");
                                using (var bmp = new Bitmap(r.R - r.L, r.B - r.T)) {
                                    using (var g = Graphics.FromImage(bmp)) g.CopyFromScreen(r.L, r.T, 0, 0, bmp.Size);
                                    bmp.Save(args[0]);
                                    captured = true;
                                }
                                var work = Screen.FromControl(flyout).WorkingArea;
                                Check(r.B == work.Bottom - 15, "Expected 12 DIP taskbar gap");
                                var settingsRow = (ToolStripMenuItem)flyout.Items[4];
                                settingsRow.ShowDropDown(); Application.DoEvents();
                                Check(settingsRow.DropDown.Visible, "Settings submenu opens");
                                Check(((ToolStripMenuItem)settingsRow.DropDownItems[0]).Checked, "Mica selection visible");
                                Check(((ToolStripMenuItem)settingsRow.DropDownItems[3]).Checked, "Startup selection visible");
                                using (var sub = new Bitmap(settingsRow.DropDown.Width, settingsRow.DropDown.Height)) {
                                    CheckMaterial(settingsRow.DropDown);
                                    using (var g = Graphics.FromImage(sub)) g.CopyFromScreen(settingsRow.DropDown.Location, Point.Empty, sub.Size);
                                    sub.Save(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(args[0]), "tray-settings.png"));
                                }
                                settingsRow.DropDownItems[3].PerformClick();
                                Check(clicks == 7, "Themed menu dispatch");
                                settingsRow.HideDropDown();
                                Check(FlyoutPlacement.Bounds(new Rectangle(0,0,1920,1080), new Rectangle(0,0,1920,1032), new Size(260,220), 12).Bottom == 1020, "Bottom taskbar placement");
                                Check(FlyoutPlacement.Bounds(new Rectangle(-1920,0,1920,1080), new Rectangle(-1920,48,1920,1032), new Size(260,220), 12).Top == 60, "Top taskbar and negative monitor placement");
                                Console.WriteLine("PASS: themed menus, checks, dispatch, placement and capture " + (r.R - r.L) + "x" + (r.B - r.T));
                            } catch (Exception ex) { captureError = ex; }
                            flyout.Close(); host.Close();
                        };
                        timer.Start(); Application.Run(host);
                        if (captureError != null) throw captureError;
                        Check(captured, "Menu dismissed before screenshot; rerun without interacting with the desktop");
                    }
                }
            }
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
}
