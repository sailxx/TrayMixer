@echo off
rem Regenerates README images into assets\readme (requires a built TrayMixer.exe)
cd /d "%~dp0.."
"%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /out:"%TEMP%\ReadmeArt.exe" /r:System.Drawing.dll tools\ReadmeArt.cs tools\ReadmeLang.cs || exit /b 1
start "" /wait TrayMixer.exe --render "%TEMP%\traymixer-popup.png"
"%TEMP%\ReadmeArt.exe" assets\readme "%TEMP%\traymixer-popup.png"
