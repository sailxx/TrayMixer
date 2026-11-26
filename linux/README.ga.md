[English](README.md) · [العربية](README.ar.md) · [简体中文](README.zh-CN.md) · [繁體中文](README.zh-TW.md) · [Čeština](README.cs.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [Ελληνικά](README.el.md) · [Bahasa Indonesia](README.id.md) · **Gaeilge** · [Italiano](README.it.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Polski](README.pl.md) · [Română](README.ro.md) · [Русский](README.ru.md) · [Español](README.es.md) · [Українська](README.uk.md) · [اردو](README.ur.md) · [ئۇيغۇرچە](README.ug.md) · [Tiếng Việt](README.vi.md)

# TrayMixer le haghaidh Linux - réamhamharc 3.1.1

Port deisce ar leith C# / Avalonia. Níl an fhoinse agus an suiteálaí Windows reatha gan athrú.

## Bunlíne tacaithe

x86_64 Linux , glibc 2.35 +, X11 nó XWayland , PulseAudio 15+ nó PipeWire le `pipewire-pulse`, agus `pactl`. Ritheann seiceálacha tóg agus pacáiste ar Ubuntu 24.04 . Tá suiteáil dáileadh-sonrach agus glacadh deasc GNOME fós ag teastáil; ní réamhamharc é seo, ní éileamh ar phaireacht Windows iomlán.

## Suiteáil

Íoslódáil DEB , RPM nó AppImage ón scaoileadh réamhamharc Linux.

- Ubuntu/Debian: `sudo apt install ./TrayMixer_3.1.1_amd64.deb`
- Fedora: `sudo dnf install ./TrayMixer-3.1.1-1.x86_64.rpm`
- AppImage: `chmod +x TrayMixer-3.1.1-x86_64.AppImage`, rith ansin é. Éilíonn córas `pactl`, Leabharlanna X11, fontconfig, ICU agus OpenSSL. Suiteáil `pulseaudio-utils` ar Ubuntu/Fedora. FUSE 2 nó `--appimage-extract-and-run` d’fhéadfadh go mbeadh gá leis. cuachtaí AppImage . NET , ní freastalaí fuaime an chórais nó leabharlanna deisce.

Seoladh **TrayMixer** ón roghchlár feidhmchlár. Osclaíonn an chéad seoladh an meascthóir; agus é á dhúnadh coinníonn sé deilbhín an tráidire ag rith. Scoir ón roghchlár tráidire. GNOME Teastaíonn síneadh AipTáscaire/StatusNotifier ó Shell (cuimsíonn Ubuntu ceann de ghnáth). Gan óstach tráidire, ná cumasaigh autostart tráidire amháin; bain úsáid as an roghchlár feidhmchlár chun é a sheoladh. Ag seoladh arís iarrtar ar an bpróiseas atá ann faoi láthair a fhuinneog a athoscailt trí phíobán áitiúil reatha-úsáideoir amháin.

## Rialuithe

- Osclaíonn/dúnann gníomhachtú tráidire an meascthóir. Tairgeann an roghchlár tráidire meascthóir Oscailte, balbhú aschuir réamhshocraithe, aischuir sraitheanna folaithe, tús uathoibríoch agus Scoir. Déanann tráidire deisce ina hóstach ar iompar cliceáil a rialú; Ní ráthaítear paireacht lárchliceáil Windows.
- Sleamhnáin aschuir agus sruth feidhme: 0–100%, balbh, roth luiche ±2. Díbhalbhaítear go follasach nuair a shocraítear toirt deimhneach.
- Cliceáil ar ainm aschuir chun é a dhéanamh mar an réamhshocrú le haghaidh athsheinm nua. Ní dhéantar na sruthanna atá ann cheana a bhogadh go héigeantach.
- Deaschliceáil as a chéile chun dul i bhfolach; Athchóirigh sraitheanna folaithe tugann sé gach sraith ar ais. Taispeántar sruthanna ar leith ar leithligh, lena n-áirítear sruthanna ar fheistí neamh-mhainneachtana.
- Úsáideann Tosaigh ag logáil isteach iontráil autostart XDG; díchumasaithe go dtí go roghnófar é. Caithfidh AppImage fanacht ina shuíomh buan.
- Dealramh: Leanann GNOME / System dathanna éadroma/dorcha agus accent GNOME, athnuachan agus an meascthóir ar oscailt. Úsáideann GNOME níos sine gan tacaíocht accent gorm. Tá Gualach, Nord, Catppuccin agus Light tógtha isteach. Níl aon trédhearcacht Mica nó uile-fhuinneog.
- Socruithe: `$XDG_CONFIG_HOME/traymixer/settings.json`, réamhshocraithe `~/.config/traymixer/settings.json`.

## Bearnaí aitheanta i gcoinne Windows

Níl gile in aghaidh an mhonatóra chomhtháite, neart an tsolais ghorm agus na sleamhnáin bhogearraí chun maolú a iompar fós. **Socruithe taispeána & solas oíche** osclaíonn sé socruithe GNOME / KDE ina ionad sin. Níl coigeartú gáma uilíoch ann ar Wayland ; tá gá le cur chun feidhme a bhaineann go sonrach le cumadóir. Ní dhéantar aon scríbhinní pribhléide treallach nó athruithe ceada. Ní mhacasamhail a dhéantar socrúcháin aníos cruinn, dífhostú fócas-caillteanas agus éifeachtaí cúlra Windows.

## Tógáil agus fíoraigh

Éilíonn . NET SDK 10.0 . Ar Linux , ó chlón:

```sh
cd TrayMixer
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
dotnet run --project linux/TrayMixer.Linux.csproj
# Packaging requires dpkg-deb, rpm/rpmbuild and curl:
bash linux/package.sh 3.1.1
```

Ar an meaisín forbartha Windows atá ann faoi láthair:

```cmd
cd /d C:\Users\Vlad\sx\traymixer
dotnet build linux/TrayMixer.Linux.csproj -c Release
dotnet run --project linux/tests/TrayMixer.Linux.Tests.csproj -c Release
dotnet publish linux/TrayMixer.Linux.csproj -c Release -r linux-x64 --self-contained true -o linux/artifacts/publish
```

Is féidir le Windows dénártha Linux a thrasfhoilsiú ach ní féidir leo iad a fhorghníomhú ná pacáistí dúchais a chruthú gan timpeallacht Linux. `.github/workflows/linux.yml` ritheann tástálacha, fíor-léamh PulseAudio i gcoinne aschuir nialasach, bailíochtú meiteashonraí pacáiste agus seiceálacha deataigh seolta Xvfb. Ní chuirtear iad sin in ionad tráidire GNOME iarbhír, téamaí, crua-earraí nó éisteacht le fuaim a thástáil. Úsáideann clibeanna scaoileadh Linux `linux-v*` mar sin ní spreagtar an sreabhadh oibre scaoileadh Windows atá ann cheana féin. Coinnítear na heisiúintí Windows atá ann cheana féin.
