[English](README.md) · [العربية](README.ar.md) · [简体中文](README.zh-CN.md) · [繁體中文](README.zh-TW.md) · [Čeština](README.cs.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [Ελληνικά](README.el.md) · [Bahasa Indonesia](README.id.md) · [Gaeilge](README.ga.md) · [Italiano](README.it.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Polski](README.pl.md) · [Română](README.ro.md) · [Русский](README.ru.md) · **Español** · [Українська](README.uk.md) · [اردو](README.ur.md) · [ئۇيغۇرچە](README.ug.md) · [Tiếng Việt](README.vi.md) · [Português](README.pt.md) · [Türkçe](README.tr.md)

<div align="center">

<img src="assets/readme/logo-blue.svg" width="128" alt="TrayMixer">

# TrayMixer

**Controle el volumen de cada salida y aplicación desde un pequeño panel en la bandeja del sistema de su Windows 11 o Linux.**

Baje un juego mientras mantiene su llamada audible, cambie entre parlantes y auriculares, o ajuste el brillo, el filtrado de luz azul y la atenuación en Windows.

**Sin telemetría ni cuenta.** Código abierto bajo Apache 2.0.

<img src="https://img.shields.io/badge/Windows-v3.0.2-2E9BFF" alt="Lanzamiento Windows v3.0.2">
<img src="https://img.shields.io/badge/Windows-11%20x64-2E9BFF" alt="Windows 11x64">
<img src="https://img.shields.io/badge/Linux-v3.1.1%20preview-2E9BFF" alt="Vista previa de Linux v3.1.1">
<img src="https://img.shields.io/badge/Linux-AppImage%20%7C%20deb%20%7C%20rpm-2E9BFF" alt="Linux AppImage, DEB y RPM">
<img src="https://img.shields.io/badge/code-C%23-2E9BFF" alt="do sostenido">
<img src="https://img.shields.io/badge/Linux%20UI-Avalonia-2E9BFF" alt="Avalonia para Linux">
<img src="https://img.shields.io/badge/license-Apache%202.0-2E9BFF" alt="Apache 2.0">
<img src="https://img.shields.io/github/stars/sailxx/TrayMixer?style=flat&amp;logo=github&amp;color=%232E9BFF" alt="estrellas GitHub">
<br>

<img src="assets/readme/hero-en.svg" width="100%" alt="Banner TrayMixer con menú desplegable Windows, volumen de salida y aplicación, controles de pantalla y preferencias locales">

<a href="https://github.com/sailxx/TrayMixer/releases/download/v3.0.2/TrayMixer-Setup.exe"><img src="assets/readme/btn-windows-en.svg" height="52" alt="Descargue el instalador del Windows"></a>&nbsp;;<a href="https://github.com/sailxx/TrayMixer/releases/tag/linux-v3.1.1"><img src="assets/readme/btn-linux-en.svg" height="52" alt="Descargar paquetes Linux"></a>&nbsp;;<a href="https://github.com/sailxx/TrayMixer/releases/download/v3.0.2/TrayMixer.exe"><img src="assets/readme/btn-portable-en.svg" height="52" alt="Descargue el ejecutable portátil Windows"></a>&nbsp;;<a href="https://github.com/sailxx/TrayMixer/releases"><img src="assets/readme/btn-new-en.svg" height="52" alt="Lea las notas de la versión"></a>

</div>

<br>

<img src="assets/readme/features-en.svg" width="100%" alt="Características: salida separada y volumen de aplicaciones, controles de pantalla Windows, controles de bandeja, temas y preferencias locales">

<details>
<summary><b>Más sobre las características</b></summary>

### Salidas y volumen de aplicaciones

Cada salida tiene su propio control deslizante y de silencio. Haga clic en el nombre de una salida para seleccionarla como predeterminada. En Linux, esto se aplica a la reproducción nueva y cada transmisión de aplicación aparece por separado.

### Controles de pantalla Windows

Ajuste el brillo en monitores compatibles, caliente la pantalla con el filtro de luz azul y utilice el software de atenuación desde el mismo mezclador. [Vea la captura de pantalla del Windows en tamaño completo](assets/readme/screenshot.png).

### Controles de bandeja

Haga clic con el botón izquierdo en el icono de la bandeja Windows para mostrar u ocultar el mezclador y haga clic con el botón central para alternar el silencio en la salida predeterminada. Desplácese sobre una fila para ajustarla en dos puntos, haga clic en el ícono de una fila de audio para silenciarla o haga clic derecho en una fila para ocultarla. Restaura filas ocultas desde el menú de la bandeja. Prensa <kbd>ESC</kbd> o haga clic afuera para cerrar el menú lateral Windows. Los clics en la bandeja Linux dependen de su escritorio.

### Apariencia, inicio y preferencias

Windows utiliza un menú desplegable redondeado. Linux sigue los colores claros, oscuros y de acento del GNOME u ofrece Charcoal, Nord, Catppuccin y Light. Puede habilitar el inicio al iniciar sesión desde el menú de la bandeja. Las preferencias se almacenan localmente.

</details>

<br>

<img src="assets/readme/keys-en.svg" width="100%" alt="Controles del Windows: haga clic en el icono de la bandeja para abrir o cerrar, haga clic con el botón central para silenciarlo, desplácese para ajustar, haga clic en un nombre de salida para seleccionarlo, haga clic derecho en una fila para ocultarla y presione Escape para cerrar">

<br>

<img src="assets/readme/states-en.svg" width="100%" alt="Acciones del mezclador: volumen de salida, volumen de aplicaciones, brillo del Windows, filtrado de luz azul, atenuación y silencio del software">

<br>

<img src="assets/readme/numbers-en.svg" width="100%" alt="219 KB ejecutable Windows independiente en v3.0.2, cero cuentas o telemetría, dos plataformas de escritorio, tres formatos de paquete Linux y cinco opciones de apariencia de Linux">

<br>

<img src="assets/readme/roadmap-en.svg" width="100%" alt="Hoja de ruta: cuatro lanzamientos publicados, próximas pruebas de GNOME, Fedora y Debian, y controles de visualización integrados de Linux como una brecha conocida">

<details>
<summary><b>Instalar</b></summary>

### Windows 11 · x64 · v3.0.2

1. Descargar [**TrayMixer-Setup.exe**](https://github.com/sailxx/TrayMixer/releases/download/v3.0.2/TrayMixer-Setup.exe) para un instalador fuera de línea o [**TrayMixer.exe**](https://github.com/sailxx/TrayMixer/releases/download/v3.0.2/TrayMixer.exe) para ejecutar la aplicación directamente.
2. En Configuración, elija inglés o ruso, la carpeta de instalación y si desea iniciar TrayMixer al iniciar sesión. La interfaz de la aplicación está en inglés. El programa de instalación crea accesos directos al escritorio y al menú Inicio.
3. Haga clic en el icono TrayMixer en el área de notificación para abrir el mezclador.

Ejecute la instalación nuevamente para actualizar o reinstalar manteniendo las preferencias y la opción de inicio automático. Eliminar la aplicación a través de Windows **Aplicaciones instaladas**.

Las versiones no están firmadas, por lo que Windows puede mostrar una advertencia de SmartScreen. Compare el archivo SHA -256 con el [Notas de la versión Windows](https://github.com/sailxx/TrayMixer/releases/tag/v3.0.2).

Para una instalación desatendida sin iniciar la aplicación, ejecute `TrayMixer-Setup.exe /S /NOLAUNCH` desde la carpeta del instalador. Los interruptores opcionales incluyen `/DIR="C:\path\TrayMixer"`, `/NOAUTOSTART` y `/LAUNCH`. Para una eliminación desatendida, ejecute `Uninstall.exe /uninstall /S` desde la carpeta de instalación, que por defecto es `%LOCALAPPDATA%\TrayMixer`.

### Vista previa de Linux · x86_64 · v3.1.1

| Paquete | Instalar o iniciar desde la carpeta de descargas |
| --- | --- |
| [DEB](https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.1/TrayMixer_3.1.1_amd64.deb) | `sudo apt install ./TrayMixer_3.1.1_amd64.deb` |
| [RPM](https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.1/TrayMixer-3.1.1-1.x86_64.rpm) | `sudo dnf install ./TrayMixer-3.1.1-1.x86_64.rpm` |
| [AppImage](https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.1/TrayMixer-3.1.1-x86_64.AppImage) | `chmod +x TrayMixer-3.1.1-x86_64.AppImage`, entonces `./TrayMixer-3.1.1-x86_64.AppImage` |

La vista previa usa X11 o XWayland y necesita glibc 2.35 +, PulseAudio 15+ o PipeWire-Pulse, y `pactl`. GNOME requiere compatibilidad con AppIndicator para el icono de la bandeja. El AppImage necesita bibliotecas de escritorio del sistema y acepta `--appimage-extract-and-run` si FUSE no está disponible.

Ver el [Guía Linux](linux/README.md) para los requisitos del escritorio y el comportamiento de la bandeja. Comparar hashes de paquetes con [SHA256SUMS](https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.1/SHA256SUMS).

</details>

<a id="platforms"></a>

<details>
<summary><b>Plataformas y límites conocidos</b></summary>

| | Windows 11 | Vista previa de Linux |
| --- | --- | --- |
| Audio | Volumen de dispositivos y aplicaciones agrupadas | Dispositivos y transmisiones de aplicaciones individuales a través de PulseAudio o PipeWire-Pulse |
| Bandeja | Área de notificación Windows | Depende del host de la bandeja del escritorio |
| Mostrar | Brillo, filtrado de luz azul y atenuación. | Abre la configuración de pantalla y luz nocturna del escritorio. |
| Preferencias | `HKCU\Software\TrayMixer` | `$XDG_CONFIG_HOME/traymixer/settings.json` |

La compatibilidad con monitores y controladores afecta el brillo del Windows. El empaquetado y el inicio de Linux se verifican en Ubuntu 24.04. Las instalaciones de Fedora y Debian, el comportamiento de la bandeja GNOME y la compatibilidad del hardware aún necesitan pruebas de escritorio.

</details>

<details>
<summary><b>Desarrollo</b></summary>

TrayMixer es mantenido por [Vlad (sailxx)](https://github.com/sailxx) bajo el [Licencia Apache 2.0](LICENSE).

Compile Windows a partir de un clon con el archivo . Compilador NET Framework C#.

```cmd
build.cmd
```

Construya Linux con . NET SDK 10.0. Ver el [Guía de construcción y empaquetado del Linux](linux/README.md#build-and-verify).

```sh
dotnet build linux/TrayMixer.Linux.csproj -c Release
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
```

Para reconstruir la ilustración en inglés README, utilice las fuentes Golos Text y JetBrains Mono en `tools/readme/.font` como se describe en [build.mjs](tools/readme/build.mjs).

```sh
cd tools/readme
npm ci
node build.mjs --showcase
```

Para verificar una descarga, utilice `Get-FileHash .\TrayMixer-Setup.exe -Algorithm SHA256` en PowerShell y comparar con el [Notas de la versión Windows](https://github.com/sailxx/TrayMixer/releases/tag/v3.0.2), o usar `sha256sum <filename>` en Linux y comparar con [SHA256SUMS](https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.1/SHA256SUMS).

TrayMixer almacena las preferencias localmente. Linux envía comandos de audio a su servidor PulseAudio o PipeWire-Pulse configurado. Ver el [política de privacidad](PRIVACY.md) y [política de seguridad](SECURITY.md). Para obtener un error reproducible, incluya la versión OS, el escritorio y el hardware afectado en un [Problema con GitHub](https://github.com/sailxx/TrayMixer/issues).

</details>

---

<div align="center">


Las traducciones pueden describir versiones anteriores.

</div>
