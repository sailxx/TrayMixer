[English](README.md) · [العربية](README.ar.md) · [简体中文](README.zh-CN.md) · [繁體中文](README.zh-TW.md) · [Čeština](README.cs.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [Ελληνικά](README.el.md) · [Bahasa Indonesia](README.id.md) · [Gaeilge](README.ga.md) · [Italiano](README.it.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Polski](README.pl.md) · [Română](README.ro.md) · [Русский](README.ru.md) · [Español](README.es.md) · [Українська](README.uk.md) · [اردو](README.ur.md) · [ئۇيغۇرچە](README.ug.md) · **Tiếng Việt**

# TrayMixer cho Linux — xem trước 3.1.1

Một cổng máy tính để bàn C# / Avalonia riêng biệt. Nguồn và trình cài đặt Windows hiện tại không thay đổi.

## Đường cơ sở được hỗ trợ

x86_64 Linux , glibc 2.35+, X11 hoặc XWayland , PulseAudio 15+ hoặc PipeWire có `pipewire-pulse`, Và `pactl`. Kiểm tra bản dựng và gói chạy trên Ubuntu 24.04. Vẫn cần phải cài đặt dành riêng cho phân phối và chấp nhận máy tính để bàn GNOME; đây là bản xem trước, không phải là tuyên bố về tính chẵn lẻ đầy đủ của Windows.

## Cài đặt

Tải xuống DEB , RPM hoặc AppImage từ bản phát hành xem trước Linux.

- Ubuntu/Debian: `sudo apt install ./TrayMixer_3.1.1_amd64.deb`
- Fedora: `sudo dnf install ./TrayMixer-3.1.1-1.x86_64.rpm`
- AppImage: `chmod +x TrayMixer-3.1.1-x86_64.AppImage`, sau đó chạy nó. Yêu cầu hệ thống `pactl`, Thư viện X11, fontconfig, ICU và OpenSSL. Cài đặt `pulseaudio-utils` trên Ubuntu/Fedora. FUSE 2 hoặc `--appimage-extract-and-run` có thể được yêu cầu. Gói AppImage. NET, không phải máy chủ âm thanh hệ thống hoặc thư viện máy tính để bàn.

Phóng **TrayMixer** từ menu ứng dụng. Lần khởi động đầu tiên sẽ mở máy trộn; đóng nó giữ cho biểu tượng khay chạy. Thoát khỏi menu khay. GNOME Shell cần tiện ích mở rộng AppIndicator/StatusNotifier (Ubuntu thường bao gồm một tiện ích mở rộng). Nếu không có máy chủ khay, không bật tính năng tự khởi động chỉ dành cho khay; sử dụng menu ứng dụng để khởi chạy. Việc khởi chạy lại sẽ yêu cầu quy trình hiện có mở lại cửa sổ của nó thông qua một đường ống cục bộ chỉ dành cho người dùng hiện tại.

## Điều khiển

- Kích hoạt khay mở/đóng máy trộn. Menu khay cung cấp Bộ trộn mở, tắt tiếng đầu ra mặc định, khôi phục các hàng bị ẩn, tự động khởi động và Thoát. Máy chủ khay máy tính để bàn kiểm soát hành vi nhấp chuột; Tính chẵn lẻ của nhấp chuột giữa Windows không được đảm bảo.
- Thanh trượt đầu ra và luồng ứng dụng: 0–100%, tắt tiếng, con lăn chuột ±2. Đặt âm lượng dương sẽ tắt tiếng một cách rõ ràng.
- Bấm vào tên đầu ra để đặt làm tên mặc định cho lần phát lại mới. Các luồng hiện tại không bị buộc phải di chuyển.
- Nhấp chuột phải vào một hàng để ẩn; Khôi phục các hàng bị ẩn sẽ đưa tất cả các hàng trở lại. Các luồng riêng biệt được hiển thị riêng, bao gồm các luồng trên các thiết bị không mặc định.
- Bắt đầu khi đăng nhập sử dụng mục nhập tự khởi động XDG; bị vô hiệu hóa cho đến khi được chọn. AppImage phải ở vị trí cố định.
- Hình thức: GNOME / Hệ thống tuân theo các màu nhấn sáng/tối và GNOME, được làm mới khi máy trộn đang mở. GNOME cũ hơn không có hỗ trợ giọng sử dụng màu xanh lam. Than, Nord, Catppuccin và Light được tích hợp sẵn. Không có Mica hoặc độ trong suốt toàn bộ cửa sổ.
- Cài đặt: `$XDG_CONFIG_HOME/traymixer/settings.json`, mặc định `~/.config/traymixer/settings.json`.

## Khoảng cách đã biết so với Windows

Độ sáng tích hợp trên mỗi màn hình, cường độ ánh sáng xanh và thanh trượt làm mờ phần mềm vẫn chưa được chuyển. **Cài đặt màn hình và đèn ngủ** thay vào đó hãy mở cài đặt GNOME / KDE. Điều chỉnh gamma phổ quát không tồn tại trên Wayland; việc triển khai dành riêng cho nhà soạn nhạc là cần thiết. Không có sự thay đổi quyền hoặc ghi đặc quyền tùy ý nào được thực hiện. Vị trí bật lên chính xác, loại bỏ mất tiêu điểm và hiệu ứng phông nền Windows không được sao chép.

## Xây dựng và xác minh

Yêu cầu . NET SDK 10.0 . Trên Linux, từ một bản sao:

```sh
cd TrayMixer
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
dotnet run --project linux/TrayMixer.Linux.csproj
# Packaging requires dpkg-deb, rpm/rpmbuild and curl:
bash linux/package.sh 3.1.1
```

Trên máy phát triển Windows hiện tại:

```cmd
cd /d C:\Users\Vlad\sx\traymixer
dotnet build linux/TrayMixer.Linux.csproj -c Release
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
dotnet publish linux/TrayMixer.Linux.csproj -c Release -r linux-x64 --self-contained true -o linux/artifacts/publish
```

Windows có thể xuất bản chéo các tệp nhị phân Linux nhưng không thể thực thi chúng hoặc tạo các gói gốc mà không có môi trường Linux. `.github/workflows/linux.yml` chạy thử nghiệm, đọc lại PulseAudio thực đối với đầu ra rỗng, xác thực siêu dữ liệu gói và kiểm tra khói khởi chạy Xvfb. Những điều đó không thay thế việc kiểm tra khay GNOME, chủ đề, phần cứng hoặc nghe âm thanh thực tế. Sử dụng thẻ phát hành Linux `linux-v*` vì vậy quy trình phát hành Windows hiện tại không được kích hoạt. Các bản phát hành Windows hiện tại được giữ lại.
