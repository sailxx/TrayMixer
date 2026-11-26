using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Automation;
using System.Windows.Forms;

namespace TrayMixer
{
    sealed class BatteryStrip : NativeWindow, IDisposable
    {
        const int StripHeight = 23;

        internal struct Placement
        {
            public IntPtr Taskbar;
            public Rectangle Gap;
            public int Dpi;
        }

        IntPtr owner;
        Rectangle shownBounds;
        string shownValues;
        int shownDpi;
        bool shownLight;
        Bitmap image;

        internal static Placement FindPlacement()
        {
            var taskbar = FindWindow("Shell_TrayWnd", null);
            var tray = FindWindowEx(taskbar, IntPtr.Zero, "TrayNotifyWnd", null);
            Bounds barRect, trayRect;
            if (taskbar == IntPtr.Zero || tray == IntPtr.Zero || !IsWindowVisible(taskbar) ||
                !GetWindowRect(taskbar, out barRect) || !GetWindowRect(tray, out trayRect))
                return default(Placement);

            int dpi = (int)GetDpiForWindow(taskbar);
            var rect = barRect.Rectangle;
            if (dpi == 0 || rect.Width < rect.Height * 3 || rect.Height < Scale(StripHeight, dpi) ||
                !Screen.FromHandle(taskbar).Bounds.Contains(rect))
                return default(Placement);

            // The legacy task list window omits some Windows 11 buttons. Measure
            // the actual task buttons so the strip never needs their space.
            try
            {
                var frame = AutomationElement.FromHandle(taskbar).FindFirst(TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.AutomationIdProperty, "TaskbarFrame"));
                if (frame == null) return default(Placement);

                int left = rect.Left;
                var request = new CacheRequest { AutomationElementMode = AutomationElementMode.None };
                request.Add(AutomationElement.BoundingRectangleProperty);
                request.Add(AutomationElement.IsOffscreenProperty);
                using (request.Activate())
                {
                    var buttons = frame.FindAll(TreeScope.Descendants,
                        new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button));
                    if (buttons.Count == 0) return default(Placement);
                    foreach (AutomationElement button in buttons)
                    {
                        var bounds = button.Cached.BoundingRectangle;
                        if (!button.Cached.IsOffscreen && bounds.Width > 0 && bounds.X < trayRect.Left &&
                            bounds.Y < rect.Bottom && bounds.Bottom > rect.Top)
                            left = Math.Max(left, (int)Math.Ceiling(bounds.Right));
                    }
                }

                int padding = Scale(10, dpi);
                return new Placement
                {
                    Taskbar = taskbar,
                    Dpi = dpi,
                    Gap = Rectangle.FromLTRB(left + padding, rect.Top, trayRect.Left - padding, rect.Bottom)
                };
            }
            catch (ElementNotAvailableException) { return default(Placement); }
            catch (COMException) { return default(Placement); }
        }

        internal bool TryShow(Placement placement, IEnumerable<WirelessCharge> charges)
        {
            if (placement.Taskbar == IntPtr.Zero || placement.Gap.Width <= 0 || !IsWindow(placement.Taskbar))
                return false;

            var foreground = GetForegroundWindow();
            Bounds active;
            if (foreground != placement.Taskbar && foreground != IntPtr.Zero && foreground != GetShellWindow() &&
                GetWindowRect(foreground, out active) &&
                active.Rectangle.Contains(Screen.FromHandle(placement.Taskbar).Bounds))
                return false;

            var sorted = charges.OrderBy(c => c.Kind == WirelessKind.Other ? int.MaxValue : (int)c.Kind)
                .ThenBy(c => c.Name, StringComparer.CurrentCultureIgnoreCase).ThenBy(c => c.Percent).ToArray();
            string values = string.Join("|", sorted.Select(c => (int)c.Kind + " " + c.Percent));
            bool light = Theme.TaskbarLight();
            bool redraw = image == null || shownValues != values || shownDpi != placement.Dpi || shownLight != light;
            if (redraw)
            {
                if (image != null) image.Dispose();
                image = Render(sorted, placement.Dpi, light);
                shownValues = values;
                shownDpi = placement.Dpi;
                shownLight = light;
            }

            var gap = placement.Gap;
            if (image.Width > gap.Width || image.Height > gap.Height) return false;
            var bounds = new Rectangle(gap.Right - image.Width, gap.Top + (gap.Height - image.Height) / 2, image.Width, image.Height);
            if (Handle == IntPtr.Zero || owner != placement.Taskbar || !IsWindow(Handle))
            {
                if (Handle != IntPtr.Zero) DestroyHandle();
                owner = placement.Taskbar;
                // An owned, click-through popup follows the taskbar's stacking order.
                CreateHandle(new CreateParams
                {
                    Caption = "TrayMixer battery levels", Parent = owner, Style = unchecked((int)0x80000000),
                    ExStyle = 0x080800A0 // NOACTIVATE | LAYERED | TOOLWINDOW | TRANSPARENT
                });
                redraw = true;
            }

            if (redraw || shownBounds.IsEmpty)
            {
                if (!Publish(image, bounds)) return false;
                ShowWindow(Handle, 4); // SW_SHOWNOACTIVATE
            }
            else if (shownBounds != bounds && !SetWindowPos(Handle, IntPtr.Zero, bounds.X, bounds.Y, 0, 0, 0x15))
                return false;
            shownBounds = bounds;
            return true;
        }

        internal void Hide()
        {
            if (Handle != IntPtr.Zero && !shownBounds.IsEmpty) ShowWindow(Handle, 0);
            shownBounds = Rectangle.Empty;
        }

        public void Dispose()
        {
            if (Handle != IntPtr.Zero) DestroyHandle();
            if (image != null) image.Dispose();
        }

        internal static Bitmap Render(WirelessCharge[] charges, int dpi, bool light)
        {
            var ink = light ? Color.FromArgb(220, 38, 43, 49) : Color.FromArgb(240, 249, 250, 252);
            using (var font = new Font(Theme.FontSemi, Scale(14, dpi), GraphicsUnit.Pixel))
            using (var format = new StringFormat(StringFormat.GenericTypographic) { LineAlignment = StringAlignment.Center })
            using (var brush = new SolidBrush(ink))
            {
                var values = charges.Select(c => c.Percent.ToString(CultureInfo.InvariantCulture) + "%").ToArray();
                int iconWidth = Scale(11, dpi), pairGap = Scale(4, dpi), deviceGap = Scale(12, dpi);
                int height = Scale(StripHeight, dpi), inset = Scale(2, dpi);
                int[] widths;
                using (var sizing = new Bitmap(1, 1))
                using (var measure = Graphics.FromImage(sizing))
                    widths = values.Select(v => (int)Math.Ceiling(measure.MeasureString(v, font, PointF.Empty, format).Width) + 1).ToArray();

                int width = inset * 2 + values.Length * (iconWidth + pairGap) +
                    Math.Max(0, values.Length - 1) * deviceGap + widths.Sum();
                var bitmap = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    int x = inset;
                    for (int i = 0; i < values.Length; i++)
                    {
                        var state = g.Save();
                        g.TranslateTransform(x, (height - iconWidth) / 2f);
                        g.ScaleTransform(iconWidth / 18f, iconWidth / 18f);
                        DrawDevice(g, charges[i].Kind, ink);
                        g.Restore(state);
                        x += iconWidth + pairGap;
                        g.DrawString(values[i], font, brush, new RectangleF(x, 0, widths[i] + 2, height), format);
                        x += widths[i] + deviceGap;
                    }
                }
                return bitmap;
            }
        }

        static void DrawDevice(Graphics g, WirelessKind kind, Color ink)
        {
            using (var pen = new Pen(ink, 1.45f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
            using (var brush = new SolidBrush(ink))
            {
                switch (kind)
                {
                    case WirelessKind.Mouse:
                        using (var path = new GraphicsPath())
                        {
                            path.AddArc(3.5f, 1, 11, 9, 180, 180);
                            path.AddArc(3.5f, 8, 11, 9, 0, 180);
                            path.CloseFigure();
                            g.DrawPath(pen, path);
                        }
                        g.DrawLine(pen, 9, 3.5f, 9, 7);
                        break;
                    case WirelessKind.Keyboard:
                        using (var path = new GraphicsPath())
                        {
                            path.AddArc(0.8f, 3, 3, 3, 180, 90);
                            path.AddArc(14.2f, 3, 3, 3, 270, 90);
                            path.AddArc(14.2f, 12, 3, 3, 0, 90);
                            path.AddArc(0.8f, 12, 3, 3, 90, 90);
                            path.CloseFigure();
                            g.DrawPath(pen, path);
                        }
                        for (int x = 4; x <= 13; x += 3)
                            for (int y = 6; y <= 9; y += 3)
                                g.FillRectangle(brush, x, y, 1.4f, 1.3f);
                        g.DrawLine(pen, 6.5f, 12.3f, 11.5f, 12.3f);
                        break;
                    case WirelessKind.Headphones:
                        g.DrawArc(pen, 2.3f, 2, 13.4f, 13, 180, 180);
                        g.DrawLine(pen, 2.3f, 8.3f, 2.3f, 12);
                        g.DrawLine(pen, 15.7f, 8.3f, 15.7f, 12);
                        g.DrawRectangle(pen, 2.3f, 9.5f, 3.5f, 5.5f);
                        g.DrawRectangle(pen, 12.2f, 9.5f, 3.5f, 5.5f);
                        break;
                    default:
                        g.DrawRectangle(pen, 1, 4.5f, 13.7f, 9);
                        g.DrawLine(pen, 17, 7.3f, 17, 10.7f);
                        g.FillRectangle(brush, 3.5f, 7, 7, 4);
                        break;
                }
            }
        }

        static int Scale(int pixels, int dpi) { return (pixels * dpi + 48) / 96; }

        bool Publish(Bitmap bitmap, Rectangle bounds)
        {
            var bi = new Native.BITMAPINFOHEADER
            {
                biSize = 40, biWidth = bitmap.Width, biHeight = -bitmap.Height, biPlanes = 1, biBitCount = 32
            };
            IntPtr bits;
            IntPtr dib = Native.CreateDIBSection(IntPtr.Zero, ref bi, 0, out bits, IntPtr.Zero, 0);
            if (dib == IntPtr.Zero) return false;
            IntPtr dc = Native.CreateCompatibleDC(IntPtr.Zero);
            IntPtr old = IntPtr.Zero;
            try
            {
                if (dc == IntPtr.Zero) return false;
                old = Native.SelectObject(dc, dib);
                if (old == IntPtr.Zero || old == new IntPtr(-1)) return false;
                using (var surface = new Bitmap(bitmap.Width, bitmap.Height, bitmap.Width * 4, PixelFormat.Format32bppPArgb, bits))
                using (var g = Graphics.FromImage(surface))
                {
                    g.CompositingMode = CompositingMode.SourceCopy;
                    g.DrawImageUnscaled(bitmap, 0, 0);
                }
                var point = bounds.Location;
                var origin = Point.Empty;
                var size = bounds.Size;
                var blend = new Blend { Opacity = 255, PerPixelAlpha = 1 };
                return UpdateLayeredWindow(Handle, IntPtr.Zero, ref point, ref size, dc, ref origin, 0, ref blend, 2);
            }
            finally
            {
                if (old != IntPtr.Zero && old != new IntPtr(-1)) Native.SelectObject(dc, old);
                if (dc != IntPtr.Zero) Native.DeleteDC(dc);
                Native.DeleteObject(dib);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct Bounds
        {
            public int Left, Top, Right, Bottom;
            public Rectangle Rectangle { get { return Rectangle.FromLTRB(Left, Top, Right, Bottom); } }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct Blend { public byte Operation, Flags, Opacity, PerPixelAlpha; }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern IntPtr FindWindow(string className, string title);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern IntPtr FindWindowEx(IntPtr parent, IntPtr previous, string className, string title);
        [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr window, out Bounds rect);
        [DllImport("user32.dll")] static extern bool IsWindow(IntPtr window);
        [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr window);
        [DllImport("user32.dll")] static extern uint GetDpiForWindow(IntPtr window);
        [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] static extern IntPtr GetShellWindow();
        [DllImport("user32.dll")] static extern bool ShowWindow(IntPtr window, int command);
        [DllImport("user32.dll")] static extern bool SetWindowPos(IntPtr window, IntPtr after, int x, int y, int width, int height, uint flags);
        [DllImport("user32.dll")] static extern bool UpdateLayeredWindow(IntPtr window, IntPtr screenDc, ref Point position,
            ref Size size, IntPtr sourceDc, ref Point origin, uint key, ref Blend blend, uint flags);
    }
}
