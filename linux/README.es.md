[English](README.md) · [العربية](README.ar.md) · [简体中文](README.zh-CN.md) · [繁體中文](README.zh-TW.md) · [Čeština](README.cs.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [Ελληνικά](README.el.md) · [Bahasa Indonesia](README.id.md) · [Gaeilge](README.ga.md) · [Italiano](README.it.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Polski](README.pl.md) · [Română](README.ro.md) · [Русский](README.ru.md) · **Español** · [Українська](README.uk.md) · [اردو](README.ur.md) · [ئۇيغۇرچە](README.ug.md) · [Tiếng Việt](README.vi.md)

# TrayMixer para Linux — Vista previa de 3.1.1

Un puerto de escritorio C#/Avalonia independiente. La fuente y el instalador del Windows existentes no han cambiado.

## Línea base admitida

x86_64 Linux , glibc 2.35 +, X11 o XWayland , PulseAudio 15+ o PipeWire con `pipewire-pulse`, y `pactl`. Las comprobaciones de compilación y paquetes se ejecutan en Ubuntu 24.04. Aún se requiere la instalación específica de la distribución y la aceptación del escritorio GNOME; Esta es una vista previa, no una afirmación de la paridad total del Windows.

## Instalar

Descargue una DEB, RPM o AppImage desde la versión preliminar de Linux.

- Ubuntu/Debian: `sudo apt install ./TrayMixer_3.1.1_amd64.deb`
- sombrero: `sudo dnf install ./TrayMixer-3.1.1-1.x86_64.rpm`
- AppImage: `chmod +x TrayMixer-3.1.1-x86_64.AppImage`, luego ejecútelo. Requiere sistema `pactl`, Bibliotecas X11, fontconfig, ICU y OpenSSL. Instalar `pulseaudio-utils` en Ubuntu/Fedora. FUSE 2 o `--appimage-extract-and-run` puede ser necesario. Paquetes AppImage. NET, no el servidor de audio del sistema ni las bibliotecas de escritorio.

Lanzamiento **TrayMixer** desde el menú de la aplicación. El primer lanzamiento abre el mezclador; cerrarlo mantiene el icono de la bandeja funcionando. Salir del menú de la bandeja. GNOME Shell necesita una extensión AppIndicator/StatusNotifier (Ubuntu normalmente incluye una). Sin un host de bandeja, no habilite el inicio automático solo de bandeja; use el menú de la aplicación para iniciar. Al iniciarlo nuevamente, se solicita al proceso existente que vuelva a abrir su ventana a través de una tubería local exclusiva para el usuario actual.

## Controles

- La activación de la bandeja abre/cierra la batidora. El menú de la bandeja ofrece Abrir mezclador, silenciar la salida predeterminada, restaurar filas ocultas, inicio automático y Salir. Los hosts de la bandeja de escritorio controlan el comportamiento de los clics; La paridad del clic central del Windows no está garantizada.
- Controles deslizantes de salida y flujo de aplicaciones: 0–100%, silencio, rueda del mouse ±2. Establecer un volumen positivo activa explícitamente el sonido.
- Haga clic en el nombre de una salida para convertirla en la predeterminada para una nueva reproducción. Las corrientes existentes no se mueven por la fuerza.
- Haga clic derecho en una fila para ocultarla; Restaurar filas ocultas recupera todas las filas. Las transmisiones separadas se muestran por separado, incluidas las transmisiones en dispositivos no predeterminados.
- Iniciar al iniciar sesión utiliza una entrada de inicio automático XDG; deshabilitado hasta que sea elegido. AppImage debe permanecer en su ubicación permanente.
- Apariencia: GNOME / El sistema sigue los colores claros/oscuros y de acento GNOME, renovados mientras el mezclador está abierto. El modelo GNOME más antiguo sin soporte para acentos usa azul. Charcoal, Nord, Catppuccin y Light están integrados. Sin Mica ni transparencia en toda la ventana.
- Ajustes: `$XDG_CONFIG_HOME/traymixer/settings.json`, por defecto `~/.config/traymixer/settings.json`.

## Brechas conocidas versus Windows

El brillo integrado por monitor, la intensidad de la luz azul y los controles deslizantes de atenuación del software aún no están adaptados. **Configuración de pantalla y luz nocturna** abre la configuración de GNOME / KDE en su lugar. El ajuste de gamma universal no existe en Wayland; Se necesita una implementación específica del compositor. No se realizan escrituras privilegiadas arbitrarias ni cambios de permisos. La ubicación exacta de las ventanas emergentes, la eliminación de la pérdida de enfoque y los efectos de fondo del Windows no se replican.

## Construir y verificar

Requiere. NET SDK 10.0. En Linux, de un clon:

```sh
cd TrayMixer
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
dotnet run --project linux/TrayMixer.Linux.csproj
# Packaging requires dpkg-deb, rpm/rpmbuild and curl:
bash linux/package.sh 3.1.1
```

En la máquina de desarrollo actual Windows:

```cmd
cd /d C:\Users\Vlad\sx\traymixer
dotnet build linux/TrayMixer.Linux.csproj -c Release
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
dotnet publish linux/TrayMixer.Linux.csproj -c Release -r linux-x64 --self-contained true -o linux/artifacts/publish
```

Windows puede realizar publicaciones cruzadas de archivos binarios de Linux, pero no puede ejecutarlos ni crear paquetes nativos sin un entorno Linux. `.github/workflows/linux.yml` ejecuta pruebas, lectura real de PulseAudio frente a una salida nula, validación de metadatos del paquete y comprobaciones de humo de lanzamiento de Xvfb. Estos no reemplazan la prueba de una bandeja, temas, hardware GNOME real o escuchar audio. Uso de etiquetas de lanzamiento Linux `linux-v*` por lo que no se activa el flujo de trabajo de lanzamiento de Windows existente. Se conservan las versiones existentes de Windows.
