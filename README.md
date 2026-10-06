<div align="center">

**Русский** · [English](README.en.md)

<br>

<img src="assets/readme/hero-ru.svg" width="100%" alt="TrayMixer — громкость каждого устройства и приложения в одном окне Windows 11">

<a href="https://github.com/sailxx/TrayMixer/releases/latest/download/TrayMixer.exe"><img src="assets/readme/cta-ru.svg" height="44" alt="Скачать для Windows 11"></a>

</div>

<br>

<img src="assets/readme/screenshot.png" width="100%" alt="Окно TrayMixer: наушники, колонки, браузер и системные звуки на одном экране">

Стандартный микшер Windows 11 спрятан в «Параметрах», а значок громкости в трее меняет только одно устройство. **TrayMixer** — один клик по значку, и перед вами все наушники, колонки, мониторы и каждое приложение со своим ползунком. Без установки, без рекламы, без фоновой нагрузки.

<br>

<img src="assets/readme/features-ru.svg" width="100%" alt="Возможности: все устройства, каждое приложение, родной Windows 11, скрытие лишнего, лёгкость, безопасность">

<details>
<summary><b>Управление</b></summary>

| Действие | Результат |
|---|---|
| **Левый клик** по значку в трее | Открыть или закрыть окно |
| **Средний клик** по значку | Выключить или включить звук основного устройства |
| **Правый клик** по значку | Параметры звука, микшер Windows, скрытые строки, фон Mica/Acrylic, автозапуск, выход |
| Клик по **иконке строки** | Выключить или включить звук |
| Клик по **названию устройства** | Сделать его основным |
| **Правый клик** по строке | Скрыть её |
| **Колёсико мыши** над строкой | Громкость ±2 |
| **Esc** или клик мимо окна | Закрыть |

</details>

<br>

<img src="assets/readme/numbers-ru.svg" width="100%" alt="~2 МБ памяти, 0% CPU, 160 КБ, 0 зависимостей">

<br>

<img src="assets/readme/security-ru.svg" width="100%" alt="Безопасность: без прав администратора, без сети, защита от подмены DLL, проверяемые сборки">

<details>
<summary><b>Как проверить, что exe собран из этого кода</b></summary>

Каждый релиз собирает GitHub Actions прямо из исходников, а GitHub подписывает происхождение файла (build provenance). Проверить можно так:

```bash
gh attestation verify TrayMixer.exe --repo sailxx/TrayMixer
```

Контрольная сумма SHA-256 лежит рядом с exe в каждом релизе (`TrayMixer.exe.sha256`):

```powershell
Get-FileHash .\TrayMixer.exe -Algorithm SHA256
```

</details>

## Установка

1. Скачайте [`TrayMixer.exe`](https://github.com/sailxx/TrayMixer/releases/latest/download/TrayMixer.exe) и положите в постоянную папку, например `%LOCALAPPDATA%\TrayMixer`.
2. Запустите. Значок появится в трее, автозапуск включится сам (выключается в меню).

Нужна Windows 10 или 11 — .NET Framework 4.8 уже встроен в систему. Mica и скруглённые углы — в Windows 11.

> Windows SmartScreen может предупредить о неизвестном издателе: файл не подписан платным сертификатом. Нажмите «Подробнее → Выполнить в любом случае» или соберите программу сами.

## Сборка из исходников

```bash
git clone https://github.com/sailxx/TrayMixer.git
```

```bash
TrayMixer\build.cmd
```

Компилятор C# уже есть в Windows — ничего устанавливать не нужно. Картинки для README пересобирает `tools\readme-art.cmd`.

| Файл | Что внутри |
|---|---|
| `TrayMixer.cs` | Окно, отрисовка с Mica, трей, настройки |
| `Audio.cs` | Windows Core Audio: устройства, приложения, уведомления |
| `app.manifest` | Запуск без прав администратора |
| `tools/` | Генератор картинок README |

## Лицензия

[MIT](LICENSE) © sailxx
