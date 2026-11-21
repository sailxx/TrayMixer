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
        WriteAtomically(FilePath, JsonSerializer.Serialize(this));
    }
    static string AutostartPath => Path.Combine(ConfigRoot, "autostart", "traymixer.desktop");
    public static bool Autostart => File.Exists(AutostartPath);
    public static void SetAutostart(bool enabled)
    {
        if (!enabled) { File.Delete(AutostartPath); return; }
        // APPIMAGE is the installed portable file, not its temporary mount point.
        var executable = Environment.GetEnvironmentVariable("APPIMAGE") ?? Environment.ProcessPath ?? throw new IOException("Executable path unavailable");
        if (!Path.IsPathFullyQualified(executable) || !File.Exists(executable) || executable.Any(char.IsControl) || executable.Contains('='))
            throw new IOException("Invalid executable path");
        var quoted = executable.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("`", "\\`").Replace("$", "\\$").Replace("%", "%%");
        var escaped = quoted.Replace("\\", "\\\\");
        WriteAtomically(AutostartPath, "[Desktop Entry]\nType=Application\nName=TrayMixer\nExec=\"" + escaped + "\" --tray\nIcon=traymixer\nTerminal=false\n");
    }

    static void WriteAtomically(string path, string text)
    {
        var directory = Path.GetDirectoryName(path)!;
        Directory.CreateDirectory(directory);
        var temp = Path.Combine(directory, ".traymixer-" + Guid.NewGuid().ToString("N"));
        var options = new FileStreamOptions { Mode = FileMode.CreateNew, Access = FileAccess.Write };
        if (OperatingSystem.IsLinux()) options.UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;
        var stream = new FileStream(temp, options);
        try
        {
            using (stream)
            using (var writer = new StreamWriter(stream)) writer.Write(text);
            File.Move(temp, path, true);
        }
        finally { File.Delete(temp); }
    }
}
