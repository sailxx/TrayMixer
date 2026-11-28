@echo off
setlocal
cd /d "%~dp0" || exit /b 1
set CSC="%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
set APPFLAGS=/noconfig /nologo /target:winexe /r:System.dll /r:System.Core.dll /platform:x64 /optimize+ /debug- /win32manifest:app.manifest /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Management.dll
set WINMD=%SystemRoot%\System32\WinMetadata
set WINRT=/r:"%WINMD%\Windows.Media.winmd" /r:"%WINMD%\Windows.Foundation.winmd" /r:"%WINMD%\Windows.Devices.winmd" /r:"%SystemRoot%\Microsoft.NET\assembly\GAC_MSIL\System.Runtime\v4.0_4.0.0.0__b03f5f7f11d50a3a\System.Runtime.dll"
set WPF=%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\WPF
set AUTOMATION=/r:"%WPF%\UIAutomationClient.dll" /r:"%WPF%\UIAutomationTypes.dll" /r:"%WPF%\WindowsBase.dll"
set SOURCES=TrayMixer.cs Audio.cs Brightness.cs BlueLight.cs HeadsetBattery.cs WindowsMedia.cs BatteryTray.cs BatteryStrip.cs FrostedWallpaper.cs
if not exist TrayMixer.ico (
  %CSC% %APPFLAGS% %WINRT% %AUTOMATION% /out:TrayMixer.exe %SOURCES% || exit /b 1
  start "" /wait "%~dp0TrayMixer.exe" --make-icon TrayMixer.ico || exit /b 1
)
%CSC% %APPFLAGS% %WINRT% %AUTOMATION% /out:TrayMixer.exe /win32icon:TrayMixer.ico %SOURCES% || exit /b 1
rem Embed the exact app build in the offline installer.
%CSC% /noconfig /nologo /target:winexe /r:System.dll /r:System.Core.dll /platform:x64 /optimize+ /debug- /win32manifest:installer\setup.manifest /win32icon:TrayMixer.ico /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:Microsoft.CSharp.dll /resource:TrayMixer.exe,TrayMixer.Payload /out:TrayMixer-Setup.exe installer\Setup.cs installer\SetupForm.cs installer\InstallSnapshot.cs || exit /b 1
echo Build complete: TrayMixer-Setup.exe (offline installer)
