using System.Diagnostics;

namespace TrayMixer.Linux;

public static class DesktopSettings
{
    public static Task OpenDisplay()
    {
        var desktop = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP") ?? "";
        var info = new ProcessStartInfo { UseShellExecute = false };
        if (desktop.Contains("GNOME", StringComparison.OrdinalIgnoreCase)) { info.FileName = "gnome-control-center"; info.ArgumentList.Add("display"); }
        else if (desktop.Contains("KDE", StringComparison.OrdinalIgnoreCase)) { info.FileName = "systemsettings"; info.ArgumentList.Add("kcm_kscreen"); }
        else throw new IOException("Open your desktop's Display settings to adjust brightness and night light. Universal gamma control is not available on Wayland.");
        // A settings window is long-lived: do not run it through the short command timeout.
        using var process = Process.Start(info) ?? throw new IOException("Cannot open display settings");
        return Task.CompletedTask;
    }
}
