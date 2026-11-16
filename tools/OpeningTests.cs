using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using TrayMixer;

internal static class OpeningTests
{
    static void Check(bool ok, string text) { if (!ok) throw new Exception(text); }
    [STAThread]
    static int Main(string[] args)
    {
        try
        {
            Native.SetProcessDPIAware();
            var timer = Stopwatch.StartNew();
            WallpaperGlass.Refresh();
            Console.WriteLine("Wallpaper load: " + timer.ElapsedMilliseconds + " ms");
            var bounds = Screen.PrimaryScreen.Bounds;
            using (var bmp = new Bitmap(450, 500))
            using (var g = Graphics.FromImage(bmp))
            {
                timer.Restart();
                for (int i = 0; i < 100; i++) WallpaperGlass.Paint(g, new Rectangle(0, 0, 450, 500), bounds, new Rectangle(bounds.Right - 470, bounds.Bottom - 550, 450, 500), Theme.Load());
                Console.WriteLine("100 wallpaper paints: " + timer.ElapsedMilliseconds + " ms");
                int cachedSurfaces = WallpaperGlass.SurfaceBuilds;
                WallpaperGlass.Paint(g, new Rectangle(0, 0, 450, 500), bounds, new Rectangle(bounds.Right - 470, bounds.Bottom - 550, 450, 500), Theme.Load());
                Check(WallpaperGlass.SurfaceBuilds == cachedSurfaces, "Unchanged crop must reuse final tinted surface");
                int builds = WallpaperGlass.CacheBuilds;
                WallpaperGlass.Refresh();
                WallpaperGlass.Paint(g, new Rectangle(0, 0, 450, 500), bounds, new Rectangle(10, 10, 450, 500), Theme.Load());
                Check(WallpaperGlass.CacheBuilds == builds, "Same wallpaper must reuse prepared blur");
                using (var font = new Font("Segoe UI", 14, GraphicsUnit.Pixel))
                {
                    int before = GdiText.Rasterizations;
                    for (int i = 0; i < 100; i++) GdiText.Draw(g, "Volume", font, Color.White, new RectangleF(0, i % 10, 100, 24), StringAlignment.Near);
                    Check(GdiText.Rasterizations == before + 1, "Text must rasterize once regardless of position");
                }
                Check(Popup.EaseOut(0) == 0 && Popup.EaseOut(1) == 1 && Popup.EaseOut(.5) > .5, "Ease-out endpoints and curve");
                Console.WriteLine("PASS: wallpaper cache reuse, text cache reuse and easing");
            }
            using (var probe = new Popup(Popup.Mica, 1f))
            {
                int writes = 0; float last = -1;
                var row = new Item { Value = 0, SetVol = v => { writes++; last = v; } };
                var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
                var apply = typeof(Popup).GetMethod("Apply", flags);
                var flush = typeof(Popup).GetMethod("FlushVolume", flags);
                for (int i = 1; i <= 100; i++) apply.Invoke(probe, new object[] { row, i });
                Check(row.Value == 100 && writes == 0, "Input must update immediately without synchronous audio writes");
                flush.Invoke(probe, null);
                Check(writes == 1 && last == 1f, "Burst must apply only final requested value");
                row.Muted = true; row.SetMute = m => row.Muted = m;
                apply.Invoke(probe, new object[] { row, 50 });
                flush.Invoke(probe, null);
                Check(writes == 2 && last == .5f && !row.Muted, "Final write must preserve auto-unmute");
                probe.CreateControl(); var handle = probe.Handle;
                int callbacks = 0;
                var notifier = new Notifier(probe) { VolumeChanged = () => callbacks++ };
                for (int i = 0; i < 100; i++) notifier.OnNotify(IntPtr.Zero);
                Application.DoEvents();
                Check(callbacks == 1, "Notification burst must queue one reconciliation");
                Console.WriteLine("PASS: immediate slider state, coalesced final audio write, auto-unmute and notification burst");
            }
            using (var popup = new Popup(Popup.Mica, 1f))
            {
                popup.CreateControl();
                timer.Restart(); popup.ShowPopup();
                Console.WriteLine("First ShowPopup return: " + timer.ElapsedMilliseconds + " ms");
                var expected = FlyoutPlacement.Bounds(Screen.FromControl(popup).Bounds, Screen.FromControl(popup).WorkingArea, popup.Size, 12);
                for (int i = 0; i < 35; i++) { Application.DoEvents(); Thread.Sleep(10); }
                Check(popup.Visible, "Popup must remain visible");
                expected = FlyoutPlacement.Bounds(Screen.FromControl(popup).Bounds, Screen.FromControl(popup).WorkingArea, popup.Size, 12);
                Check(popup.Bounds == expected, "Animation must finish at taskbar gap: " + popup.Bounds + " expected " + expected);
                using (var image = new Bitmap(popup.Width, popup.Height))
                {
                    using (var graphics = Graphics.FromImage(image)) graphics.CopyFromScreen(popup.Location, Point.Empty, image.Size);
                    image.Save("bin/menu-tests/opening-final.png");
                }
                timer.Restart(); popup.RefreshContent(false);
                Check(timer.ElapsedMilliseconds < 50, "Periodic discovery must return without blocking the UI");
                for (int i = 0; i < 60; i++) { Application.DoEvents(); Thread.Sleep(10); }
                Check(popup.Visible && popup.Bounds == expected, "Background refresh must preserve placement and visibility");
                popup.HidePopup(); Check(!popup.Visible, "Hide must cancel animation");
                timer.Restart(); popup.ShowPopup();
                Console.WriteLine("Repeat ShowPopup return: " + timer.ElapsedMilliseconds + " ms");
                popup.HidePopup();
                for (int i = 0; i < 20; i++) { Application.DoEvents(); Thread.Sleep(10); }
                Check(!popup.Visible, "Cancelled animation must not reshow popup");
            }
            Console.WriteLine("PASS: popup open, final placement, hide and rapid reopen");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
}
