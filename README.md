**English** · [العربية](README.ar.md) · [简体中文](README.zh-CN.md) · [繁體中文](README.zh-TW.md) · [Čeština](README.cs.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [Ελληνικά](README.el.md) · [Bahasa Indonesia](README.id.md) · [Gaeilge](README.ga.md) · [Italiano](README.it.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Polski](README.pl.md) · [Română](README.ro.md) · [Русский](README.ru.md) · [Español](README.es.md) · [Українська](README.uk.md) · [اردو](README.ur.md) · [ئۇيغۇرچە](README.ug.md) · [Tiếng Việt](README.vi.md) · [Português](README.pt.md) · [Türkçe](README.tr.md)

<div align="center">

<img src="assets/readme/logo-blue.svg" width="128" alt="TrayMixer">

# TrayMixer

**Control the volume of every output and app from one small panel in your Windows 11 or Linux system tray.**

Turn down a game while keeping your call audible, switch between speakers and headphones, or adjust brightness, blue-light filtering and dimming on Windows.

**No telemetry or account.** Open source under Apache 2.0.

<img src="https://img.shields.io/badge/Windows-v3.0.2-2E9BFF" alt="Windows release v3.0.2">
<img src="https://img.shields.io/badge/Windows-11%20x64-2E9BFF" alt="Windows 11 x64">
<img src="https://img.shields.io/badge/Linux-v3.1.1%20preview-2E9BFF" alt="Linux preview v3.1.1">
<img src="https://img.shields.io/badge/Linux-AppImage%20%7C%20deb%20%7C%20rpm-2E9BFF" alt="Linux AppImage, DEB and RPM">
<img src="https://img.shields.io/badge/code-C%23-2E9BFF" alt="C sharp">
<img src="https://img.shields.io/badge/Linux%20UI-Avalonia-2E9BFF" alt="Avalonia for Linux">
<img src="https://img.shields.io/badge/license-Apache%202.0-2E9BFF" alt="Apache 2.0">
<img src="https://img.shields.io/github/stars/sailxx/TrayMixer?style=flat&amp;logo=github&amp;color=%232E9BFF" alt="GitHub stars">
<br>

<img src="assets/readme/hero-en.svg" width="100%" alt="TrayMixer banner with the Windows flyout, output and app volume, display controls and local preferences">

<a href="https://github.com/sailxx/TrayMixer/releases/download/v3.0.2/TrayMixer-Setup.exe"><img src="assets/readme/btn-windows-en.svg" height="52" alt="Download the Windows installer"></a>&nbsp;&nbsp;<a href="https://github.com/sailxx/TrayMixer/releases/tag/linux-v3.1.1"><img src="assets/readme/btn-linux-en.svg" height="52" alt="Download Linux packages"></a>&nbsp;&nbsp;<a href="https://github.com/sailxx/TrayMixer/releases/download/v3.0.2/TrayMixer.exe"><img src="assets/readme/btn-portable-en.svg" height="52" alt="Download the portable Windows executable"></a>&nbsp;&nbsp;<a href="https://github.com/sailxx/TrayMixer/releases"><img src="assets/readme/btn-new-en.svg" height="52" alt="Read the release notes"></a>

</div>

<br>

<img src="assets/readme/features-en.svg" width="100%" alt="Features: separate output and app volume, Windows display controls, tray controls, themes and local preferences">

<details>
<summary><b>More about the features</b></summary>

### Outputs and app volume

Each output has its own slider and mute control. Click an output name to select it as the default. On Linux, this applies to new playback and each app stream appears separately.

### Windows display controls

Adjust brightness on supported monitors, warm the display with the blue-light filter, and use software dimming from the same mixer. [See the Windows screenshot at full size](assets/readme/screenshot.png).

### Tray controls

Left-click the Windows tray icon to show or hide the mixer and middle-click it to toggle mute on the default output. Scroll over a row to adjust it by two points, click an audio row’s icon to mute, or right-click a row to hide it. Restore hidden rows from the tray menu. Press <kbd>Esc</kbd> or click outside to close the Windows flyout. Linux tray clicks depend on your desktop.

### Appearance, startup and preferences

Windows uses a rounded flyout. Linux follows GNOME light, dark and accent colors or offers Charcoal, Nord, Catppuccin and Light. You can enable startup at login from the tray menu. Preferences are stored locally.

</details>

<br>

<img src="assets/readme/keys-en.svg" width="100%" alt="Windows controls: click the tray icon to open or close, middle-click to mute, scroll to adjust, click an output name to select it, right-click a row to hide it, and press Escape to close">

<br>

<img src="assets/readme/states-en.svg" width="100%" alt="Mixer actions: output volume, app volume, Windows brightness, blue-light filtering, software dimming and mute">

<br>

<img src="assets/readme/numbers-en.svg" width="100%" alt="219 KB standalone Windows executable in v3.0.2, zero accounts or telemetry, two desktop platforms, three Linux package formats and five Linux appearance options">

<br>

<img src="assets/readme/roadmap-en.svg" width="100%" alt="Roadmap: four published releases, upcoming GNOME, Fedora and Debian tests, and integrated Linux display controls as a known gap">

<details>
<summary><b>Install</b></summary>

### Windows 11 · x64 · v3.0.2

1. Download [**TrayMixer-Setup.exe**](https://github.com/sailxx/TrayMixer/releases/download/v3.0.2/TrayMixer-Setup.exe) for an offline installer or [**TrayMixer.exe**](https://github.com/sailxx/TrayMixer/releases/download/v3.0.2/TrayMixer.exe) to run the app directly.
2. In Setup, choose English or Russian, the install folder and whether to start TrayMixer at sign-in. The app interface is in English. Setup creates desktop and Start menu shortcuts.
3. Click the TrayMixer icon in the notification area to open the mixer.

Run Setup again to update or reinstall while keeping preferences and the autostart choice. Remove the app through Windows **Installed apps**.

The releases are unsigned, so Windows may show a SmartScreen warning. Compare the file’s SHA-256 with the [Windows release notes](https://github.com/sailxx/TrayMixer/releases/tag/v3.0.2).

For an unattended install without launching the app, run `TrayMixer-Setup.exe /S /NOLAUNCH` from the installer folder. Optional switches include `/DIR="C:\path\TrayMixer"`, `/NOAUTOSTART` and `/LAUNCH`. For unattended removal, run `Uninstall.exe /uninstall /S` from the installation folder, which defaults to `%LOCALAPPDATA%\TrayMixer`.

### Linux · x86_64 · v3.1.1 preview

| Package | Install or launch from the download folder |
| --- | --- |
| [DEB](https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.1/TrayMixer_3.1.1_amd64.deb) | `sudo apt install ./TrayMixer_3.1.1_amd64.deb` |
| [RPM](https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.1/TrayMixer-3.1.1-1.x86_64.rpm) | `sudo dnf install ./TrayMixer-3.1.1-1.x86_64.rpm` |
| [AppImage](https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.1/TrayMixer-3.1.1-x86_64.AppImage) | `chmod +x TrayMixer-3.1.1-x86_64.AppImage`, then `./TrayMixer-3.1.1-x86_64.AppImage` |

The preview uses X11 or XWayland and needs glibc 2.35+, PulseAudio 15+ or PipeWire-Pulse, and `pactl`. GNOME requires AppIndicator support for the tray icon. The AppImage needs system desktop libraries and accepts `--appimage-extract-and-run` if FUSE is unavailable.

See the [Linux guide](linux/README.md) for desktop requirements and tray behavior. Compare package hashes with [SHA256SUMS](https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.1/SHA256SUMS).

</details>

<a id="platforms"></a>

<details>
<summary><b>Platforms and known limits</b></summary>

| | Windows 11 | Linux preview |
| --- | --- | --- |
| Audio | Device and grouped app volume | Devices and individual app streams via PulseAudio or PipeWire-Pulse |
| Tray | Windows notification area | Depends on the desktop tray host |
| Display | Brightness, blue-light filtering and dimming | Opens the desktop’s display and night light settings |
| Preferences | `HKCU\Software\TrayMixer` | `$XDG_CONFIG_HOME/traymixer/settings.json` |

Monitor and driver support affect brightness on Windows. Linux packaging and startup are checked on Ubuntu 24.04. Fedora and Debian installations, GNOME tray behavior and hardware compatibility still need desktop testing.

</details>

<details>
<summary><b>Development</b></summary>

TrayMixer is maintained by [Vlad (sailxx)](https://github.com/sailxx) under the [Apache License 2.0](LICENSE).

Build Windows from a clone with the built-in .NET Framework C# compiler.

```cmd
build.cmd
```

Build Linux with .NET SDK 10.0. See the [Linux build and packaging guide](linux/README.md#build-and-verify).

```sh
dotnet build linux/TrayMixer.Linux.csproj -c Release
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
```

To rebuild the English README artwork, use the Golos Text and JetBrains Mono fonts in `tools/readme/.font` as described in [build.mjs](tools/readme/build.mjs).

```sh
cd tools/readme
npm ci
node build.mjs --showcase
```

To verify a download, use `Get-FileHash .\TrayMixer-Setup.exe -Algorithm SHA256` in PowerShell and compare with the [Windows release notes](https://github.com/sailxx/TrayMixer/releases/tag/v3.0.2), or use `sha256sum <filename>` on Linux and compare with [SHA256SUMS](https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.1/SHA256SUMS).

TrayMixer stores preferences locally. Linux sends audio commands to your configured PulseAudio or PipeWire-Pulse server. See the [privacy policy](PRIVACY.md) and [security policy](SECURITY.md). For a reproducible bug, include the version, OS, desktop and affected hardware in a [GitHub issue](https://github.com/sailxx/TrayMixer/issues).

</details>

---

<div align="center">


Translations may describe earlier releases.

</div>
