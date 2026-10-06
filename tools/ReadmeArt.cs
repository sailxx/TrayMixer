// Генератор картинок для README: SVG-плашки в стиле Windows 11 (Fluent) и скриншот на фоне.
// Запуск: tools\readme-art.cmd
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

static class ReadmeArt
{
    const string Font = "'Segoe UI Variable Display','Segoe UI Variable Text','Segoe UI',-apple-system,BlinkMacSystemFont,Inter,Roboto,'Helvetica Neue',Arial,sans-serif";
    const string Accent = "#4CC2FF";
    static string outDir;

    static void Main(string[] args)
    {
        outDir = args[0];
        Directory.CreateDirectory(outDir);
        foreach (var lang in new[] { "ru", "en" })
        {
            bool ru = lang == "ru";
            Save("hero-" + lang + ".svg", Hero(ru));
            Save("cta-" + lang + ".svg", Cta(ru));
            Save("features-" + lang + ".svg", Features(ru));
            Save("numbers-" + lang + ".svg", Numbers(ru));
            Save("security-" + lang + ".svg", Security(ru));
        }
        if (args.Length > 1) Screenshot(args[1], Path.Combine(outDir, "screenshot.png"));
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

    // Символ микшера — та же геометрия, что у иконки программы (три вертикальных фейдера)
    static int clipId;

    // Логотип — та же геометрия, что у иконки программы: три фейдера с прорезью-ручкой
    static string Mixer(double x, double y, double n, string color)
    {
        var ci = System.Globalization.CultureInfo.InvariantCulture;
        var sb = new StringBuilder();
        double bw = n * 0.2, gap = n * 0.12, cut = n * 0.075, m = (n - (3 * bw + 2 * gap)) / 2, top = y + n * 0.08, bot = y + n * 0.92;
        double[] lv = { 0.58, 0.22, 0.42 };
        for (int i = 0; i < 3; i++)
        {
            double bx = x + m + i * (bw + gap), k = top + (bot - top) * lv[i];
            string a = "c" + (clipId++), b = "c" + (clipId++);
            sb.AppendFormat(ci, "<clipPath id='{0}'><rect x='{1:0.##}' y='{2:0.##}' width='{3:0.##}' height='{4:0.##}'/></clipPath>", a, bx - 1, y, bw + 2, k - y);
            sb.AppendFormat(ci, "<clipPath id='{0}'><rect x='{1:0.##}' y='{2:0.##}' width='{3:0.##}' height='{4:0.##}'/></clipPath>", b, bx - 1, k + cut, bw + 2, y + n - k - cut);
            string bar = string.Format(ci, "x='{0:0.##}' y='{1:0.##}' width='{2:0.##}' height='{3:0.##}' rx='{4:0.##}' fill='{5}'", bx, top, bw, bot - top, bw / 2, color);
            sb.Append("<rect " + bar + " fill-opacity='0.45' clip-path='url(#" + a + ")'/>");
            sb.Append("<rect " + bar + " clip-path='url(#" + b + ")'/>");
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

    static string Hero(bool ru)
    {
        var b = new StringBuilder();
        b.Append(Card(0, 0, 880, 300, true));
        // Плитка с иконкой
        b.Append("<rect x='56' y='64' width='96' height='96' rx='20' fill='url(#tile)'/>");
        b.Append("<rect x='56.5' y='64.5' width='95' height='95' rx='19.5' fill='none' stroke='#fff' stroke-opacity='0.25'/>");
        b.Append(Mixer(76, 84, 56, "#fff"));
        b.Append(Text(184, 118, "TrayMixer", 54, "#fff", "600"));
        b.Append(Text(186, 154, ru ? "Громкость каждого устройства и приложения" : "Volume for every device and every app", 20, "#fff", "400", "start", 0.86));
        b.Append(Text(186, 180, ru ? "в одном окне в стиле Windows 11" : "in one native Windows 11 flyout", 20, "#fff", "400", "start", 0.86));
        string[] chips = ru ? new[] { "Mica · Acrylic", "~2 МБ памяти", "0% CPU", "Без установки", "Открытый код" }
                            : new[] { "Mica · Acrylic", "~2 MB RAM", "0% CPU", "No install", "Open source" };
        int[] widths = ru ? new[] { 132, 132, 84, 132, 130 } : new[] { 132, 112, 84, 104, 118 };
        int px = 56;
        for (int i = 0; i < chips.Length; i++) { b.Append(Pill(px, 222, widths[i], chips[i])); px += widths[i] + 10; }
        return Svg(880, 300, ru ? "TrayMixer — громкость каждого устройства и приложения в одном окне Windows 11" : "TrayMixer — volume for every device and app in one Windows 11 flyout", b.ToString());
    }

    static string Cta(bool ru)
    {
        string label = ru ? "Скачать для Windows 11" : "Download for Windows 11";
        int w = ru ? 286 : 290;
        var b = new StringBuilder();
        b.Append("<rect x='0' y='0' width='" + w + "' height='48' rx='6' fill='" + Accent + "'/>");
        b.Append("<rect x='0.5' y='0.5' width='" + (w - 1) + "' height='47' rx='5.5' fill='none' stroke='#fff' stroke-opacity='0.3'/>");
        b.Append("<rect x='0' y='45' width='" + w + "' height='3' rx='1.5' fill='#000' fill-opacity='0.12'/>");
        // Стрелка загрузки
        b.Append("<path d='M30 15v13m-5-5 5 5 5-5M23 33h14' stroke='#000' stroke-opacity='0.9' stroke-width='2' fill='none' stroke-linecap='round' stroke-linejoin='round'/>");
        b.Append(Text(52, 30, label, 16, "#000", "600", "start", 0.9));
        return Svg(w, 48, label, b.ToString());
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

    static string Features(bool ru)
    {
        string[][] f = ru ? new[]
        {
            new[] { "Все устройства", "Наушники, колонки, монитор —", "у каждого свой ползунок. Клик", "по имени делает его основным." },
            new[] { "Каждое приложение", "Браузер, игра и Discord —", "отдельно, с их иконками.", "Несколько вкладок = одна строка." },
            new[] { "Родной Windows 11", "Mica или Acrylic, скруглённые", "углы, тёмная и светлая тема,", "ваш цвет акцента." },
            new[] { "Ничего лишнего", "Правый клик — скрыть строку.", "Вернуть можно в меню трея.", "Всё запоминается." },
            new[] { "Мгновенно и легко", "Слушает события Windows вместо", "опроса: 0% CPU в простое", "и около 2 МБ памяти." },
            new[] { "Безопасно", "Без интернета и прав админа.", "Открытый код, сборка на GitHub", "с подтверждением происхождения." },
        } : new[]
        {
            new[] { "Every device", "Headphones, speakers, monitor —", "each gets its own slider. Click", "a name to make it the default." },
            new[] { "Every app", "Browser, game and Discord —", "separately, with their icons.", "Many tabs = one row." },
            new[] { "Native Windows 11", "Mica or Acrylic, rounded", "corners, dark and light theme,", "your accent color." },
            new[] { "Nothing extra", "Right-click a row to hide it.", "Bring it back from the tray menu.", "Everything is remembered." },
            new[] { "Instant and light", "Listens to Windows events instead", "of polling: 0% CPU when idle", "and about 2 MB of RAM." },
            new[] { "Secure", "No internet, no admin rights.", "Open source, built on GitHub", "with provenance attestation." },
        };
        var b = new StringBuilder();
        b.Append(Text(4, 26, ru ? "ВОЗМОЖНОСТИ" : "FEATURES", 13, Accent, "600"));
        int cw = 284, chh = 152, gap = 14;
        for (int i = 0; i < 6; i++)
        {
            int x = (i % 3) * (cw + gap), y = 44 + (i / 3) * (chh + gap);
            b.Append(Card(x, y, cw, chh, false));
            b.Append("<rect x='" + (x + 20) + "' y='" + (y + 20) + "' width='40' height='40' rx='8' fill='" + Accent + "' fill-opacity='0.12'/>");
            b.Append(Icon(i, x + 28, y + 28));
            b.Append(Text(x + 76, y + 46, f[i][0], 17, "#fff", "600"));
            for (int l = 1; l < 4; l++) b.Append(Text(x + 20, y + 68 + l * 20, f[i][l], 13, "#fff", "400", "start", 0.72));
        }
        return Svg(880, 44 + 2 * chh + gap, ru ? "Возможности TrayMixer" : "TrayMixer features", b.ToString());
    }

    static string Numbers(bool ru)
    {
        string[][] n = ru ? new[]
        {
            new[] { "~2 МБ", "памяти в простое" }, new[] { "0%", "CPU без опроса" },
            new[] { "160 КБ", "один exe-файл" }, new[] { "0", "зависимостей" },
        } : new[]
        {
            new[] { "~2 MB", "RAM when idle" }, new[] { "0%", "CPU, no polling" },
            new[] { "160 KB", "a single exe" }, new[] { "0", "dependencies" },
        };
        var b = new StringBuilder();
        b.Append(Text(4, 26, ru ? "ЛЁГКИЙ, КАК СИСТЕМНЫЙ" : "AS LIGHT AS A SYSTEM APP", 13, Accent, "600"));
        int w = 209, gap = 14;
        for (int i = 0; i < 4; i++)
        {
            int x = i * (w + gap);
            b.Append(Card(x, 44, w, 120, i == 0));
            b.Append(Text(x + 22, 44 + 62, n[i][0], 38, "#fff", "600"));
            b.Append(Text(x + 22, 44 + 94, n[i][1], 14, "#fff", "400", "start", 0.72));
        }
        return Svg(880, 164, ru ? "TrayMixer в цифрах" : "TrayMixer by the numbers", b.ToString());
    }

    static string Security(bool ru)
    {
        string[] items = ru ? new[]
        {
            "Работает без прав администратора (манифест asInvoker)",
            "Нет сети: не отправляет и не скачивает ничего",
            "Системные DLL только из System32 — защита от подмены",
            "Релизы собирает GitHub Actions + SHA-256 и attestation",
            "Проверка кода CodeQL при каждом изменении",
            "Настройки — только в HKCU\\Software\\TrayMixer",
        } : new[]
        {
            "Runs without admin rights (asInvoker manifest)",
            "No network: never sends or downloads anything",
            "System DLLs from System32 only — no DLL hijacking",
            "Releases built by GitHub Actions + SHA-256 & attestation",
            "CodeQL code scanning on every change",
            "Settings live only in HKCU\\Software\\TrayMixer",
        };
        var b = new StringBuilder();
        b.Append(Card(0, 0, 880, 222, true));
        b.Append(Icon(5, 28, 26));
        b.Append(Text(64, 45, ru ? "Безопасность" : "Security", 20, "#fff", "600"));
        for (int i = 0; i < items.Length; i++)
        {
            int x = 28 + (i % 2) * 420, y = 92 + (i / 2) * 46;
            b.Append("<circle cx='" + (x + 10) + "' cy='" + (y - 5) + "' r='10' fill='" + Accent + "' fill-opacity='0.14'/>");
            b.Append("<path d='M" + (x + 5.5) + " " + (y - 5) + "l3 3 6-6' stroke='" + Accent + "' stroke-width='2' fill='none' stroke-linecap='round' stroke-linejoin='round'/>");
            b.Append(Text(x + 30, y, items[i], 14, "#fff", "400", "start", 0.82));
        }
        return Svg(880, 222, ru ? "Безопасность TrayMixer" : "TrayMixer security", b.ToString());
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
