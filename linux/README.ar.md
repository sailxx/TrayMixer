[English](README.md) · **العربية** · [简体中文](README.zh-CN.md) · [繁體中文](README.zh-TW.md) · [Čeština](README.cs.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [Ελληνικά](README.el.md) · [Bahasa Indonesia](README.id.md) · [Gaeilge](README.ga.md) · [Italiano](README.it.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Polski](README.pl.md) · [Română](README.ro.md) · [Русский](README.ru.md) · [Español](README.es.md) · [Українська](README.uk.md) · [اردو](README.ur.md) · [ئۇيغۇرچە](README.ug.md) · [Tiếng Việt](README.vi.md)

# TrayMixer لـ Linux - معاينة 3.1.1

منفذ سطح مكتب C# / Avalonia منفصل. لم يتغير مصدر Windows والمثبت الحالي.

## خط الأساس المعتمد

x86_64 Linux أو glibc 2.35 + أو X11 أو XWayland أو PulseAudio 15+ أو PipeWire مع `pipewire-pulse`, و `pactl`. يتم تشغيل فحوصات البناء والحزمة على Ubuntu 24.04. لا تزال هناك حاجة إلى التثبيت الخاص بالتوزيع وقبول سطح المكتب GNOME؛ هذه معاينة وليست مطالبة بالتكافؤ الكامل لـ Windows.

## ثَبَّتَ

قم بتنزيل DEB أو RPM أو AppImage من إصدار المعاينة Linux.

- أوبونتو/ديبيان: `sudo apt install ./TrayMixer_3.1.1_amd64.deb`
- فيدورا: `sudo dnf install ./TrayMixer-3.1.1-1.x86_64.rpm`
- AppImage: `chmod +x TrayMixer-3.1.1-x86_64.AppImage`, ثم قم بتشغيله. يتطلب النظام `pactl`, مكتبات X11 وfontconfig وICU وOpenSSL. ثَبَّتَ `pulseaudio-utils` على أوبونتو/فيدورا. FUSE 2 أو `--appimage-extract-and-run` قد تكون هناك حاجة. حزم AppImage. NET، وليس خادم النظام الصوتي أو مكتبات سطح المكتب.

يطلق **TrayMixer** من قائمة التطبيق. الإطلاق الأول يفتح الخلاط؛ يؤدي إغلاقه إلى استمرار تشغيل رمز الدرج. الخروج من قائمة الدرج. يحتاج GNOME Shell إلى ملحق AppIndicator/StatusNotifier (يتضمن Ubuntu عادةً واحدًا). بدون مضيف الدرج، لا تقم بتمكين التشغيل التلقائي للدرج فقط؛ استخدم قائمة التطبيق لبدء التشغيل. يتطلب التشغيل مرة أخرى من العملية الحالية إعادة فتح نافذتها من خلال قناة محلية خاصة بالمستخدم الحالي فقط.

## الضوابط

- يؤدي تنشيط الدرج إلى فتح/إغلاق الخلاط. توفر قائمة الدرج فتح الخلاط وكتم الصوت الافتراضي واستعادة الصفوف المخفية والتشغيل التلقائي والخروج. يستضيف مضيفو سطح المكتب سلوك النقر؛ Windows تكافؤ النقر الأوسط غير مضمون.
- أشرطة تمرير الإخراج ودفق التطبيق: 0–100%، كتم الصوت، عجلة الماوس ±2. ضبط مستوى الصوت الإيجابي يتم إلغاء كتم الصوت بشكل صريح.
- انقر فوق اسم الإخراج لجعله الافتراضي للتشغيل الجديد. لا يتم نقل التدفقات الموجودة بالقوة.
- انقر بزر الماوس الأيمن فوق صف لإخفائه؛ استعادة الصفوف المخفية تعيد كافة الصفوف مرة أخرى. يتم عرض التدفقات المنفصلة بشكل منفصل، بما في ذلك التدفقات الموجودة على الأجهزة غير الافتراضية.
- البدء عند تسجيل الدخول يستخدم إدخال التشغيل التلقائي XDG؛ تعطيل حتى يتم اختياره. يجب أن يبقى AppImage في موقعه الدائم.
- المظهر: GNOME / يتبع النظام الألوان الفاتحة/الداكنة وألوان GNOME المميزة، ويتم تحديثه أثناء فتح الخلاط. يستخدم GNOME الأقدم بدون دعم لهجة اللون الأزرق. تم دمج الفحم، والنورد، والكاتبوتشين، واللايت. ولا يوجد ميكا أو شفافية النافذة بأكملها.
- إعدادات: `$XDG_CONFIG_HOME/traymixer/settings.json`, تقصير `~/.config/traymixer/settings.json`.

## الفجوات المعروفة مقابل Windows

لم يتم بعد نقل السطوع المتكامل لكل شاشة وقوة الضوء الأزرق وشرائح تمرير تعتيم البرامج. **إعدادات العرض والإضاءة الليلية** يفتح إعدادات GNOME / KDE بدلاً من ذلك. تعديل جاما الشامل غير موجود في Wayland؛ هناك حاجة إلى تنفيذ خاص بالمركب. لا يتم تنفيذ أي عمليات كتابة ذات امتيازات تعسفية أو تغييرات في الأذونات. لا يتم تكرار موضع النافذة المنبثقة بالضبط وتجاهل فقدان التركيز وتأثيرات الخلفية Windows.

## بناء والتحقق

يتطلب . NET SDK 10.0 . على Linux، من نسخة:

```sh
cd TrayMixer
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
dotnet run --project linux/TrayMixer.Linux.csproj
# Packaging requires dpkg-deb, rpm/rpmbuild and curl:
bash linux/package.sh 3.1.1
```

على آلة التطوير Windows الحالية:

```cmd
cd /d C:\Users\Vlad\sx\traymixer
dotnet build linux/TrayMixer.Linux.csproj -c Release
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
dotnet publish linux/TrayMixer.Linux.csproj -c Release -r linux-x64 --self-contained true -o linux/artifacts/publish
```

يمكن لـ Windows نشر ثنائيات Linux بشكل متبادل ولكن لا يمكنه تنفيذها أو إنشاء حزم أصلية بدون بيئة Linux. `.github/workflows/linux.yml` يجري اختبارات، وقراءة PulseAudio حقيقية مقابل مخرجات فارغة، والتحقق من صحة البيانات الوصفية للحزمة، وفحص دخان إطلاق Xvfb. لا تحل هذه محل اختبار علبة GNOME الفعلية أو السمات أو الأجهزة أو الاستماع إلى الصوت. استخدام علامات الإصدار Linux `linux-v*` لذلك لا يتم تشغيل سير عمل إصدار Windows الحالي. يتم الاحتفاظ بإصدارات Windows الموجودة.
