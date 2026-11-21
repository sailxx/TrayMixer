using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;

namespace TrayMixer.Linux;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        using var instance = new Mutex(true, "TrayMixer.Linux." + Environment.UserName, out var created);
        if (!created) { if (!args.Contains("--tray")) Instance.ActivateExisting(); return; }
        AppBuilder.Configure<MixerApp>().UsePlatformDetect().LogToTrace().StartWithClassicDesktopLifetime(args);
    }
}
public sealed class MixerApp : Application
{
    public override void Initialize() { Styles.Add(new FluentTheme()); RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Dark; }
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            var window = new MixerWindow();
            desktop.MainWindow = window;
            var menu = new NativeMenu();
            void Menu(string title, Action action) { var item = new NativeMenuItem(title); item.Click += (_, _) => action(); menu.Items.Add(item); }
            Menu("Open mixer", () => window.OpenMixer());
            Menu("Mute default output", () => window.Perform(window.Audio.MuteDefault));
            Menu("Restore hidden rows", window.Restore);
            Menu("Toggle autostart", window.ToggleAutostart);
            menu.Items.Add(new NativeMenuItemSeparator());
            Menu("Exit", () => desktop.Shutdown());
            var tray = new TrayIcon { ToolTipText = "TrayMixer", Menu = menu, Icon = new WindowIcon(AssetLoader.Open(new Uri("avares://TrayMixer/Assets/TrayMixer.ico"))) };
            tray.Clicked += (_, _) => { if (window.IsVisible) window.Hide(); else window.OpenMixer(); };
            TrayIcon.SetIcons(this, new TrayIcons { tray });
            var cancellation = new CancellationTokenSource();
            _ = Instance.Listen(window.OpenMixer, cancellation.Token);
            desktop.Exit += (_, _) => { cancellation.Cancel(); tray.Dispose(); };
            // Opening a window by default also makes the app usable without a tray host.
            if (!desktop.Args!.Contains("--tray")) window.OpenMixer();
        }
        base.OnFrameworkInitializationCompleted();
    }
}
public sealed class MixerWindow : Window
{
    public AudioBackend Audio { get; } = new(new CommandRunner());
    readonly Settings settings = Settings.Load();
    public Appearance Appearance { get; } = new();
    readonly CheckBox autostart = new() { Content = "Start at login", IsChecked = Settings.Autostart };
    DateTime lastThemeCheck;
    bool applyingTheme;
    readonly StackPanel rows = new() { Spacing = 8 };
    readonly TextBlock status = new() { TextWrapping = TextWrapping.Wrap, Foreground = Brushes.LightGray };
    readonly DispatcherTimer timer = new() { Interval = TimeSpan.FromSeconds(2) };
    readonly Dictionary<string, RowControl> controls = [];
    bool reading, refreshAgain;
    long editRevision;
    public void MarkEdit() => editRevision++;
    public MixerWindow()
    {
        Title = "TrayMixer"; Width = 380; Height = 560; MinWidth = 340; MinHeight = 280;
        Background = new SolidColorBrush(Color.Parse("#202020"));
        var header = new TextBlock { Text = "TrayMixer", FontSize = 22, FontWeight = FontWeight.SemiBold };
        var restore = new Button { Content = "Restore hidden rows" }; restore.Click += (_, _) => Restore();
        autostart.Click += (_, _) => ToggleAutostart();
        var themes = new ComboBox { ItemsSource = Appearance.Names, SelectedItem = Appearance.Names.Contains(settings.Theme) ? settings.Theme : Appearance.Names[0], HorizontalAlignment = HorizontalAlignment.Stretch };
        themes.SelectionChanged += (_, _) => { settings.Theme = themes.SelectedItem?.ToString() ?? Appearance.Names[0]; Save(); _ = ApplyTheme(); };
        Appearance.Changed += () => { Background = Appearance.Surface; foreach (var control in controls.Values) control.Background = Appearance.Row; };
        _ = ApplyTheme();
        var display = new Button { Content = "Display & night light settings" }; display.Click += (_, _) => Perform(DesktopSettings.OpenDisplay);
        var root = new DockPanel { Margin = new Thickness(16) };
        var top = new StackPanel { Spacing = 10, Children = { header, status } }; DockPanel.SetDock(top, Dock.Top); root.Children.Add(top);
        var bottom = new StackPanel { Spacing = 6, Margin = new Thickness(0, 10, 0, 0), Children = { new TextBlock { Text = "Appearance", FontSize = 12 }, themes, restore, autostart, display } }; DockPanel.SetDock(bottom, Dock.Bottom); root.Children.Add(bottom);
        root.Children.Add(new ScrollViewer { Content = rows, VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto }); Content = root;
        KeyDown += (_, e) => { if (e.Key == Key.Escape) Hide(); };
        Closing += (_, e) => { if (e.CloseReason == WindowCloseReason.WindowClosing) { e.Cancel = true; Hide(); } };
        timer.Tick += async (_, _) => { if (IsVisible) await Refresh(); };
        timer.Start();
    }
    public void OpenMixer() { Show(); Activate(); _ = Refresh(); }
    public void Perform(Func<Task> operation) => _ = Execute(operation);
    async Task Execute(Func<Task> operation)
    {
        try { MarkEdit(); await operation(); MarkEdit(); await Refresh(); }
        catch (Exception ex) { status.Text = ex.Message; }
    }
    public void Restore() { settings.Hidden.Clear(); Save(); _ = Refresh(); }
    void Save() { try { settings.Save(); } catch (Exception ex) { status.Text = "Cannot save settings: " + ex.Message; } }
    public void ToggleAutostart() => Perform(() => { try { Settings.SetAutostart(!Settings.Autostart); } finally { autostart.IsChecked = Settings.Autostart; } return Task.CompletedTask; });
    async Task ApplyTheme()
    {
        applyingTheme = true;
        try { await Appearance.Apply(settings.Theme); lastThemeCheck = DateTime.UtcNow; }
        finally { applyingTheme = false; }
    }
    async Task Refresh()
    {
        if (reading) { refreshAgain = true; return; }
        reading = true;
        var revision = editRevision;
        try
        {
            if (settings.Theme == Appearance.Names[0] && DateTime.UtcNow - lastThemeCheck > TimeSpan.FromSeconds(5) && !applyingTheme) await ApplyTheme();
            var snapshot = await Audio.Read();
            if (revision != editRevision) { refreshAgain = true; return; }
            var visible = snapshot.Where(row => !settings.Hidden.Contains(row.Key)).ToList();
            // Stream IDs are transient; hidden preferences use the stable application key.
            var ids = visible.Select(row => (row.Device ? "d:" : "s:") + row.Target).ToHashSet();
            foreach (var key in controls.Keys.Where(key => !ids.Contains(key)).ToArray()) { rows.Children.Remove(controls[key]); controls[key].Stop(); controls.Remove(key); }
            foreach (var row in visible)
            {
                var id = (row.Device ? "d:" : "s:") + row.Target;
                if (!controls.TryGetValue(id, out var control))
                {
                    control = new RowControl(row, this, () => { settings.Hidden.Add(row.Key); Save(); _ = Refresh(); });
                    controls.Add(id, control); rows.Children.Add(control);
                }
                control.Sync(row);
            }
            status.Foreground = this.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Light ? Brushes.DimGray : Brushes.LightGray;
            status.Text = snapshot.Count == 0 ? "No audio outputs. Connect a device, then retry." : visible.Count == 0 ? "All rows are hidden. Restore them below." : "Outputs & applications · " + snapshot.Count + " controls";
        }
        catch (Exception ex)
        {
            foreach (var control in controls.Values) control.Stop(); controls.Clear(); rows.Children.Clear();
            status.Text = "Audio unavailable. Install pactl and start PipeWire-Pulse or PulseAudio.\n" + ex.Message;
        }
        finally { reading = false; if (refreshAgain) { refreshAgain = false; _ = Refresh(); } }
    }
}
sealed class RowControl : Border
{
    AudioRow row;
    readonly Slider slider;
    readonly Button title;
    readonly Button mute;
    readonly TextBlock value;
    readonly DispatcherTimer debounce = new() { Interval = TimeSpan.FromMilliseconds(140) };
    bool syncing, pending, sending, stopped;
    DateTime lastEdit;
    public RowControl(AudioRow initial, MixerWindow owner, Action hide)
    {
        row = initial; Padding = new Thickness(10); CornerRadius = new CornerRadius(10); Background = owner.Appearance.Row;
        title = new Button { HorizontalAlignment = HorizontalAlignment.Stretch, HorizontalContentAlignment = HorizontalAlignment.Left, Background = Brushes.Transparent };
        title.Click += (_, _) => { if (row.Device) owner.Perform(() => owner.Audio.Default(row)); };
        mute = new Button { MinWidth = 62 }; mute.Click += (_, _) => owner.Perform(() => owner.Audio.Mute(row));
        slider = new Slider { Minimum = 0, Maximum = 100, TickFrequency = 2, HorizontalAlignment = HorizontalAlignment.Stretch };
        value = new TextBlock { Width = 42, VerticalAlignment = VerticalAlignment.Center, TextAlignment = TextAlignment.Right };
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"), ColumnSpacing = 6 };
        grid.Children.Add(mute); Grid.SetColumn(slider, 1); grid.Children.Add(slider); Grid.SetColumn(value, 2); grid.Children.Add(value);
        Child = new StackPanel { Spacing = 2, Children = { title, grid } };
        var hideItem = new MenuItem { Header = "Hide this row" }; hideItem.Click += (_, _) => hide(); ContextMenu = new ContextMenu { ItemsSource = new[] { hideItem } };
        slider.PropertyChanged += (_, e) => { if (e.Property == Slider.ValueProperty && !syncing) { owner.MarkEdit(); Queue(); } };
        slider.PointerWheelChanged += (_, e) => { slider.Value = Math.Clamp(slider.Value + Math.Sign(e.Delta.Y) * 2, 0, 100); e.Handled = true; };
        debounce.Tick += (_, _) =>
        {
            debounce.Stop();
            if (sending) { debounce.Start(); return; }
            sending = true; pending = false;
            var target = (int)Math.Round(slider.Value); var snapshot = row;
            owner.Perform(async () => { try { await owner.Audio.Volume(snapshot, target); if (target > 0 && snapshot.Muted) await owner.Audio.Unmute(snapshot); } finally { sending = false; if (pending && !stopped) debounce.Start(); } });
        };
        Sync(initial);
    }
    void Queue() { lastEdit = DateTime.UtcNow; pending = true; value.Text = ((int)slider.Value) + "%"; debounce.Stop(); debounce.Start(); }
    public void Stop() { stopped = true; pending = false; debounce.Stop(); }
    public void Sync(AudioRow current)
    {
        row = current; title.Content = (row.Default ? "● " : "") + row.Name; ToolTip.SetTip(title, row.Device ? "Make default output" : row.Name);
        mute.Content = row.Muted ? "Unmute" : "Mute";
        if (pending || sending || slider.IsPointerOver || DateTime.UtcNow - lastEdit < TimeSpan.FromSeconds(1)) return;
        syncing = true; slider.Value = Math.Clamp(row.Volume, 0, 100); syncing = false; value.Text = row.Volume + "%";
    }
}
