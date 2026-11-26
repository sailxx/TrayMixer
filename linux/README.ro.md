[English](README.md) · [العربية](README.ar.md) · [简体中文](README.zh-CN.md) · [繁體中文](README.zh-TW.md) · [Čeština](README.cs.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [Ελληνικά](README.el.md) · [Bahasa Indonesia](README.id.md) · [Gaeilge](README.ga.md) · [Italiano](README.it.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Polski](README.pl.md) · **Română** · [Русский](README.ru.md) · [Español](README.es.md) · [Українська](README.uk.md) · [اردو](README.ur.md) · [ئۇيغۇرچە](README.ug.md) · [Tiếng Việt](README.vi.md)

# TrayMixer pentru Linux — previzualizare 3.1.1

Un port separat pentru desktop C# / Avalonia. Sursa și programul de instalare Windows existente sunt neschimbate.

## Linie de bază acceptată

x86_64 Linux , glibc 2.35 +, X11 sau XWayland , PulseAudio 15+ sau PipeWire cu `pipewire-pulse`, şi `pactl`. Verificările de build și pachet rulează pe Ubuntu 24.04. Instalarea specifică distribuției și acceptarea desktopului GNOME sunt încă necesare; aceasta este o previzualizare, nu o revendicare a parității complete Windows.

## Instala

Descărcați un DEB , RPM sau AppImage din versiunea de previzualizare Linux.

- Ubuntu/Debian: `sudo apt install ./TrayMixer_3.1.1_amd64.deb`
- Fedora: `sudo dnf install ./TrayMixer-3.1.1-1.x86_64.rpm`
- AppImage: `chmod +x TrayMixer-3.1.1-x86_64.AppImage`, apoi rulează-l. Necesită sistem `pactl`, Biblioteci X11, fontconfig, ICU și OpenSSL. Instala `pulseaudio-utils` pe Ubuntu/Fedora. FUSE 2 sau `--appimage-extract-and-run` poate fi necesar. pachete AppImage . NET , nu serverul audio de sistem sau bibliotecile desktop.

Lansa **TrayMixer** din meniul aplicației. Prima lansare deschide mixerul; închiderea acestuia menține pictograma tavă în funcțiune. Ieșiți din meniul tavă. GNOME Shell are nevoie de o extensie AppIndicator/StatusNotifier (Ubuntu include în mod normal una). Fără o gazdă de tavă, nu activați pornirea automată numai pentru tavă; utilizați meniul aplicației pentru a lansa. Lansarea solicită din nou procesului existent să-și redeschidă fereastra printr-o conductă locală numai pentru utilizatorul curent.

## Controale

- Activarea tăvii deschide/închide mixerul. Meniul din tavă oferă Deschidere mixer, dezactivare implicită a ieșirii, restabilire a rândurilor ascunse, pornire automată și Ieșire. Gazdele din tava de desktop controlează comportamentul clicurilor; Windows paritatea clicului mijlociu nu este garantată.
- Glisoare de ieșire și flux de aplicații: 0–100%, dezactivare, roată mouse-ului ±2. Setarea volumului pozitiv activează în mod explicit sunetul.
- Faceți clic pe un nume de ieșire pentru ca acesta să fie implicit pentru o nouă redare. Fluxurile existente nu sunt mutate forțat.
- Faceți clic dreapta pe un rând pentru a ascunde; Restaurarea rândurilor ascunse aduce toate rândurile înapoi. Fluxurile separate sunt afișate separat, inclusiv fluxurile pe dispozitive care nu sunt implicite.
- Începeți la conectare utilizează o intrare de pornire automată XDG; dezactivat până la alegere. AppImage trebuie să rămână în locația sa permanentă.
- Aspect: GNOME / Sistemul urmărește culorile de accent deschis/întunecat și GNOME, reîmprospătat în timp ce mixerul este deschis. GNOME mai vechi fără suport pentru accent folosește albastru. Cărbune, Nord, Catppuccin și Light sunt încorporate. Fără Mica sau transparență pe toată geamul.
- Setări: `$XDG_CONFIG_HOME/traymixer/settings.json`, implicit `~/.config/traymixer/settings.json`.

## Lacune cunoscute față de Windows

Luminozitatea integrată pentru fiecare monitor, puterea luminii albastre și glisoarele de reglare software nu sunt încă portate. **Setări pentru afișaj și lumini de noapte** deschide în schimb setările GNOME / KDE. Ajustarea gamma universală nu există pe Wayland; este necesară o implementare specifică compozitorului. Nu se efectuează scriere arbitrară cu privilegii sau modificări ale permisiunilor. Plasarea exactă a ferestrelor pop-up, eliminarea pierderii focalizării și efectele de fundal Windows nu sunt replicate.

## Construiți și verificați

Necesită . NET SDK 10.0 . Pe Linux , dintr-o clonă:

```sh
cd TrayMixer
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
dotnet run --project linux/TrayMixer.Linux.csproj
# Packaging requires dpkg-deb, rpm/rpmbuild and curl:
bash linux/package.sh 3.1.1
```

Pe actuala mașină de dezvoltare Windows:

```cmd
cd /d C:\Users\Vlad\sx\traymixer
dotnet build linux/TrayMixer.Linux.csproj -c Release
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
dotnet publish linux/TrayMixer.Linux.csproj -c Release -r linux-x64 --self-contained true -o linux/artifacts/publish
```

Windows poate publica încrucișat binarele Linux, dar nu le poate executa sau crea pachete native fără un mediu Linux. `.github/workflows/linux.yml` rulează teste, citirea PulseAudio reală față de o ieșire nulă, validarea metadatelor pachetului și verificările de lansare Xvfb. Acestea nu înlocuiesc testarea unei tăvi GNOME, teme, hardware sau ascultarea audio. Etichetele de lansare Linux folosesc `linux-v*` astfel încât fluxul de lucru existent pentru lansarea Windows nu este declanșat. Versiunile Windows existente sunt păstrate.
