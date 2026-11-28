using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TrayMixer
{
    sealed class FrostedWallpaper : IDisposable
    {
        [ComImport, Guid("B92B56A9-8B55-4E14-9A89-0199BBB6F93B"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IDesktopWallpaper
        {
            void SetWallpaper([MarshalAs(UnmanagedType.LPWStr)] string monitor, [MarshalAs(UnmanagedType.LPWStr)] string path);
            void GetWallpaper([MarshalAs(UnmanagedType.LPWStr)] string monitor, [MarshalAs(UnmanagedType.LPWStr)] out string path);
            void GetMonitorDevicePathAt(uint index, [MarshalAs(UnmanagedType.LPWStr)] out string monitor);
            void GetMonitorDevicePathCount(out uint count);
            [PreserveSig] int GetMonitorRECT([MarshalAs(UnmanagedType.LPWStr)] string monitor, out MonitorRect rect);
            void SetBackgroundColor(uint color);
            void GetBackgroundColor(out uint color);
            void SetPosition(WallpaperPosition position);
            void GetPosition(out WallpaperPosition position);
        }

        enum WallpaperPosition { Center, Tile, Stretch, Fit, Fill, Span }

        [StructLayout(LayoutKind.Sequential)]
        struct MonitorRect { public int Left, Top, Right, Bottom; }

        static readonly Guid DesktopWallpaperClass = new Guid("C2CF3110-460E-4FC1-B9D0-8A1C0C9CC4BD");
        const float BlurAmount = 20f;
        const int TintAlpha = 38; // FrostyGlass: 15% black.
        Bitmap glass;
        Rectangle capture;
        Rectangle area;

        public static bool Enabled(int backdrop)
        {
            return (backdrop == Popup.FrostedGlass || backdrop == Popup.Acrylic || backdrop == Popup.Mica)
                && Theme.TransparencyEnabled() && !SystemInformation.HighContrast;
        }

        public static Color BaseColor(Theme theme, int backdrop)
        {
            if (backdrop == Popup.Acrylic) return theme.Light ? Color.FromArgb(249, 249, 249) : Color.FromArgb(38, 38, 38);
            if (backdrop == Popup.FrostedGlass && !theme.Light) return Color.FromArgb(19, 12, 12);
            return theme.Light ? Color.FromArgb(243, 243, 243) : Color.FromArgb(32, 32, 32);
        }

        void Refresh(Rectangle screen, Rectangle window, float scale)
        {
            Dispose();
            area = window;
            if (window.Width <= 0 || window.Height <= 0) return;
            IDesktopWallpaper desktop = null;
            try
            {
                desktop = (IDesktopWallpaper)Activator.CreateInstance(Type.GetTypeFromCLSID(DesktopWallpaperClass));
                uint background;
                WallpaperPosition position;
                desktop.GetBackgroundColor(out background);
                desktop.GetPosition(out position);
                string path = GetPath(desktop, screen);
                if (position == WallpaperPosition.Span) screen = SystemInformation.VirtualScreen;
                var color = Color.FromArgb((int)(background & 255), (int)((background >> 8) & 255), (int)((background >> 16) & 255));

                // Three box passes per axis approximate a Gaussian at half resolution.
                int radius = Math.Max(1, (int)Math.Round(BlurAmount * scale / 2f - 0.5f));
                capture = Rectangle.Inflate(window, 6 * radius, 6 * radius);
                var image = new Bitmap((capture.Width + 1) / 2, (capture.Height + 1) / 2, PixelFormat.Format32bppRgb);
                try
                {
                    using (var g = Graphics.FromImage(image))
                    {
                        g.Clear(color);
                        g.ScaleTransform(0.5f, 0.5f);
                        g.TranslateTransform(-capture.X, -capture.Y);
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        if (!string.IsNullOrEmpty(path))
                            using (var wallpaper = LoadWallpaper(path))
                                if (wallpaper != null) DrawWallpaper(g, wallpaper, position, screen);
                    }
                    Blur(image, radius);
                    glass = image;
                }
                finally { if (glass == null) image.Dispose(); }
            }
            catch (COMException) { }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            catch (ArgumentException) { }
            catch (OutOfMemoryException) { }
            finally { if (desktop != null) Marshal.ReleaseComObject(desktop); }
        }

        static Image LoadWallpaper(string path)
        {
            var image = TryLoadImage(path);
            if (image != null) return image;

            // Windows may still report the original path after replacing a temporary wallpaper.
            var themes = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Microsoft\Windows\Themes");
            return TryLoadImage(Path.Combine(themes, "TranscodedWallpaper"));
        }

        static Image TryLoadImage(string path)
        {
            try { return Image.FromFile(path); }
            catch (IOException) { return null; }
            catch (UnauthorizedAccessException) { return null; }
            catch (ArgumentException) { return null; }
        }

        static string GetPath(IDesktopWallpaper desktop, Rectangle screen)
        {
            uint count;
            desktop.GetMonitorDevicePathCount(out count);
            var center = new Point(screen.X + screen.Width / 2, screen.Y + screen.Height / 2);
            for (uint i = 0; i < count; i++)
            {
                string monitor, path;
                MonitorRect rect;
                desktop.GetMonitorDevicePathAt(i, out monitor);
                if (desktop.GetMonitorRECT(monitor, out rect) != 0 ||
                    !Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom).Contains(center)) continue;
                desktop.GetWallpaper(monitor, out path);
                return path;
            }
            string primary;
            desktop.GetWallpaper(null, out primary);
            return primary;
        }

        static void DrawWallpaper(Graphics g, Image wallpaper, WallpaperPosition position, Rectangle screen)
        {
            if (position == WallpaperPosition.Tile)
            {
                using (var tiles = new TextureBrush(wallpaper, WrapMode.Tile))
                {
                    tiles.TranslateTransform(screen.X, screen.Y);
                    g.FillRectangle(tiles, screen);
                }
                return;
            }
            float xScale = (float)screen.Width / wallpaper.Width, yScale = (float)screen.Height / wallpaper.Height;
            float uniform = position == WallpaperPosition.Fit ? Math.Min(xScale, yScale) : Math.Max(xScale, yScale);
            if (position == WallpaperPosition.Center) uniform = 1;
            float width = position == WallpaperPosition.Stretch ? screen.Width : wallpaper.Width * uniform;
            float height = position == WallpaperPosition.Stretch ? screen.Height : wallpaper.Height * uniform;
            g.DrawImage(wallpaper, screen.X + (screen.Width - width) / 2, screen.Y + (screen.Height - height) / 2, width, height);
        }

        static void Blur(Bitmap image, int radius)
        {
            int w = image.Width, h = image.Height;
            var data = image.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, image.PixelFormat);
            try
            {
                var pixels = new int[w * h];
                var scratch = new int[pixels.Length];
                Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
                for (int pass = 0; pass < 3; pass++)
                {
                    for (int y = 0; y < h; y++) BlurLine(pixels, scratch, y * w, 1, w, radius);
                    for (int x = 0; x < w; x++) BlurLine(scratch, pixels, x, w, h, radius);
                }
                Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
            }
            finally { image.UnlockBits(data); }
        }

        static void BlurLine(int[] from, int[] to, int offset, int stride, int length, int radius)
        {
            int count = radius * 2 + 1, r = 0, g = 0, b = 0;
            for (int i = -radius; i <= radius; i++)
            {
                int pixel = from[offset + Math.Min(length - 1, Math.Max(0, i)) * stride];
                r += (pixel >> 16) & 255; g += (pixel >> 8) & 255; b += pixel & 255;
            }
            for (int i = 0; i < length; i++)
            {
                to[offset + i * stride] = ((r + radius) / count << 16) | ((g + radius) / count << 8) | ((b + radius) / count);
                int add = from[offset + Math.Min(length - 1, i + radius + 1) * stride];
                int remove = from[offset + Math.Max(0, i - radius) * stride];
                r += ((add >> 16) & 255) - ((remove >> 16) & 255);
                g += ((add >> 8) & 255) - ((remove >> 8) & 255);
                b += (add & 255) - (remove & 255);
            }
        }

        public void Paint(Graphics g, Control window, float scale)
        {
            Paint(g, window, scale, window.ClientRectangle, Color.FromArgb(TintAlpha, Color.Black));
        }

        public static Color SurfaceTint(Color baseColor, int backdrop)
        {
            if (backdrop == Popup.FrostedGlass) return Color.FromArgb(TintAlpha, Color.Black);
            return Color.FromArgb(backdrop == Popup.Mica ? 232 : 183, baseColor);
        }

        public void Paint(Graphics g, Control window, float scale, Rectangle region, Color tint)
        {
            var bounds = window.RectangleToScreen(window.ClientRectangle);
            if (area != bounds) Refresh(Screen.FromRectangle(bounds).Bounds, bounds, scale);
            if (glass == null || region.Width <= 0 || region.Height <= 0) return;
            var destination = new Rectangle(Point.Empty, region.Size);
            var source = new RectangleF((bounds.X + region.X - capture.X) / 2f, (bounds.Y + region.Y - capture.Y) / 2f,
                region.Width / 2f, region.Height / 2f);
            var state = g.Save();
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(glass, destination, source, GraphicsUnit.Pixel);
            using (var brush = new SolidBrush(tint)) g.FillRectangle(brush, destination);
            g.Restore(state);
        }

        public static void PaintOutline(Graphics g, Size size, float scale)
        {
            var outline = new RectangleF(0.5f, 0.5f, size.Width - 1, size.Height - 1);
            if (outline.Width <= 0 || outline.Height <= 0) return;
            var state = g.Save();
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = Popup.Round(outline, (float)Math.Round(10 * scale)))
            using (var border = new LinearGradientBrush(outline, Color.Gray, Color.Gray, LinearGradientMode.Vertical))
            {
                var light = Color.FromArgb(80, 128, 128, 128);
                border.InterpolationColors = new ColorBlend { Colors = new[] { light, Color.FromArgb(80, 64, 64, 64), light }, Positions = new[] { 0f, 0.25f, 1f } };
                using (var pen = new Pen(border, scale)) g.DrawPath(pen, path);
            }
            g.Restore(state);
        }

        public void Dispose()
        {
            area = Rectangle.Empty;
            if (glass != null) { glass.Dispose(); glass = null; }
        }
    }
}
