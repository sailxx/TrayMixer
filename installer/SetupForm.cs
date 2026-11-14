using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

internal sealed class SetupForm : Form
{
    private readonly TextBox folder = new TextBox();
    private readonly CheckBox auto = new CheckBox();
    private readonly CheckBox launch = new CheckBox();
    private readonly Button install = new Button();
    private readonly Button browse = new Button();
    private readonly Label status = new Label();
    private readonly ProgressBar progress = new ProgressBar();
    private readonly Version bundled;
    private bool busy, finished;

    internal SetupForm(string directory)
    {
        Text = "TrayMixer Setup";
        ClientSize = new Size(560, 390);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10);
        BackColor = Color.White;
        AutoScaleMode = AutoScaleMode.Dpi;
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        string temp = Path.Combine(Path.GetTempPath(), "TrayMixer-version-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temp);
        try { bundled = Program.VersionOf(Program.ExtractPayload(temp)); }
        finally { Directory.Delete(temp, true); }
        Controls.Add(new Label { Text = "TrayMixer", Font = new Font("Segoe UI", 24, FontStyle.Bold), AutoSize = true, Location = new Point(26, 22) });
        Controls.Add(new Label { Text = "Setup " + bundled.ToString(3) + "  •  No administrator rights required", AutoSize = true, Location = new Point(29, 77) });
        status.SetBounds(30, 112, 500, 50);
        Controls.Add(status);
        Controls.Add(new Label { Text = "Installation folder", AutoSize = true, Location = new Point(29, 168) });
        folder.SetBounds(30, 194, 390, 30);
        folder.Text = directory;
        Controls.Add(folder);
        browse.Text = "Browse…";
        browse.SetBounds(430, 193, 100, 30);
        browse.Click += delegate
        {
            using (var picker = new FolderBrowserDialog { SelectedPath = folder.Text })
                if (picker.ShowDialog(this) == DialogResult.OK) folder.Text = picker.SelectedPath;
        };
        Controls.Add(browse);
        folder.TextChanged += delegate { RefreshAction(); };
        auto.Text = "Run TrayMixer at Windows startup";
        auto.Checked = Program.DefaultAutostart(directory);
        auto.SetBounds(30, 239, 450, 27);
        Controls.Add(auto);
        launch.Text = "Launch TrayMixer when finished";
        launch.Checked = true;
        launch.SetBounds(30, 270, 450, 27);
        Controls.Add(launch);
        Controls.Add(new Label { Text = "Desktop and Start menu shortcuts are created automatically.", AutoSize = true, Location = new Point(29, 307), ForeColor = Color.FromArgb(75, 75, 75) });
        progress.SetBounds(30, 347, 350, 12);
        progress.Visible = false;
        Controls.Add(progress);
        install.SetBounds(400, 337, 130, 36);
        install.Click += async delegate { await Install(); };
        Controls.Add(install);
        AcceptButton = install;
        FormClosing += delegate(object sender, FormClosingEventArgs e) { if (busy) e.Cancel = true; };
        RefreshAction();
    }

    private void RefreshAction()
    {
        try
        {
            Version current = Program.VersionOf(Path.Combine(folder.Text, Program.ExeName));
            bool exists = current > new Version(0, 0, 0, 0);
            install.Text = !exists ? "Install" : current < bundled ? "Update" : "Reinstall";
            install.Enabled = current <= bundled;
            status.Text = current > bundled ? "A newer version is installed. Download the latest Setup." : exists
                ? "Installed: " + current.ToString(3) + ". Your preferences will be preserved."
                : "Install TrayMixer for your Windows account. No internet connection needed.";
        }
        catch { install.Enabled = false; status.Text = "Choose a valid installation folder."; }
    }

    private async Task Install()
    {
        if (finished) { Close(); return; }
        busy = true;
        install.Enabled = browse.Enabled = folder.Enabled = auto.Enabled = launch.Enabled = false;
        progress.Visible = true;
        progress.Style = ProgressBarStyle.Marquee;
        status.Text = "Installing TrayMixer…";
        try
        {
            // Keep COM shortcut creation on the STA UI thread.
            await Task.Delay(100);
            Program.Install(folder.Text, auto.Checked, launch.Checked);
            status.Text = "TrayMixer " + bundled.ToString(3) + " is installed.\nFind it on your desktop, in Start, or in the system tray.";
            finished = true;
            install.Text = "Finish";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Installation failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            browse.Enabled = folder.Enabled = auto.Enabled = launch.Enabled = true;
            RefreshAction();
        }
        finally { busy = false; progress.Visible = false; install.Enabled = true; }
    }
}
