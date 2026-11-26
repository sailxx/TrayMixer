[English](README.md) · [العربية](README.ar.md) · [简体中文](README.zh-CN.md) · [繁體中文](README.zh-TW.md) · [Čeština](README.cs.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [Ελληνικά](README.el.md) · [Bahasa Indonesia](README.id.md) · [Gaeilge](README.ga.md) · **Italiano** · [日本語](README.ja.md) · [한국어](README.ko.md) · [Polski](README.pl.md) · [Română](README.ro.md) · [Русский](README.ru.md) · [Español](README.es.md) · [Українська](README.uk.md) · [اردو](README.ur.md) · [ئۇيغۇرچە](README.ug.md) · [Tiếng Việt](README.vi.md)

# TrayMixer per Linux: anteprima 3.1.1

Una porta desktop C#/Avalonia separata. La sorgente e il programma di installazione Windows esistenti rimangono invariati.

## Linea di base supportata

x86_64 Linux , glibc 2.35 +, X11 o XWayland , PulseAudio 15+ o PipeWire con `pipewire-pulse`, E `pactl`. I controlli di compilazione e pacchetto vengono eseguiti su Ubuntu 24.04. Sono ancora necessarie l'installazione specifica della distribuzione e l'accettazione del desktop GNOME; questa è un'anteprima, non un'affermazione della piena parità Windows.

## Installare

Scarica uno DEB, RPM o AppImage dalla versione di anteprima Linux.

- Ubuntu/Debian: `sudo apt install ./TrayMixer_3.1.1_amd64.deb`
- Fedora: `sudo dnf install ./TrayMixer-3.1.1-1.x86_64.rpm`
- AppImage: `chmod +x TrayMixer-3.1.1-x86_64.AppImage`, quindi eseguilo. Richiede sistema `pactl`, Librerie X11, fontconfig, ICU e OpenSSL. Installare `pulseaudio-utils` su Ubuntu/Fedora. FUSE2 o `--appimage-extract-and-run` potrebbe essere richiesto. Fasci AppImage . NET, non il server audio di sistema o le librerie desktop.

Lancio **TrayMixer** dal menu dell'applicazione. Il primo lancio apre il mixer; chiudendolo si mantiene attiva l'icona nella barra delle applicazioni. Uscire dal menu del vassoio. GNOME Shell necessita di un'estensione AppIndicator/StatusNotifier (Ubuntu normalmente ne include una). Senza un host del vassoio, non abilitare l'avvio automatico solo del vassoio; utilizzare il menu dell'applicazione per avviarla. Il nuovo avvio richiede al processo esistente di riaprire la finestra tramite una pipe locale riservata solo all'utente corrente.

## Controlli

- L'attivazione del vassoio apre/chiude il mixer. Il menu della barra delle applicazioni offre Apri mixer, disattivazione audio dell'output predefinito, ripristino di righe nascoste, avvio automatico ed Esci. Il vassoio del desktop ospita il controllo del comportamento dei clic; La parità del clic centrale Windows non è garantita.
- Cursori del flusso di uscita e dell'applicazione: 0–100%, disattivazione audio, rotella del mouse ±2. L'impostazione del volume positivo riattiva esplicitamente l'audio.
- Fare clic sul nome di un output per renderlo quello predefinito per la nuova riproduzione. I flussi esistenti non vengono spostati forzatamente.
- Fare clic con il pulsante destro del mouse su una riga da nascondere; Ripristina righe nascoste ripristina tutte le righe. I flussi separati vengono visualizzati separatamente, inclusi i flussi su dispositivi non predefiniti.
- Avvia all'accesso utilizza una voce di avvio automatico XDG; disabilitato fino alla scelta. AppImage deve rimanere nella sua posizione permanente.
- Aspetto: GNOME / Il sistema segue i colori chiari/scuri e GNOME, rinfrescati mentre il mixer è aperto. Il vecchio GNOME senza supporto per gli accenti utilizza il blu. Charcoal, Nord, Catppuccin e Light sono integrati. Nessuna mica o trasparenza dell'intera finestra.
- Impostazioni: `$XDG_CONFIG_HOME/traymixer/settings.json`, predefinito `~/.config/traymixer/settings.json`.

## Lacune note rispetto a Windows

La luminosità integrata per monitor, l'intensità della luce blu e i cursori di attenuazione del software non sono ancora stati trasferiti. **Impostazioni display e luce notturna** apre invece le impostazioni GNOME / KDE. La regolazione gamma universale non esiste su Wayland; è necessaria un'implementazione specifica del compositore. Non vengono eseguite scritture privilegiate arbitrarie o modifiche delle autorizzazioni. Il posizionamento esatto dei popup, l'esclusione della perdita di messa a fuoco e gli effetti dello sfondo Windows non vengono replicati.

## Costruisci e verifica

Richiede. NET SDK 10.0 . Su Linux, da un clone:

```sh
cd TrayMixer
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
dotnet run --project linux/TrayMixer.Linux.csproj
# Packaging requires dpkg-deb, rpm/rpmbuild and curl:
bash linux/package.sh 3.1.1
```

Sull'attuale macchina di sviluppo Windows:

```cmd
cd /d C:\Users\Vlad\sx\traymixer
dotnet build linux/TrayMixer.Linux.csproj -c Release
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
dotnet publish linux/TrayMixer.Linux.csproj -c Release -r linux-x64 --self-contained true -o linux/artifacts/publish
```

Windows può pubblicare in modo incrociato i binari Linux ma non può eseguirli o creare pacchetti nativi senza un ambiente Linux. `.github/workflows/linux.yml` esegue test, rilettura reale di PulseAudio rispetto a un output nullo, convalida dei metadati del pacchetto e controlli del fumo di lancio di Xvfb. Questi non sostituiscono il test di un vero vassoio GNOME, temi, hardware o l'ascolto di audio. Utilizzo dei tag di rilascio Linux `linux-v*` quindi il flusso di lavoro di rilascio Windows esistente non viene attivato. Le versioni Windows esistenti vengono mantenute.
