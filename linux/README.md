# TrayMixer for Linux — 3.1.0 preview

A separate C# / Avalonia desktop port. The existing Windows source and installer are unchanged.

## Supported baseline

x86_64 Linux, glibc 2.35+, X11 or XWayland, PulseAudio 15+ or PipeWire with `pipewire-pulse`, and `pactl`. Build and package checks run on Ubuntu 24.04. Distribution-specific installation and GNOME desktop acceptance are still required; this is a preview, not a claim of full Windows parity.

## Install

Download a DEB, RPM or AppImage from the Linux preview release.

- Ubuntu/Debian: `sudo apt install ./TrayMixer_3.1.0_amd64.deb`
- Fedora: `sudo dnf install ./TrayMixer-3.1.0-1.x86_64.rpm`
- AppImage: `chmod +x TrayMixer-3.1.0-x86_64.AppImage`, then run it. Requires system `pactl`, X11 libraries, fontconfig, ICU and OpenSSL. Install `pulseaudio-utils` on Ubuntu/Fedora. FUSE 2 or `--appimage-extract-and-run` may be required. AppImage bundles .NET, not the system audio server or desktop libraries.

Launch **TrayMixer** from the application menu. The first launch opens the mixer; closing it keeps the tray icon running. Exit from the tray menu. GNOME Shell needs an AppIndicator/StatusNotifier extension (Ubuntu normally includes one). Without a tray host, do not enable tray-only autostart; use the application menu to launch. Launching again asks the existing process to reopen its window through a current-user-only local pipe.

## Controls

- Tray activation opens/closes the mixer. The tray menu offers Open mixer, default-output mute, restore hidden rows, autostart and Exit. Desktop tray hosts control click behavior; Windows middle-click parity is not guaranteed.
- Output and application-stream sliders: 0–100%, mute, mouse-wheel ±2. Setting positive volume explicitly unmutes.
- Click an output name to make it the default for new playback. Existing streams are not forcibly moved.
- Right-click a row to hide; Restore hidden rows brings all rows back. Separate streams are shown separately, including streams on non-default devices.
- Start at login uses an XDG autostart entry; disabled until chosen. AppImage must remain in its permanent location.
- Appearance: GNOME / System follows light/dark and GNOME accent colors, refreshed while the mixer is open. Older GNOME without accent support uses blue. Charcoal, Nord, Catppuccin and Light are built in. No Mica or whole-window transparency.
- Settings: `$XDG_CONFIG_HOME/traymixer/settings.json`, default `~/.config/traymixer/settings.json`.

## Known gaps versus Windows

Integrated per-monitor brightness, blue-light strength and software-dimming sliders are not yet ported. **Display & night light settings** opens GNOME/KDE settings instead. Universal gamma adjustment does not exist on Wayland; a compositor-specific implementation is needed. No arbitrary privileged writes or permission changes are performed. Exact popup placement, focus-loss dismissal and Windows backdrop effects are not replicated.

## Build and verify

Requires .NET SDK 10.0. On Linux, from a clone:

```sh
cd TrayMixer
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
dotnet run --project linux/TrayMixer.Linux.csproj
# Packaging requires dpkg-deb, rpm/rpmbuild and curl:
bash linux/package.sh 3.1.0
```

On the current Windows development machine:

```cmd
cd /d C:\Users\Vlad\sx\traymixer
dotnet build linux/TrayMixer.Linux.csproj -c Release
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
dotnet publish linux/TrayMixer.Linux.csproj -c Release -r linux-x64 --self-contained true -o linux/artifacts/publish
```

Windows can cross-publish Linux binaries but cannot execute them or create native packages without a Linux environment. `.github/workflows/linux.yml` runs tests, real PulseAudio readback against a null output, package metadata validation and Xvfb launch smoke checks. Those do not replace testing an actual GNOME tray, themes, hardware or listening to audio. Linux release tags use `linux-v*` so the existing Windows release workflow is not triggered. Existing Windows releases are retained.
