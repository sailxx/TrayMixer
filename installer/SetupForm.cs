using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

internal static class SetupLanguage
{
    internal static bool Russian;
    internal static string T(string en, string ru) { return Russian ? ru : en; }
}

internal class SetupWindow : Form
{
    internal SetupWindow()
    {
        Font = new Font("Segoe UI", 9);
        AutoScaleDimensions = new SizeF(96, 96);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = SystemColors.Control;
        ForeColor = SystemColors.ControlText;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
    }
    internal static string T(string en, string ru) { return SetupLanguage.T(en, ru); }
    internal static Label Label(string text, int x, int y, int width, int height, bool bold)
    {
        return new Label { Text = text, Location = new Point(x, y), Size = new Size(width, height), Font = new Font("Segoe UI", bold ? 12 : 9, bold ? FontStyle.Bold : FontStyle.Regular) };
    }
    internal static Button Button(string text, int x, int y, int width)
    {
        return new Button { Text = text, Location = new Point(x, y), Size = new Size(width, 30), UseVisualStyleBackColor = true };
    }
}

internal sealed class LanguageForm : SetupWindow
{
    private readonly RadioButton english = new RadioButton();
    private readonly RadioButton russian = new RadioButton();
    private readonly Button next;
    internal LanguageForm()
    {
        Text = "Setup Language / Язык установки";
        ClientSize = new Size(400, 220);
        Controls.Add(Label("Select the language to use during installation.\nВыберите язык установщика.", 22, 20, 356, 44, false));
        english.Text = "English";
        english.SetBounds(25, 80, 350, 26);
        russian.Text = "Русский";
        russian.SetBounds(25, 112, 350, 26);
        Controls.Add(english); Controls.Add(russian);
        english.Checked = !SetupLanguage.Russian;
        russian.Checked = SetupLanguage.Russian;
        next = Button("OK", 190, 170, 88);
        next.Click += delegate { SetupLanguage.Russian = russian.Checked; DialogResult = DialogResult.OK; };
        var cancel = Button("Cancel / Отмена", 286, 170, 94);
        cancel.DialogResult = DialogResult.Cancel;
        Controls.Add(next); Controls.Add(cancel);
        AcceptButton = next; CancelButton = cancel;
    }
}

internal sealed class SetupForm : SetupWindow
{
    private readonly TextBox folder = new TextBox();
    private readonly CheckBox auto = new CheckBox();
    private readonly CheckBox launch = new CheckBox();
    private readonly Button install, browse, cancel, back;
    private readonly Label status;
    private readonly ProgressBar progress = new ProgressBar();
    private readonly Panel welcome = new Panel(), options = new Panel(), complete = new Panel();
    private readonly Label completion;
    private readonly Version bundled;
    private bool busy, finished;
    private int page;

    internal SetupForm(string directory)
    {
        Text = T("TrayMixer Setup", "Установка TrayMixer");
        ClientSize = new Size(540, 410);
        string temp = Path.Combine(Path.GetTempPath(), "TrayMixer-version-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temp);
        try { bundled = Program.VersionOf(Program.ExtractPayload(temp)); }
        finally { Directory.Delete(temp, true); }
        foreach (var panel in new[] { welcome, options, complete })
        {
            panel.SetBounds(0, 0, 540, 350);
            panel.BackColor = SystemColors.Window;
            panel.ForeColor = SystemColors.WindowText;
            Controls.Add(panel);
        }
        welcome.Controls.Add(Label(T("Welcome to the TrayMixer Setup Wizard", "Вас приветствует мастер установки TrayMixer"), 28, 28, 482, 60, true));
        welcome.Controls.Add(Label(T("This will install TrayMixer " , "Мастер установит TrayMixer ") + bundled.ToString(3) + T(" on your computer.\n\nYour existing settings will be kept.\nNo administrator rights or internet connection are required.\n\nClick Next to continue, or Cancel to exit Setup.", " на ваш компьютер.\n\nВаши настройки будут сохранены.\nПрава администратора и интернет не требуются.\n\nНажмите «Далее», чтобы продолжить, или «Отмена», чтобы выйти."), 28, 112, 482, 200, false));
        options.Controls.Add(Label(T("Installation options", "Параметры установки"), 24, 18, 490, 32, true));
        status = Label("", 24, 62, 490, 42, false);
        options.Controls.Add(status);
        options.Controls.Add(Label(T("Destination folder:", "Папка установки:"), 24, 115, 490, 22, false));
        folder.SetBounds(24, 143, 380, 25);
        folder.Text = directory;
        options.Controls.Add(folder);
        browse = Button(T("Browse…", "Обзор…"), 414, 140, 102);
        browse.Click += delegate {
            using (var picker = new FolderBrowserDialog { SelectedPath = folder.Text, Description = T("Choose installation folder", "Выберите папку установки") })
                if (picker.ShowDialog(this) == DialogResult.OK) folder.Text = picker.SelectedPath;
        };
        options.Controls.Add(browse);
        auto.Text = T("Run TrayMixer at Windows startup", "Запускать TrayMixer при входе в Windows");
        auto.Checked = Program.DefaultAutostart(directory);
        auto.SetBounds(24, 190, 492, 26);
        options.Controls.Add(auto);
        launch.Text = T("Launch TrayMixer when finished", "Запустить TrayMixer после установки");
        launch.Checked = true;
        launch.SetBounds(24, 223, 492, 26);
        options.Controls.Add(launch);
        options.Controls.Add(Label(T("Setup creates shortcuts on the desktop and in the Start menu.", "Мастер создаст ярлыки на рабочем столе и в меню «Пуск»."), 24, 268, 492, 42, false));
        progress.SetBounds(24, 320, 492, 16);
        progress.Visible = false;
        options.Controls.Add(progress);
        complete.Controls.Add(Label(T("Completing the TrayMixer Setup Wizard", "Завершение установки TrayMixer"), 28, 28, 482, 60, true));
        completion = Label("", 28, 112, 482, 190, false);
        complete.Controls.Add(completion);
        Controls.Add(new Label { BorderStyle = BorderStyle.Fixed3D, Location = new Point(0, 350), Size = new Size(540, 2) });
        back = Button(T("< Back", "< Назад"), 220, 369, 94);
        back.Click += delegate { page = 0; ShowPage(); };
        install = Button(T("Next >", "Далее >"), 320, 369, 94);
        install.Click += async delegate {
            if (finished) { Close(); return; }
            if (page == 0) { page = 1; ShowPage(); return; }
            await Install();
        };
        cancel = Button(T("Cancel", "Отмена"), 426, 369, 94);
        cancel.Click += delegate { Close(); };
        Controls.Add(back); Controls.Add(install); Controls.Add(cancel);
        folder.TextChanged += delegate { RefreshAction(); };
        AcceptButton = install; CancelButton = cancel;
        FormClosing += delegate(object sender, FormClosingEventArgs e) { if (busy) e.Cancel = true; };
        ShowPage();
    }
    private void ShowPage()
    {
        welcome.Visible = page == 0;
        options.Visible = page == 1;
        complete.Visible = page == 2;
        back.Visible = page == 1;
        cancel.Visible = page != 2;
        if (page == 0) { install.Text = T("Next >", "Далее >"); install.Enabled = true; }
        else if (page == 1) RefreshAction();
        else { install.Text = T("Finish", "Готово"); install.Enabled = true; }
    }
    private void RefreshAction()
    {
        if (page != 1 || busy) return;
        try
        {
            string path = Path.GetFullPath(folder.Text);
            Version current = Program.VersionOf(Path.Combine(path, Program.ExeName));
            bool exists = current > new Version(0, 0, 0, 0);
            install.Text = !exists ? T("Install", "Установить") : current < bundled ? T("Update", "Обновить") : T("Reinstall", "Переустановить");
            install.Enabled = current <= bundled;
            status.Text = current > bundled ? T("A newer version is installed. Download the latest Setup.", "Установлена более новая версия. Скачайте последний установщик.") : exists
                ? T("Installed: ", "Установлена версия: ") + current.ToString(3) + T(". Your preferences will be preserved.", ". Ваши настройки будут сохранены.")
                : T("Choose where to install TrayMixer and select additional options.", "Выберите папку установки TrayMixer и дополнительные параметры.");
        }
        catch { install.Enabled = false; status.Text = T("Choose a valid installation folder.", "Укажите корректную папку установки."); }
    }
    private async Task Install()
    {
        busy = true;
        install.Enabled = cancel.Enabled = back.Enabled = browse.Enabled = folder.Enabled = auto.Enabled = launch.Enabled = false;
        progress.Visible = true; progress.Style = ProgressBarStyle.Marquee;
        status.Text = T("Installing TrayMixer…", "Установка TrayMixer…");
        try
        {
            // Shortcut creation requires the STA UI thread.
            await Task.Delay(100);
            Program.Install(folder.Text, auto.Checked, launch.Checked);
            completion.Text = T("TrayMixer has been installed on your computer.\n\nYou can open it from the desktop, the Start menu or the system tray.\n\nClick Finish to close Setup.", "TrayMixer установлен на ваш компьютер.\n\nОткрыть программу можно с рабочего стола, из меню «Пуск» или трея.\n\nНажмите «Готово», чтобы закрыть мастер установки.");
            finished = true; page = 2;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, T("Installation failed", "Не удалось установить"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            busy = false; progress.Visible = false;
            cancel.Enabled = back.Enabled = browse.Enabled = folder.Enabled = auto.Enabled = launch.Enabled = true;
            ShowPage();
        }
    }
}
