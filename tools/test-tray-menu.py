"""Build and exercise the tray menu without starting audio, gamma or autostart."""
import os
from pathlib import Path
import subprocess

root = Path(__file__).resolve().parents[1]
os.chdir(root)
out = root / 'bin' / 'menu-tests'
out.mkdir(parents=True, exist_ok=True)
csc = Path(os.environ['WINDIR']) / 'Microsoft.NET' / 'Framework64' / 'v4.0.30319' / 'csc.exe'
common = [str(csc), '/nologo', '/platform:x64', '/optimize+', '/r:System.Windows.Forms.dll', '/r:System.Drawing.dll', '/r:System.Management.dll']
sources = ['TrayMixer.cs', 'Audio.cs', 'Brightness.cs', 'bin/stable-installer/BlueLight.cs']
subprocess.run(common + ['/target:exe', '/main:TrayMenuTests', '/out:' + str(out / 'TrayMenuTests.exe')] + sources + [str(root / 'tools' / 'TrayMenuTests.cs')], check=True)
subprocess.run([str(out / 'TrayMenuTests.exe'), str(out / 'tray-menu.png')], check=True, timeout=20)
subprocess.run(common + ['/target:winexe', '/win32manifest:app.manifest', '/win32icon:TrayMixer.ico', '/out:' + str(out / 'TrayMixer.exe')] + sources, check=True)
subprocess.run([str(csc), '/nologo', '/target:winexe', '/platform:x64', '/optimize+', '/win32manifest:installer/setup.manifest', '/win32icon:TrayMixer.ico', '/r:System.Windows.Forms.dll', '/r:System.Drawing.dll', '/r:Microsoft.CSharp.dll', '/resource:' + str(out / 'TrayMixer.exe') + ',TrayMixer.Payload', '/out:' + str(out / 'TrayMixer-Setup.exe'), str(root / 'installer' / 'Setup.cs'), str(root / 'installer' / 'SetupForm.cs'), str(root / 'installer' / 'InstallSnapshot.cs')], check=True)
print('PASS: app and offline installer compiled')
