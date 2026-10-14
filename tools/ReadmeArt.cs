// Скриншот окна для README: настоящее окно TrayMixer на чёрном фоне с синим свечением, как у плашек.
// Плашки (SVG) собирает tools/readme/build.mjs. Запуск обоих: tools\readme-art.cmd
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

static class ReadmeArt
{
    static void Main(string[] args)
    {
        Screenshot(args[0], args[1]);
    }

    static void Screenshot(string popupPath, string outPath)
    {
        using (var pop = Image.FromFile(popupPath))
        using (var canvas = new Bitmap(880, pop.Height + 120, PixelFormat.Format32bppArgb))
        using (var g = Graphics.FromImage(canvas))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var full = new Rectangle(0, 0, canvas.Width, canvas.Height);
            using (var card = Round(full, 24))
            {
                using (var bg = new LinearGradientBrush(full, Color.FromArgb(13, 13, 13), Color.Black, 90f)) g.FillPath(bg, card);
                g.SetClip(card);
                Glow(g, new Rectangle(380, -280, 700, 640), Color.FromArgb(46, 139, 255), 70);
                Glow(g, new Rectangle(-260, canvas.Height / 2, 700, 600), Color.FromArgb(122, 92, 255), 45);
                g.ResetClip();
                using (var pen = new Pen(Color.FromArgb(36, 36, 36))) g.DrawPath(pen, card);
            }
            var r = new Rectangle((canvas.Width - pop.Width) / 2, 60, pop.Width, pop.Height);
            for (int i = 24; i > 0; i -= 2)
                using (var sh = new SolidBrush(Color.FromArgb(8, 0, 0, 0))) g.FillPath(sh, Round(new Rectangle(r.X - i, r.Y - i + 10, r.Width + 2 * i, r.Height + 2 * i), 10 + i));
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

    // Мягкое радиальное свечение: цвет в центре, прозрачность к краю
    static void Glow(Graphics g, Rectangle area, Color c, int alpha)
    {
        using (var path = new GraphicsPath())
        {
            path.AddEllipse(area);
            using (var pg = new PathGradientBrush(path) { CenterColor = Color.FromArgb(alpha, c), SurroundColors = new[] { Color.FromArgb(0, c) } })
                g.FillPath(pg, path);
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
