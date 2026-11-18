using TrayMixer.Linux;

var count = 0;
void Check(bool condition, string name) { if (!condition) throw new Exception("FAIL: " + name); Console.WriteLine("PASS: " + name); count++; }
const string sink = """
[{"index":42,"name":"alsa_output.pci stereo","description":"Speakers","mute":false,"volume":{"front-left":{"value":32768},"front-right":{"value":65536}}}]
""";
const string streams = """
[{"index":7,"mute":true,"properties":{"application.name":"Browser","application.process.binary":"browser"},"volume":{"mono":{"value":65536}}},
 {"index":8,"mute":false,"properties":{"media.name":"System sounds"},"volume":{"mono":{"value":98304}}}]
""";
var device = AudioBackend.Parse(sink, true, "alsa_output.pci stereo").Single();
Check(device.Volume == 75 && device.Default && !device.Muted, "stereo volume and default output");
var apps = AudioBackend.Parse(streams, false, "");
Check(apps[0].Name == "Browser" && apps[0].Muted && apps[1].Name == "System sounds", "stream metadata fallbacks and mute");
Check(apps[1].Volume == 150, "amplified volume is displayed without silent modification");
Check(AudioBackend.Parse("[]", true, "").Count == 0, "empty outputs");
try { AudioBackend.Parse("broken", true, ""); throw new Exception("Accepted invalid JSON"); } catch (System.Text.Json.JsonException) { Check(true, "malformed server response rejected"); }
var fake = new FakeRunner(sink, streams);
var audio = new AudioBackend(fake);
Check((await audio.Read()).Count == 3, "async snapshot reads devices and streams");
await audio.Volume(device, 200); Check(fake.Last.SequenceEqual(new[] { "pactl", "set-sink-volume", device.Target, "100%" }), "volume clamped and target remains one argument");
await audio.Volume(apps[0], -10); Check(fake.Last.SequenceEqual(new[] { "pactl", "set-sink-input-volume", "7", "0%" }), "stream command and lower clamp");
await audio.Mute(apps[0]); Check(fake.Last.SequenceEqual(new[] { "pactl", "set-sink-input-mute", "7", "toggle" }), "stream mute command");
await audio.Default(device); Check(fake.Last.SequenceEqual(new[] { "pactl", "set-default-sink", device.Target }), "default device command");
await audio.MuteDefault(); Check(fake.Last[2] == "@DEFAULT_SINK@", "tray default mute command");
var runner = new CommandRunner();
try { await runner.Run("traymixer-nonexistent-command"); throw new Exception("Accepted missing command"); } catch (System.ComponentModel.Win32Exception) { Check(true, "missing dependency returns error"); }
var previous = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
var temp = Path.Combine(Path.GetTempPath(), "traymixer-tests-" + Guid.NewGuid());
Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", temp);
try
{
    var settings = Settings.Load(); Check(settings.Theme == "GNOME / System", "default GNOME theme");
    settings.Hidden.Add("device:test"); settings.Theme = "Nord"; settings.Save();
    var loaded = Settings.Load(); Check(loaded.Hidden.Contains("device:test") && loaded.Theme == "Nord", "settings round trip");
    File.WriteAllText(Path.Combine(temp, "traymixer", "settings.json"), "invalid"); Check(Settings.Load().Hidden.Count == 0, "corrupt preferences recover");
    Settings.SetAutostart(true); Check(Settings.Autostart && File.ReadAllText(Path.Combine(temp, "autostart", "traymixer.desktop")).Contains("--tray"), "XDG autostart creation");
    Settings.SetAutostart(false); Check(!Settings.Autostart, "autostart removal");
}
finally { Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", previous); if (Directory.Exists(temp)) Directory.Delete(temp, true); }
if (args.Contains("--integration"))
{
    Check(OperatingSystem.IsLinux(), "native Linux integration environment");
    var real = new AudioBackend(new CommandRunner());
    var outputs = await real.Read();
    var test = outputs.Single(row => row.Device && row.Target == "traymixer_test");
    await real.Default(test); await real.Volume(test, 37);
    var changed = (await real.Read()).Single(row => row.Target == test.Target);
    Check(changed.Volume == 37 && changed.Default, "real PulseAudio volume and default readback");
    await real.Unmute(test); await real.Mute(test);
    Check((await real.Read()).Single(row => row.Target == test.Target).Muted, "real PulseAudio mute readback");
    await real.Unmute(test);
    Check(!(await real.Read()).Single(row => row.Target == test.Target).Muted, "real PulseAudio explicit unmute readback");
}
Console.WriteLine($"{count} checks passed");

sealed class FakeRunner(string sinks, string streams) : ICommandRunner
{
    public string[] Last = [];
    public Task<string> Run(string program, params string[] args)
    {
        Last = new[] { program }.Concat(args).ToArray();
        return Task.FromResult(args.Contains("sinks") ? sinks : args.Contains("sink-inputs") ? streams : "alsa_output.pci stereo\n");
    }
}
