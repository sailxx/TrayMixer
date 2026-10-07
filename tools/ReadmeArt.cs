// Генератор картинок для README: SVG-плашки в стиле Windows 11 (Fluent) и скриншот на фоне.
// Запуск: tools\readme-art.cmd
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Collections.Generic;

static class ReadmeArt
{
    const string Font = "'Segoe UI Variable Display','Segoe UI Variable Text','Segoe UI',-apple-system,BlinkMacSystemFont,Inter,Roboto,'Helvetica Neue',Arial,sans-serif";
    const string Accent = "#4CC2FF";
    static string outDir;

    static void Main(string[] args)
    {
        outDir = args[0];
        Directory.CreateDirectory(outDir);
        foreach (var l in Langs.All)
        {
            Save("hero-" + l.Code + ".svg", Hero(l));
            Save("cta-" + l.Code + ".svg", Cta(l));
            Save("features-" + l.Code + ".svg", Features(l));
            Save("numbers-" + l.Code + ".svg", Numbers(l));
            Save("security-" + l.Code + ".svg", Security(l));
        }
        if (args.Length > 1) Screenshot(args[1], Path.Combine(outDir, "screenshot.png"));
        if (overflow) { Console.Error.WriteLine("Есть тексты шире своего места — сократите их в ReadmeLang.cs"); Environment.Exit(1); }
    }

    // Ширина текста в пикселях по Segoe UI — так плашки и переносы подстраиваются под любой язык
    static readonly Graphics measure = Graphics.FromImage(new Bitmap(1, 1));
    static bool overflow;
    static double Width(string s, int size, bool bold)
    {
        using (var f = new System.Drawing.Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Pixel))
            return measure.MeasureString(s, f, int.MaxValue, StringFormat.GenericTypographic).Width;
    }
    static string Fit(string s, int size, bool bold, double max)
    {
        if (Width(s, size, bold) > max) { overflow = true; Console.Error.WriteLine("Не помещается (" + max + " px): " + s); }
        return s;
    }
    static List<string> Wrap(string s, int size, double max)
    {
        var lines = new List<string>(); string cur = "";
        foreach (var w in s.Split(' '))
        {
            string next = cur.Length == 0 ? w : cur + " " + w;
            if (cur.Length > 0 && Width(next, size, false) > max) { lines.Add(cur); cur = w; } else cur = next;
        }
        if (cur.Length > 0) lines.Add(cur);
        return lines;
    }

    static void Save(string name, string svg) { File.WriteAllText(Path.Combine(outDir, name), svg, new UTF8Encoding(false)); }
    static string X(string s) { return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;"); }

    // Общие определения: фон-карта в стиле Mica, акцентное свечение, тень
    static string Defs()
    {
        return "<defs>" +
            "<linearGradient id='bg' x1='0' y1='0' x2='1' y2='1'><stop offset='0' stop-color='#202020'/><stop offset='1' stop-color='#1a1d24'/></linearGradient>" +
            "<radialGradient id='glow' cx='0.85' cy='0.1' r='0.8'><stop offset='0' stop-color='" + Accent + "' stop-opacity='0.28'/><stop offset='1' stop-color='" + Accent + "' stop-opacity='0'/></radialGradient>" +
            "<radialGradient id='glow2' cx='0.05' cy='1' r='0.7'><stop offset='0' stop-color='#7A5CFF' stop-opacity='0.16'/><stop offset='1' stop-color='#7A5CFF' stop-opacity='0'/></radialGradient>" +
            "<linearGradient id='tile' x1='0' y1='0' x2='1' y2='1'><stop offset='0' stop-color='#5AC8FA'/><stop offset='1' stop-color='#0A64D8'/></linearGradient>" +
            "<linearGradient id='stroke' x1='0' y1='0' x2='0' y2='1'><stop offset='0' stop-color='#fff' stop-opacity='0.14'/><stop offset='1' stop-color='#fff' stop-opacity='0.05'/></linearGradient>" +
            "</defs>";
    }

    static string Svg(int w, int h, string label, string body)
    {
        return "<svg xmlns='http://www.w3.org/2000/svg' width='" + w + "' height='" + h + "' viewBox='0 0 " + w + " " + h + "' role='img' aria-label='" + X(label) + "'>" + Defs() +
            "<style>text{font-family:" + Font + ";}</style>" + body + "</svg>";
    }

    // Окно-карта Windows 11: скругление 8, тонкая светлая рамка
    static string Card(int x, int y, int w, int h, bool glow)
    {
        return "<rect x='" + x + ".5' y='" + y + ".5' width='" + (w - 1) + "' height='" + (h - 1) + "' rx='8' fill='url(#bg)' stroke='url(#stroke)'/>" +
            (glow ? "<rect x='" + x + "' y='" + y + "' width='" + w + "' height='" + h + "' rx='8' fill='url(#glow)'/><rect x='" + x + "' y='" + y + "' width='" + w + "' height='" + h + "' rx='8' fill='url(#glow2)'/>" : "");
    }

    // Логотип — та же геометрия, что у иконки программы: три горизонтальных слайдера с красным уровнем
    const string Red = "#FF453A";

    static string Mixer(double x, double y, double n)
    {
        var ci = System.Globalization.CultureInfo.InvariantCulture;
        var sb = new StringBuilder();
        double th = n * 0.07, kr = n * 0.11, x0 = x + n * 0.08, x1 = x + n * 0.92;
        double[] rows = { 0.22, 0.5, 0.78 }, lv = { 0.72, 0.3, 0.55 };
        for (int i = 0; i < 3; i++)
        {
            double cy = y + n * rows[i], kx = x0 + kr + (x1 - x0 - 2 * kr) * lv[i];
            sb.AppendFormat(ci, "<rect x='{0:0.##}' y='{1:0.##}' width='{2:0.##}' height='{3:0.##}' rx='{4:0.##}' fill='#fff' fill-opacity='0.4'/>", x0, cy - th / 2, x1 - x0, th, th / 2);
            sb.AppendFormat(ci, "<rect x='{0:0.##}' y='{1:0.##}' width='{2:0.##}' height='{3:0.##}' rx='{4:0.##}' fill='{5}'/>", x0, cy - th / 2, kx - x0, th, th / 2, Red);
            sb.AppendFormat(ci, "<circle cx='{0:0.##}' cy='{1:0.##}' r='{2:0.##}' fill='#fff'/>", kx, cy, kr);
            sb.AppendFormat(ci, "<circle cx='{0:0.##}' cy='{1:0.##}' r='{2:0.##}' fill='{3}'/>", kx, cy, kr * 0.45, Red);
        }
        return sb.ToString();
    }

    static string Text(int x, int y, string s, int size, string color, string weight, string anchor = "start", double opacity = 1)
    {
        return "<text x='" + x + "' y='" + y + "' font-size='" + size + "' font-weight='" + weight + "' fill='" + color + "'" +
            (opacity < 1 ? " fill-opacity='" + opacity.ToString(System.Globalization.CultureInfo.InvariantCulture) + "'" : "") +
            (anchor != "start" ? " text-anchor='" + anchor + "'" : "") + ">" + X(s) + "</text>";
    }

    static string Pill(int x, int y, int w, string s)
    {
        return "<rect x='" + x + ".5' y='" + y + ".5' width='" + (w - 1) + "' height='31' rx='16' fill='#fff' fill-opacity='0.06' stroke='#fff' stroke-opacity='0.1'/>" +
            Text(x + w / 2, y + 21, s, 13, "#fff", "500", "middle", 0.9);
    }

    static string Hero(Lang l)
    {
        var b = new StringBuilder();
        // Чёрная карта и логотип без плитки
        b.Append("<rect x='0.5' y='0.5' width='879' height='299' rx='8' fill='#000' stroke='url(#stroke)'/>");
        b.Append(Mixer(56, 64, 96));
        b.Append(Text(184, 118, "TrayMixer", 54, "#fff", "600"));
        b.Append(Text(186, 154, Fit(l.Line1, 20, false, 660), 20, "#fff", "400", "start", 0.86));
        b.Append(Text(186, 180, Fit(l.Line2, 20, false, 660), 20, "#fff", "400", "start", 0.86));
        int px = 56;
        foreach (var chip in l.Chips)
        {
            int w = (int)Math.Ceiling(Width(chip, 13, true) + 34);
            b.Append(Pill(px, 222, w, chip)); px += w + 10;
        }
        if (px - 10 > 840) { overflow = true; Console.Error.WriteLine("Плашки шапки не помещаются: " + l.Code); }
        return Svg(880, 300, l.Alt, b.ToString());
    }

    static string Cta(Lang l)
    {
        int w = (int)Math.Ceiling(Width(l.Cta, 16, true) + 52 + 26);
        var b = new StringBuilder();
        b.Append("<rect x='0' y='0' width='" + w + "' height='48' rx='6' fill='" + Accent + "'/>");
        b.Append("<rect x='0.5' y='0.5' width='" + (w - 1) + "' height='47' rx='5.5' fill='none' stroke='#fff' stroke-opacity='0.3'/>");
        b.Append("<rect x='0' y='45' width='" + w + "' height='3' rx='1.5' fill='#000' fill-opacity='0.12'/>");
        // Стрелка загрузки
        b.Append("<path d='M30 15v13m-5-5 5 5 5-5M23 33h14' stroke='#000' stroke-opacity='0.9' stroke-width='2' fill='none' stroke-linecap='round' stroke-linejoin='round'/>");
        b.Append(Text(52, 30, l.Cta, 16, "#000", "600", "start", 0.9));
        return Svg(w, 48, l.Cta, b.ToString());
    }
    // Иконки возможностей (простые формы, чтобы одинаково выглядели везде)
    static string Icon(int kind, int x, int y)
    {
        string s = "stroke='" + Accent + "' stroke-width='2' fill='none' stroke-linecap='round' stroke-linejoin='round'";
        switch (kind)
        {
            case 0: return "<path " + s + " d='M" + (x + 4) + " " + (y + 16) + "v-3a8 8 0 0 1 16 0v3m-16 0v5h4v-7h-4m16 2v5h-4v-7h4'/>"; // наушники
            case 1: return "<rect " + s + " x='" + (x + 3) + "' y='" + (y + 3) + "' width='18' height='18' rx='4'/><path " + s + " d='M" + (x + 8) + " " + (y + 12) + "h8M" + (x + 12) + " " + (y + 8) + "v8'/>"; // приложения
            case 2: return "<rect " + s + " x='" + (x + 2) + "' y='" + (y + 4) + "' width='20' height='16' rx='3'/><path " + s + " d='M" + (x + 2) + " " + (y + 9) + "h20'/>"; // окно
            case 3: return "<path " + s + " d='M" + (x + 3) + " " + (y + 12) + "s3-6 9-6 9 6 9 6-3 6-9 6-9-6-9-6zM" + (x + 4) + " " + (y + 4) + "l16 16'/>"; // скрыть
            case 4: return "<path " + s + " d='M" + (x + 13) + " " + (y + 2) + "l-8 12h7l-1 8 8-12h-7z'/>"; // молния
            default: return "<path " + s + " d='M" + (x + 12) + " " + (y + 2) + "l8 3v6c0 5-3.5 9-8 11-4.5-2-8-6-8-11v-6zM" + (x + 8.5) + " " + (y + 12) + "l2.5 2.5 4.5-5'/>"; // щит
        }
    }

    static string Features(Lang l)
    {
        var b = new StringBuilder();
        b.Append(Text(4, 26, l.FeaturesTitle, 13, Accent, "600"));
        int cw = 284, chh = 152, gap = 14;
        for (int i = 0; i < 6; i++)
        {
            int x = (i % 3) * (cw + gap), y = 44 + (i / 3) * (chh + gap);
            b.Append(Card(x, y, cw, chh, false));
            b.Append("<rect x='" + (x + 20) + "' y='" + (y + 20) + "' width='40' height='40' rx='8' fill='" + Accent + "' fill-opacity='0.12'/>");
            b.Append(Icon(i, x + 28, y + 28));
            b.Append(Text(x + 76, y + 46, Fit(l.Features[i][0], 17, true, cw - 76 - 12), 17, "#fff", "600"));
            var lines = Wrap(l.Features[i][1], 13, cw - 44);
            if (lines.Count > 3) { overflow = true; Console.Error.WriteLine("Больше трёх строк: " + l.Features[i][1]); }
            for (int k = 0; k < lines.Count; k++) b.Append(Text(x + 20, y + 88 + k * 20, lines[k], 13, "#fff", "400", "start", 0.72));
        }
        return Svg(880, 44 + 2 * chh + gap, l.FeaturesAlt, b.ToString());
    }

    static string Numbers(Lang l)
    {
        var b = new StringBuilder();
        b.Append(Text(4, 26, l.NumbersTitle, 13, Accent, "600"));
        int w = 209, gap = 14;
        for (int i = 0; i < 4; i++)
        {
            int x = i * (w + gap);
            b.Append(Card(x, 44, w, 120, i == 0));
            b.Append(Text(x + 22, 44 + 62, l.Numbers[i][0], 38, "#fff", "600"));
            b.Append(Text(x + 22, 44 + 94, Fit(l.Numbers[i][1], 14, false, w - 36), 14, "#fff", "400", "start", 0.72));
        }
        return Svg(880, 164, l.NumbersAlt, b.ToString());
    }

    static string Security(Lang l)
    {
        var b = new StringBuilder();
        b.Append(Card(0, 0, 880, 222, true));
        b.Append(Icon(5, 28, 26));
        b.Append(Text(64, 45, l.SecurityTitle, 20, "#fff", "600"));
        for (int i = 0; i < l.Security.Length; i++)
        {
            int x = 28 + (i % 2) * 420, y = 92 + (i / 2) * 46;
            b.Append("<circle cx='" + (x + 10) + "' cy='" + (y - 5) + "' r='10' fill='" + Accent + "' fill-opacity='0.14'/>");
            b.Append("<path d='M" + (x + 5.5) + " " + (y - 5) + "l3 3 6-6' stroke='" + Accent + "' stroke-width='2' fill='none' stroke-linecap='round' stroke-linejoin='round'/>");
            b.Append(Text(x + 30, y, Fit(l.Security[i], 14, false, 420 - 30 - 8), 14, "#fff", "400", "start", 0.82));
        }
        return Svg(880, 222, l.SecurityAlt, b.ToString());
    }

    // Скриншот окна на фоне в духе обоев Windows 11: скруглённые углы, рамка и мягкая тень
    static void Screenshot(string popupPath, string outPath)
    {
        using (var pop = Image.FromFile(popupPath))
        using (var canvas = new Bitmap(880, pop.Height + 120, PixelFormat.Format32bppArgb))
        using (var g = Graphics.FromImage(canvas))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var full = new Rectangle(0, 0, canvas.Width, canvas.Height);
            using (var bg = new LinearGradientBrush(full, Color.FromArgb(10, 22, 48), Color.FromArgb(4, 8, 20), 60f)) g.FillPath(bg, Round(full, 12));
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(-200, canvas.Height / 3, 900, 700);
                using (var pg = new PathGradientBrush(path) { CenterColor = Color.FromArgb(150, 40, 110, 230), SurroundColors = new[] { Color.FromArgb(0, 40, 110, 230) } })
                { g.SetClip(Round(full, 12)); g.FillPath(pg, path); }
            }
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(420, -300, 700, 600);
                using (var pg = new PathGradientBrush(path) { CenterColor = Color.FromArgb(110, 90, 200, 255), SurroundColors = new[] { Color.FromArgb(0, 90, 200, 255) } })
                    g.FillPath(pg, path);
            }
            var r = new Rectangle((canvas.Width - pop.Width) / 2, 60, pop.Width, pop.Height);
            for (int i = 24; i > 0; i -= 2)
                using (var sh = new SolidBrush(Color.FromArgb(6, 0, 0, 0))) g.FillPath(sh, Round(new Rectangle(r.X - i, r.Y - i + 10, r.Width + 2 * i, r.Height + 2 * i), 10 + i));
            using (var clip = Round(r, 10))
            {
                g.SetClip(clip);
                g.DrawImage(pop, r);
                g.ResetClip();
                using (var pen = new Pen(Color.FromArgb(40, 255, 255, 255))) g.DrawPath(pen, clip);
            }
            canvas.Save(outPath, ImageFormat.Png);
        }
    }

    static GraphicsPath Round(Rectangle r, int rad)
    {
        var p = new GraphicsPath(); int d = rad * 2;
        p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        p.CloseFigure(); return p;
    }
}
