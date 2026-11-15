using System;
using System.IO;
using System.Reflection;
using Microsoft.Win32;

internal static class InstallerTests
{
    [STAThread]
    private static int Main()
    {
        System.Windows.Forms.Application.EnableVisualStyles();
        System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
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
            TestLanguageFlow(root);
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
                Check(button.Text == "Next >", "wizard opens with welcome");
                button.PerformClick();
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
            // Failed recovery must preserve the installation for a later retry.
            File.Copy(Path.Combine(Path.GetDirectoryName(stub), "FailingResetStub.exe"), exe, true);
            byte[] beforeFailure = File.ReadAllBytes(exe);
            bool resetUpdateRefused = false;
            try { Program.Install(app, true, false); }
            catch (IOException) { resetUpdateRefused = true; }
            Check(resetUpdateRefused && Convert.ToBase64String(File.ReadAllBytes(exe)) == Convert.ToBase64String(beforeFailure), "failed gamma reset prevents app replacement");
            Check(!Program.DefaultAutostart(app), "failed gamma reset preserves startup choice");
            Check(Directory.GetDirectories(app, ".setup-*").Length == 0, "failed gamma reset cleans staging");
            bool resetUninstallRefused = false;
            try { Program.Uninstall(app); }
            catch (IOException) { resetUninstallRefused = true; }
            Check(resetUninstallRefused && File.Exists(exe) && File.Exists(Path.Combine(app, "Uninstall.exe")), "failed gamma reset prevents uninstall");
            Check(File.Exists(Path.Combine(Program.Desktop, "TrayMixer.lnk")) && File.Exists(Path.Combine(Program.Menu, "TrayMixer.lnk")), "failed gamma reset preserves shortcuts");
            using (var reg = Registry.CurrentUser.OpenSubKey(Program.UninstallKey)) Check(reg != null && (string)reg.GetValue("DisplayVersion") == "2.6.0", "failed gamma reset preserves registration");
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
    private static T Field<T>(object form, string name) where T : class
    {
        return form.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form) as T;
    }
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool PrintWindow(IntPtr window, IntPtr dc, uint flags);
    private static void Capture(System.Windows.Forms.Form form, string name)
    {
        form.Activate();
        form.Refresh();
        DateTime paintDeadline = DateTime.UtcNow.AddMilliseconds(350);
        while (DateTime.UtcNow < paintDeadline) { System.Windows.Forms.Application.DoEvents(); System.Threading.Thread.Sleep(15); }
        using (var image = new System.Drawing.Bitmap(form.Width, form.Height))
        using (var graphics = System.Drawing.Graphics.FromImage(image))
        {
            form.DrawToBitmap(image, new System.Drawing.Rectangle(0, 0, image.Width, image.Height));
            Console.WriteLine("PASS: form-only render " + name);
            image.Save(Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), name + ".png"));
        }
    }
    private static void TestLanguageFlow(string root)
    {
        foreach (bool ru in new[] { false, true })
        {
            SetupLanguage.Russian = false;
            using (var language = new LanguageForm())
            {
                language.Show();
                Field<System.Windows.Forms.RadioButton>(language, ru ? "russian" : "english").Checked = true;
                Capture(language, ru ? "language-ru" : "language-en");
                Field<System.Windows.Forms.Button>(language, "next").PerformClick();
                Check(language.DialogResult == System.Windows.Forms.DialogResult.OK && SetupLanguage.Russian == ru, "language selection " + ru);
            }
            using (var form = new SetupForm(Path.Combine(root, "App")))
            {
                form.Show();
                var button = Field<System.Windows.Forms.Button>(form, "install");
                Check(button.Text == (ru ? "Далее >" : "Next >"), "localized welcome");
                Capture(form, ru ? "welcome-ru" : "welcome-en");
                button.PerformClick();
                Check(button.Text == (ru ? "Установить" : "Install"), "localized fresh install");
                Field<System.Windows.Forms.Button>(form, "back").PerformClick();
                Check(button.Text == (ru ? "Далее >" : "Next >"), "back returns to welcome");
                button.PerformClick();
                Check(Field<System.Windows.Forms.CheckBox>(form, "launch").Text == (ru ? "Запустить TrayMixer после установки" : "Launch TrayMixer when finished"), "localized options");
                Capture(form, ru ? "setup-ru" : "setup-en");
                Field<System.Windows.Forms.CheckBox>(form, "launch").Checked = false;
                button.PerformClick();
                string finish = ru ? "Готово" : "Finish";
                DateTime deadline = DateTime.UtcNow.AddSeconds(10);
                while ((button.Text != finish || !button.Enabled) && DateTime.UtcNow < deadline) { System.Windows.Forms.Application.DoEvents(); System.Threading.Thread.Sleep(20); }
                Check(button.Text == finish, "localized installation completes");
                Capture(form, ru ? "finished-ru" : "finished-en");
                form.Activate();
                button.Focus();
                button.PerformClick();
                System.Windows.Forms.Application.DoEvents();
                Check(!form.Visible, "finish closes installer");
            }
            Program.Uninstall(Path.Combine(root, "App"));
        }
        using (var language = new LanguageForm()) { language.Show(); language.Close(); Check(language.DialogResult != System.Windows.Forms.DialogResult.OK, "language cancellation does not continue"); }
        SetupLanguage.Russian = false;
    }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception("FAIL: " + message);
        Console.WriteLine("PASS: " + message);
    }
}
