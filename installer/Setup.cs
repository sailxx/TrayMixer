// TrayMixer installer. Per-user, no administrator rights, no dependencies beyond .NET Framework 4.
// TrayMixer.exe is embedded as a resource (see build.cmd). The same exe is copied to the install
// folder as Uninstall.exe and removes everything this installer created.
//
// Command line:  TrayMixer-Setup.exe [/S] [/DIR=path] [/NOAUTOSTART] [/DESKTOP] [/LAUNCH] [/NOLAUNCH] [/uninstall]
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

[assembly: AssemblyTitle("TrayMixer Setup")]
[assembly: AssemblyProduct("TrayMixer")]
[assembly: AssemblyCompany("sailxx")]

namespace TrayMixerSetup
{
    class Options
    {
        public bool Silent, Uninstall, Autostart = true, Desktop;
        public bool? Launch;
        public string Dir;
    }

    static class Core
    {
        public const string App = "TrayMixer", ExeName = "TrayMixer.exe", UninstName = "Uninstall.exe";
        const string UninstKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\TrayMixer";
        const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        const string CfgKey = @"Software\TrayMixer";

        public static string DefaultDir
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\TrayMixer"); }
        }

        static string StartMenuLnk { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), App + ".lnk"); } }
        static string DesktopLnk { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), App + ".lnk"); } }

        // Folder of an existing installation made by this installer, or null
        public static string InstalledDir()
        {
            try
            {
                using (var k = Registry.CurrentUser.OpenSubKey(UninstKey))
                {
                    var d = k == null ? null : k.GetValue("InstallLocation") as string;
                    return !string.IsNullOrEmpty(d) && File.Exists(Path.Combine(d, ExeName)) ? d : null;
                }
            }
            catch { return null; }
        }

        static byte[] AppBytes()
        {
            using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(ExeName))
            using (var ms = new MemoryStream()) { s.CopyTo(ms); return ms.ToArray(); }
        }

        public static string AppVersion()
        {
            try { var v = Assembly.ReflectionOnlyLoad(AppBytes()).GetName().Version; return v.Major + "." + v.Minor + "." + v.Build; }
            catch { return ""; }
        }

        // TrayMixer allows one instance only, so a running copy has to go before the exe can be replaced
        static void StopApp()
        {
            foreach (var p in Process.GetProcessesByName(App))
            {
                try { p.Kill(); p.WaitForExit(5000); } catch { }
                p.Dispose();
            }
        }

        // A killed TrayMixer can leave the blue light filter on the screens; the exe puts normal gamma back and quits
        static void ResetGamma(string exe)
        {
            try
            {
                var psi = new ProcessStartInfo(exe, "--reset-gamma") { UseShellExecute = false, CreateNoWindow = true };
                using (var p = Process.Start(psi))
                    if (!p.WaitForExit(8000)) p.Kill();
            }
            catch { }
        }

        static void Retry(Action a)
        {
            for (int i = 0; ; i++)
            {
                try { a(); return; }
                catch (IOException) { if (i >= 10) throw; Thread.Sleep(300); }
                catch (UnauthorizedAccessException) { if (i >= 10) throw; Thread.Sleep(300); }
            }
        }

        static void Shortcut(string lnk, string target, string dir)
        {
            var t = Type.GetTypeFromProgID("WScript.Shell");
            object sh = Activator.CreateInstance(t), l = null;
            try
            {
                l = t.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null, sh, new object[] { lnk });
                var lt = l.GetType();
                lt.InvokeMember("TargetPath", BindingFlags.SetProperty, null, l, new object[] { target });
                lt.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, l, new object[] { dir });
                lt.InvokeMember("IconLocation", BindingFlags.SetProperty, null, l, new object[] { target + ",0" });
                lt.InvokeMember("Description", BindingFlags.SetProperty, null, l, new object[] { "Volume and brightness for every device and app" });
                lt.InvokeMember("Save", BindingFlags.InvokeMethod, null, l, null);
            }
            finally
            {
                if (l != null) Marshal.FinalReleaseComObject(l);
                Marshal.FinalReleaseComObject(sh);
            }
        }

        public static string CheckDir(string dir)
        {
            if (string.IsNullOrWhiteSpace(dir)) return "Choose an install folder.";
            try
            {
                var full = Path.GetFullPath(dir);
                if (!Path.IsPathRooted(full) || string.Equals(full.TrimEnd('\\'), Path.GetPathRoot(full).TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
                    return "Choose a folder, not a drive root.";
            }
            catch { return "The install folder path is not valid."; }
            return null;
        }

        public static void Install(Options o)
        {
            var dir = Path.GetFullPath(o.Dir);
            var exe = Path.Combine(dir, ExeName);
            var bytes = AppBytes();
            StopApp();
            Directory.CreateDirectory(dir);
            Retry(delegate { File.WriteAllBytes(exe, bytes); });
            var uninst = Path.Combine(dir, UninstName);
            if (!string.Equals(Path.GetFullPath(Application.ExecutablePath), uninst, StringComparison.OrdinalIgnoreCase))
                Retry(delegate { File.Copy(Application.ExecutablePath, uninst, true); });

            using (var k = Registry.CurrentUser.CreateSubKey(UninstKey))
            {
                k.SetValue("DisplayName", App);
                k.SetValue("DisplayVersion", AppVersion());
                k.SetValue("Publisher", "sailxx");
                k.SetValue("DisplayIcon", exe);
                k.SetValue("InstallLocation", dir);
                k.SetValue("UninstallString", "\"" + uninst + "\" /uninstall");
                k.SetValue("QuietUninstallString", "\"" + uninst + "\" /uninstall /S");
                k.SetValue("URLInfoAbout", "https://github.com/sailxx/TrayMixer");
                k.SetValue("ModifyPath", "\"" + uninst + "\"");;                k.SetValue("NoModify",0, RegistryValueKind.DWord);
                k.SetValue("NoRepair", 0, RegistryValueKind.DWord);
                k.SetValue("EstimatedSize", (int)((new FileInfo(exe).Length + new FileInfo(uninst).Length) / 1024), RegistryValueKind.DWord);
            }
            // The app turns autostart on by itself at first launch; the choice made here replaces that
            using (var k = Registry.CurrentUser.CreateSubKey(CfgKey)) k.SetValue("Initialized", 1);
            using (var k = Registry.CurrentUser.CreateSubKey(RunKey))
                if (o.Autostart) k.SetValue(App, "\"" + exe + "\""); else k.DeleteValue(App, false);

            Shortcut(StartMenuLnk, exe, dir);
            if (o.Desktop) Shortcut(DesktopLnk, exe, dir);
            else try { File.Delete(DesktopLnk); } catch { }

            ResetGamma(exe);
            if (o.Launch ?? false) Process.Start(new ProcessStartInfo(exe) { UseShellExecute = false, WorkingDirectory = dir });
        }

        public static void Uninstall()
        {
            var dir = InstalledDir() ?? Path.GetDirectoryName(Application.ExecutablePath);
            var exe = Path.Combine(dir, ExeName);
            Environment.CurrentDirectory = Path.GetTempPath();
            StopApp();
            if (File.Exists(exe)) ResetGamma(exe);

            using (var k = Registry.CurrentUser.OpenSubKey(RunKey, true))
            {
                // Only our own entry: a portable copy somewhere else keeps its autostart
                var v = k == null ? null : k.GetValue(App) as string;
                if (v != null && string.Equals(v.Trim('"'), exe, StringComparison.OrdinalIgnoreCase)) k.DeleteValue(App, false);
            }
            try { File.Delete(StartMenuLnk); } catch { }
            try { File.Delete(DesktopLnk); } catch { }
            Registry.CurrentUser.DeleteSubKeyTree(UninstKey, false);
            Registry.CurrentUser.DeleteSubKeyTree(CfgKey, false);

            // Only the two files this installer wrote are removed, never the whole folder
            Retry(delegate { if (File.Exists(exe)) File.Delete(exe); });
            var self = Path.GetFullPath(Application.ExecutablePath);
            var uninst = Path.Combine(dir, UninstName);
            if (!string.Equals(self, uninst, StringComparison.OrdinalIgnoreCase)) { try { File.Delete(uninst); } catch { } return; }
            // The running Uninstall.exe cannot delete itself: a short helper does it after this process exits
            var cmd = "/c ping 127.0.0.1 -n 3 >nul & del /f /q \"" + uninst + "\" 2>nul & rmdir \"" + dir + "\" 2>nul";
            Process.Start(new ProcessStartInfo("cmd.exe", cmd) { CreateNoWindow = true, UseShellExecute = false, WorkingDirectory = Path.GetTempPath() });
        }
    }

    class SetupForm : Form
    {
        readonly TextBox dirBox = new TextBox();
        readonly CheckBox autoBox = new CheckBox(), deskBox = new CheckBox(), launchBox = new CheckBox();
        readonly Button go = new Button(), cancel = new Button();
        readonly Label status = new Label();
        readonly Options opt;
        bool done;

        public SetupForm(Options o)
        {
            opt = o;
            var existing = Core.InstalledDir();
            Text = "TrayMixer Setup"; Font = new Font("Segoe UI", 9f);
            AutoScaleMode = AutoScaleMode.Dpi; AutoScaleDimensions = new SizeF(96f, 96f);
            ClientSize = new Size(440, 300); FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false; StartPosition = FormStartPosition.CenterScreen;
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

            var title = new Label { Text = "TrayMixer " + Core.AppVersion(), Font = new Font("Segoe UI Semibold", 15f), Left = 20, Top = 16, Width = 400, Height = 32 };
            var sub = new Label { Text = existing == null ? "Volume and brightness for every device and app." : "TrayMixer is already installed. This will update it.", Left = 22, Top = 52, Width = 400, Height = 34 };
            var dl = new Label { Text = "Install to", Left = 22, Top = 96, Width = 200, Height = 18 };
            dirBox.SetBounds(22, 118, 310, 24); dirBox.Text = existing ?? Core.DefaultDir;
            var browse = new Button { Text = "Browse...", Left = 340, Top = 116, Width = 80, Height = 27 };
            browse.Click += delegate
            {
                using (var d = new FolderBrowserDialog { Description = "Choose the install folder", SelectedPath = dirBox.Text })
                    if (d.ShowDialog(this) == DialogResult.OK) dirBox.Text = Path.Combine(d.SelectedPath, Core.App);
            };
            Check(autoBox, "Start TrayMixer with Windows", 156, true);
            Check(deskBox, "Create a desktop shortcut", 180, fa lse); Check(launchBox, "Launch TrayMixer when setup finishes", 204, true);
            status.SetBounds(22, 232, 400, 20);
            go.SetBounds(244, 258, 84, 28); go.Text = existing == null ? "Install" : "Update";
            cancel.SetBounds(336, 258, 84, 28); cancel.Text = "Cancel";
            go.Click += delegate { if (done) Close(); else Run(); };
            cancel.Click += delegate { Close(); };
            AcceptButton = go; CancelButton = cancel;
            Shown += delegate { go.Focus(); }; // the controls cannot take focus before the form is shown
            Controls.AddRange(new Control[] { title, sub, dl, dirBox, browse, autoBox, deskBox, launchBox, status, go, cancel });
        }

        void Check(CheckBox c, string text, int top, bool on)
        {
            c.Text = text; c.Checked = on; c.SetBounds(22, top, 380, 22);
        }

        void Run()
        {
            var err = Core.CheckDir(dirBox.Text);
            if (err != null) { MessageBox.Show(this, err, "TrayMixer Setup", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            opt.Dir = dirBox.Text; opt.Autostart = autoBox.Checked; opt.Desktop = deskBox.Checked; opt.Launch = launchBox.Checked;
            foreach (Control c in Controls) if (!(c is Label)) c.Enabled = false;
            status.Text = "Installing..."; Cursor = Cursors.WaitCursor; Application.DoEvents();
            try
            {
                Core.Install(opt);
                Cursor = Cursors.Default; status.Text = "TrayMixer is installed.";
                done = true; go.Text = "Close"; go.Enabled = true; go.Focus();
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                MessageBox.Show(this, "Setup could not finish:\n" + ex.Message, "TrayMixer Setup", MessageBoxButtons.OK, MessageBoxIcon.Error);
                foreach (Control c in Controls) c.Enabled = true;
                status.Text = "";
            }
        }
    }

    static class Program
    {
        [DllImport("kernel32.dll")] static extern bool SetDefaultDllDirectories(int flags);

        [STAThread]
        static int Main(string[] args)
        {
            try { SetDefaultDllDirectories(0x800); } catch { } // system libraries only from System32
            var o = new Options();
            if (string.Equals(Path.GetFileName(Application.ExecutablePath), Core.UninstName, StringComparison.OrdinalIgnoreCase)) o.Uninstall = true;
            foreach (var a in args)
            {
                var u = a.ToUpperInvariant();
                if (u == "/S") o.Silent = true;
                else if (u == "/UNINSTALL") o.Uninstall = true;
                else if (u == "/NOAUTOSTART") o.Autostart = false;
                else if (u == "/DESKTOP") o.Desktop = true;
                else if (u == "/LAUNCH") o.Launch = true;
                else if (u == "/NOLAUNCH") o.Launch = false;
                else if (u.StartsWith("/DIR=")) o.Dir = a.Substring(5).Trim('"');
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                if (o.Uninstall)
                {
                    if (!o.Silent && MessageBox.Show("Remove TrayMixer and its saved settings from this PC?", "TrayMixer Setup", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return 0;
                    Core.Uninstall();
                    if (!o.Silent) MessageBox.Show("TrayMixer was removed.", "TrayMixer Setup", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return 0;
                }
                if (o.Silent)
                {
                    o.Dir = o.Dir ?? Core.InstalledDir() ?? Core.DefaultDir;
                    var err = Core.CheckDir(o.Dir);
                    if (err != null) return 2;
                    Core.Install(o);
                    return 0;
                }
                o.Dir = o.Dir ?? Core.DefaultDir;
                Application.Run(new SetupForm(o));
                return 0;
            }
            catch (Exception ex)
            {
                if (!o.Silent) MessageBox.Show("Setup failed:\n" + ex.Message, "TrayMixer Setup", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 1;
            }
        }
    }
}
