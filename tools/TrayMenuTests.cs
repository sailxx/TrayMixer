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
    static int clicks;
    static void Check(bool ok, string message) { if (!ok) throw new Exception(message); }
    [STAThread]
    static int Main(string[] args)
    {
        try
        {
            Native.SetProcessDPIAware();
            try { Native.SetPreferredAppMode(1); Native.FlushMenuThemes(); } catch { }
            var mica = new MenuItem("Background: Mica", delegate { clicks++; }) { RadioCheck = true, Checked = true };
            var acrylic = new MenuItem("Background: Acrylic", delegate { clicks++; }) { RadioCheck = true };
            var startup = new MenuItem("Run at Windows startup", delegate { clicks++; }) { Checked = true };
            var hidden = new MenuItem("Hidden", new[] { new MenuItem("No hidden items") { Enabled = false } });
            using (var menu = TrayApp.CreateTrayMenu("TrayMixer 2.9.0", hidden, mica, acrylic, startup,
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
                Check(!menu.MenuItems[6].Enabled && menu.MenuItems[6].Text == "TrayMixer 2.9.0", "Version footer");
                menu.MenuItems[0].PerformClick(); menu.MenuItems[1].PerformClick(); menu.MenuItems[7].PerformClick();
                mica.PerformClick(); acrylic.PerformClick(); startup.PerformClick();
                Check(clicks == 6, "Action dispatch");
                Console.WriteLine("PASS: structure, preference state, preserved submenus, all 6 action handlers");
                if (args.Length > 0)
                {
                    using (var flyout = new TrayFlyout(menu, 1.25f))
                    using (var host = new Form { ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(40, 40), Size = new Size(1, 1), Opacity = 0 })
                    using (var timer = new Timer { Interval = 500 })
                    {
                        Exception captureError = null;
                        bool captured = false;
                        host.Shown += delegate { flyout.ShowFlyout(); };
                        timer.Tick += delegate {
                            timer.Stop();
                            try {
                                RECT r;
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
                                    settingsRow.DropDown.DrawToBitmap(sub, new Rectangle(Point.Empty, sub.Size));
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
