@echo off
rem Regenerates README images into assets\readme: SVG plates (Node.js) and the window screenshot (requires a built TrayMixer.exe)
cd /d "%~dp0readme"
call npm install --silent || exit /b 1
node build.mjs || exit /b 1
cd /d "%~dp0.."
"%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /out:"%TEMP%\ReadmeArt.exe" /r:System.Drawing.dll tools\ReadmeArt.cs || exit /b 1
start "" /wait TrayMixer.exe --render "%TEMP%\traymixer-popup.png"
"%TEMP%\ReadmeArt.exe" "%TEMP%\traymixer-popup.png" assets\readme\screenshot.png
