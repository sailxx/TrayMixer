[English](README.md) · [العربية](README.ar.md) · [简体中文](README.zh-CN.md) · [繁體中文](README.zh-TW.md) · [Čeština](README.cs.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [Ελληνικά](README.el.md) · **Bahasa Indonesia** · [Gaeilge](README.ga.md) · [Italiano](README.it.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Polski](README.pl.md) · [Română](README.ro.md) · [Русский](README.ru.md) · [Español](README.es.md) · [Українська](README.uk.md) · [اردو](README.ur.md) · [ئۇيغۇرچە](README.ug.md) · [Tiếng Việt](README.vi.md)

# TrayMixer untuk Linux — pratinjau 3.1.1

Port desktop C# / Avalonia terpisah. Sumber dan penginstal Windows yang ada tidak berubah.

## Garis dasar yang didukung

x86_64 Linux , glibc 2.35 +, X11 atau XWayland , PulseAudio 15+ atau PipeWire dengan `pipewire-pulse`, Dan `pactl`. Pemeriksaan build dan paket dijalankan di Ubuntu 24.04. Instalasi khusus distribusi dan penerimaan desktop GNOME masih diperlukan; ini adalah pratinjau, bukan klaim paritas penuh Windows.

## Memasang

Unduh DEB, RPM, atau AppImage dari rilis pratinjau Linux.

- Ubuntu/Debian: `sudo apt install ./TrayMixer_3.1.1_amd64.deb`
- fedora: `sudo dnf install ./TrayMixer-3.1.1-1.x86_64.rpm`
- AppImage: `chmod +x TrayMixer-3.1.1-x86_64.AppImage`, lalu jalankan. Membutuhkan sistem `pactl`, Perpustakaan X11, fontconfig, ICU dan OpenSSL. Memasang `pulseaudio-utils` di Ubuntu/Fedora. FUSE 2 atau `--appimage-extract-and-run` mungkin diperlukan. Paket AppImage. NET, bukan server audio sistem atau perpustakaan desktop.

Meluncurkan **TrayMixer** dari menu aplikasi. Peluncuran pertama membuka mixer; menutupnya membuat ikon baki tetap berjalan. Keluar dari menu baki. GNOME Shell memerlukan ekstensi AppIndicator/StatusNotifier (Ubuntu biasanya menyertakannya). Tanpa host baki, jangan aktifkan autostart khusus baki; gunakan menu aplikasi untuk meluncurkan. Peluncuran lagi meminta proses yang ada untuk membuka kembali jendelanya melalui pipa lokal khusus pengguna saat ini.

## Kontrol

- Aktivasi baki membuka/menutup mixer. Menu baki menawarkan Open mixer, mute output default, mengembalikan baris tersembunyi, autostart dan Exit. Host baki desktop mengontrol perilaku klik; Paritas klik tengah Windows tidak dijamin.
- Penggeser keluaran dan aliran aplikasi: 0–100%, senyap, roda mouse ±2. Menyetel volume positif secara eksplisit akan membunyikan suara.
- Klik nama keluaran untuk menjadikannya default untuk pemutaran baru. Aliran sungai yang ada tidak dipindahkan secara paksa.
- Klik kanan satu baris untuk disembunyikan; Pulihkan baris tersembunyi mengembalikan semua baris. Aliran terpisah ditampilkan secara terpisah, termasuk aliran pada perangkat non-default.
- Mulai saat login menggunakan entri autostart XDG; dinonaktifkan sampai dipilih. AppImage harus tetap berada di lokasi permanennya.
- Penampilan: GNOME / Sistem mengikuti warna aksen terang/gelap dan GNOME, disegarkan saat mixer terbuka. GNOME lawas tanpa dukungan aksen menggunakan warna biru. Arang, Nord, Catppuccin, dan Light sudah terpasang. Tidak ada Mika atau transparansi seluruh jendela.
- Pengaturan: `$XDG_CONFIG_HOME/traymixer/settings.json`, bawaan `~/.config/traymixer/settings.json`.

## Kesenjangan yang diketahui versus Windows

Kecerahan per monitor yang terintegrasi, kekuatan cahaya biru, dan penggeser peredupan perangkat lunak belum di-porting. **Pengaturan tampilan & lampu malam** membuka pengaturan GNOME / KDE sebagai gantinya. Penyesuaian gamma universal tidak ada di Wayland; diperlukan implementasi khusus kompositor. Tidak ada penulisan hak istimewa atau perubahan izin sewenang-wenang yang dilakukan. Penempatan popup yang tepat, hilangnya fokus, dan efek latar Windows tidak ditiru.

## Bangun dan verifikasi

Memerlukan . NET SDK 10.0 . Di Linux, dari klon:

```sh
cd TrayMixer
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
dotnet run --project linux/TrayMixer.Linux.csproj
# Packaging requires dpkg-deb, rpm/rpmbuild and curl:
bash linux/package.sh 3.1.1
```

Pada mesin pengembangan Windows saat ini:

```cmd
cd /d C:\Users\Vlad\sx\traymixer
dotnet build linux/TrayMixer.Linux.csproj -c Release
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
dotnet publish linux/TrayMixer.Linux.csproj -c Release -r linux-x64 --self-contained true -o linux/artifacts/publish
```

Windows dapat memublikasikan silang binari Linux tetapi tidak dapat menjalankannya atau membuat paket asli tanpa lingkungan Linux. `.github/workflows/linux.yml` menjalankan pengujian, pembacaan kembali PulseAudio yang sebenarnya terhadap keluaran nol, validasi metadata paket, dan pemeriksaan asap peluncuran Xvfb. Itu tidak menggantikan pengujian baki GNOME yang sebenarnya, tema, perangkat keras, atau mendengarkan audio. Tag rilis Linux digunakan `linux-v*` jadi alur kerja rilis Windows yang ada tidak terpicu. Rilis Windows yang ada dipertahankan.
