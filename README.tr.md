<div align="center">

[Русский](README.ru.md) · [English](README.md) · [Español](README.es.md) · [Português](README.pt.md) · [Deutsch](README.de.md) · [Français](README.fr.md) · [Italiano](README.it.md) · **Türkçe** · [Українська](README.uk.md) · [Polski](README.pl.md)

<br>

<img src="assets/readme/hero-tr.svg" width="100%" alt="TrayMixer — her cihazın ve uygulamanın sesi tek bir Windows 11 penceresinde">

<a href="https://github.com/sailxx/TrayMixer/releases/latest/download/TrayMixer.exe"><img src="assets/readme/cta-tr.svg" height="52" alt="Windows 11 için indir"></a>

</div>

<br>

<img src="assets/readme/screenshot.png" width="100%" alt="TrayMixer penceresi: kulaklık, hoparlör, tarayıcı ve sistem sesleri tek ekranda">

Windows 11'in ses karıştırıcısı Ayarlar'ın içinde saklı, tepsideki ses simgesi ise yalnızca tek bir cihazı kontrol ediyor. **TrayMixer** bir tık uzağınızda: tüm kulaklıklar, hoparlörler, monitörler ve her uygulama, her biri kendi kaydırıcısıyla. Kurulum yok, reklam yok, arka plan yükü yok.

<br>

<img src="assets/readme/features-tr.svg" width="100%" alt="Özellikler: tüm cihazlar, her uygulama, yerel Windows 11, gereksizi gizleme, hafif, güvenli">

<details>
<summary><b>Kontroller</b></summary>

| Eylem | Sonuç |
|---|---|
| Tepsi simgesine **sol tık** | Pencereyi aç veya kapat |
| Simgeye **orta tık** | Varsayılan cihazın sesini kapat veya aç |
| Simgeye **sağ tık** | Ses ayarları, Windows karıştırıcısı, gizli satırlar, Mica/Acrylic, otomatik başlatma, çıkış |
| **Satır simgesine** tık | Sesi kapat veya aç |
| **Cihaz adına** tık | Varsayılan cihaz yap |
| Satıra **sağ tık** | Satırı gizle |
| Satırın üzerinde **fare tekerleği** | Ses veya parlaklık ±2 |
| **Esc** veya pencere dışına tık | Kapat |

</details>

> [!NOTE]
> TrayMixer'ın arayüzü Rusçadır.

<br>

<img src="assets/readme/numbers-tr.svg" width="100%" alt="~2 MB RAM, %0 CPU, 165 KB, 0 bağımlılık">

<br>

<img src="assets/readme/security-tr.svg" width="100%" alt="Güvenlik: yönetici hakkı yok, ağ yok, DLL ele geçirmeye karşı koruma, doğrulanabilir derlemeler">

<details>
<summary><b>exe'nin bu koddan derlendiği nasıl doğrulanır</b></summary>

Her sürümü GitHub Actions doğrudan kaynak koddan derler ve GitHub dosyanın kaynağını imzalar (build provenance):

```bash
gh attestation verify TrayMixer.exe --repo sailxx/TrayMixer
```

SHA-256 sağlama toplamı her sürümde exe'nin yanında bulunur (`TrayMixer.exe.sha256`):

```powershell
Get-FileHash .\TrayMixer.exe -Algorithm SHA256
```

</details>

## Kurulum

1. [`TrayMixer.exe`](https://github.com/sailxx/TrayMixer/releases/latest/download/TrayMixer.exe) dosyasını indirin ve kalıcı bir klasöre koyun, örneğin `%LOCALAPPDATA%\TrayMixer`.
2. Çalıştırın. Simge tepside görünür ve otomatik başlatma kendiliğinden açılır (menüden kapatılabilir).

## Code signing policy

Free code signing provided by [SignPath.io](https://signpath.io), certificate by [SignPath Foundation](https://signpath.org).

- **What is signed:** only the official `TrayMixer.exe` that GitHub Actions builds from this repository when a `v*` tag is pushed (see `.github/workflows/release.yml`). Builds from forks, branches or local machines are never signed.
- **Team roles:** committer, reviewer and approver of every signing request — [@sailxx](https://github.com/sailxx). Two-factor authentication is required for the GitHub and SignPath accounts.
- **Privacy:** this program will not transfer any information to other networked systems unless specifically requested by the user or the person installing or operating it.
- **Verification:** besides the signature, every release carries a SHA-256 file and a GitHub build provenance attestation (`gh attestation verify TrayMixer.exe --repo sailxx/TrayMixer`).
