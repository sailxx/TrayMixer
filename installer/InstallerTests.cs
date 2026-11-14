using System;
using System.IO;
using System.Reflection;
using Microsoft.Win32;

internal static class InstallerTests
{
    [STAThread]
    private static int Main()
    {
        string root = Path.Combine(Path.GetTempPath(), "TrayMixer-tests-" + Guid.NewGuid().ToString("N"));
        string key = @"Software\TrayMixerInstallerTests\" + Guid.NewGuid().ToString("N");
        Directory.CreateDirectory(root);
        Program.UninstallKey = key + @"\Uninstall";
        Program.RunKey = key + @"\Run";
        Program.ConfigKey = key + @"\Config";
        Program.Desktop = Path.Combine(root, "Desktop");
        Program.Menu = Path.Combine(root, "Menu");
        Directory.CreateDirectory(Program.Desktop);
        string app = Path.Combine(root, "App"), exe = Path.Combine(app, "TrayMixer.exe");
        try
        {
            using (var cfg = Registry.CurrentUser.CreateSubKey(Program.ConfigKey)) cfg.SetValue("Preference", "preserved");
            Program.Install(app, false, false);
            Check(File.Exists(exe), "fresh install");
            Check(File.Exists(Path.Combine(app, "Uninstall.exe")), "uninstaller");
            Check(File.Exists(Path.Combine(Program.Desktop, "TrayMixer.lnk")), "desktop shortcut");
            Check(File.Exists(Path.Combine(Program.Menu, "TrayMixer.lnk")), "Start menu shortcut");
            using (var reg = Registry.CurrentUser.OpenSubKey(Program.UninstallKey))
                Check(reg.GetValue("SystemComponent") == null && reg.GetValue("QuietUninstallString") != null, "visible Windows uninstall registration");
            Check(!Program.DefaultAutostart(app), "autostart disabled");
            string older = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "OlderStub.exe");
            File.Copy(older, exe, true);
            Program.Install(app, false, false);
            Check(Program.VersionOf(exe).Major == 2, "update older version");
            using (var form = new SetupForm(app))
            {
                form.Show();
                var flag = BindingFlags.Instance | BindingFlags.NonPublic;
                var button = (System.Windows.Forms.Button)typeof(SetupForm).GetField("install", flag).GetValue(form);
                Check(button.Text == "Reinstall", "GUI detects same-version reinstall");
                var launchBox = (System.Windows.Forms.CheckBox)typeof(SetupForm).GetField("launch", flag).GetValue(form);
                launchBox.Checked = false;
                button.PerformClick();
                DateTime deadline = DateTime.UtcNow.AddSeconds(10);
                while (button.Text != "Finish" && DateTime.UtcNow < deadline) { System.Windows.Forms.Application.DoEvents(); System.Threading.Thread.Sleep(20); }
                Check(button.Text == "Finish", "GUI reinstall button completes");
                form.Close();
            }
            // A harmless executable replaces the app for lifecycle checks: never touch real display gamma.
            string stub = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "TestStub.exe");
            File.Copy(stub, exe, true);
            Program.Install(app, true, false);
            Check(Program.DefaultAutostart(app), "same-version reinstall and enable autostart");
            File.Copy(Path.Combine(Path.GetDirectoryName(stub), "NewerStub.exe"), exe, true);
            bool downgradeRefused = false;
            try { Program.Install(app, true, false); }
            catch (InvalidOperationException) { downgradeRefused = true; }
            Check(downgradeRefused && Program.VersionOf(exe).Major == 99, "downgrade refused without replacing app");
            File.Copy(stub, exe, true);
            Program.Install(app, false, false);
            Check(!Program.DefaultAutostart(app), "repeat install preserves disabled choice");
            string marker = Path.Combine(app, "user-file.txt");
            File.WriteAllText(marker, "keep");
            bool refused = false;
            try { Program.Install(Path.Combine(root, "Other"), false, false); }
            catch (InvalidOperationException) { refused = true; }
            Check(refused, "registered custom folder protected");
            bool wrongUninstallRefused = false;
            try { Program.Uninstall(Path.Combine(root, "Other")); }
            catch (InvalidOperationException) { wrongUninstallRefused = true; }
            Check(wrongUninstallRefused && File.Exists(exe), "wrong uninstall folder refused");
            string originalMenu = Program.Menu;
            Program.Menu = Path.Combine(root, "blocked-menu");
            File.WriteAllText(Program.Menu, "not a directory");
            File.Copy(older, exe, true);
            bool rollbackTriggered = false;
            try { Program.Install(app, true, false); }
            catch (IOException) { rollbackTriggered = true; }
            finally { Program.Menu = originalMenu; }
            Check(rollbackTriggered && Program.VersionOf(exe).Major == 1 && !Program.DefaultAutostart(app), "failed update rolls back app and startup choice");
            using (var reg = Registry.CurrentUser.OpenSubKey(Program.UninstallKey)) Check((string)reg.GetValue("DisplayVersion") == "2.6.0", "failed update preserves registration");
            // Use harmless test executable for the uninstall reset switch, too.
            File.Copy(stub, exe, true);
            Program.Uninstall(app);
            Check(!File.Exists(exe) && !File.Exists(Path.Combine(app, "Uninstall.exe")), "uninstall files");
            Check(File.Exists(marker), "uninstall preserves unrelated files");
            Check(!File.Exists(Path.Combine(Program.Desktop, "TrayMixer.lnk")), "uninstall desktop shortcut");
            using (var reg = Registry.CurrentUser.OpenSubKey(Program.UninstallKey)) Check(reg == null, "uninstall registration removed");
            using (var cfg = Registry.CurrentUser.OpenSubKey(Program.ConfigKey)) Check((string)cfg.GetValue("Preference") == "preserved", "preferences preserved");
            Console.WriteLine("PASS: isolated installer lifecycle");
            return 0;
        }
        finally
        {
            Registry.CurrentUser.DeleteSubKeyTree(key, false);
            Directory.Delete(root, true);
        }
    }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception("FAIL: " + message);
        Console.WriteLine("PASS: " + message);
    }
}
