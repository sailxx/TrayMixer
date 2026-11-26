[English](README.md) · [العربية](README.ar.md) · [简体中文](README.zh-CN.md) · [繁體中文](README.zh-TW.md) · **Čeština** · [Français](README.fr.md) · [Deutsch](README.de.md) · [Ελληνικά](README.el.md) · [Bahasa Indonesia](README.id.md) · [Gaeilge](README.ga.md) · [Italiano](README.it.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Polski](README.pl.md) · [Română](README.ro.md) · [Русский](README.ru.md) · [Español](README.es.md) · [Українська](README.uk.md) · [اردو](README.ur.md) · [ئۇيغۇرچە](README.ug.md) · [Tiếng Việt](README.vi.md)

# TrayMixer pro Linux — Náhled 3.1.1

Samostatný port pro stolní počítač C# / Avalonia. Stávající zdroj Windows a instalační program se nezměnily.

## Podporovaná základní linie

x86_64 Linux , glibc 2.35 +, X11 nebo XWayland , PulseAudio 15+ nebo PipeWire s `pipewire-pulse`, a `pactl`. Kontroly sestavení a balíčků běží na Ubuntu 24.04. Stále je vyžadována instalace specifická pro distribuci a akceptace GNOME desktopem; toto je náhled, ne nárok na plnou paritu Windows.

## Instalovat

Stáhněte si DEB , RPM nebo AppImage z verze náhledu Linux.

- Ubuntu/Debian: `sudo apt install ./TrayMixer_3.1.1_amd64.deb`
- Fedora: `sudo dnf install ./TrayMixer-3.1.1-1.x86_64.rpm`
- AppImage: `chmod +x TrayMixer-3.1.1-x86_64.AppImage`, pak to spusťte. Vyžaduje systém `pactl`, Knihovny X11, fontconfig, ICU a OpenSSL. Instalovat `pulseaudio-utils` na Ubuntu/Fedoře. FUSE 2 nebo `--appimage-extract-and-run` může být vyžadováno. Svazky AppImage . NET , nikoli systémový audio server nebo desktopové knihovny.

Zahájit **TrayMixer** z nabídky aplikace. První spuštění otevře mixér; zavřením zůstane ikona na hlavním panelu spuštěná. Ukončete nabídku zásobníku. GNOME Shell potřebuje rozšíření AppIndicator/StatusNotifier (Ubuntu obvykle jedno obsahuje). Bez hostitele zásobníku nepovolujte automatické spouštění pouze zásobníku; ke spuštění použijte nabídku aplikace. Opětovné spuštění požádá stávající proces, aby znovu otevřel své okno prostřednictvím místního kanálu pouze pro aktuálního uživatele.

## Ovládací prvky

- Aktivace zásobníku otevírá/zavírá mixér. Nabídka zásobníku nabízí Otevřít směšovač, výchozí ztlumení výstupu, obnovení skrytých řádků, automatické spuštění a ukončení. Hostitelé na ploše řídí chování kliknutí; Windows parita prostředního kliknutí není zaručena.
- Posuvníky výstupu a proudu aplikací: 0–100 %, ztlumení, kolečko myši ±2. Nastavení kladné hlasitosti explicitně zapíná zvuk.
- Klepnutím na název výstupu jej nastavíte jako výchozí pro nové přehrávání. Stávající proudy nejsou násilně přesunuty.
- Klepněte pravým tlačítkem na řádek, který chcete skrýt; Obnovit skryté řádky vrátí všechny řádky zpět. Samostatné streamy se zobrazují samostatně, včetně streamů na jiných než výchozích zařízeních.
- Start at login používá položku automatického spuštění XDG; zakázáno, dokud není vybráno. AppImage musí zůstat na svém trvalém místě.
- Vzhled: GNOME / Systém sleduje světlé/tmavé a zvýrazňující barvy GNOME, obnovuje se, když je mixér otevřený. Starší GNOME bez podpory akcentu používá modrou. Dřevěné uhlí, Nord, Catppuccin a Light jsou zabudovány. Žádná slída ani průhlednost celého okna.
- Nastavení: `$XDG_CONFIG_HOME/traymixer/settings.json`, výchozí `~/.config/traymixer/settings.json`.

## Známé mezery oproti Windows

Integrované posuvníky jasu pro jednotlivé monitory, intenzity modrého světla a softwarového stmívání ještě nejsou portovány. **Nastavení displeje a nočního osvětlení** místo toho otevře nastavení GNOME / KDE. Univerzální nastavení gama na Wayland neexistuje; je nutná implementace specifická pro konkrétní sazbu. Nejsou prováděny žádné svévolné privilegované zápisy ani změny oprávnění. Přesné umístění vyskakovacích oken, vyloučení ztráty zaměření a efekty pozadí Windows nejsou replikovány.

## Sestavte a ověřte

Vyžaduje . NET SDK 10.0 . Na Linux z klonu:

```sh
cd TrayMixer
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
dotnet run --project linux/TrayMixer.Linux.csproj
# Packaging requires dpkg-deb, rpm/rpmbuild and curl:
bash linux/package.sh 3.1.1
```

Na aktuálním vývojovém stroji Windows:

```cmd
cd /d C:\Users\Vlad\sx\traymixer
dotnet build linux/TrayMixer.Linux.csproj -c Release
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
dotnet publish linux/TrayMixer.Linux.csproj -c Release -r linux-x64 --self-contained true -o linux/artifacts/publish
```

Windows může křížově publikovat binární soubory Linux, ale nemůže je spouštět ani vytvářet nativní balíčky bez prostředí Linux. `.github/workflows/linux.yml` spouští testy, skutečné zpětné čtení PulseAudio proti nulovému výstupu, ověřování metadat balíčku a kontroly kouře při spuštění Xvfb. Ty nenahrazují testování skutečného zásobníku GNOME, témat, hardwaru nebo poslechu zvuku. Použití uvolňovacích štítků Linux `linux-v*` takže stávající pracovní postup vydání Windows se nespustí. Stávající verze Windows jsou zachovány.
