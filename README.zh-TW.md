[English](README.md) · [العربية](README.ar.md) · [简体中文](README.zh-CN.md) · **繁體中文** · [Čeština](README.cs.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [Ελληνικά](README.el.md) · [Bahasa Indonesia](README.id.md) · [Gaeilge](README.ga.md) · [Italiano](README.it.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Polski](README.pl.md) · [Română](README.ro.md) · [Русский](README.ru.md) · [Español](README.es.md) · [Українська](README.uk.md) · [اردو](README.ur.md) · [ئۇيغۇرچە](README.ug.md) · [Tiếng Việt](README.vi.md) · [Português](README.pt.md) · [Türkçe](README.tr.md)

<div align="center">

<img src="assets/readme/logo-blue.svg" width="128" alt="TrayMixer">

# TrayMixer

**透過 Windows 11 或 Linux 系統托盤中的一個小面板控制每個輸出和應用程式的音量。**

在保持通話清晰的同時關閉遊戲，在揚聲器和耳機之間切換，或在 Windows 上調整亮度、藍光過濾和調光。

**沒有遙測或帳戶。** 在 Apache 2.0 下開源。

<img src="https://img.shields.io/badge/Windows-v3.0.2-2E9BFF" alt="Windows發表v3.0.2">
<img src="https://img.shields.io/badge/Windows-11%20x64-2E9BFF" alt="Windows 11 x64">
<img src="https://img.shields.io/badge/Linux-v3.1.1%20preview-2E9BFF" alt="Linux 預覽 v3.1.1">
<img src="https://img.shields.io/badge/Linux-AppImage%20%7C%20deb%20%7C%20rpm-2E9BFF" alt="Linux、AppImage、DEB 和 RPM">
<img src="https://img.shields.io/badge/code-C%23-2E9BFF" alt="升C">
<img src="https://img.shields.io/badge/Linux%20UI-Avalonia-2E9BFF" alt="Avalonia 為 Linux">
<img src="https://img.shields.io/badge/license-Apache%202.0-2E9BFF" alt="Apache 2.0">
<img src="https://img.shields.io/github/stars/sailxx/TrayMixer?style=flat&amp;logo=github&amp;color=%232E9BFF" alt="GitHub星星">
<br>

<img src="assets/readme/hero-en.svg" width="100%" alt="TrayMixer 橫幅，帶有 Windows 彈出按鈕、輸出和應用程式音量、顯示控制和本機首選項">

<a href="https://github.com/sailxx/TrayMixer/releases/download/v3.0.2/TrayMixer-Setup.exe"><img src="assets/readme/btn-windows-en.svg" height="52" alt="下載Windows安裝程序"></a>&nbsp;;<a href="https://github.com/sailxx/TrayMixer/releases/tag/linux-v3.1.1"><img src="assets/readme/btn-linux-en.svg" height="52" alt="下載Linux包"></a>&nbsp;;<a href="https://github.com/sailxx/TrayMixer/releases/download/v3.0.2/TrayMixer.exe"><img src="assets/readme/btn-portable-en.svg" height="52" alt="下載便攜式 Windows 可執行文件"></a>&nbsp;;<a href="https://github.com/sailxx/TrayMixer/releases"><img src="assets/readme/btn-new-en.svg" height="52" alt="閱讀發行說明"></a>

</div>

<br>

<img src="assets/readme/features-en.svg" width="100%" alt="功能：單獨的輸出和應用程式音量、Windows 顯示控制、托盤控制、主題和本地首選項">

<details>
<summary><b>有關功能的更多信息</b></summary>

### 輸出和應用程式音量

每個輸出都有自己的滑桿和靜音控制。按一下輸出名稱將其選為預設名稱。在 Linux 上，這適用於新播放，每個應用程式串流單獨顯示。

### Windows 顯示控件

調整支援的顯示器的亮度，使用藍光濾鏡溫暖顯示器，並使用同一混音器的軟體調光。 [查看完整尺寸的 Windows 螢幕截圖](assets/readme/screenshot.png).

### 托盤控件

左鍵點選 Windows 托盤圖示可顯示或隱藏混音器，中鍵點選可在預設輸出上切換靜音。滾動一行將其調整兩點，點擊音訊行的圖示將其靜音，或右鍵單擊一行將其隱藏。從托盤選單恢復隱藏的行。按 <kbd>Esc鍵</kbd> 或點選外部以關閉 Windows 彈出按鈕。 Linux 托盤點擊次數取決於您的桌面。

### 外觀、啟動和偏好

Windows 使用圓形彈出式按鈕。 Linux 沿用 GNOME 淺色、深色和強調色，或提供木炭色、北色、卡布丁色和淺色。您可以從托盤選單啟用登入時啟動。首選項儲存在本地。

</details>

<br>

<img src="assets/readme/keys-en.svg" width="100%" alt="Windows 控制：按一下托盤圖示開啟或關閉，按一下中鍵靜音，捲動調整，按一下輸出名稱將其選中，右鍵點選一行將其隱藏，然後按 Escape 關閉">

<br>

<img src="assets/readme/states-en.svg" width="100%" alt="混音器操作：輸出音量、應用程式音量、Windows 亮度、藍光過濾、軟體調光和靜音">

<br>

<img src="assets/readme/numbers-en.svg" width="100%" alt="219 個 KB 獨立 Windows 在 v3.0.2 中可執行，零帳戶或遙測，兩個桌面平台，三種 Linux 封裝格式和五個 Linux 外觀選項">

<br>

<img src="assets/readme/roadmap-en.svg" width="100%" alt="路線圖：四個已發布的版本，即將進行的 GNOME 、Fedora 和 Debian 測試，以及集成的 Linux 顯示控製作為已知的差距">

<details>
<summary><b>安裝</b></summary>

### Windows 11·x64·v3.0.2

1. 下載 [**TrayMixer-Setup.exe**](https://github.com/sailxx/TrayMixer/releases/download/v3.0.2/TrayMixer-Setup.exe) 對於離線安裝程式或 [**TrayMixer.exe**](https://github.com/sailxx/TrayMixer/releases/download/v3.0.2/TrayMixer.exe) 直接運行應用程式。
2. 在安裝程式中，選擇英語或俄語、安裝資料夾以及是否在登入時啟動 TrayMixer。該應用程式介面是英文的。安裝程式建立桌面和「開始」功能表捷徑。
3. 點擊通知區域中的 TrayMixer 圖示以開啟混音器。

再次執行安裝程式以更新或重新安裝，同時保留首選項和自動啟動選擇。透過Windows刪除應用程式 **已安裝的應用程式**.

此版本未簽名，因此 Windows 可能會顯示 SmartScreen 警告。將檔案的 SHA -256 與 [Windows 發行說明](https://github.com/sailxx/TrayMixer/releases/tag/v3.0.2).

對於無需啟動應用程式的無人值守安裝，請執行 `TrayMixer-Setup.exe /S /NOLAUNCH` 從安裝程式資料夾。可選開關包括 `/DIR="C:\path\TrayMixer"`, `/NOAUTOSTART` 和 `/LAUNCH`。對於無人值守刪除，請執行 `Uninstall.exe /uninstall /S` 從安裝資料夾中，預設為 `%LOCALAPPDATA%\TrayMixer`.

### Linux · x86_64 · v3.1.1 預覽

| 包裹 | 從下載資料夾安裝或啟動 |
| --- | --- |
| [DEB](https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.1/TrayMixer_3.1.1_amd64.deb) | `sudo apt install ./TrayMixer_3.1.1_amd64.deb` |
| [RPM](https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.1/TrayMixer-3.1.1-1.x86_64.rpm) | `sudo dnf install ./TrayMixer-3.1.1-1.x86_64.rpm` |
| [AppImage](https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.1/TrayMixer-3.1.1-x86_64.AppImage) | `chmod +x TrayMixer-3.1.1-x86_64.AppImage`, 然後 `./TrayMixer-3.1.1-x86_64.AppImage` |

預覽使用 X11 或 XWayland 並需要 glibc 2.35 +、PulseAudio 15+ 或 PipeWire-Pulse，並且 `pactl`。 GNOME 需要 AppIndicator 支援托盤圖示。 AppImage需要係統桌面庫並接受 `--appimage-extract-and-run` 如果 FUSE 不可用。

請參閱 [Linux導軌](linux/README.md) 滿足桌面要求和托盤行為。比較包哈希值 [SHA256SUMS](https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.1/SHA256SUMS).

</details>

<a id="platforms"></a>

<details>
<summary><b>平台和已知限制</b></summary>

| | Windows 11 | Linux 預覽 |
| --- | --- | --- |
| 聲音的 | 設備和分組應用程式音量 | 透過 PulseAudio 或 PipeWire-Pulse 的裝置和單一應用程式串流 |
| 托盤 | Windows 通知區 | 取決於桌面托盤主機 |
| 展示 | 亮度、藍光過濾和調光 | 打開桌面的顯示和夜燈設置 |
| 偏好設定 | `HKCU\Software\TrayMixer` | `$XDG_CONFIG_HOME/traymixer/settings.json` |

顯示器和驅動程式支援會影響 Windows 的亮度。 Linux 打包和啟動在 Ubuntu 24.04 上檢查。 Fedora 和 Debian 安裝、GNOME 托盤行為和硬體相容性仍然需要桌面測試。

</details>

<details>
<summary><b>發展</b></summary>

TrayMixer 的維護者 [弗拉德 ( sailxx )](https://github.com/sailxx) 根據 [Apache 授權 2.0](LICENSE).

建造 Windows 來自具有內建 .NET 框架 C# 編譯器。

```cmd
build.cmd
```

使用 建造 Linux。 NET SDK 10.0 。請參閱 [Linux 建造與包裝指南](linux/README.md#build-and-verify).

```sh
dotnet build linux/TrayMixer.Linux.csproj -c Release
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
```

若要重建英文 README 圖稿，請使用 Golos Text 和 JetBrains Mono 字體 `tools/readme/.font` 如中所述 [build.mjs](tools/readme/build.mjs).

```sh
cd tools/readme
npm ci
node build.mjs --showcase
```

若要驗證下載，請使用 `Get-FileHash .\TrayMixer-Setup.exe -Algorithm SHA256` 在PowerShell中並與 [Windows 發行說明](https://github.com/sailxx/TrayMixer/releases/tag/v3.0.2), 或使用 `sha256sum <filename>` 在 Linux 上並與 [SHA256SUMS](https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.1/SHA256SUMS).

TrayMixer 在本機儲存首選項。 Linux 將音訊指令傳送到您設定的 PulseAudio 或 PipeWire-Pulse 伺服器。請參閱 [隱私權政策](PRIVACY.md) 和 [安全政策](SECURITY.md)。對於可重現的錯誤，請將版本、 OS 、桌面和受影響的硬體包含在 [GitHub問題](https://github.com/sailxx/TrayMixer/issues).

</details>

---

<div align="center">


翻譯可能會描述早期版本。

</div>
