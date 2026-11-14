@echo off
setlocal
cd /d "%~dp0.."
if not exist bin\installer-tests mkdir bin\installer-tests
set CSC="%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
%CSC% /nologo /target:exe /out:bin\installer-tests\TestStub.exe installer\TestStub.cs || exit /b 1
%CSC% /nologo /target:exe /define:OLDER /out:bin\installer-tests\OlderStub.exe installer\TestStub.cs || exit /b 1
%CSC% /nologo /target:exe /define:NEWER /out:bin\installer-tests\NewerStub.exe installer\TestStub.cs || exit /b 1
%CSC% /nologo /target:exe /main:InstallerTests /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:Microsoft.CSharp.dll /resource:bin\installer-tests\TestStub.exe,TrayMixer.Payload /out:bin\installer-tests\InstallerTests.exe installer\Setup.cs installer\SetupForm.cs installer\InstallSnapshot.cs installer\InstallerTests.cs || exit /b 1
bin\installer-tests\InstallerTests.exe
exit /b %errorlevel%
