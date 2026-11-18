using System.Diagnostics;
using System.Globalization;
using System.Text.Json;

namespace TrayMixer.Linux;

public record AudioRow(string Key, string Name, string Target, bool Device, int Volume, bool Muted, bool Default);
public interface ICommandRunner { Task<string> Run(string program, params string[] args); }
public sealed class CommandRunner : ICommandRunner
{
    public async Task<string> Run(string program, params string[] args)
    {
        var info = new ProcessStartInfo(program) { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
        foreach (var arg in args) info.ArgumentList.Add(arg);
        info.Environment["LC_ALL"] = "C";
        using var process = Process.Start(info) ?? throw new IOException($"Cannot start {program}");
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(8));
        try { await process.WaitForExitAsync(timeout.Token); }
        catch (OperationCanceledException) { process.Kill(true); throw new IOException($"{program} timed out"); }
        var text = await output;
        var message = await error;
        if (process.ExitCode != 0) throw new IOException($"{program}: {message.Trim()}");
        return text;
    }
}
public sealed class AudioBackend(ICommandRunner runner)
{
    public async Task<List<AudioRow>> Read()
    {
        var sinks = runner.Run("pactl", "--format=json", "list", "sinks");
        var streams = runner.Run("pactl", "--format=json", "list", "sink-inputs");
        var current = runner.Run("pactl", "get-default-sink");
        await Task.WhenAll(sinks, streams, current);
        return Parse(await sinks, true, (await current).Trim()).Concat(Parse(await streams, false, "")).ToList();
    }
    public static List<AudioRow> Parse(string json, bool device, string current)
    {
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.EnumerateArray().Select(item =>
        {
            string Property(string key) => item.TryGetProperty("properties", out var props) && props.TryGetProperty(key, out var value) ? value.GetString() ?? "" : "";
            var index = item.GetProperty("index").ToString();
            var target = device ? item.GetProperty("name").GetString()! : index;
            var name = device ? (item.TryGetProperty("description", out var description) ? description.GetString() : target) : Property("application.name");
            if (string.IsNullOrWhiteSpace(name)) name = Property("media.name");
            if (string.IsNullOrWhiteSpace(name)) name = "Audio stream " + index;
            var volumes = item.GetProperty("volume").EnumerateObject().Select(channel => channel.Value.GetProperty("value").GetDouble() / 65536 * 100).ToArray();
            var volume = volumes.Length == 0 ? 0 : (int)Math.Round(volumes.Average());
            var key = device ? "device:" + target : "app:" + Property("application.process.binary") + ":" + name;
            return new AudioRow(key, name!, target, device, volume, item.GetProperty("mute").GetBoolean(), device && target == current);
        }).ToList();
    }
    public Task Volume(AudioRow row, int value) => runner.Run("pactl", row.Device ? "set-sink-volume" : "set-sink-input-volume", row.Target, Math.Clamp(value, 0, 100).ToString(CultureInfo.InvariantCulture) + "%");
    public Task Unmute(AudioRow row) => runner.Run("pactl", row.Device ? "set-sink-mute" : "set-sink-input-mute", row.Target, "0");
    public Task Mute(AudioRow row) => runner.Run("pactl", row.Device ? "set-sink-mute" : "set-sink-input-mute", row.Target, "toggle");
    public Task Default(AudioRow row) => row.Device ? runner.Run("pactl", "set-default-sink", row.Target) : Task.CompletedTask;
    public Task MuteDefault() => runner.Run("pactl", "set-sink-mute", "@DEFAULT_SINK@", "toggle");
}
