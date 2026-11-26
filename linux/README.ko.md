[English](README.md) · [العربية](README.ar.md) · [简体中文](README.zh-CN.md) · [繁體中文](README.zh-TW.md) · [Čeština](README.cs.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [Ελληνικά](README.el.md) · [Bahasa Indonesia](README.id.md) · [Gaeilge](README.ga.md) · [Italiano](README.it.md) · [日本語](README.ja.md) · **한국어** · [Polski](README.pl.md) · [Română](README.ro.md) · [Русский](README.ru.md) · [Español](README.es.md) · [Українська](README.uk.md) · [اردو](README.ur.md) · [ئۇيغۇرچە](README.ug.md) · [Tiếng Việt](README.vi.md)

# Linux용 TrayMixer — 3.1.1 미리보기

별도의 C# / Avalonia 데스크탑 포트. 기존 Windows 소스와 설치 프로그램은 변경되지 않았습니다.

## 지원되는 기준선

x86_64 Linux , glibc 2.35 +, X11 또는 XWayland , PulseAudio 15+ 또는 PipeWire `pipewire-pulse`, 그리고 `pactl`. 빌드 및 패키지 검사는 Ubuntu 24.04에서 실행됩니다. 배포판별 설치 및 GNOME 데스크탑 승인은 여전히 ​​필요합니다. 이는 전체 Windows 패리티에 대한 주장이 아닌 미리보기입니다.

## 설치하다

Linux 미리보기 릴리스에서 DEB, RPM 또는 AppImage를 다운로드하세요.

- 우분투/데비안: `sudo apt install ./TrayMixer_3.1.1_amd64.deb`
- 페도라: `sudo dnf install ./TrayMixer-3.1.1-1.x86_64.rpm`
- AppImage: `chmod +x TrayMixer-3.1.1-x86_64.AppImage`, 그런 다음 실행하십시오. 시스템 필요 `pactl`, X11 라이브러리, 글꼴 구성, ICU 및 OpenSSL. 설치하다 `pulseaudio-utils` 우분투/페도라에서. FUSE 2 또는 `--appimage-extract-and-run` 필요할 수 있습니다. AppImage 번들. 시스템 오디오 서버나 데스크탑 라이브러리가 아닌 NET.

시작하다 **TrayMixer** 응용프로그램 메뉴에서 처음 실행하면 믹서가 열립니다. 닫으면 트레이 아이콘이 계속 실행됩니다. 트레이 메뉴를 종료합니다. GNOME 쉘에는 AppIndicator/StatusNotifier 확장이 필요합니다(Ubuntu에는 일반적으로 확장이 포함되어 있습니다). 트레이 호스트가 없으면 트레이 전용 자동 시작을 활성화하지 마십시오. 응용프로그램 메뉴를 사용하여 실행하세요. 다시 시작하면 기존 프로세스에 현재 사용자 전용 로컬 파이프를 통해 창을 다시 열도록 요청합니다.

## 통제 수단

- 트레이 활성화는 믹서를 열거나 닫습니다. 트레이 메뉴는 믹서 열기, 기본 출력 음소거, 숨겨진 행 복원, 자동 시작 및 종료를 제공합니다. 데스크탑 트레이 호스트는 클릭 동작을 제어합니다. Windows 중간 클릭 패리티는 보장되지 않습니다.
- 출력 및 애플리케이션 스트림 슬라이더: 0~100%, 음소거, 마우스 휠 ±2. 양수 볼륨을 설정하면 명시적으로 음소거가 해제됩니다.
- 새 재생의 기본값으로 만들려면 출력 이름을 클릭하세요. 기존 스트림은 강제로 이동되지 않습니다.
- 숨기려면 행을 마우스 오른쪽 버튼으로 클릭하세요. 숨겨진 행을 복원하면 모든 행이 다시 복원됩니다. 기본이 아닌 장치의 스트림을 포함하여 별도의 스트림이 별도로 표시됩니다.
- 로그인 시 시작은 XDG 자동 시작 항목을 사용합니다. 선택될 때까지 비활성화됩니다. AppImage는 영구 위치에 남아 있어야 합니다.
- 모양: GNOME / 시스템은 밝은/어두운 색상과 GNOME 강조 색상을 따르며 믹서가 열려 있는 동안 새로 고쳐집니다. 악센트를 지원하지 않는 이전 GNOME는 파란색을 사용합니다. Charcoal, Nord, Catppuccin 및 Light가 내장되어 있습니다. 운모 또는 전체 창 투명도가 없습니다.
- 설정: `$XDG_CONFIG_HOME/traymixer/settings.json`, 기본 `~/.config/traymixer/settings.json`.

## Windows와 알려진 격차

통합된 모니터별 밝기, 청색광 강도 및 소프트웨어 조광 슬라이더는 아직 포팅되지 않았습니다. **디스플레이 및 야간 조명 설정** 대신 GNOME / KDE 설정을 엽니다. Wayland에는 범용 감마 조정이 없습니다. 합성기별 구현이 필요합니다. 임의의 권한 있는 쓰기 또는 권한 변경은 수행되지 않습니다. 정확한 팝업 배치, 초점 손실 해제 및 Windows 배경 효과는 복제되지 않습니다.

## 빌드 및 확인

가 필요합니다. NET SDK 10.0 . Linux의 경우 클론에서:

```sh
cd TrayMixer
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
dotnet run --project linux/TrayMixer.Linux.csproj
# Packaging requires dpkg-deb, rpm/rpmbuild and curl:
bash linux/package.sh 3.1.1
```

현재 Windows 개발 머신에서:

```cmd
cd /d C:\Users\Vlad\sx\traymixer
dotnet build linux/TrayMixer.Linux.csproj -c Release
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
dotnet publish linux/TrayMixer.Linux.csproj -c Release -r linux-x64 --self-contained true -o linux/artifacts/publish
```

Windows는 Linux 바이너리를 교차 게시할 수 있지만 Linux 환경 없이는 이를 실행하거나 기본 패키지를 만들 수 없습니다. `.github/workflows/linux.yml` 테스트, null 출력에 대한 실제 PulseAudio 다시 읽기, 패키지 메타데이터 유효성 검사 및 Xvfb 실행 스모크 검사를 실행합니다. 이는 실제 GNOME 트레이, 테마, 하드웨어 테스트 또는 오디오 청취를 대체하지 않습니다. Linux 릴리스 태그 사용 `linux-v*` 따라서 기존 Windows 릴리스 워크플로가 트리거되지 않습니다. 기존 Windows 릴리스는 유지됩니다.
