@echo off
cd /d "%~dp0"
set CSC="%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:winexe /platform:x64 /optimize+ /debug- /win32manifest:app.manifest /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Management.dll
rem The icon is drawn by the app itself (--make-icon), so build twice if it is missing
if not exist TrayMixer.ico (
  %CSC% /out:TrayMixer.exe TrayMixer.cs Audio.cs Brightness.cs || exit /b 1
  start "" /wait TrayMixer.exe --make-icon TrayMixer.ico
)
%CSC% /out:TrayMixer.exe /win32icon:TrayMixer.ico TrayMixer.cs Audio.cs Brightness.cs
