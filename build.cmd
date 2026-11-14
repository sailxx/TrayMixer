@echo off
setlocal
cd /d "%~dp0"
set CSC="%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
set APPFLAGS=/nologo /target:winexe /platform:x64 /optimize+ /debug- /win32manifest:app.manifest /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Management.dll
if not exist TrayMixer.ico (
  %CSC% %APPFLAGS% /out:TrayMixer.exe TrayMixer.cs Audio.cs Brightness.cs BlueLight.cs || exit /b 1
  start "" /wait TrayMixer.exe --make-icon TrayMixer.ico
)
%CSC% %APPFLAGS% /out:TrayMixer.exe /win32icon:TrayMixer.ico TrayMixer.cs Audio.cs Brightness.cs BlueLight.cs || exit /b 1
rem Embed the exact app build: the release needs only Setup, with no network dependency.
%CSC% /nologo /target:winexe /platform:x64 /optimize+ /debug- /win32manifest:installer\setup.manifest /win32icon:TrayMixer.ico /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:Microsoft.CSharp.dll /resource:TrayMixer.exe,TrayMixer.Payload /out:TrayMixer-Setup.exe installer\Setup.cs installer\SetupForm.cs installer\InstallSnapshot.cs || exit /b 1
echo Build complete: TrayMixer-Setup.exe (offline installer)
