[English](README.md) · [العربية](README.ar.md) · [简体中文](README.zh-CN.md) · [繁體中文](README.zh-TW.md) · [Čeština](README.cs.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [Ελληνικά](README.el.md) · [Bahasa Indonesia](README.id.md) · [Gaeilge](README.ga.md) · [Italiano](README.it.md) · **日本語** · [한국어](README.ko.md) · [Polski](README.pl.md) · [Română](README.ro.md) · [Русский](README.ru.md) · [Español](README.es.md) · [Українська](README.uk.md) · [اردو](README.ur.md) · [ئۇيغۇرچە](README.ug.md) · [Tiếng Việt](README.vi.md)

# TrayMixer 用 Linux — 3.1.1 プレビュー

独立した C# / Avalonia デスクトップ ポート。既存の Windows ソースとインストーラーは変更されていません。

## サポートされているベースライン

x86_64 Linux 、glibc 2.35 +、X11 または XWayland 、PulseAudio 15+ または PipeWire 付き `pipewire-pulse`, そして `pactl`。ビルドとパッケージのチェックは Ubuntu 24.04 で実行されます。ディストリビューション固有のインストールと GNOME デスクトップの承認は引き続き必要です。これはプレビューであり、Windows と完全に同等であると主張するものではありません。

## インストール

Linux プレビュー リリースから DEB 、 RPM または AppImage をダウンロードします。

- Ubuntu/Debian: `sudo apt install ./TrayMixer_3.1.1_amd64.deb`
- フェドーラ: `sudo dnf install ./TrayMixer-3.1.1-1.x86_64.rpm`
- AppImage: `chmod +x TrayMixer-3.1.1-x86_64.AppImage`, それからそれを実行します。システムが必要です `pactl`, X11 ライブラリ、fontconfig、ICU、および OpenSSL。インストール `pulseaudio-utils` Ubuntu/Fedora 上。 FUSE2または `--appimage-extract-and-run` が必要になる場合があります。 AppImageバンドル。 NET 、システムオーディオサーバーまたはデスクトップライブラリではありません。

打ち上げ **TrayMixer** アプリケーションメニューから。最初の起動ではミキサーが開きます。閉じてもトレイ アイコンは実行されたままになります。トレイメニューを終了します。 GNOME シェルには AppIndicator/StatusNotifier 拡張機能が必要です (Ubuntu には通常、拡張機能が含まれています)。トレイ ホストがない場合は、トレイのみの自動起動を有効にしないでください。アプリケーションメニューを使用して起動します。再度起動すると、既存のプロセスに対して、現在のユーザー専用のローカル パイプを通じてウィンドウを再度開くように要求されます。

## コントロール

- トレイをアクティブにすると、ミキサーが開閉します。トレイ メニューには、ミキサーを開く、デフォルトの出力ミュート、非表示の行の復元、自動開始、および終了が用意されています。デスクトップ トレイ ホストはクリック動作を制御します。 Windows のミドルクリックのパリティは保証されていません。
- 出力およびアプリケーション ストリーム スライダー: 0 ～ 100%、ミュート、マウス ホイール ±2。正の音量を設定すると明示的にミュートが解除されます。
- 出力名をクリックして、新しい再生のデフォルトにします。既存のストリームは強制的に移動されません。
- 非表示にする行を右クリックします。非表示の行を復元すると、すべての行が元に戻ります。デフォルト以外のデバイス上のストリームも含め、個別のストリームが個別に表示されます。
- ログイン時に開始では、XDG 自動開始エントリが使用されます。選択されるまで無効になります。 AppImage は恒久的な場所に保管しておく必要があります。
- 外観: GNOME / システムは明暗と GNOME のアクセントカラーに従い、ミキサーが開いている間に更新されます。アクセントサポートのない旧型GNOMEはブルーを採用。チャコール、ノルド、キャットプッチン、ライトが組み込まれています。マイカやウィンドウ全体の透明度はありません。
- 設定: `$XDG_CONFIG_HOME/traymixer/settings.json`, デフォルト `~/.config/traymixer/settings.json`.

## 既知のギャップと Windows

統合されたモニターごとの明るさ、ブルーライト強度、およびソフトウェア調光スライダーはまだ移植されていません。 **ディスプレイと夜間照明の設定** 代わりに、GNOME / KDE の設定が開きます。ユニバーサルガンマ調整は Wayland には存在しません。コンポジター固有の実装が必要です。任意の特権書き込みや権限の変更は実行されません。正確なポップアップの配置、フォーカス損失の解除、および Windows の背景効果は複製されません。

## 構築して検証する

必要 。 NET SDK 10.0 。 Linux では、クローンから:

```sh
cd TrayMixer
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
dotnet run --project linux/TrayMixer.Linux.csproj
# Packaging requires dpkg-deb, rpm/rpmbuild and curl:
bash linux/package.sh 3.1.1
```

現在のWindows開発機の場合:

```cmd
cd /d C:\Users\Vlad\sx\traymixer
dotnet build linux/TrayMixer.Linux.csproj -c Release
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
dotnet publish linux/TrayMixer.Linux.csproj -c Release -r linux-x64 --self-contained true -o linux/artifacts/publish
```

Windows は Linux バイナリをクロスパブリッシュできますが、Linux 環境なしではバイナリを実行したり、ネイティブ パッケージを作成したりすることはできません。 `.github/workflows/linux.yml` テスト、null 出力に対する実際の PulseAudio リードバック、パッケージ メタデータ検証、および Xvfb 起動スモーク チェックを実行します。これらは、実際の GNOME トレイ、テーマ、ハードウェアのテスト、またはオーディオの試聴に代わるものではありません。 Linuxリリースタグ使用 `linux-v*` そのため、既存の Windows リリース ワークフローはトリガーされません。既存の Windows リリースは保持されます。
