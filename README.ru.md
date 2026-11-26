[English](README.md) · [العربية](README.ar.md) · [简体中文](README.zh-CN.md) · [繁體中文](README.zh-TW.md) · [Čeština](README.cs.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [Ελληνικά](README.el.md) · [Bahasa Indonesia](README.id.md) · [Gaeilge](README.ga.md) · [Italiano](README.it.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Polski](README.pl.md) · [Română](README.ro.md) · **Русский** · [Español](README.es.md) · [Українська](README.uk.md) · [اردو](README.ur.md) · [ئۇيغۇرچە](README.ug.md) · [Tiếng Việt](README.vi.md) · [Português](README.pt.md) · [Türkçe](README.tr.md)

<div align="center">

<img src="assets/readme/logo-blue.svg" width="128" alt="TrayMixer">

# TrayMixer

**Управляйте громкостью каждого вывода и приложения с одной небольшой панели на панели задач Windows 11 или Linux.**

Выключите игру, сохраняя при этом звук вашего вызова, переключайтесь между динамиками и наушниками или отрегулируйте яркость, фильтрацию синего света и затемнение на Windows.

**Никакой телеметрии и аккаунта.** Открытый исходный код под Apache 2.0.

<img src="https://img.shields.io/badge/Windows-v3.0.2-2E9BFF" alt="Выпуск Windows v3.0.2">
<img src="https://img.shields.io/badge/Windows-11%20x64-2E9BFF" alt="Windows 11 x64">
<img src="https://img.shields.io/badge/Linux-v3.1.1%20preview-2E9BFF" alt="Linux предварительный просмотр v3.1.1">
<img src="https://img.shields.io/badge/Linux-AppImage%20%7C%20deb%20%7C%20rpm-2E9BFF" alt="Linux AppImage , DEB и RPM">
<img src="https://img.shields.io/badge/code-C%23-2E9BFF" alt="До-диез">
<img src="https://img.shields.io/badge/Linux%20UI-Avalonia-2E9BFF" alt="Avalonia для Linux">
<img src="https://img.shields.io/badge/license-Apache%202.0-2E9BFF" alt="Apache 2.0">
<img src="https://img.shields.io/github/stars/sailxx/TrayMixer?style=flat&amp;logo=github&amp;color=%232E9BFF" alt="GitHub звезды">
<br>

<img src="assets/readme/hero-en.svg" width="100%" alt="Баннер TrayMixer с всплывающим меню Windows, громкостью вывода и приложения, элементами управления отображением и локальными настройками.">

<a href="https://github.com/sailxx/TrayMixer/releases/download/v3.0.2/TrayMixer-Setup.exe"><img src="assets/readme/btn-windows-en.svg" height="52" alt="Загрузите установщик Windows"></a>&nbsp; &nbsp;<a href="https://github.com/sailxx/TrayMixer/releases/tag/linux-v3.1.1"><img src="assets/readme/btn-linux-en.svg" height="52" alt="Скачать пакеты Linux"></a>&nbsp; &nbsp;<a href="https://github.com/sailxx/TrayMixer/releases/download/v3.0.2/TrayMixer.exe"><img src="assets/readme/btn-portable-en.svg" height="52" alt="Загрузите портативный исполняемый файл Windows."></a>&nbsp; &nbsp;<a href="https://github.com/sailxx/TrayMixer/releases"><img src="assets/readme/btn-new-en.svg" height="52" alt="Прочтите примечания к выпуску"></a>

</div>

<br>

<img src="assets/readme/features-en.svg" width="100%" alt="Особенности: отдельный вывод и громкость приложений, элементы управления дисплеем Windows, элементы управления лотком, темы и локальные настройки.">

<details>
<summary><b>Подробнее о функциях</b></summary>

### Результаты и объем приложения

Каждый выход имеет собственный ползунок и регулятор отключения звука. Щелкните имя выхода, чтобы выбрать его по умолчанию. На Linux это относится к новому воспроизведению, и каждый поток приложения отображается отдельно.

### Элементы управления дисплеем Windows

Отрегулируйте яркость на поддерживаемых мониторах, обогрейте дисплей с помощью фильтра синего света и используйте программное затемнение с того же микшера. [См. скриншот Windows в полном размере.](assets/readme/screenshot.png).

### Элементы управления лотком

Щелкните левой кнопкой мыши значок Windows на панели задач, чтобы отобразить или скрыть микшер, и щелкните его средней кнопкой мыши, чтобы отключить звук на выходе по умолчанию. Прокрутите строку, чтобы отрегулировать ее на две точки, щелкните значок строки аудио, чтобы отключить звук, или щелкните строку правой кнопкой мыши, чтобы скрыть ее. Восстановить скрытые строки из меню в трее. Нажимать <kbd>Esc</kbd> или щелкните снаружи, чтобы закрыть всплывающее меню Windows. Щелчки в лотке Linux зависят от вашего рабочего стола.

### Внешний вид, запуск и предпочтения

Windows использует закругленное всплывающее меню. Linux следует за GNOME светлыми, темными и акцентными цветами или предлагает варианты Charcoal, Nord, Catppuccin и Light. Вы можете включить запуск при входе в систему из меню в трее. Настройки хранятся локально.

</details>

<br>

<img src="assets/readme/keys-en.svg" width="100%" alt="Элементы управления Windows: щелкните значок на панели задач, чтобы открыть или закрыть, щелкните средней кнопкой мыши, чтобы отключить звук, прокрутите для настройки, щелкните имя вывода, чтобы выбрать его, щелкните правой кнопкой мыши строку, чтобы скрыть ее, и нажмите Escape, чтобы закрыть">

<br>

<img src="assets/readme/states-en.svg" width="100%" alt="Действия микшера: выходная громкость, громкость приложения, яркость Windows, фильтрация синего света, программное затемнение и отключение звука.">

<br>

<img src="assets/readme/numbers-en.svg" width="100%" alt="219 KB Автономный Windows, исполняемый файл в v3.0.2, отсутствие учетных записей и телеметрии, две настольные платформы, три формата пакета Linux и пять вариантов внешнего вида Linux.">

<br>

<img src="assets/readme/roadmap-en.svg" width="100%" alt="Дорожная карта: четыре опубликованных выпуска, предстоящие тесты GNOME, Fedora и Debian, а также интегрированные элементы управления отображением Linux как известный пробел.">

<details>
<summary><b>Установить</b></summary>

### Windows 11 · x64 · v3.0.2

1. Скачать [**TrayMixer-Setup.exe**](https://github.com/sailxx/TrayMixer/releases/download/v3.0.2/TrayMixer-Setup.exe) для автономного установщика или [**TrayMixer.exe**](https://github.com/sailxx/TrayMixer/releases/download/v3.0.2/TrayMixer.exe) чтобы запустить приложение напрямую.
2. В Настройках выберите английский или русский, папку установки и запускать ли TrayMixer при входе в систему. Интерфейс приложения на английском языке. Программа установки создает ярлыки на рабочем столе и в меню «Пуск».
3. Нажмите значок TrayMixer в области уведомлений, чтобы открыть микшер.

Запустите программу установки еще раз, чтобы обновить или переустановить, сохранив настройки и выбор автозапуска. Удалить приложение через Windows **Установленные приложения**.

Релизы не подписаны, поэтому Windows может отображать предупреждение SmartScreen. Сравните SHA -256 файла с [Примечания к выпуску Windows](https://github.com/sailxx/TrayMixer/releases/tag/v3.0.2).

Для автоматической установки без запуска приложения запустите `TrayMixer-Setup.exe /S /NOLAUNCH` из папки установщика. Дополнительные переключатели включают в себя `/DIR="C:\path\TrayMixer"`, `/NOAUTOSTART` и `/LAUNCH`. Для автоматического удаления запустите `Uninstall.exe /uninstall /S` из папки установки, которая по умолчанию `%LOCALAPPDATA%\TrayMixer`.

### Linux · x86_64 · v3.1.1 предварительный просмотр

| Упаковка | Установить или запустить из папки загрузки |
| --- | --- |
| [DEB](https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.1/TrayMixer_3.1.1_amd64.deb) | `sudo apt install ./TrayMixer_3.1.1_amd64.deb` |
| [RPM](https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.1/TrayMixer-3.1.1-1.x86_64.rpm) | `sudo dnf install ./TrayMixer-3.1.1-1.x86_64.rpm` |
| [AppImage](https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.1/TrayMixer-3.1.1-x86_64.AppImage) | `chmod +x TrayMixer-3.1.1-x86_64.AppImage`, затем `./TrayMixer-3.1.1-x86_64.AppImage` |

Предварительная версия использует X11 или XWayland и требует glibc 2.35+, PulseAudio 15+ или PipeWire-Pulse, и `pactl`. GNOME требует поддержки AppIndicator для значка на панели задач. AppImage нуждается в системных библиотеках рабочего стола и принимает `--appimage-extract-and-run` если FUSE недоступен.

См. [Руководство Linux](linux/README.md) для требований к рабочему столу и поведения лотка. Сравните хэши пакетов с [SHA256SUMS](https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.1/SHA256SUMS).

</details>

<a id="platforms"></a>

<details>
<summary><b>Платформы и известные ограничения</b></summary>

| | Windows 11 | Linux предварительный просмотр |
| --- | --- | --- |
| Аудио | Том устройства и сгруппированных приложений | Устройства и отдельные потоки приложений через PulseAudio или PipeWire-Pulse |
| Поднос | Область уведомлений Windows | Зависит от хоста лотка рабочего стола |
| Отображать | Яркость, фильтрация синего света и затемнение | Открывает экран рабочего стола и настройки ночного освещения. |
| Предпочтения | `HKCU\Software\TrayMixer` | `$XDG_CONFIG_HOME/traymixer/settings.json` |

Поддержка монитора и драйверов влияет на яркость Windows. Упаковка и запуск Linux проверены на Ubuntu 24.04. Установки Fedora и Debian, поведение лотка GNOME и совместимость оборудования по-прежнему требуют тестирования на рабочем столе.

</details>

<details>
<summary><b>Разработка</b></summary>

TrayMixer поддерживается [Влад ( sailxx )](https://github.com/sailxx) под [Лицензия Apache 2.0](LICENSE).

Соберите Windows из клона со встроенным . NET Framework C# компилятор.

```cmd
build.cmd
```

Соберите Linux с помощью . NET SDK 10.0 . См. [Руководство по сборке и упаковке Linux](linux/README.md#build-and-verify).

```sh
dotnet build linux/TrayMixer.Linux.csproj -c Release
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
```

Чтобы восстановить английскую иллюстрацию README, используйте шрифты Golos Text и JetBrains Mono в `tools/readme/.font` как описано в [build.mjs](tools/readme/build.mjs).

```sh
cd tools/readme
npm ci
node build.mjs --showcase
```

Чтобы проверить загрузку, используйте `Get-FileHash .\TrayMixer-Setup.exe -Algorithm SHA256` в PowerShell и сравните с [Примечания к выпуску Windows](https://github.com/sailxx/TrayMixer/releases/tag/v3.0.2), или используйте `sha256sum <filename>` на Linux и сравните с [SHA256SUMS](https://github.com/sailxx/TrayMixer/releases/download/linux-v3.1.1/SHA256SUMS).

TrayMixer сохраняет настройки локально. Linux отправляет аудиокоманды на настроенный вами сервер PulseAudio или PipeWire-Pulse. См. [политика конфиденциальности](PRIVACY.md) и [политика безопасности](SECURITY.md). Для воспроизводимой ошибки включите версию OS, рабочий стол и затронутое оборудование в файл. [Проблема GitHub](https://github.com/sailxx/TrayMixer/issues).

</details>

---

<div align="center">


В переводах могут описываться более ранние выпуски.

</div>
