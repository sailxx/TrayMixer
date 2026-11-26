[English](README.md) · [العربية](README.ar.md) · **简体中文** · [繁體中文](README.zh-TW.md) · [Čeština](README.cs.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [Ελληνικά](README.el.md) · [Bahasa Indonesia](README.id.md) · [Gaeilge](README.ga.md) · [Italiano](README.it.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Polski](README.pl.md) · [Română](README.ro.md) · [Русский](README.ru.md) · [Español](README.es.md) · [Українська](README.uk.md) · [اردو](README.ur.md) · [ئۇيغۇرچە](README.ug.md) · [Tiếng Việt](README.vi.md)

# TrayMixer 适用于 Linux — 3.1.1 预览

单独的 C# / Avalonia 桌面端口。现有的 Windows 源代码和安装程序保持不变。

## 支持的基线

x86_64 Linux 、glibc 2.35 +、X11 或 XWayland 、PulseAudio 15+ 或 PipeWire `pipewire-pulse`, 和 `pactl`。构建和包检查在 Ubuntu 24.04 上运行。仍需要特定于发行版的安装和GNOME桌面验收；这是一个预览，而不是声称完全与 Windows 平价。

## 安装

从 Linux 预览版下载 DEB 、 RPM 或 AppImage 。

- Ubuntu/Debian: `sudo apt install ./TrayMixer_3.1.1_amd64.deb`
- 软呢帽: `sudo dnf install ./TrayMixer-3.1.1-1.x86_64.rpm`
- AppImage: `chmod +x TrayMixer-3.1.1-x86_64.AppImage`, 然后运行它。需要系统 `pactl`, X11 库、fontconfig、ICU 和 OpenSSL。安装 `pulseaudio-utils` 在 Ubuntu/Fedora 上。 FUSE 2 或 `--appimage-extract-and-run` 可能需要。 AppImage 捆绑包。 NET ，不是系统音频服务器或桌面库。

发射 **TrayMixer** 从应用程序菜单。第一次启动会打开混合器；关闭它会使托盘图标保持运行。从托盘菜单中退出。 GNOME Shell 需要 AppIndicator/StatusNotifier 扩展（Ubuntu 通常包含一个）。如果没有托盘主机，请勿启用仅托盘自动启动；使用应用程序菜单启动。再次启动会要求现有进程通过仅限当前用户的本地管道重新打开其窗口。

## 控制

- 托盘激活可打开/关闭混合器。托盘菜单提供打开混音器、默认输出静音、恢复隐藏行、自动启动和退出。桌面托盘主机控制点击行为；不保证 Windows 中键奇偶校验。
- 输出和应用程序流滑块：0–100%、静音、鼠标滚轮 ±2。明确设置正音量可取消静音。
- 单击输出名称可将其设为新播放的默认名称。现有流不会被强制移动。
- 右键单击要隐藏的行；恢复隐藏行会恢复所有行。单独的流单独显示，包括非默认设备上的流。
- 登录时启动使用 XDG 自动启动条目；禁用直到选择。 AppImage 必须保留在其永久位置。
- 外观：GNOME /系统遵循浅色/深色和GNOME强调色，在调音台打开时焕然一新。不支持重音的旧版 GNOME 使用蓝色。内置木炭、Nord、Catppuccin 和 Light。无云母或整个窗口透明度。
- 设置: `$XDG_CONFIG_HOME/traymixer/settings.json`, 默认 `~/.config/traymixer/settings.json`.

## 与 Windows 的已知差距

集成的每个显示器亮度、蓝光强度和软件调光滑块尚未移植。 **显示和夜灯设置** 打开 GNOME / KDE 设置。 Wayland 上不存在通用伽玛调整；需要特定于合成器的实现。不会执行任意特权写入或权限更改。无法复制精确的弹出窗口放置、失焦消除和 Windows 背景效果。

## 构建并验证

需要 . NET SDK 10.0 。在 Linux 上，来自克隆:

```sh
cd TrayMixer
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
dotnet run --project linux/TrayMixer.Linux.csproj
# Packaging requires dpkg-deb, rpm/rpmbuild and curl:
bash linux/package.sh 3.1.1
```

在当前的Windows开发机上:

```cmd
cd /d C:\Users\Vlad\sx\traymixer
dotnet build linux/TrayMixer.Linux.csproj -c Release
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
dotnet publish linux/TrayMixer.Linux.csproj -c Release -r linux-x64 --self-contained true -o linux/artifacts/publish
```

Windows 可以交叉发布 Linux 二进制文件，但无法在没有 Linux 环境的情况下执行它们或创建本机包。 `.github/workflows/linux.yml` 运行测试、针对空输出的真实 PulseAudio 回读、包元数据验证和 Xvfb 启动烟雾检查。这些并不能取代测试实际的 GNOME 托盘、主题、硬件或收听音频。 Linux释放标签使用 `linux-v*` 因此现有的Windows发布工作流程不会被触发。保留现有的 Windows 版本。
