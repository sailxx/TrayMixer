using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

// Captures only values and files the installer owns, leaving unrelated state untouched.
internal sealed class InstallSnapshot
{
    private readonly List<Action> restore = new List<Action>();
    internal InstallSnapshot(string dir)
    {
        FileState(Path.Combine(dir, Program.ExeName));
        FileState(Path.Combine(dir, Program.UninstName));
        FileState(Path.Combine(Program.Desktop, "TrayMixer.lnk"));
        FileState(Path.Combine(Program.Menu, "TrayMixer.lnk"));
        FileState(Path.Combine(Program.Menu, "Uninstall TrayMixer.lnk"));
        using (var key = Registry.CurrentUser.OpenSubKey(Program.UninstallKey))
        {
            bool existed = key != null;
            var values = new List<Action<RegistryKey>>();
            if (existed)
                foreach (string name in key.GetValueNames())
                {
                    string n = name;
                    object value = key.GetValue(n, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
                    RegistryValueKind kind = key.GetValueKind(n);
                    values.Add(k => k.SetValue(n, value, kind));
                }
            restore.Add(delegate
            {
                Registry.CurrentUser.DeleteSubKeyTree(Program.UninstallKey, false);
                if (existed) using (var k = Registry.CurrentUser.CreateSubKey(Program.UninstallKey)) foreach (var set in values) set(k);
            });
        }
        ValueState(Program.RunKey, Program.AppName);
        ValueState(Program.ConfigKey, "Initialized");
    }
    private void FileState(string path)
    {
        byte[] bytes = File.Exists(path) ? File.ReadAllBytes(path) : null;
        restore.Add(delegate { if (bytes == null) { if (File.Exists(path)) File.Delete(path); } else File.WriteAllBytes(path, bytes); });
    }
    private void ValueState(string path, string name)
    {
        using (var key = Registry.CurrentUser.OpenSubKey(path))
        {
            object value = key == null ? null : key.GetValue(name, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
            RegistryValueKind kind = value == null ? RegistryValueKind.String : key.GetValueKind(name);
            restore.Add(delegate
            {
                using (var k = Registry.CurrentUser.CreateSubKey(path))
                {
                    if (value == null) k.DeleteValue(name, false);
                    else k.SetValue(name, value, kind);
                }
            });
        }
    }
    internal void Restore()
    {
        var errors = new List<Exception>();
        foreach (var action in restore) try { action(); } catch (Exception ex) { errors.Add(ex); }
        if (errors.Count != 0) throw new AggregateException("Rollback was incomplete. Keep the setup backup folder and retry Setup.", errors);
    }
}
