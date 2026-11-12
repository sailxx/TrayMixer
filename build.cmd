@echo off
cd /d "%~dp0"
set CSC="%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:winexe /platform:x64 /optimize+ /debug- /win32manifest:app.manifest /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Management.dll
rem The icon is drawn by the app itself (--make-icon), so build twice if it is missing
if not exist TrayMixer.ico (
  %CSC% /out:TrayMixer.exe TrayMixer.cs Audio.cs Brightness.cs BlueLight.cs || exit /b 1
  start "" /wait TrayMixer.exe --make-icon TrayMixer.ico
)
%CSC% /out:TrayMixer.exe /win32icon:TrayMixer.ico TrayMixer.cs Audio.cs Brightness.cs BlueLight.cs || exit /b 1

rem Build the installer (Setup.cs) using dotnet
dotnet build installer\Setup.cs -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:PublishTrimmed=true -p:DebugType=None -p:DebugSymbols=false -o bin\Release\net8.0\win-x64\installer\ || exit /b 1

copy bin\Release\net8.0\win-x64\installer\Setup.exe TrayMixer-Setup.exe >nul

echo.
echo Build complete:
echo   TrayMixer.exe        (portable, for manual use)
echo   TrayMixer-Setup.exe  (installer that downloads from GitHub releases)
