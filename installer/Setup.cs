using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Win32;

internal static class Program
{
    private const string AppName = "TrayMixer";
    private const string ExeName = "TrayMixer.exe";
    private const string UninstName = "Uninstall.exe";
    private const string RepoOwner = "sailxx";
    private const string RepoName = "TrayMixer";

    [STAThread]
    private static int Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) => Log("Fatal: " + e.ExceptionObject);

        bool install = false;
        bool uninstall = false;
        string? dirArg = null;

        foreach (var a in args)
        {
            if (a.Equals("/install", StringComparison.OrdinalIgnoreCase)) install = true;
            else if (a.Equals("/uninstall", StringComparison.OrdinalIgnoreCase)) uninstall = true;
            else if (a.StartsWith("/DIR=", StringComparison.OrdinalIgnoreCase)) dirArg = a.Substring(5).Trim('"');
        }

        string baseDir = dirArg ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppName);

        if (uninstall) return Uninstall(baseDir);
        if (install) return Install(baseDir);

        // Interactive mode (no args): check installed version and prompt
        return InteractiveInstall(baseDir);
    }

    private static int InteractiveInstall(string dir)
    {
        // Read installed version from registry
        string? installedVersion = GetInstalledVersion();

        // Fetch latest version from GitHub
        Console.WriteLine($"Checking latest {AppName} version...");
        string? latestVersion = GetLatestVersion();

        if (string.IsNullOrEmpty(latestVersion))
        {
            Console.WriteLine("ERROR: Cannot reach GitHub. Check your internet connection.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
            return 1;
        }

        bool isInstalled = !string.IsNullOrEmpty(installedVersion) &&
                           File.Exists(Path.Combine(dir, ExeName));

        if (isInstalled)
        {
            // Compare versions (strip leading 'v')
            string installed = installedVersion!.TrimStart('v');
            string latest = latestVersion.TrimStart('v');

            if (string.Equals(installed, latest, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"{AppName} {latest} is already installed and up to date.");
                Console.Write("Reinstall? [y/N] ");
                string? answer = Console.ReadLine()?.Trim().ToLowerInvariant();
                if (answer != "y" && answer != "yes")
                {
                    Console.WriteLine("No changes made.");
                    return 0;
                }
            }
            else
            {
                Console.WriteLine($"Installed: {installed}  →  Latest: {latest}");
                Console.Write($"Update {AppName} to {latest}? [Y/n] ");
                string? answer = Console.ReadLine()?.Trim().ToLowerInvariant();
                if (answer == "n" || answer == "no")
                {
                    Console.WriteLine("Update cancelled.");
                    return 0;
                }
            }
        }
        else
        {
            Console.WriteLine($"Installing {AppName} {latestVersion}.");
            Console.Write("Continue? [Y/n] ");
            string? answer = Console.ReadLine()?.Trim().ToLowerInvariant();
            if (answer == "n" || answer == "no")
            {
                Console.WriteLine("Installation cancelled.");
                return 0;
            }
        }

        return Install(dir);
    }

    private static string? GetInstalledVersion()
    {
        try
        {
            string keyPath = $@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{AppName}";
            using var key = Registry.CurrentUser.OpenSubKey(keyPath);
            return key?.GetValue("DisplayVersion") as string;
        }
        catch { }
        return null;
    }

    private static int Install(string dir)
    {
        try
        {
            Directory.CreateDirectory(dir);
            string exePath = Path.Combine(dir, ExeName);

            Log($"Installing {AppName} to {dir}");

            // Stop running instance
            StopApp(exePath);

            // Get latest version from GitHub API
            string version = GetLatestVersion();
            if (string.IsNullOrEmpty(version))
            {
                Log("ERROR: Cannot fetch latest version from GitHub API");
                return 1;
            }

            Log($"Latest version: {version}");

            // Download the exe
            string downloadUrl = $"https://github.com/{RepoOwner}/{RepoName}/releases/download/{version}/{ExeName}";
            string sha256Url = $"https://github.com/{RepoOwner}/{RepoName}/releases/download/{version}/{ExeName}.sha256";

            Log($"Downloading {downloadUrl}");
            if (!DownloadExe(downloadUrl, exePath))
            {
                Log("ERROR: Download failed");
                return 1;
            }

            // Verify SHA-256
            Log("Verifying SHA-256 checksum");
            if (!VerifySha256(sha256Url, exePath))
            {
                Log("ERROR: SHA-256 verification failed");
                try { File.Delete(exePath); } catch { }
                return 1;
            }

            Log("SHA-256 verified");

            // Reset gamma after the new exe is in place
            RunResetGamma(exePath);

            // Create uninstaller (copy of self)
            string uninstPath = Path.Combine(dir, UninstName);
            File.Copy(Assembly.GetExecutingAssembly().Location, uninstPath, true);
            Log($"Uninstaller created: {uninstPath}");

            // Create shortcuts
            CreateShortcuts(dir, exePath, uninstPath);

            // Write uninstall registry
            WriteUninstallRegistry(dir, uninstPath, version);

            Log("Installation complete");
            return 0;
        }
        catch (Exception ex)
        {
            Log("Install failed: " + ex.Message);
            return 1;
        }
    }

    private static int Uninstall(string dir)
    {
        try
        {
            string exePath = Path.Combine(dir, ExeName);
            string uninstPath = Path.Combine(dir, UninstName);

            Log($"Uninstalling {AppName} from {dir}");

            StopApp(exePath);
            RunResetGamma(exePath);

            // Remove shortcuts
            RemoveShortcuts();

            // Delete files
            TryDelete(uninstPath);
            TryDelete(exePath);
            TryDelete(Path.Combine(dir, ExeName + ".sha256"));

            // Remove uninstall registry
            RemoveUninstallRegistry();

            // Self-delete (uninstaller deletes itself)
            string self = Assembly.GetExecutingAssembly().Location;
            if (self.Equals(uninstPath, StringComparison.OrdinalIgnoreCase))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c timeout /t 2 /nobreak >nul && del \"{self}\" && rmdir \"{dir}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                });
            }

            Log("Uninstall complete");
            return 0;
        }
        catch (Exception ex)
        {
            Log("Uninstall failed: " + ex.Message);
            return 1;
        }
    }

    private static string? GetLatestVersion()
    {
        try
        {
            string apiUrl = $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest";
            using var client = new WebClient();
            client.Headers["User-Agent"] = $"{AppName}-Installer";
            client.Headers["Accept"] = "application/vnd.github+json";
            string json = client.DownloadString(apiUrl);

            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("tag_name", out var tagProp))
            {
                return tagProp.GetString();
            }
        }
        catch (Exception ex)
        {
            Log("GetLatestVersion error: " + ex.Message);
        }
        return null;
    }

    private static bool DownloadExe(string url, string destPath)
    {
        try
        {
            using var client = new WebClient();
            client.Headers["User-Agent"] = $"{AppName}-Installer";
            client.DownloadFile(url, destPath);
            return File.Exists(destPath) && new FileInfo(destPath).Length > 0;
        }
        catch (Exception ex)
        {
            Log("DownloadExe error: " + ex.Message);
            return false;
        }
    }

    private static bool VerifySha256(string sha256Url, string exePath)
    {
        try
        {
            string expectedHash;
            using (var client = new WebClient())
            {
                client.Headers["User-Agent"] = $"{AppName}-Installer";
                expectedHash = client.DownloadString(sha256Url).Trim().ToLowerInvariant();
            }

            using var sha256 = System.Security.Cryptography.SHA256.Create();
            using var stream = File.OpenRead(exePath);
            byte[] hashBytes = sha256.ComputeHash(stream);
            string actualHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

            return string.Equals(expectedHash, actualHash, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            Log("VerifySha256 error: " + ex.Message);
            return false;
        }
    }

    private static void StopApp(string exePath)
    {
        try
        {
            string exeName = Path.GetFileNameWithoutExtension(exePath);
            foreach (var p in Process.GetProcessesByName(exeName))
            {
                try
                {
                    string? pPath = p.MainModule?.FileName;
                    if (!string.IsNullOrEmpty(pPath) &&
                        pPath.Equals(exePath, StringComparison.OrdinalIgnoreCase))
                    {
                        p.Kill();
                        p.WaitForExit(3000);
                    }
                }
                catch { }
            }
        }
        catch { }
    }

    private static void RunResetGamma(string exePath)
    {
        try
        {
            if (File.Exists(exePath))
            {
                var psi = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = "--reset-gamma",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                Process.Start(psi)?.WaitForExit(5000);
            }
        }
        catch { }
    }

    private static void CreateShortcuts(string dir, string exePath, string uninstPath)
    {
        try
        {
            // Start Menu shortcut
            string startMenu = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
            string progMenu = Path.Combine(startMenu, "Programs", AppName);
            Directory.CreateDirectory(progMenu);
            CreateShortcut(Path.Combine(progMenu, $"{AppName}.lnk"), exePath, "");
            CreateShortcut(Path.Combine(progMenu, $"Uninstall {AppName}.lnk"), uninstPath, "/uninstall");

            // Desktop shortcut (optional)
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            CreateShortcut(Path.Combine(desktop, $"{AppName}.lnk"), exePath, "");
        }
        catch (Exception ex)
        {
            Log("CreateShortcuts error: " + ex.Message);
        }
    }

    private static void RemoveShortcuts()
    {
        try
        {
            string startMenu = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
            string progMenu = Path.Combine(startMenu, "Programs", AppName);
            TryDelete(Path.Combine(progMenu, $"{AppName}.lnk"));
            TryDelete(Path.Combine(progMenu, $"Uninstall {AppName}.lnk"));
            TryDelete(progMenu); // remove folder if empty

            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            TryDelete(Path.Combine(desktop, $"{AppName}.lnk"));
        }
        catch { }
    }

    private static void CreateShortcut(string lnkPath, string targetPath, string arguments)
    {
        Type? shellType = Type.GetTypeFromProgID("WScript.Shell");
        if (shellType == null) return;
        dynamic shell = Activator.CreateInstance(shellType);
        dynamic shortcut = shell.CreateShortcut(lnkPath);
        shortcut.TargetPath = targetPath;
        shortcut.Arguments = arguments;
        shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath) ?? "";
        shortcut.Save();
    }

    private static void WriteUninstallRegistry(string dir, string uninstPath, string version)
    {
        try
        {
            string keyPath = $@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{AppName}";
            using var key = Registry.CurrentUser.CreateSubKey(keyPath);
            key.SetValue("DisplayName", AppName);
            key.SetValue("DisplayVersion", version.TrimStart('v'));
            key.SetValue("Publisher", RepoOwner);
            key.SetValue("UninstallString", $"\"{uninstPath}\" /uninstall");
            key.SetValue("InstallLocation", dir);
            key.SetValue("NoModify", 1);
            key.SetValue("NoRepair", 1);
            key.SetValue("SystemComponent", 1); // hide from "Apps & features" list clutter
        }
        catch (Exception ex)
        {
            Log("WriteUninstallRegistry error: " + ex.Message);
        }
    }

    private static void RemoveUninstallRegistry()
    {
        try
        {
            string keyPath = $@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{AppName}";
            Registry.CurrentUser.DeleteSubKeyTree(keyPath, false);
        }
        catch { }
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path)) File.Delete(path);
            else if (Directory.Exists(path)) Directory.Delete(path, true);
        }
        catch { }
    }

    private static void Log(string msg)
    {
        string logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            AppName, "install.log");
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
            File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {msg}\n");
        }
        catch { }
        Console.WriteLine(msg);
    }
}