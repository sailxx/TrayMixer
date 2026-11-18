using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;

namespace TrayMixer.Linux;

public sealed class Appearance
{
    public static readonly string[] Names = ["GNOME / System", "Charcoal", "Nord", "Catppuccin", "Light"];
    public IBrush Surface { get; private set; } = Brushes.Transparent;
    public IBrush Row { get; private set; } = Brushes.Transparent;
    public event Action? Changed;
    int revision;
    public async Task Apply(string name)
    {
        var request = ++revision;
        var app = Application.Current!;
        bool light = name == "Light";
        string accent = "#4CC2FF", background = "#202020", row = "#2C2C2C";
        if (name == Names[0])
        {
            app.RequestedThemeVariant = ThemeVariant.Default;
            light = app.PlatformSettings?.GetColorValues().ThemeVariant == Avalonia.Platform.PlatformThemeVariant.Light;
            try
            {
                var runner = new CommandRunner();
                var scheme = await runner.Run("gsettings", "get", "org.gnome.desktop.interface", "color-scheme");
                light = scheme.Contains("prefer-light") || (!scheme.Contains("prefer-dark") && light);
                var color = (await runner.Run("gsettings", "get", "org.gnome.desktop.interface", "accent-color")).Trim().Trim('\'');
                accent = color switch { "teal" => "#2190A4", "green" => "#3A944A", "yellow" => "#C88800", "orange" => "#ED5B00", "red" => "#E62D42", "pink" => "#D56199", "purple" => "#9141AC", "slate" => "#6F8396", _ => "#3584E4" };
            }
            catch (Exception) { /* Older GNOME and other desktops use the platform theme. */ }
            background = light ? "#FAFAFA" : "#242424"; row = light ? "#EEEEEE" : "#303030";
        }
        else if (name == "Nord") { background = "#2E3440"; row = "#3B4252"; accent = "#88C0D0"; }
        else if (name == "Catppuccin") { background = "#1E1E2E"; row = "#313244"; accent = "#CBA6F7"; }
        else if (light) { background = "#FAFAFA"; row = "#EEEEEE"; accent = "#005FB8"; }
        if (request != revision) return;
        app.RequestedThemeVariant = light ? ThemeVariant.Light : ThemeVariant.Dark;
        var theme = app.Styles.OfType<FluentTheme>().First();
        theme.Palettes[light ? ThemeVariant.Light : ThemeVariant.Dark] = new ColorPaletteResources { Accent = Color.Parse(accent) };
        Surface = new SolidColorBrush(Color.Parse(background)); Row = new SolidColorBrush(Color.Parse(row));
        Changed?.Invoke();
    }
}
