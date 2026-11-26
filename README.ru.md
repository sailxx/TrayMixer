<div align="center">

**Русский** · [English](README.md) · [Español](README.es.md) · [Português](README.pt.md) · [Deutsch](README.de.md) · [Français](README.fr.md) · [Italiano](README.it.md) · [Türkçe](README.tr.md) · [Українська](README.uk.md) · [Polski](README.pl.md)

<br>

<img src="assets/readme/logo-white.svg" width="112" height="112" alt="Логотип TrayMixer — три синих ползунка на белом фоне">

# TrayMixer

**Звук в одном клике.** Микшер в системном трее для Windows 11 и Linux.

[![Скачать для Windows 11](assets/readme/cta-ru.svg)](https://github.com/sailxx/TrayMixer/releases/download/v3.0.2/TrayMixer-Setup.exe) · [Сборки для Linux](https://github.com/sailxx/TrayMixer/releases/tag/linux-v3.1.1) · [Все релизы](https://github.com/sailxx/TrayMixer/releases)

</div>

<br>

<img src="assets/readme/screenshot-ru.png" width="100%" alt="Окно TrayMixer: наушники, колонки, браузер и системные звуки на одном экране">

Стандартный микшер Windows 11 спрятан в «Параметрах», а значок громкости в трее меняет только одно устройство. **TrayMixer** — один клик по значку, и перед вами все наушники, колонки, мониторы и каждое приложение со своим ползунком. В Windows здесь же можно настроить яркость, тёплый оттенок и затемнение. В Linux пока доступна предварительная версия с управлением звуком.

<br>

<img src="assets/readme/features-ru.svg" width="100%" alt="Возможности: все устройства, каждое приложение, родной Windows 11, скрытие лишнего, лёгкость, безопасность">

<details>
<summary><b>Управление</b></summary>

| Действие | Результат |
|---|---|
| **Левый клик** по значку в трее | Открыть или закрыть окно |
| **Средний клик** по значку | Выключить или включить звук основного устройства |
| **Правый клик** по значку | Параметры звука, микшер Windows, скрытые строки, фон Mica/Acrylic, автозапуск, выход |
| Клик по **иконке строки** | Выключить или включить звук, фильтр синего света или затемнение |
| Клик по **названию устройства** | Сделать его основным |
| **Правый клик** по строке | Скрыть её |
| **Колёсико мыши** над строкой | Громкость, яркость, сила фильтра или затемнение ±2 |
| **Esc** или клик мимо окна | Закрыть |

</details>

<br>

<img src="assets/readme/numbers-ru.svg" width="100%" alt="~2 МБ памяти, 0% CPU, 165 КБ, 0 зависимостей">

<br>

<img src="assets/readme/security-ru.svg" width="100%" alt="Безопасность: без прав администратора, без сети, защита от подмены DLL, проверяемые сборки">

<details>
<summary><b>Как проверить, что exe собран из этого кода</b></summary>

Каждый релиз собирает GitHub Actions прямо из исходников, а GitHub подписывает происхождение файла (build provenance). Проверить можно так:

```bash
gh attestation verify TrayMixer-Setup.exe --repo sailxx/TrayMixer
```

Контрольная сумма SHA-256 установщика указана в описании каждого релиза:

```powershell
Get-FileHash .\TrayMixer-Setup.exe -Algorithm SHA256
```

</details>

## Установка

1. Скачайте [`TrayMixer-Setup.exe`](https://github.com/sailxx/TrayMixer/releases/download/v3.0.2/TrayMixer-Setup.exe) и запустите.
2. Выберите папку (по умолчанию `%LOCALAPPDATA%\TrayMixer`), автозапуск и запуск после установки. Ярлыки на рабочем столе и в меню «Пуск» создаются автоматически. Интернет для установки не нужен.
3. Для обновления запустите Setup нового релиза; для переустановки — той же версии. Настройки сохраняются, установка более старой версии блокируется. Удаление доступно в списке установленных приложений Windows; настройки сохраняются.

Контрольная сумма Windows-установщика указана в релизе. Для Linux есть пакеты DEB, RPM и AppImage. [Скачать предварительную версию для Linux](https://github.com/sailxx/TrayMixer/releases/tag/linux-v3.1.1).

## Подпись и проверка

Установщик и встроенное приложение не имеют цифровой подписи. Windows SmartScreen может показать предупреждение; перед запуском проверьте источник релиза и контрольную сумму.

GitHub Actions собирает релизы из исходников и публикует подтверждение происхождения сборки (`gh attestation verify TrayMixer-Setup.exe --repo sailxx/TrayMixer`). SHA-256 указан в описании релиза.


## О проекте

<img src="assets/readme/about-ru.svg" width="100%" alt="О TrayMixer. Начало проекта — 6 октября 2026 года">

[Влад (sailxx)](https://github.com/sailxx) начал TrayMixer **6 октября 2026 года**, чтобы до настроек звука можно было добраться одним кликом. Это бесплатный проект с открытым исходным кодом.

В TrayMixer нет аккаунтов, рекламы и трекеров. Приложение читает названия устройств и программ для отображения в микшере, хранит настройки на компьютере и не отправляет статистику использования. [Политика конфиденциальности](PRIVACY.md)

## Дорожная карта

<img src="assets/readme/timeline-ru.svg" width="100%" alt="Дорожная карта TrayMixer от 1.0 до 3.1.1">

- **1.0 · 6 октября 2026 года** — первый лёгкий микшер громкости в трее Windows 11.
- **2.x** — к звуку добавились яркость, фильтр синего света и затемнение. Появился установщик Windows.
- [**Windows 3.0**](https://github.com/sailxx/TrayMixer/releases/tag/v3.0.0) — окно получило более спокойный стеклянный фон и стало плавнее менять размер.
- [**Linux 3.1**](https://github.com/sailxx/TrayMixer/releases/tag/linux-v3.1.0) — первая предварительная сборка с управлением звуком из трея, поддержкой тем GNOME и пакетами DEB, RPM и AppImage.
- [**Windows 3.0.2**](https://github.com/sailxx/TrayMixer/releases/tag/v3.0.2) и [**Linux 3.1.1**](https://github.com/sailxx/TrayMixer/releases/tag/linux-v3.1.1) — текущая стабильная версия для Windows и предварительная версия для Linux.

Дальше — проверка Linux-версии на разных рабочих столах и оборудовании. Настройки экрана в Linux зависят от поддержки со стороны рабочего стола.
