using System.Text.Json;

namespace TrayMixer.Linux;

public sealed class Settings
{
    public HashSet<string> Hidden { get; set; } = [];
    public string Theme { get; set; } = "GNOME / System";
    public static string ConfigRoot => Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") is { Length: > 0 } value && Path.IsPathRooted(value) ? value : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
    static string FilePath => Path.Combine(ConfigRoot, "traymixer", "settings.json");
    public static Settings Load()
    {
        try { var settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(FilePath)) ?? new(); settings.Hidden ??= []; return settings; }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException) { return new(); }
    }
    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath + ".tmp", JsonSerializer.Serialize(this));
        File.Move(FilePath + ".tmp", FilePath, true);
    }
    static string AutostartPath => Path.Combine(ConfigRoot, "autostart", "traymixer.desktop");
    public static bool Autostart => File.Exists(AutostartPath);
    public static void SetAutostart(bool enabled)
    {
        if (!enabled) { File.Delete(AutostartPath); return; }
        // APPIMAGE is the installed portable file, not its temporary mount point.
        var executable = Environment.GetEnvironmentVariable("APPIMAGE") ?? Environment.ProcessPath ?? throw new IOException("Executable path unavailable");
        if (executable.Contains('\n') || executable.Contains('\r')) throw new IOException("Invalid executable path");
        var escaped = executable.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("`", "\\`").Replace("$", "\\$").Replace("%", "%%");
        Directory.CreateDirectory(Path.GetDirectoryName(AutostartPath)!);
        File.WriteAllText(AutostartPath, "[Desktop Entry]\nType=Application\nName=TrayMixer\nExec=\"" + escaped + "\" --tray\nIcon=traymixer\nTerminal=false\n");
    }
}
