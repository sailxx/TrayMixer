[English](README.md) · [العربية](README.ar.md) · [简体中文](README.zh-CN.md) · [繁體中文](README.zh-TW.md) · [Čeština](README.cs.md) · [Français](README.fr.md) · **Deutsch** · [Ελληνικά](README.el.md) · [Bahasa Indonesia](README.id.md) · [Gaeilge](README.ga.md) · [Italiano](README.it.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Polski](README.pl.md) · [Română](README.ro.md) · [Русский](README.ru.md) · [Español](README.es.md) · [Українська](README.uk.md) · [اردو](README.ur.md) · [ئۇيغۇرچە](README.ug.md) · [Tiếng Việt](README.vi.md)

# Vorschau TrayMixer für Linux – 3.1.1

Ein separater C#/Avalonia-Desktop-Port. Die vorhandene Windows-Quelle und das Installationsprogramm bleiben unverändert.

## Unterstützte Basislinie

x86_64 Linux, glibc 2.35 +, X11 oder XWayland, PulseAudio 15+ oder PipeWire mit `pipewire-pulse`, Und `pactl`. Build- und Paketprüfungen werden unter Ubuntu 24.04 ausgeführt. Eine verteilungsspezifische Installation und die Akzeptanz des GNOME-Desktops sind weiterhin erforderlich; Dies ist eine Vorschau und kein Anspruch auf vollständige Windows-Parität.

## Installieren

Laden Sie einen DEB, RPM oder AppImage aus der Linux-Vorschauversion herunter.

- Ubuntu/Debian: `sudo apt install ./TrayMixer_3.1.1_amd64.deb`
- Fedora: `sudo dnf install ./TrayMixer-3.1.1-1.x86_64.rpm`
- AppImage: `chmod +x TrayMixer-3.1.1-x86_64.AppImage`, dann führe es aus. Erfordert System `pactl`, X11-Bibliotheken, Fontconfig, ICU und OpenSSL. Installieren `pulseaudio-utils` auf Ubuntu/Fedora. FUSE 2 oder `--appimage-extract-and-run` kann erforderlich sein. AppImage-Pakete. NET, nicht der System-Audioserver oder die Desktop-Bibliotheken.

Start **TrayMixer** aus dem Anwendungsmenü. Der erste Start öffnet den Mixer; Wenn Sie es schließen, bleibt das Taskleistensymbol aktiv. Verlassen Sie das Taskleistenmenü. GNOME Shell benötigt eine AppIndicator/StatusNotifier-Erweiterung (Ubuntu enthält normalerweise eine). Aktivieren Sie ohne einen Tray-Host nicht den Nur-Tray-Autostart. Verwenden Sie zum Starten das Anwendungsmenü. Beim erneuten Starten wird der vorhandene Prozess aufgefordert, sein Fenster über eine lokale Pipe, die nur dem aktuellen Benutzer vorbehalten ist, erneut zu öffnen.

## Kontrollen

- Die Tablettaktivierung öffnet/schließt den Mixer. Das Taskleistenmenü bietet „Mixer öffnen“, „Standardausgabe stumm schalten“, „Ausgeblendete Zeilen wiederherstellen“, „Autostart“ und „Beenden“. Desktop-Tray-Hosts steuern das Klickverhalten; Die mittlere Klickparität von Windows ist nicht garantiert.
- Ausgabe- und Anwendungsstream-Schieberegler: 0–100 %, Stummschaltung, Mausrad ±2. Wenn Sie eine positive Lautstärke einstellen, wird die Stummschaltung explizit aufgehoben.
- Klicken Sie auf einen Ausgabenamen, um ihn als Standard für die neue Wiedergabe festzulegen. Vorhandene Streams werden nicht zwangsweise verschoben.
- Klicken Sie mit der rechten Maustaste auf eine Zeile, um sie auszublenden. Ausgeblendete Zeilen wiederherstellen bringt alle Zeilen zurück. Separate Streams werden separat angezeigt, einschließlich Streams auf nicht standardmäßigen Geräten.
- Beim Anmelden starten verwendet einen XDG-Autostarteintrag. bis zur Auswahl deaktiviert. AppImage muss an seinem festen Standort bleiben.
- Aussehen: GNOME / System folgt Hell/Dunkel und GNOME-Akzentfarben, aufgefrischt bei geöffnetem Mischer. Ältere GNOME ohne Akzentunterstützung verwenden Blau. Charcoal, Nord, Catppuccin und Light sind eingebaut. Kein Glimmer oder Transparenz des gesamten Fensters.
- Einstellungen: `$XDG_CONFIG_HOME/traymixer/settings.json`, Standard `~/.config/traymixer/settings.json`.

## Bekannte Lücken im Vergleich zu Windows

Integrierte Schieberegler für Helligkeit, Blaulichtstärke und Software-Dimmung pro Monitor sind noch nicht portiert. **Anzeige- und Nachtlichteinstellungen** Öffnet stattdessen die GNOME/KDE-Einstellungen. Beim Wayland gibt es keine universelle Gamma-Einstellung. Es ist eine kompositorspezifische Implementierung erforderlich. Es werden keine willkürlichen privilegierten Schreibvorgänge oder Berechtigungsänderungen durchgeführt. Die exakte Popup-Platzierung, Fokusverlust-Entfernung und Windows-Hintergrundeffekte werden nicht repliziert.

## Erstellen und überprüfen

Erfordert . NET SDK 10.0 . Auf Linux, von einem Klon:

```sh
cd TrayMixer
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
dotnet run --project linux/TrayMixer.Linux.csproj
# Packaging requires dpkg-deb, rpm/rpmbuild and curl:
bash linux/package.sh 3.1.1
```

Auf der aktuellen Entwicklungsmaschine Windows:

```cmd
cd /d C:\Users\Vlad\sx\traymixer
dotnet build linux/TrayMixer.Linux.csproj -c Release
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
dotnet publish linux/TrayMixer.Linux.csproj -c Release -r linux-x64 --self-contained true -o linux/artifacts/publish
```

Windows kann Linux-Binärdateien übergreifend veröffentlichen, sie jedoch ohne eine Linux-Umgebung weder ausführen noch native Pakete erstellen. `.github/workflows/linux.yml` führt Tests, echtes PulseAudio-Readback gegen eine Nullausgabe, Paketmetadatenvalidierung und Xvfb-Startrauchprüfungen durch. Diese ersetzen nicht das Testen eines tatsächlichen GNOME-Trays, von Themen, von Hardware oder das Anhören von Audio. Verwendung von Linux-Release-Tags `linux-v*` Daher wird der bestehende Windows-Release-Workflow nicht ausgelöst. Bestehende Windows-Versionen bleiben erhalten.
