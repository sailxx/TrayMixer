using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TrayMixer
{
    enum WirelessKind { Other, Mouse, Keyboard, Headphones }

    sealed class WirelessCharge
    {
        public readonly string Name;
        public readonly WirelessKind Kind;
        public readonly int Percent;

        public WirelessCharge(string name, WirelessKind kind, int percent)
        {
            Name = name;
            Kind = kind;
            Percent = percent;
        }
    }

    sealed class BatteryTray : IDisposable
    {
        sealed class Indicator
        {
            public readonly TrayIcon Icon;
            public WirelessCharge Charge;

            public Indicator(Guid id) { Icon = new TrayIcon(null, id); }
        }

        static readonly byte[] IconNamespace = new Guid("90bd5df4-f228-4f08-8b4d-1cb3d83c652a").ToByteArray();
        readonly Dictionary<Guid, Indicator> icons = new Dictionary<Guid, Indicator>();
        readonly Timer timer = new Timer { Interval = 5000 };
        readonly Timer placementTimer = new Timer { Interval = 1000 };
        readonly BatteryStrip strip = new BatteryStrip();
        Dictionary<Guid, WirelessCharge> charges = new Dictionary<Guid, WirelessCharge>();
        BatteryStrip.Placement placement;
        bool locating;
        bool scanning, disposed, light;
        int size;

        public bool Enabled { get; private set; }

        public BatteryTray(bool enabled)
        {
            timer.Tick += delegate { Refresh(); };
            placementTimer.Tick += delegate { UpdatePlacement(); };
            SetEnabled(enabled);
        }

        public void SetEnabled(bool enabled)
        {
            if (Enabled == enabled) return;
            Enabled = enabled;
            timer.Enabled = enabled;
            if (enabled)
            {
                Refresh();
                return;
            }

            placementTimer.Stop();
            charges.Clear();
            placement = default(BatteryStrip.Placement);
            strip.Hide();
            ClearIcons();
        }

        async void Refresh()
        {
            if (scanning || !Enabled) return;
            scanning = true;
            try
            {
                var next = await Task.Run(() => HeadsetBattery.ReadConnectedAsync());
                if (disposed || !Enabled) return;
                bool wasEmpty = charges.Count == 0;
                charges = next;
                placementTimer.Enabled = charges.Count != 0;
                if (charges.Count == 0) placement = default(BatteryStrip.Placement);
                ApplyPlacement();
                if (wasEmpty && charges.Count != 0) UpdatePlacement();
            }
            catch (Exception e) { Debug.WriteLine(e); }
            finally { scanning = false; }
        }

        async void UpdatePlacement()
        {
            if (locating || !Enabled) return;
            locating = true;
            try
            {
                placement = await Task.Run(() => BatteryStrip.FindPlacement());
                if (!disposed && Enabled && charges.Count != 0) ApplyPlacement();
            }
            catch (Exception e) { Debug.WriteLine(e); }
            finally { locating = false; }
        }

        void ApplyPlacement()
        {
            if (charges.Count != 0 && strip.TryShow(placement, charges.Values))
            {
                ClearIcons();
                return;
            }
            strip.Hide();
            ShowCharges(charges);
        }

        void ShowCharges(Dictionary<Guid, WirelessCharge> charges)
        {
            var removed = new List<Guid>();
            foreach (var pair in icons)
                if (!charges.ContainsKey(pair.Key)) removed.Add(pair.Key);
            foreach (var id in removed)
            {
                icons[id].Icon.Dispose();
                icons.Remove(id);
            }

            bool nextLight = Theme.TaskbarLight();
            int nextSize = SystemInformation.SmallIconSize.Width;
            bool redraw = nextLight != light || nextSize != size;
            foreach (var pair in charges)
            {
                var charge = pair.Value;
                Indicator indicator;
                if (!icons.TryGetValue(pair.Key, out indicator))
                {
                    // Keep Explorer's visibility preference for each device across reconnects.
                    var bytes = pair.Key.ToByteArray();
                    for (int i = 0; i < bytes.Length; i++) bytes[i] ^= IconNamespace[i];
                    indicator = new Indicator(new Guid(bytes));
                    icons.Add(pair.Key, indicator);
                }
                var previous = indicator.Charge;
                bool replace = redraw || previous == null || previous.Percent != charge.Percent || previous.Kind != charge.Kind;
                if (replace || previous.Name != charge.Name)
                {
                    var tooltip = charge.Percent + "%";
                    if (!string.IsNullOrWhiteSpace(charge.Name)) tooltip += " · " + charge.Name;
                    indicator.Icon.Update(replace ? DrawIcon(charge, nextSize, nextLight) : null, tooltip);
                    indicator.Charge = charge;
                }
            }
            light = nextLight;
            size = nextSize;
        }

        public void Dispose()
        {
            disposed = true;
            timer.Dispose();
            placementTimer.Dispose();
            strip.Dispose();
            ClearIcons();
        }

        void ClearIcons()
        {
            foreach (var indicator in icons.Values) indicator.Icon.Dispose();
            icons.Clear();
        }

        internal static Icon DrawIcon(WirelessCharge charge, int size, bool light)
        {
            Color ink = light ? Color.Black : Color.White;
            using (var bitmap = new Bitmap(size, size))
            {
                using (var g = Graphics.FromImage(bitmap))
                using (var brush = new SolidBrush(ink))
                {
                    var device = Devices[(int)charge.Kind];
                    DrawGlyph(g, brush, device, (16 - device[0].Length) / 2, 0, size);

                    // The tooltip carries the percent sign so all three digits can use a full-width glyph.
                    string value = charge.Percent.ToString(CultureInfo.InvariantCulture);
                    int x = (17 - value.Length * 5) / 2;
                    foreach (char digit in value)
                    {
                        DrawGlyph(g, brush, Digits[digit - '0'], x, 8, size);
                        x += 5;
                    }
                }
                IntPtr handle = bitmap.GetHicon();
                try { using (var icon = Icon.FromHandle(handle)) return (Icon)icon.Clone(); }
                finally { Native.DestroyIcon(handle); }
            }
        }

        // Pixel-aligned silhouettes stay crisp at the 16-pixel taskbar size.
        static readonly string[][] Devices =
        {
            new[] { "............", ".#########..", ".#.......##.", ".#.####..##.", ".#.......##.", ".#########.." },
            new[] { ".####.", "#.##.#", "#....#", "#....#", "#....#", ".####." },
            new[] { ".##########.", "#..........#", "#.##.##.##.#", "#..........#", "#..######..#", ".##########." },
            new[] { "...####...", "..#....#..", ".#......#.", "###....###", "###....###", ".##....##." }
        };

        static readonly string[][] Digits =
        {
            new[] { ".##.", "#..#", "#..#", "#..#", "#..#", "#..#", ".##." },
            new[] { "..#.", ".##.", "..#.", "..#.", "..#.", "..#.", ".###" },
            new[] { ".##.", "#..#", "...#", "..#.", ".#..", "#...", "####" },
            new[] { "###.", "...#", "...#", ".##.", "...#", "...#", "###." },
            new[] { "..#.", ".##.", "#.#.", "#.#.", "####", "..#.", "..#." },
            new[] { "####", "#...", "#...", "###.", "...#", "#..#", ".##." },
            new[] { ".##.", "#...", "#...", "###.", "#..#", "#..#", ".##." },
            new[] { "####", "...#", "...#", "..#.", "..#.", ".#..", ".#.." },
            new[] { ".##.", "#..#", "#..#", ".##.", "#..#", "#..#", ".##." },
            new[] { ".##.", "#..#", "#..#", ".###", "...#", "...#", ".##." }
        };

        static void DrawGlyph(Graphics g, Brush brush, string[] rows, int x, int y, int size)
        {
            for (int row = 0; row < rows.Length; row++)
            {
                int top = ((y + row) * size + 8) / 16;
                int bottom = ((y + row + 1) * size + 8) / 16;
                for (int col = 0; col < rows[row].Length; col++)
                {
                    if (rows[row][col] != '#') continue;
                    int left = ((x + col) * size + 8) / 16;
                    int right = ((x + col + 1) * size + 8) / 16;
                    g.FillRectangle(brush, left, top, right - left, bottom - top);
                }
            }
        }
    }
}
