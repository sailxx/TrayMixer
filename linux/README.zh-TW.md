[English](README.md) · [العربية](README.ar.md) · [简体中文](README.zh-CN.md) · **繁體中文** · [Čeština](README.cs.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [Ελληνικά](README.el.md) · [Bahasa Indonesia](README.id.md) · [Gaeilge](README.ga.md) · [Italiano](README.it.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Polski](README.pl.md) · [Română](README.ro.md) · [Русский](README.ru.md) · [Español](README.es.md) · [Українська](README.uk.md) · [اردو](README.ur.md) · [ئۇيغۇرچە](README.ug.md) · [Tiếng Việt](README.vi.md)

# TrayMixer 適用於 Linux — 3.1.1 預覽

單獨的 C# / Avalonia 桌面連接埠。現有的 Windows 原始碼和安裝程式保持不變。

## 支持的基線

x86_64 Linux 、glibc 2.35 +、X11 或 XWayland 、PulseAudio 15+ 或 PipeWire `pipewire-pulse`, 和 `pactl`。建置和套件檢查在 Ubuntu 24.04 上運行。仍需要特定於發行版的安裝和GNOME桌面驗收；這是一個預覽，而不是聲稱完全與 Windows 平價。

## 安裝

從 Linux 預覽版下載 DEB 、 RPM 或 AppImage 。

- Ubuntu/Debian: `sudo apt install ./TrayMixer_3.1.1_amd64.deb`
- 軟呢帽: `sudo dnf install ./TrayMixer-3.1.1-1.x86_64.rpm`
- AppImage: `chmod +x TrayMixer-3.1.1-x86_64.AppImage`, 然後運行它。需要係統 `pactl`, X11 函式庫、fontconfig、ICU 和 OpenSSL。安裝 `pulseaudio-utils` 在 Ubuntu/Fedora 上。 FUSE 2 或 `--appimage-extract-and-run` 可能需要。 AppImage 捆綁包。 NET ，不是系統音訊伺服器或桌面庫。

發射 **TrayMixer** 從應用程式選單。第一次啟動會打開混合器；關閉它會使托盤圖示保持運作。從托盤選單中退出。 GNOME Shell 需要 AppIndicator/StatusNotifier 擴充（Ubuntu 通常包含一個）。如果沒有托盤主機，請勿啟用僅托盤自動啟動；使用應用程式選單啟動。再次啟動會要求現有程序透過僅限目前使用者的本機管道重新開啟其視窗。

## 控制

- 托盤啟動可開啟/關閉混合器。托盤選單提供開啟混音器、預設輸出靜音、恢復隱藏行、自動啟動和退出。桌面托盤主機控制點擊行為；不保證 Windows 中鍵奇偶校驗。
- 輸出與應用程式流滑桿：0–100%、靜音、滑鼠滾輪 ±2。明確設定正音量可取消靜音。
- 按一下輸出名稱可將其設為新播放的預設名稱。現有流不會被強制移動。
- 右鍵點選要隱藏的行；恢復隱藏行會恢復所有行。單獨的串流單獨顯示，包括非預設裝置上的串流。
- 登入時啟動使用 XDG 自動啟動條目；停用直到選擇。 AppImage 必須保留在其永久位置。
- 外觀：GNOME /系統遵循淺色/深色和GNOME強調色，在混音器開啟時煥然一新。不支援重音的舊版 GNOME 使用藍色。內建木炭、Nord、Catppuccin 和 Light。無雲母或整個視窗透明度。
- 設定: `$XDG_CONFIG_HOME/traymixer/settings.json`, 預設 `~/.config/traymixer/settings.json`.

## 與 Windows 的已知差距

整合的每個顯示器亮度、藍光強度和軟體調光滑塊尚未移植。 **顯示和夜燈設置** 開啟 GNOME / KDE 設定。 Wayland 上不存在通用伽瑪調整；需要特定於合成器的實現。不會執行任意特權寫入或權限變更。無法複製精確的彈出視窗放置、失焦消除和 Windows 背景效果。

## 建置並驗證

需要 . NET SDK 10.0 。在 Linux 上，來自克隆:

```sh
cd TrayMixer
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
dotnet run --project linux/TrayMixer.Linux.csproj
# Packaging requires dpkg-deb, rpm/rpmbuild and curl:
bash linux/package.sh 3.1.1
```

在目前的Windows開發機上:

```cmd
cd /d C:\Users\Vlad\sx\traymixer
dotnet build linux/TrayMixer.Linux.csproj -c Release
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
dotnet publish linux/TrayMixer.Linux.csproj -c Release -r linux-x64 --self-contained true -o linux/artifacts/publish
```

Windows 可以交叉發布 Linux 二進位文件，但無法在沒有 Linux 環境的情況下執行它們或建立本機套件。 `.github/workflows/linux.yml` 執行測試、針對空輸出的真實 PulseAudio 回讀、包元資料驗證和 Xvfb 啟動煙霧檢查。這些並不能取代測試實際的 GNOME 托盤、主題、硬體或收聽音訊。 Linux釋放標籤使用 `linux-v*` 因此現有的Windows發布工作流程不會被觸發。保留現有的 Windows 版本。
