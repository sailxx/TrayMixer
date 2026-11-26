[English](README.md) · [العربية](README.ar.md) · [简体中文](README.zh-CN.md) · [繁體中文](README.zh-TW.md) · [Čeština](README.cs.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [Ελληνικά](README.el.md) · [Bahasa Indonesia](README.id.md) · [Gaeilge](README.ga.md) · [Italiano](README.it.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Polski](README.pl.md) · [Română](README.ro.md) · [Русский](README.ru.md) · [Español](README.es.md) · [Українська](README.uk.md) · **اردو** · [ئۇيغۇرچە](README.ug.md) · [Tiếng Việt](README.vi.md)

# TrayMixer برائے Linux — 3.1.1 پیش منظر

ایک علیحدہ C# / Avalonia ڈیسک ٹاپ پورٹ۔ موجودہ Windows سورس اور انسٹالر میں کوئی تبدیلی نہیں ہے۔

## تائید شدہ بیس لائن

x86_64 Linux، glibc 2.35 +، X11 یا XWayland، PulseAudio 15+ یا PipeWire کے ساتھ `pipewire-pulse`, اور `pactl`. تعمیر اور پیکج کی جانچ Ubuntu 24.04 پر چلتی ہے۔ تقسیم کے لیے مخصوص تنصیب اور GNOME ڈیسک ٹاپ کی منظوری ابھی بھی درکار ہے۔ یہ ایک پیش نظارہ ہے، مکمل Windows برابری کا دعوی نہیں ہے۔

## انسٹال کریں۔

Linux پیش نظارہ ریلیز سے DEB، RPM یا AppImage ڈاؤن لوڈ کریں۔

- اوبنٹو/ڈیبین: `sudo apt install ./TrayMixer_3.1.1_amd64.deb`
- فیڈورا: `sudo dnf install ./TrayMixer-3.1.1-1.x86_64.rpm`
- AppImage: `chmod +x TrayMixer-3.1.1-x86_64.AppImage`, پھر اسے چلائیں. سسٹم کی ضرورت ہے۔ `pactl`, X11 لائبریریاں، fontconfig، ICU اور OpenSSL۔ انسٹال کریں۔ `pulseaudio-utils` Ubuntu/Fedora پر۔ FUSE 2 یا `--appimage-extract-and-run` ضرورت ہو سکتی ہے. AppImage بنڈلز NET، سسٹم آڈیو سرور یا ڈیسک ٹاپ لائبریریز نہیں۔

لانچ کریں۔ **TrayMixer** درخواست کے مینو سے۔ پہلی لانچ مکسر کھولتا ہے؛ اسے بند کرنے سے ٹرے کا آئیکن چلتا رہتا ہے۔ ٹرے مینو سے باہر نکلیں۔ GNOME شیل کو ایک AppIndicator/StatusNotifier ایکسٹینشن کی ضرورت ہے (Ubuntu میں عام طور پر ایک شامل ہوتا ہے)۔ ٹرے میزبان کے بغیر، صرف ٹرے آٹو اسٹارٹ کو فعال نہ کریں۔ لانچ کرنے کے لیے ایپلیکیشن مینو کا استعمال کریں۔ دوبارہ شروع کرنا موجودہ عمل سے اپنی ونڈو کو صرف موجودہ صارف کے مقامی پائپ کے ذریعے دوبارہ کھولنے کو کہتا ہے۔

## کنٹرول کرتا ہے۔

- ٹرے کو چالو کرنے سے مکسر کھلتا/بند ہوتا ہے۔ ٹرے مینو اوپن مکسر، ڈیفالٹ آؤٹ پٹ میوٹ، پوشیدہ قطاروں کو بحال کرنے، آٹو اسٹارٹ اور ایگزٹ پیش کرتا ہے۔ ڈیسک ٹاپ ٹرے میزبان کلک رویے کو کنٹرول کرتے ہیں۔ Windows مڈل کلک برابری کی ضمانت نہیں ہے۔
- آؤٹ پٹ اور ایپلیکیشن اسٹریم سلائیڈرز: 0–100%، خاموش، ماؤس وہیل ±2۔ مثبت والیوم سیٹ کرنا واضح طور پر چالو کرتا ہے۔
- نئے پلے بیک کے لیے اسے ڈیفالٹ بنانے کے لیے آؤٹ پٹ نام پر کلک کریں۔ موجودہ ندیوں کو زبردستی منتقل نہیں کیا جاتا ہے۔
- چھپانے کے لیے قطار پر دائیں کلک کریں۔ پوشیدہ قطاروں کو بحال کرنا تمام قطاروں کو واپس لاتا ہے۔ نان ڈیفالٹ ڈیوائسز پر اسٹریمز سمیت الگ الگ سلسلے الگ سے دکھائے جاتے ہیں۔
- لاگ ان پر شروع کریں ایک XDG آٹو اسٹارٹ اندراج کا استعمال کرتا ہے؛ منتخب ہونے تک غیر فعال۔ AppImage اپنی مستقل جگہ پر رہنا چاہیے۔
- ظاہری شکل: GNOME / سسٹم ہلکے/گہرے اور GNOME لہجے کے رنگوں کی پیروی کرتا ہے، مکسر کھلنے کے دوران تازہ ہوتا ہے۔ پرانا GNOME بغیر لہجے کی حمایت کے نیلے رنگ کا استعمال کرتا ہے۔ چارکول، نورڈ، کیٹپوکین اور لائٹ اندر بنے ہوئے ہیں۔ کوئی میکا یا پوری کھڑکی کی شفافیت نہیں۔
- ترتیبات: `$XDG_CONFIG_HOME/traymixer/settings.json`, پہلے سے طے شدہ `~/.config/traymixer/settings.json`.

## معلوم خلا بمقابلہ Windows

انٹیگریٹڈ فی مانیٹر چمک، نیلی روشنی کی طاقت اور سافٹ ویئر کو مدھم کرنے والے سلائیڈرز ابھی تک پورٹ نہیں ہوئے ہیں۔ **ڈسپلے اور رات کی روشنی کی ترتیبات** اس کے بجائے GNOME / KDE ترتیبات کھولتا ہے۔ یونیورسل گاما ایڈجسٹمنٹ Wayland پر موجود نہیں ہے۔ کمپوزر کے لیے مخصوص عمل درآمد کی ضرورت ہے۔ کوئی صوابدیدی مراعات یافتہ تحریریں یا اجازت میں تبدیلیاں نہیں کی جاتی ہیں۔ عین مطابق پاپ اپ پلیسمنٹ، فوکس لوس برخاستگی اور Windows بیک ڈراپ اثرات کو نقل نہیں کیا گیا ہے۔

## بنائیں اور تصدیق کریں۔

کی ضرورت ہے۔ NET SDK 10.0 Linux پر، کلون سے:

```sh
cd TrayMixer
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
dotnet run --project linux/TrayMixer.Linux.csproj
# Packaging requires dpkg-deb, rpm/rpmbuild and curl:
bash linux/package.sh 3.1.1
```

موجودہ Windows ڈویلپمنٹ مشین پر:

```cmd
cd /d C:\Users\Vlad\sx\traymixer
dotnet build linux/TrayMixer.Linux.csproj -c Release
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
dotnet publish linux/TrayMixer.Linux.csproj -c Release -r linux-x64 --self-contained true -o linux/artifacts/publish
```

Windows Linux بائنریز کو کراس شائع کر سکتا ہے لیکن ان پر عمل درآمد نہیں کر سکتا یا Linux ماحول کے بغیر مقامی پیکیج نہیں بنا سکتا۔ `.github/workflows/linux.yml` ٹیسٹ چلاتا ہے، اصلی PulseAudio ریڈ بیک ایک کالعدم آؤٹ پٹ کے خلاف، پیکج میٹا ڈیٹا کی توثیق اور Xvfb لانچ سموک چیکس۔ وہ اصل GNOME ٹرے، تھیمز، ہارڈویئر یا آڈیو سننے کی جانچ کی جگہ نہیں لیتے ہیں۔ Linux ریلیز ٹیگز استعمال کرتے ہیں۔ `linux-v*` لہذا موجودہ Windows ریلیز ورک فلو کو متحرک نہیں کیا گیا ہے۔ موجودہ Windows ریلیز کو برقرار رکھا گیا ہے۔
