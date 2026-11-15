using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

internal static class Program
{
    internal const string AppName = "TrayMixer";
    internal const string ExeName = "TrayMixer.exe";
    internal const string UninstName = "Uninstall.exe";
    internal static string UninstallKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\TrayMixer";
    internal static string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    internal static string ConfigKey = @"Software\TrayMixer";
    internal static string Desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
    internal static string Menu = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), AppName);

    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length == 3 && args[0] == "/cleanup")
        {
            try
            {
                if (Path.GetFileName(args[2]) != UninstName) return 2;
                using (var parent = Process.GetProcessById(int.Parse(args[1]))) parent.WaitForExit();
            }
            catch (ArgumentException) { }
            for (int attempt = 0; attempt < 30; attempt++)
            {
                try
                {
                    File.Delete(args[2]);
                    string folder = Path.GetDirectoryName(args[2]);
                    if (Directory.GetFileSystemEntries(folder).Length == 0) Directory.Delete(folder);
                    var cleanup = new ProcessStartInfo("cmd.exe", "/d /q /c ping 127.0.0.1 -n 3 >nul & del /q \"%TM_SETUP_HELPER%\"") { UseShellExecute = false, CreateNoWindow = true };
                    cleanup.EnvironmentVariables["TM_SETUP_HELPER"] = Application.ExecutablePath;
                    Process.Start(cleanup);
                    return 0;
                }
                catch (IOException) { Thread.Sleep(200); }
            }
            return 1;
        }
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        bool silent = false, uninstall = Path.GetFileName(Application.ExecutablePath).Equals(UninstName, StringComparison.OrdinalIgnoreCase), launch = false;
        bool? autostart = null;
        string dir = Path.GetFileName(Application.ExecutablePath).Equals(UninstName, StringComparison.OrdinalIgnoreCase)
            ? Path.GetDirectoryName(Application.ExecutablePath) : InstalledDirectory();
        foreach (string arg in args)
        {
            if (arg.Equals("/S", StringComparison.OrdinalIgnoreCase) || arg.Equals("/install", StringComparison.OrdinalIgnoreCase)) silent = true;
            else if (arg.Equals("/uninstall", StringComparison.OrdinalIgnoreCase)) uninstall = true;
            else if (arg.StartsWith("/DIR=", StringComparison.OrdinalIgnoreCase)) dir = arg.Substring(5).Trim('"');
            else if (arg.Equals("/NOAUTOSTART", StringComparison.OrdinalIgnoreCase)) autostart = false;
            else if (arg.Equals("/LAUNCH", StringComparison.OrdinalIgnoreCase)) launch = true;
            else if (arg.Equals("/NOLAUNCH", StringComparison.OrdinalIgnoreCase)) launch = false;
            else if (!arg.Equals("/DESKTOP", StringComparison.OrdinalIgnoreCase)) return 2;
        }
        using (var mutex = new Mutex(false, @"Local\TrayMixer.Setup"))
        {
            if (!mutex.WaitOne(0)) return 3;
            try
            {
                if (!silent)
                {
                    SetupLanguage.Russian = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ru";
                    using (var language = new LanguageForm())
                        if (language.ShowDialog() != DialogResult.OK) return 0;
                }
                if (uninstall)
                {
                    if (!silent && MessageBox.Show(SetupLanguage.T("Remove TrayMixer? Your preferences will be kept.", "Удалить TrayMixer? Ваши настройки будут сохранены."), SetupLanguage.T("TrayMixer Setup", "Установка TrayMixer"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return 0;
                    Uninstall(dir);
                    if (!silent) MessageBox.Show(SetupLanguage.T("TrayMixer has been removed.", "TrayMixer удалён."), SetupLanguage.T("TrayMixer Setup", "Установка TrayMixer"));
                }
                else if (silent) Install(dir, autostart ?? DefaultAutostart(dir), launch);
                else Application.Run(new SetupForm(dir));
                return 0;
            }
            catch (Exception ex)
            {
                if (!silent) MessageBox.Show(ex.Message, "TrayMixer Setup", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 1;
            }
            finally { mutex.ReleaseMutex(); }
        }
    }

    internal static string InstalledDirectory()
    {
        using (var key = Registry.CurrentUser.OpenSubKey(UninstallKey))
        {
            string dir = key == null ? null : key.GetValue("InstallLocation") as string;
            return string.IsNullOrEmpty(dir) ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppName) : dir;
        }
    }

    internal static bool DefaultAutostart(string dir)
    {
        if (!File.Exists(Path.Combine(dir, ExeName))) return true;
        using (var key = Registry.CurrentUser.OpenSubKey(RunKey)) return key != null && key.GetValue(AppName) != null;
    }

    internal static Version VersionOf(string path)
    {
        if (!File.Exists(path)) return new Version(0, 0, 0, 0);
        Version version;
        return Version.TryParse(FileVersionInfo.GetVersionInfo(path).FileVersion, out version) ? version : new Version(0, 0, 0, 0);
    }

    internal static string ExtractPayload(string dir)
    {
        string path = Path.Combine(dir, "TrayMixer.payload.exe");
        using (var input = Assembly.GetExecutingAssembly().GetManifestResourceStream("TrayMixer.Payload"))
        {
            if (input == null) throw new InvalidOperationException(SetupLanguage.T("The installer payload is missing. Download Setup again.", "В установщике отсутствуют файлы программы. Скачайте его заново."));
            using (var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write)) input.CopyTo(output);
        }
        return path;
    }

    internal static void Install(string directory, bool autostart, bool launch)
    {
        string dir = Path.GetFullPath(directory);
        string previous = InstalledDirectory();
        using (var registered = Registry.CurrentUser.OpenSubKey(UninstallKey))
            if (registered != null && !SamePath(dir, previous)) throw new InvalidOperationException(SetupLanguage.T("An installation already exists at ", "Программа уже установлена в ") + previous + SetupLanguage.T(". Update it there, or uninstall it before choosing another folder.", ". Обновите её в этой папке или удалите перед выбором другой."));
        Directory.CreateDirectory(dir);
        string staging = Path.Combine(dir, ".setup-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(staging);
        string exe = Path.Combine(dir, ExeName), uninstaller = Path.Combine(dir, UninstName);
        string backup = Path.Combine(staging, "previous.exe");
        bool replaced = false, keepBackup = false;
        var snapshot = new InstallSnapshot(dir);
        try
        {
            string payload = ExtractPayload(staging);
            if (VersionOf(payload) < VersionOf(exe)) throw new InvalidOperationException(SetupLanguage.T("A newer version is installed. Download the latest Setup to update or reinstall it.", "Установлена более новая версия. Скачайте последний установщик для обновления или переустановки."));
            // Stage everything before stopping the installed app or touching its files.
            File.Copy(Application.ExecutablePath, Path.Combine(staging, UninstName));
            StopApp(exe);
            ResetGamma(exe);
            if (File.Exists(exe)) File.Replace(payload, exe, backup);
            else File.Move(payload, exe);
            replaced = true;
            File.Copy(Path.Combine(staging, UninstName), uninstaller, true);
            CreateShortcuts(dir);
            using (var key = Registry.CurrentUser.CreateSubKey(UninstallKey))
            {
                key.SetValue("DisplayName", AppName);
                key.SetValue("DisplayVersion", VersionOf(exe).ToString(3));
                key.SetValue("Publisher", "sailxx");
                key.SetValue("InstallLocation", dir);
                key.SetValue("DisplayIcon", exe);
                key.SetValue("UninstallString", Quote(uninstaller) + " /uninstall");
                key.SetValue("QuietUninstallString", Quote(uninstaller) + " /uninstall /S");
                key.SetValue("NoModify", 1);
                key.SetValue("NoRepair", 1);
                key.DeleteValue("SystemComponent", false);
            }
            using (var key = Registry.CurrentUser.CreateSubKey(RunKey))
            {
                if (autostart) key.SetValue(AppName, Quote(exe));
                else key.DeleteValue(AppName, false);
            }
            // Prevent the app's first-run default from overriding the installer's choice.
            using (var key = Registry.CurrentUser.CreateSubKey(ConfigKey)) key.SetValue("Initialized", 1);
        }
        catch
        {
            if (replaced)
            {
                try { snapshot.Restore(); }
                catch { keepBackup = true; throw; }
            }
            throw;
        }
        finally { if (!keepBackup) Directory.Delete(staging, true); }
        if (launch) Process.Start(new ProcessStartInfo(exe) { UseShellExecute = true });
    }

    internal static void Uninstall(string directory)
    {
        string dir = Path.GetFullPath(directory), exe = Path.Combine(dir, ExeName);
        using (var registered = Registry.CurrentUser.OpenSubKey(UninstallKey))
            if (registered != null && !SamePath(dir, InstalledDirectory())) throw new InvalidOperationException("Uninstall must use the registered installation folder.");
        StopApp(exe);
        ResetGamma(exe);
        File.Delete(exe);
        File.Delete(Path.Combine(dir, ExeName + ".sha256"));
        File.Delete(Path.Combine(Desktop, AppName + ".lnk"));
        File.Delete(Path.Combine(Menu, AppName + ".lnk"));
        File.Delete(Path.Combine(Menu, "Uninstall " + AppName + ".lnk"));
        if (Directory.Exists(Menu) && Directory.GetFileSystemEntries(Menu).Length == 0) Directory.Delete(Menu);
        using (var key = Registry.CurrentUser.OpenSubKey(RunKey, true))
            if (key != null) key.DeleteValue(AppName, false);
        Registry.CurrentUser.DeleteSubKeyTree(UninstallKey, false);
        string self = Path.Combine(dir, UninstName);
        if (SamePath(Application.ExecutablePath, self))
        {
            // A separate helper waits for this process, then removes only the uninstaller.
            string helper = Path.Combine(Path.GetTempPath(), "TrayMixer-clean-" + Guid.NewGuid().ToString("N") + ".exe");
            File.Copy(self, helper);
            Process.Start(new ProcessStartInfo(helper, "/cleanup " + Process.GetCurrentProcess().Id + " " + Quote(self)) { UseShellExecute = false });
        }
        else File.Delete(self);
        if (Directory.Exists(dir) && Directory.GetFileSystemEntries(dir).Length == 0) Directory.Delete(dir);
    }

    internal static bool SamePath(string a, string b) { return string.Equals(Path.GetFullPath(a).TrimEnd('\\'), Path.GetFullPath(b).TrimEnd('\\'), StringComparison.OrdinalIgnoreCase); }
    internal static string Quote(string value) { return "\"" + value + "\""; }

    private static void StopApp(string exe)
    {
        foreach (var process in Process.GetProcessesByName(AppName))
            using (process)
            {
                try
                {
                    if (!SamePath(process.MainModule.FileName, exe)) continue;
                }
                catch (System.ComponentModel.Win32Exception) { continue; }
                process.Kill();
                if (!process.WaitForExit(10000)) throw new IOException(SetupLanguage.T("TrayMixer is still running. Close it and try again.", "TrayMixer всё ещё работает. Закройте программу и повторите попытку."));
            }
    }

    private static void ResetGamma(string exe)
    {
        if (!File.Exists(exe)) return;
        using (var process = Process.Start(new ProcessStartInfo(exe, "--reset-gamma") { UseShellExecute = false, CreateNoWindow = true }))
        {
            if (!process.WaitForExit(10000)) { process.Kill(); process.WaitForExit(); throw new IOException(SetupLanguage.T("TrayMixer did not finish resetting the display. Try again.", "TrayMixer не завершил восстановление экрана. Повторите попытку.")); }
            if (process.ExitCode != 0) throw new IOException(SetupLanguage.T("TrayMixer could not restore the display. Your installation has been kept. Try again.", "TrayMixer не удалось восстановить экран. Установленная программа сохранена. Повторите попытку."));
        }
    }

    private static void CreateShortcuts(string dir)
    {
        Directory.CreateDirectory(Menu);
        CreateShortcut(Path.Combine(Menu, AppName + ".lnk"), Path.Combine(dir, ExeName), "");
        CreateShortcut(Path.Combine(Menu, "Uninstall " + AppName + ".lnk"), Path.Combine(dir, UninstName), "/uninstall");
        CreateShortcut(Path.Combine(Desktop, AppName + ".lnk"), Path.Combine(dir, ExeName), "");
    }

    private static void CreateShortcut(string path, string target, string args)
    {
        Type type = Type.GetTypeFromProgID("WScript.Shell", true);
        dynamic shell = Activator.CreateInstance(type);
        dynamic shortcut = shell.CreateShortcut(path);
        try
        {
            shortcut.TargetPath = target;
            shortcut.Arguments = args;
            shortcut.WorkingDirectory = Path.GetDirectoryName(target);
            shortcut.IconLocation = target + ",0";
            shortcut.Save();
        }
        finally
        {
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shortcut);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shell);
        }
    }
}
