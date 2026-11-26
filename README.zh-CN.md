[English](README.md) · [العربية](README.ar.md) · **简体中文** · [繁體中文](README.zh-TW.md) · [Čeština](README.cs.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [Ελληνικά](README.el.md) · [Bahasa Indonesia](README.id.md) · [Gaeilge](README.ga.md) · [Italiano](README.it.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Polski](README.pl.md) · [Română](README.ro.md) · [Русский](README.ru.md) · [Español](README.es.md) · [Українська](README.uk.md) · [اردو](README.ur.md) · [ئۇيغۇرچە](README.ug.md) · [Tiếng Việt](README.vi.md) · [Português](README.pt.md) · [Türkçe](README.tr.md)

<div align="center">

<img src="assets/readme/logo-blue.svg" width="128" alt="TrayMixer">

# TrayMixer

**通过 Windows 11 或 Linux 系统托盘中的一个小面板控制每个输出和应用程序的音量。**

在保持通话清晰的同时关闭游戏，在扬声器和耳机之间切换，或者在 Windows 上调整亮度、蓝光过滤和调光。

**没有遥测或帐户。** 在 Apache 2.0 下开源。

<img src="https://img.shields.io/badge/Windows-v3.0.2-2E9BFF" alt="Windows发布v3.0.2">
<img src="https://img.shields.io/badge/Windows-11%20x64-2E9BFF" alt="Windows 11 x64">
<img src="https://img.shields.io/badge/Linux-v3.1.1%20preview-2E9BFF" alt="Linux 预览 v3.1.1">
<img src="https://img.shields.io/badge/Linux-AppImage%20%7C%20deb%20%7C%20rpm-2E9BFF" alt="Linux、AppImage、DEB 和 RPM">
<img src="https://img.shields.io/badge/code-C%23-2E9BFF" alt="升C">
<img src="https://img.shields.io/badge/Linux%20UI-Avalonia-2E9BFF" alt="Avalonia 为 Linux">
<img src="https://img.shields.io/badge/license-Apache%202.0-2E9BFF" alt="Apache 2.0">
<img src="https://img.shields.io/github/stars/sailxx/TrayMixer?style=flat&amp;logo=github&amp;color=%232E9BFF" alt="GitHub星星">
<br>

<img src="assets/readme/hero-en.svg" width="100%" alt="TrayMixer 横幅，带有 Windows 弹出按钮、输出和应用程序音量、显示控件和本地首选项">

<a href="https://github.com/sailxx/TrayMixer/releases/download/v3.0.2/TrayMixer-Setup.exe"><img src="assets/readme/btn-windows-en.svg" height="52" alt="下载Windows安装程序"></a>&nbsp;;<a href="https://github.com/sailxx/TrayMixer/releases/tag/linux-v3.1.1"><img src="assets/readme/btn-linux-en.svg" height="52" alt="下载Linux包"></a>&nbsp;;<a href="https://github.com/sailxx/TrayMixer/releases/download/v3.0.2/TrayMixer.exe"><img src="assets/readme/btn-portable-en.svg" height="52" alt="下载便携式 Windows 可执行文件"></a>&nbsp;;<a href="https://github.com/sailxx/TrayMixer/releases"><img src="assets/readme/btn-new-en.svg" height="52" alt="阅读发行说明"></a>

</div>

<br>

<img src="assets/readme/features-en.svg" width="100%" alt="功能：单独的输出和应用程序音量、Windows 显示控件、托盘控件、主题和本地首选项">

<details>
<summary><b>有关功能的更多信息</b></summary>

### 输出和应用程序音量

每个输出都有自己的滑块和静音控制。单击输出名称将其选为默认名称。在 Linux 上，这适用于新播放，每个应用程序流单独显示。

### Windows 显示控件

调整支持的显示器的亮度，使用蓝光过滤器温暖显示器，并使用同一调音台的软件调光。 [查看完整尺寸的 Windows 屏幕截图](assets/readme/screenshot.png).

### 托盘控件

左键单击 Windows 托盘图标可显示或隐藏混音器，中键单击可在默认输出上切换静音。滚动一行将其调整两点，单击音频行的图标将其静音，或右键单击一行将其隐藏。从托盘菜单恢复隐藏的行。按 <kbd>Esc键</kbd> 或单击外部以关闭 Windows 弹出按钮。 Linux 托盘点击次数取决于您的桌面。

### 外观、启动和偏好

Windows 使用圆形弹出按钮。 Linux 沿用 GNOME 浅色、深色和强调色，或提供木炭色、北色、卡布丁色和浅色。您可以从托盘菜单启用登录时启动。首选项存储在本地。

</details>

<br>

<img src="assets/readme/keys-en.svg" width="100%" alt="Windows 控件：单击托盘图标打开或关闭，单击中键静音，滚动调整，单击输出名称将其选中，右键单击一行将其隐藏，然后按 Escape 关闭">

<br>

<img src="assets/readme/states-en.svg" width="100%" alt="混音器操作：输出音量、应用程序音量、Windows 亮度、蓝光过滤、软件调光和静音">

<br>

<img src="assets/readme/numbers-en.svg" width="100%" alt="219 个 KB 独立 Windows 在 v3.0.2 中可执行，零帐户或遥测，两个桌面平台，三种 Linux 封装格式和五个 Linux 外观选项">

<br>

<img src="assets/readme/roadmap-en.svg" width="100%" alt="路线图：四个已发布的版本，即将进行的 GNOME 、Fedora 和 Debian 测试，以及集成的 Linux 显示控件作为已知的差距">

<details>
<summary><b>安装</b></summary>

### Windows 11·x64·v3.0.2

1. 下载 [**TrayMixer-Setup.exe**](https://github.com/sailxx/TrayMixer/releases/download/v3.0.2/TrayMixer-Setup.exe) 对于离线安装程序或 [**TrayMixer.exe**](https://github.com/sailxx/TrayMixer/releases/download/v3.0.2/TrayMixer.exe) 直接运行应用程序。
2. 在安装程序中，选择英语或俄语、安装文件夹以及是否在登录时启动 TrayMixer。该应用程序界面是英文的。安装程序创建桌面和“开始”菜单快捷方式。
3. 单击通知区域中的 TrayMixer 图标以打开混音器。

再次运行安装程序以更新或重新安装，同时保留首选项和自动启动选择。通过Windows删除应用程序 **已安装的应用程序**.

该版本未签名，因此 Windows 可能会显示 SmartScreen 警告。将文件的 SHA -256 与 [Windows 发行说明](https://github.com/sailxx/TrayMixer/releases/tag/v3.0.2).

对于无需启动应用程序的无人值守安装，请运行 `TrayMixer-Setup.exe /S /NOLAUNCH` 从安装程序文件夹。可选开关包括 `/DIR="C:\path\TrayMixer"`, `/NOAUTOSTART` 和 `/LAUNCH`。对于无人值守删除，请运行 `Uninstall.exe /uninstall /S` 从安装文件夹中，默认为 `%LOCALAPPDATA%\TrayMixer`.

### Linux · x86_64 · v3.1.1 预览

| 包裹 | 从下载文件夹安装或启动 |
| --- | --- |
| [DEB](https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.1/TrayMixer_3.1.1_amd64.deb) | `sudo apt install ./TrayMixer_3.1.1_amd64.deb` |
| [RPM](https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.1/TrayMixer-3.1.1-1.x86_64.rpm) | `sudo dnf install ./TrayMixer-3.1.1-1.x86_64.rpm` |
| [AppImage](https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.1/TrayMixer-3.1.1-x86_64.AppImage) | `chmod +x TrayMixer-3.1.1-x86_64.AppImage`, 然后 `./TrayMixer-3.1.1-x86_64.AppImage` |

预览使用 X11 或 XWayland 并需要 glibc 2.35 +、PulseAudio 15+ 或 PipeWire-Pulse，并且 `pactl`。 GNOME 需要 AppIndicator 支持托盘图标。 AppImage需要系统桌面库并接受 `--appimage-extract-and-run` 如果 FUSE 不可用。

请参阅 [Linux导轨](linux/README.md) 满足桌面要求和托盘行为。比较包哈希值 [SHA256SUMS](https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.1/SHA256SUMS).

</details>

<a id="platforms"></a>

<details>
<summary><b>平台和已知限制</b></summary>

| | Windows 11 | Linux 预览 |
| --- | --- | --- |
| 声音的 | 设备和分组应用程序音量 | 通过 PulseAudio 或 PipeWire-Pulse 的设备和单个应用程序流 |
| 托盘 | Windows 通知区 | 取决于桌面托盘主机 |
| 展示 | 亮度、蓝光过滤和调光 | 打开桌面的显示和夜灯设置 |
| 偏好设置 | `HKCU\Software\TrayMixer` | `$XDG_CONFIG_HOME/traymixer/settings.json` |

显示器和驱动程序支持会影响 Windows 的亮度。 Linux 打包和启动在 Ubuntu 24.04 上检查。 Fedora 和 Debian 安装、GNOME 托盘行为和硬件兼容性仍然需要桌面测试。

</details>

<details>
<summary><b>发展</b></summary>

TrayMixer 的维护者 [弗拉德 ( sailxx )](https://github.com/sailxx) 根据 [Apache 许可证 2.0](LICENSE).

建造 Windows 来自具有内置 .NET 框架 C# 编译器。

```cmd
build.cmd
```

使用 构建 Linux。 NET SDK 10.0 。请参阅 [Linux 构建和包装指南](linux/README.md#build-and-verify).

```sh
dotnet build linux/TrayMixer.Linux.csproj -c Release
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
```

要重建英文 README 图稿，请使用 Golos Text 和 JetBrains Mono 字体 `tools/readme/.font` 如中所述 [build.mjs](tools/readme/build.mjs).

```sh
cd tools/readme
npm ci
node build.mjs --showcase
```

要验证下载，请使用 `Get-FileHash .\TrayMixer-Setup.exe -Algorithm SHA256` 在PowerShell中并与 [Windows 发行说明](https://github.com/sailxx/TrayMixer/releases/tag/v3.0.2), 或使用 `sha256sum <filename>` 在 Linux 上并与 [SHA256SUMS](https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.1/SHA256SUMS).

TrayMixer 在本地存储首选项。 Linux 将音频命令发送到您配置的 PulseAudio 或 PipeWire-Pulse 服务器。请参阅 [隐私政策](PRIVACY.md) 和 [安全政策](SECURITY.md)。对于可重现的错误，请将版本、 OS 、桌面和受影响的硬件包含在 [GitHub问题](https://github.com/sailxx/TrayMixer/issues).

</details>

---

<div align="center">


翻译可能会描述早期版本。

</div>
