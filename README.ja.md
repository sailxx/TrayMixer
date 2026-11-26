[English](README.md) · [العربية](README.ar.md) · [简体中文](README.zh-CN.md) · [繁體中文](README.zh-TW.md) · [Čeština](README.cs.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [Ελληνικά](README.el.md) · [Bahasa Indonesia](README.id.md) · [Gaeilge](README.ga.md) · [Italiano](README.it.md) · **日本語** · [한국어](README.ko.md) · [Polski](README.pl.md) · [Română](README.ro.md) · [Русский](README.ru.md) · [Español](README.es.md) · [Українська](README.uk.md) · [اردو](README.ur.md) · [ئۇيغۇرچە](README.ug.md) · [Tiếng Việt](README.vi.md) · [Português](README.pt.md) · [Türkçe](README.tr.md)

<div align="center">

<img src="assets/readme/logo-blue.svg" width="128" alt="TrayMixer">

# TrayMixer

**Windows 11 または Linux システム トレイの 1 つの小さなパネルから、すべての出力とアプリの音量を制御します。**

通話を聞きながらゲームの電源を落としたり、スピーカーとヘッドフォンを切り替えたり、Windows で明るさ、ブルーライト フィルタリング、調光を調整したりできます。

**テレメトリもアカウントもありません。** Apache 2.0 でオープンソース。

<img src="https://img.shields.io/badge/Windows-v3.0.2-2E9BFF" alt="Windows 発売 v3.0.2">
<img src="https://img.shields.io/badge/Windows-11%20x64-2E9BFF" alt="Windows 11 x64">
<img src="https://img.shields.io/badge/Linux-v3.1.1%20preview-2E9BFF" alt="Linux プレビュー v3.1.1">
<img src="https://img.shields.io/badge/Linux-AppImage%20%7C%20deb%20%7C%20rpm-2E9BFF" alt="Linux AppImage、DEB、RPM">
<img src="https://img.shields.io/badge/code-C%23-2E9BFF" alt="Cシャープ">
<img src="https://img.shields.io/badge/Linux%20UI-Avalonia-2E9BFF" alt="Avalonia用 Linux用">
<img src="https://img.shields.io/badge/license-Apache%202.0-2E9BFF" alt="Apache 2.0">
<img src="https://img.shields.io/github/stars/sailxx/TrayMixer?style=flat&amp;logo=github&amp;color=%232E9BFF" alt="GitHub スター">
<br>

<img src="assets/readme/hero-en.svg" width="100%" alt="Windows フライアウト、出力およびアプリの音量、ディスプレイ コントロール、およびローカル設定を備えた TrayMixer バナー">

<a href="https://github.com/sailxx/TrayMixer/releases/download/v3.0.2/TrayMixer-Setup.exe"><img src="assets/readme/btn-windows-en.svg" height="52" alt="Windowsインストーラーをダウンロードする"></a>&nbsp;&nbsp;<a href="https://github.com/sailxx/TrayMixer/releases/tag/linux-v3.1.1"><img src="assets/readme/btn-linux-en.svg" height="52" alt="Linux パッケージをダウンロード"></a>&nbsp;&nbsp;<a href="https://github.com/sailxx/TrayMixer/releases/download/v3.0.2/TrayMixer.exe"><img src="assets/readme/btn-portable-en.svg" height="52" alt="ポータブル Windows 実行可能ファイルをダウンロードする"></a>&nbsp;&nbsp;<a href="https://github.com/sailxx/TrayMixer/releases"><img src="assets/readme/btn-new-en.svg" height="52" alt="リリースノートを読む"></a>

</div>

<br>

<img src="assets/readme/features-en.svg" width="100%" alt="機能: 個別の出力とアプリのボリューム、Windows ディスプレイ コントロール、トレイ コントロール、テーマ、ローカル設定">

<details>
<summary><b>機能の詳細</b></summary>

### 出力とアプリの音量

各出力には独自のスライダーとミュート コントロールがあります。出力名をクリックしてデフォルトとして選択します。 Linux では、これは新しい再生に適用され、各アプリ ストリームが個別に表示されます。

### Windows ディスプレイ コントロール

サポートされているモニターの明るさを調整し、ブルーライト フィルターでディスプレイを暖め、同じミキサーからソフトウェア調光を使用します。 [Windows のスクリーンショットをフルサイズでご覧ください](assets/readme/screenshot.png).

### トレイコントロール

Windows トレイ アイコンを左クリックしてミキサーを表示または非表示にし、中クリックしてデフォルト出力のミュートを切り替えます。行をスクロールして 2 ポイント調整し、オーディオ行のアイコンをクリックしてミュートするか、行を右クリックして非表示にします。トレイ メニューから非表示の行を復元します。プレス <kbd>ESC</kbd> または、外側をクリックして Windows フライアウトを閉じます。 Linux トレイのクリック音はデスクトップによって異なります。

### 外観、起動、設定

Windowsは丸みを帯びたフライアウトを採用しています。 Linux は、GNOME のライト、ダーク、アクセント カラーを踏襲するか、チャコール、ノルド、キャットプッチン、ライトを提供します。トレイメニューからログイン時の起動を有効にすることができます。設定はローカルに保存されます。

</details>

<br>

<img src="assets/readme/keys-en.svg" width="100%" alt="Windows コントロール: トレイ アイコンをクリックして開閉し、中クリックしてミュートし、スクロールして調整します。出力名をクリックして選択し、行を右クリックして非表示にし、Esc キーを押して閉じます。">

<br>

<img src="assets/readme/states-en.svg" width="100%" alt="ミキサーのアクション: 出力音量、アプリ音量、Windows の明るさ、ブルーライト フィルター、ソフトウェア調光およびミュート">

<br>

<img src="assets/readme/numbers-en.svg" width="100%" alt="219 KB v3.0.2 で実行可能なスタンドアロン Windows、アカウントまたはテレメトリなし、2 つのデスクトップ プラットフォーム、3 つの Linux パッケージ形式、および 5 つの Linux 外観オプション">

<br>

<img src="assets/readme/roadmap-en.svg" width="100%" alt="ロードマップ: 4 つの公開リリース、今後の GNOME 、Fedora および Debian のテスト、既知のギャップとしての統合された Linux ディスプレイ コントロール">

<details>
<summary><b>インストール</b></summary>

### Windows 11・x64・v3.0.2

1. ダウンロード [**TrayMixer-Setup.exe**](https://github.com/sailxx/TrayMixer/releases/download/v3.0.2/TrayMixer-Setup.exe) オフライン インストーラーの場合、または [**TrayMixer.exe**](https://github.com/sailxx/TrayMixer/releases/download/v3.0.2/TrayMixer.exe) アプリを直接実行します。
2. [セットアップ] で、英語またはロシア語、インストール フォルダー、サインイン時に TrayMixer を起動するかどうかを選択します。アプリのインターフェースは英語です。セットアップにより、デスクトップと [スタート] メニューのショートカットが作成されます。
3. 通知領域の TrayMixer アイコンをクリックしてミキサーを開きます。

設定と自動起動の選択を維持したまま、セットアップを再度実行して更新または再インストールします。 Windowsからアプリを削除する **インストールされているアプリ**.

リリースは署名されていないため、Windows では SmartScreen 警告が表示される場合があります。ファイルの SHA -256 と [Windows リリースノート](https://github.com/sailxx/TrayMixer/releases/tag/v3.0.2).

アプリを起動せずに無人インストールするには、次を実行します。 `TrayMixer-Setup.exe /S /NOLAUNCH` インストーラーフォルダーから。オプションのスイッチには以下が含まれます `/DIR="C:\path\TrayMixer"`, `/NOAUTOSTART` そして `/LAUNCH`。無人で削除するには、次を実行します。 `Uninstall.exe /uninstall /S` インストールフォルダーから (デフォルトでは `%LOCALAPPDATA%\TrayMixer`.

### Linux ・ x86_64 ・ v3.1.1 プレビュー

| パッケージ | インストールまたはダウンロードフォルダーから起動 |
| --- | --- |
| [DEB](https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.1/TrayMixer_3.1.1_amd64.deb) | `sudo apt install ./TrayMixer_3.1.1_amd64.deb` |
| [RPM](https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.1/TrayMixer-3.1.1-1.x86_64.rpm) | `sudo dnf install ./TrayMixer-3.1.1-1.x86_64.rpm` |
| [AppImage](https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.1/TrayMixer-3.1.1-x86_64.AppImage) | `chmod +x TrayMixer-3.1.1-x86_64.AppImage`, それから `./TrayMixer-3.1.1-x86_64.AppImage` |

プレビューでは X11 または XWayland を使用し、glibc 2.35 +、PulseAudio 15+ または PipeWire-Pulse が必要です。 `pactl`。 GNOME では、トレイ アイコンの AppIndicator サポートが必要です。 AppImage にはシステム デスクトップ ライブラリが必要であり、 `--appimage-extract-and-run` FUSEが利用できない場合。

を参照してください。 [Linuxガイド](linux/README.md) デスクトップの要件とトレイの動作については、パッケージのハッシュを比較する [SHA256SUMS](https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.1/SHA256SUMS).

</details>

<a id="platforms"></a>

<details>
<summary><b>プラットフォームと既知の制限</b></summary>

| | Windows 11 | Linux プレビュー |
| --- | --- | --- |
| オーディオ | デバイスとグループ化されたアプリのボリューム | PulseAudio または PipeWire-Pulse を介したデバイスと個々のアプリのストリーム |
| トレイ | Windowsの通知領域 | デスクトップトレイホストに依存 |
| 画面 | 明るさ、ブルーライトフィルタリング、調光 | デスクトップのディスプレイと夜間照明の設定を開きます |
| 設定 | `HKCU\Software\TrayMixer` | `$XDG_CONFIG_HOME/traymixer/settings.json` |

モニターとドライバーのサポートは、Windows の明るさに影響します。 Linuxのパッケージ化と起動はUbuntu 24.04で確認しています。 Fedora と Debian のインストール、GNOME トレイの動作、およびハードウェアの互換性については、依然としてデスクトップ テストが必要です。

</details>

<details>
<summary><b>発達</b></summary>

TrayMixer のメンテナンスは次のとおりです。 [ヴラド ( sailxx )](https://github.com/sailxx) の下に [Apache ライセンス 2.0](LICENSE).

を内蔵したクローンから Windows をビルドします。 NET フレームワーク C# コンパイラー。

```cmd
build.cmd
```

でLinuxをビルドします。 NET SDK 10.0 。を参照してください。 [Linux のビルドおよびパッケージ化ガイド](linux/README.md#build-and-verify).

```sh
dotnet build linux/TrayMixer.Linux.csproj -c Release
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
```

英語の README アートワークを再構築するには、次の Golos Text および JetBrains Mono フォントを使用します。 `tools/readme/.font` で説明されているように [build.mjs](tools/readme/build.mjs).

```sh
cd tools/readme
npm ci
node build.mjs --showcase
```

ダウンロードを確認するには、次を使用します `Get-FileHash .\TrayMixer-Setup.exe -Algorithm SHA256` PowerShellと比較してください。 [Windows リリースノート](https://github.com/sailxx/TrayMixer/releases/tag/v3.0.2), または使用します `sha256sum <filename>` Linuxと比較してください [SHA256SUMS](https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.1/SHA256SUMS).

TrayMixer は環境設定をローカルに保存します。 Linux は、設定された PulseAudio または PipeWire-Pulse サーバーに音声コマンドを送信します。を参照してください。 [プライバシーポリシー](PRIVACY.md) そして [セキュリティポリシー](SECURITY.md)。再現可能なバグの場合は、バージョン、 OS 、デスクトップ、および影響を受けるハードウェアを含めてください。 [GitHub問題](https://github.com/sailxx/TrayMixer/issues).

</details>

---

<div align="center">


翻訳では以前のリリースについて説明されている場合があります。

</div>
