<div align="center">

[Русский](README.ru.md) · **English** · [Español](README.es.md) · [Português](README.pt.md) · [Deutsch](README.de.md) · [Français](README.fr.md) · [Italiano](README.it.md) · [Türkçe](README.tr.md) · [Українська](README.uk.md) · [Polski](README.pl.md)

# TrayMixer
### Your audio. One click away.

[![Windows 11](https://img.shields.io/badge/Windows_11-stable-0078D4?style=for-the-badge&logo=windows11&logoColor=white)](https://github.com/sailxx/TrayMixer/releases/tag/v3.0.0)
[![Linux](https://img.shields.io/badge/Linux-x86__64_preview-FCC624?style=for-the-badge&logo=linux&logoColor=black)](https://github.com/sailxx/TrayMixer/releases/tag/linux-v3.1.0)
[![GNOME](https://img.shields.io/badge/GNOME-adaptive_theme-4A86CF?style=for-the-badge&logo=gnome&logoColor=white)](linux/README.md)

[![DEB](https://img.shields.io/badge/Ubuntu_%2F_Debian-DEB-E95420?style=for-the-badge&logo=ubuntu&logoColor=white)](https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.0/TrayMixer_3.1.0_amd64.deb)
[![RPM](https://img.shields.io/badge/Fedora-RPM-51A2DA?style=for-the-badge&logo=fedora&logoColor=white)](https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.0/TrayMixer-3.1.0-1.x86_64.rpm)
[![AppImage](https://img.shields.io/badge/Linux-AppImage-2C2D72?style=for-the-badge&logo=appimage&logoColor=white)](https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.0/TrayMixer-3.1.0-x86_64.AppImage)

</div>

![TrayMixer on Windows](assets/readme/screenshot.png)

**TrayMixer** brings output devices and application volume controls to your system tray. The Windows edition uses a compact native flyout; the new Linux preview uses Avalonia, PulseAudio/PipeWire-Pulse, GNOME-adaptive colors and four additional themes. The interface is in English.

> **Linux preview:** x86_64, Ubuntu 24.04+ baseline or compatible distributions, X11/XWayland, PulseAudio 15+ or PipeWire-Pulse. Fedora/RPM and Debian compatibility still require distribution-specific desktop testing. Integrated display brightness, blue-light and dimming sliders are not yet ported; Linux currently links to desktop display settings. [Requirements and known limitations](linux/README.md).

## Install on Windows

1. Download [**TrayMixer-Setup.exe**](https://github.com/sailxx/TrayMixer/releases/download/v3.0.0/TrayMixer-Setup.exe).
2. Select English or Russian, then choose the install folder (default: `%LOCALAPPDATA%\TrayMixer`).
3. Setup installs offline and creates desktop and Start menu shortcuts. Choose whether to enable autostart and launch the app.

Run Setup again to update or reinstall. Preferences and the autostart choice are preserved; downgrades are refused. Uninstall through Windows Installed apps.

Silent installation: `TrayMixer-Setup.exe /S /NOLAUNCH`. Optional: `/DIR="C:\path\TrayMixer"`, `/NOAUTOSTART`, `/LAUNCH`. Silent removal: `Uninstall.exe /uninstall /S`.

## Install on Linux

### Choose your package

| System / format | Download | Installation |
|---|---|---|
| Ubuntu / Debian-compatible · **DEB** | [Download .deb](https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.0/TrayMixer_3.1.0_amd64.deb) | Open with your software installer, or use `apt` below |
| Fedora-compatible · **RPM** | [Download .rpm](https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.0/TrayMixer-3.1.0-1.x86_64.rpm) | Open with your software installer, or use `dnf` below |
| Portable · **AppImage** | [Download AppImage](https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.0/TrayMixer-3.1.0-x86_64.AppImage) | Allow execution in file properties, then launch |

### Ubuntu / Debian-compatible

```sh
mkdir -p "$HOME/Downloads/TrayMixer"
cd "$HOME/Downloads/TrayMixer"
curl -fLO https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.0/TrayMixer_3.1.0_amd64.deb
sudo apt install ./TrayMixer_3.1.0_amd64.deb
traymixer
```

### Fedora-compatible

```sh
mkdir -p "$HOME/Downloads/TrayMixer"
cd "$HOME/Downloads/TrayMixer"
curl -fLO https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.0/TrayMixer-3.1.0-1.x86_64.rpm
sudo dnf install ./TrayMixer-3.1.0-1.x86_64.rpm
traymixer
```

### AppImage — portable, no package installation

Install `pulseaudio-utils` using your distribution's package manager first. The AppImage bundles .NET but requires system `pactl`, X11/XWayland libraries, fontconfig, ICU and OpenSSL.

```sh
mkdir -p "$HOME/Applications"
cd "$HOME/Applications"
curl -fLO https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.0/TrayMixer-3.1.0-x86_64.AppImage
chmod +x TrayMixer-3.1.0-x86_64.AppImage
./TrayMixer-3.1.0-x86_64.AppImage
```

If FUSE is unavailable, run `./TrayMixer-3.1.0-x86_64.AppImage --appimage-extract-and-run` instead. Keep the file in its permanent location before enabling **Start at login**.

> **GNOME tray:** enable an AppIndicator / StatusNotifier extension. Ubuntu normally includes one. Tray support depends on your desktop; no extension is installed automatically. Launch TrayMixer from the application menu to reopen a hidden mixer.

### Themes & controls

- **GNOME / System** follows desktop light/dark and GNOME accent colors.
- **Charcoal**, **Nord**, **Catppuccin**, **Light** offer alternative palettes. No Mica.
- Tray activation toggles the mixer; the menu includes default-output mute, autostart, restore rows and Exit.
- Click an output name to make it the default for new playback. Streams already playing are not forcibly moved.
- Set volume, mute, use the mouse wheel, or right-click to hide a row. Linux currently shows individual application streams, not grouped application sessions.

### Verify Linux downloads

Download `SHA256SUMS` from the [Linux release](https://github.com/sailxx/TrayMixer/releases/tag/linux-v3.1.0), then compare your package with the corresponding entry using `sha256sum <filename>`. Packages are not signed.

## Windows controls

| Action | Result |
|---|---|
| Left-click tray icon | Open / close the flyout |
| Middle-click tray icon | Mute / unmute default output |
| Right-click tray icon | Settings, hidden rows, appearance, autostart, exit |
| Click row icon | Mute / unmute |
| Click device name | Make default |
| Right-click row | Hide |
| Mouse wheel over row | Volume or brightness ±2 |
| Esc / click outside | Close |

## Build and verification

Windows: run `build.cmd` from the project directory; the compiler is included with Windows 11. Windows releases contain `TrayMixer-Setup.exe`; SHA-256 is included in their descriptions. The installer and app are unsigned. Build provenance is verifiable:

```sh
gh attestation verify TrayMixer-Setup.exe --repo sailxx/TrayMixer
```

Linux: .NET SDK 10.0, [build instructions and checks](linux/README.md). GitHub Actions builds DEB, RPM and AppImage and runs native PulseAudio and startup smoke checks before publishing a preview. These checks do not replace real GNOME desktop testing. Windows and Linux release streams remain separate.

## License

Apache-2.0 — see [LICENSE](LICENSE). [Privacy](PRIVACY.md) · [Security](SECURITY.md)
