[English](README.md) · [العربية](README.ar.md) · [简体中文](README.zh-CN.md) · [繁體中文](README.zh-TW.md) · [Čeština](README.cs.md) · **Français** · [Deutsch](README.de.md) · [Ελληνικά](README.el.md) · [Bahasa Indonesia](README.id.md) · [Gaeilge](README.ga.md) · [Italiano](README.it.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Polski](README.pl.md) · [Română](README.ro.md) · [Русский](README.ru.md) · [Español](README.es.md) · [Українська](README.uk.md) · [اردو](README.ur.md) · [ئۇيغۇرچە](README.ug.md) · [Tiếng Việt](README.vi.md)

# TrayMixer pour Linux — Aperçu du 3.1.1

Un port de bureau C# / Avalonia séparé. La source Windows et le programme d'installation existants sont inchangés.

## Base de référence prise en charge

x86_64 Linux , glibc 2.35 +, X11 ou XWayland , PulseAudio 15+ ou PipeWire avec `pipewire-pulse`, et `pactl`. Les vérifications de build et de package s'exécutent sur Ubuntu 24.04 . L'installation spécifique à la distribution et l'acceptation du bureau GNOME sont toujours requises ; il s'agit d'un aperçu, pas d'une affirmation de la parité complète du Windows.

## Installer

Téléchargez un DEB, un RPM ou un AppImage à partir de la version préliminaire du Linux.

- Ubuntu/Debian: `sudo apt install ./TrayMixer_3.1.1_amd64.deb`
- Feutre: `sudo dnf install ./TrayMixer-3.1.1-1.x86_64.rpm`
- AppImage: `chmod +x TrayMixer-3.1.1-x86_64.AppImage`, puis exécutez-le. Nécessite un système `pactl`, Bibliothèques X11, fontconfig, ICU et OpenSSL. Installer `pulseaudio-utils` sur Ubuntu/Fedora. FUSE 2 ou `--appimage-extract-and-run` peut être nécessaire. Faisceaux AppImage. NET, pas le serveur audio du système ou les bibliothèques de bureau.

Lancement **TrayMixer** à partir du menu des applications. Le premier lancement ouvre le mixeur ; sa fermeture maintient l'icône de la barre d'état active. Quittez le menu de la barre d'état. GNOME Shell nécessite une extension AppIndicator/StatusNotifier (Ubuntu en inclut normalement une). Sans hôte de plateau, n'activez pas le démarrage automatique du plateau uniquement ; utilisez le menu de l'application pour lancer. Le lancement demande au processus existant de rouvrir sa fenêtre via un canal local réservé à l'utilisateur actuel.

## Contrôles

- L'activation du plateau ouvre/ferme le mélangeur. Le menu de la barre d'état propose Ouvrir le mélangeur, couper la sortie par défaut, restaurer les lignes cachées, démarrer automatiquement et quitter. Les hôtes de la barre d'état du bureau contrôlent le comportement des clics ; La parité du clic central du Windows n’est pas garantie.
- Curseurs de sortie et de flux d'application : 0 à 100 %, muet, molette de la souris ±2. Le réglage d'un volume positif réactive explicitement le son.
- Cliquez sur un nom de sortie pour en faire le nom par défaut pour une nouvelle lecture. Les flux existants ne sont pas déplacés de force.
- Cliquez avec le bouton droit sur une ligne à masquer ; Restaurer les lignes masquées ramène toutes les lignes. Les flux distincts sont affichés séparément, y compris les flux sur les appareils autres que ceux par défaut.
- Le démarrage à la connexion utilise une entrée de démarrage automatique XDG ; désactivé jusqu’à ce qu’il soit choisi. Le AppImage doit rester à son emplacement permanent.
- Apparence : GNOME / Le système suit les couleurs d'accent clair/foncé et GNOME, rafraîchies lorsque le mélangeur est ouvert. L'ancien GNOME sans support d'accent utilise le bleu. Charcoal, Nord, Catppuccin et Light sont intégrés. Pas de mica ni de transparence sur toute la fenêtre.
- Paramètres: `$XDG_CONFIG_HOME/traymixer/settings.json`, défaut `~/.config/traymixer/settings.json`.

## Lacunes connues par rapport au Windows

Les curseurs intégrés de luminosité par moniteur, d’intensité de la lumière bleue et de gradation logicielle ne sont pas encore portés. **Paramètres d'affichage et de veilleuse** ouvre les paramètres GNOME / KDE à la place. Le réglage universel du gamma n'existe pas sur le Wayland ; une implémentation spécifique au compositeur est nécessaire. Aucune écriture privilégiée arbitraire ou modification d’autorisation n’est effectuée. Le placement exact des fenêtres contextuelles, le rejet de la perte de mise au point et les effets de fond Windows ne sont pas répliqués.

## Construire et vérifier

Nécessite . NET SDK 10.0 . Sur Linux , à partir d'un clone:

```sh
cd TrayMixer
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
dotnet run --project linux/TrayMixer.Linux.csproj
# Packaging requires dpkg-deb, rpm/rpmbuild and curl:
bash linux/package.sh 3.1.1
```

Sur la machine de développement Windows actuelle:

```cmd
cd /d C:\Users\Vlad\sx\traymixer
dotnet build linux/TrayMixer.Linux.csproj -c Release
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
dotnet publish linux/TrayMixer.Linux.csproj -c Release -r linux-x64 --self-contained true -o linux/artifacts/publish
```

Windows peut publier de manière croisée les binaires Linux mais ne peut pas les exécuter ni créer de packages natifs sans un environnement Linux. `.github/workflows/linux.yml` exécute des tests, une relecture réelle du PulseAudio par rapport à une sortie nulle, la validation des métadonnées du package et des contrôles de fumée de lancement Xvfb. Ceux-ci ne remplacent pas le test d’un véritable plateau GNOME, de thèmes, de matériel ou l’écoute de l’audio. Utilisation des balises de version Linux `linux-v*` le workflow de version Windows existant n'est donc pas déclenché. Les versions Windows existantes sont conservées.
