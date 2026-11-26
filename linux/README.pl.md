[English](README.md) · [العربية](README.ar.md) · [简体中文](README.zh-CN.md) · [繁體中文](README.zh-TW.md) · [Čeština](README.cs.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [Ελληνικά](README.el.md) · [Bahasa Indonesia](README.id.md) · [Gaeilge](README.ga.md) · [Italiano](README.it.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · **Polski** · [Română](README.ro.md) · [Русский](README.ru.md) · [Español](README.es.md) · [Українська](README.uk.md) · [اردو](README.ur.md) · [ئۇيغۇرچە](README.ug.md) · [Tiếng Việt](README.vi.md)

# TrayMixer dla Linux — podgląd 3.1.1

Oddzielny port do komputerów stacjonarnych C# / Avalonia. Istniejące źródło i instalator Windows pozostają niezmienione.

## Obsługiwana linia bazowa

x86_64 Linux , glibc 2.35 +, X11 lub XWayland , PulseAudio 15+ lub PipeWire z `pipewire-pulse`, I `pactl`. Sprawdzanie kompilacji i pakietów odbywa się na Ubuntu 24.04. Nadal wymagana jest instalacja specyficzna dla dystrybucji i akceptacja pulpitu GNOME; to jest podgląd, a nie twierdzenie o pełnej parzystości Windows.

## Zainstalować

Pobierz DEB, RPM lub AppImage z wersji poglądowej Linux.

- Ubuntu/Debiana: `sudo apt install ./TrayMixer_3.1.1_amd64.deb`
- Fedora: `sudo dnf install ./TrayMixer-3.1.1-1.x86_64.rpm`
- AppImage: `chmod +x TrayMixer-3.1.1-x86_64.AppImage`, następnie uruchom go. Wymaga systemu `pactl`, Biblioteki X11, Fontconfig, ICU i OpenSSL. Zainstalować `pulseaudio-utils` na Ubuntu/Fedorze. FUSE 2 lub `--appimage-extract-and-run` może być wymagane. Pakiety AppImage . NET, a nie systemowy serwer audio lub biblioteki komputerowe.

Początek **TrayMixer** z menu aplikacji. Pierwsze uruchomienie otwiera mikser; zamknięcie powoduje, że ikona w zasobniku nadal działa. Wyjdź z menu zasobnika. GNOME Shell wymaga rozszerzenia AppIndicator/StatusNotifier (Ubuntu zwykle je zawiera). Bez hosta zasobnika nie włączaj automatycznego uruchamiania tylko zasobnika; użyj menu aplikacji, aby uruchomić. Ponowne uruchomienie prosi istniejący proces o ponowne otwarcie okna za pośrednictwem lokalnego potoku tylko dla bieżącego użytkownika.

## Sterownica

- Aktywacja tacy otwiera/zamyka mikser. Menu zasobnika oferuje opcję Otwórz mikser, domyślne wyciszenie wyjścia, przywrócenie ukrytych wierszy, autostart i wyjście. Hosty pulpitu kontrolują zachowanie kliknięcia; Windows nie gwarantuje parzystości środkowego kliknięcia.
- Suwaki wyjściowe i strumienia aplikacji: 0–100%, wyciszenie, kółko myszy ±2. Ustawienie dodatniej głośności wyraźnie wyłącza wyciszenie.
- Kliknij nazwę wyjścia, aby ustawić ją jako domyślną dla nowego odtwarzania. Istniejące strumienie nie są przesuwane na siłę.
- Kliknij prawym przyciskiem myszy wiersz, który chcesz ukryć; Przywróć ukryte wiersze przywraca wszystkie wiersze. Oddzielne strumienie są wyświetlane osobno, łącznie ze strumieniami na urządzeniach innych niż domyślne.
- Rozpocznij przy logowaniu używa wpisu autostartu XDG; wyłączone do czasu wybrania. AppImage musi pozostać na swoim stałym miejscu.
- Wygląd: GNOME / System podąża za jasnymi/ciemnymi kolorami i akcentami GNOME, odświeżanymi, gdy mikser jest otwarty. Starszy GNOME bez obsługi akcentów używa koloru niebieskiego. Wbudowane modele Charcoal, Nord, Catppuccin i Light. Brak miki i przezroczystości całego okna.
- Ustawienia: `$XDG_CONFIG_HOME/traymixer/settings.json`, domyślny `~/.config/traymixer/settings.json`.

## Znane luki w porównaniu z Windows

Zintegrowane suwaki jasności dla każdego monitora, siły niebieskiego światła i programowego przyciemniania nie zostały jeszcze przeniesione. **Ustawienia wyświetlacza i oświetlenia nocnego** zamiast tego otwiera ustawienia GNOME / KDE. Uniwersalna regulacja gamma nie istnieje w Wayland; wymagana jest implementacja specyficzna dla kompozytora. Nie są wykonywane żadne arbitralne uprzywilejowane zapisy ani zmiany uprawnień. Dokładne rozmieszczenie wyskakujących okienek, eliminacja utraty ostrości i efekty tła Windows nie są replikowane.

## Buduj i sprawdzaj

Wymaga . NET SDK 10.0 . Na Linux, z klonu:

```sh
cd TrayMixer
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
dotnet run --project linux/TrayMixer.Linux.csproj
# Packaging requires dpkg-deb, rpm/rpmbuild and curl:
bash linux/package.sh 3.1.1
```

Na obecnej maszynie rozwojowej Windows:

```cmd
cd /d C:\Users\Vlad\sx\traymixer
dotnet build linux/TrayMixer.Linux.csproj -c Release
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
dotnet publish linux/TrayMixer.Linux.csproj -c Release -r linux-x64 --self-contained true -o linux/artifacts/publish
```

Windows może publikować pliki binarne Linux, ale nie może ich wykonywać ani tworzyć pakietów natywnych bez środowiska Linux. `.github/workflows/linux.yml` uruchamia testy, odczytuje prawdziwy PulseAudio względem zerowego wyjścia, sprawdza poprawność metadanych pakietu i uruchamia kontrolę dymu Xvfb. Nie zastępują one testowania rzeczywistej tacy GNOME, motywów, sprzętu ani słuchania dźwięku. Używane są znaczniki wydania Linux `linux-v*` dlatego istniejący przepływ pracy związany z wydaniem Windows nie zostanie uruchomiony. Istniejące wersje Windows zostają zachowane.
