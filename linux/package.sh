#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
VERSION="${1:-3.1.0}"
[[ "$VERSION" =~ ^[0-9]+\.[0-9]+\.[0-9]+$ ]] || { echo 'Invalid version'; exit 1; }
ROOT="$PWD"
OUT="$ROOT/linux/artifacts"
WORK="$(mktemp -d)"
trap 'rm -rf "$WORK"' EXIT
mkdir -p "$OUT" "$WORK/root/usr/lib/traymixer" "$WORK/root/usr/bin" "$WORK/root/usr/share/applications" "$WORK/root/usr/share/icons/hicolor/scalable/apps" "$WORK/root/usr/share/licenses/traymixer"
dotnet publish linux/TrayMixer.Linux.csproj -c Release -r linux-x64 --self-contained true -p:Version="$VERSION" -o "$WORK/root/usr/lib/traymixer"
cp linux/packaging/traymixer.desktop "$WORK/root/usr/share/applications/"
cp linux/packaging/traymixer.svg "$WORK/root/usr/share/icons/hicolor/scalable/apps/"
cp LICENSE "$WORK/root/usr/share/licenses/traymixer/LICENSE"
printf '#!/bin/sh\nexec /usr/lib/traymixer/TrayMixer "$@"\n' > "$WORK/root/usr/bin/traymixer"
chmod 755 "$WORK/root/usr/bin/traymixer" "$WORK/root/usr/lib/traymixer/TrayMixer"
mkdir -p "$WORK/root/DEBIAN"
cat > "$WORK/root/DEBIAN/control" <<EOF
Package: traymixer
Version: $VERSION
Section: sound
Priority: optional
Architecture: amd64
Maintainer: TrayMixer project <noreply@users.noreply.github.com>
Depends: pulseaudio-utils (>= 1:15), libx11-6, libice6, libsm6, libfontconfig1, libfreetype6, libxext6, libxcb1, libxrandr2, libxi6, libc6 (>= 2.35), libstdc++6, libicu70 | libicu72 | libicu74 | libicu76 | libicu78, libssl3 | libssl3t64, zlib1g
Recommends: xwayland, libglib2.0-bin, gsettings-desktop-schemas
Description: Tray volume mixer for Linux
 Separate volume and mute controls for outputs and application streams.
Homepage: https://github.com/sailxx/TrayMixer
EOF
dpkg-deb --root-owner-group --build "$WORK/root" "$OUT/TrayMixer_${VERSION}_amd64.deb"
rm -rf "$WORK/root/DEBIAN"
mkdir -p "$WORK/rpm/"{BUILD,RPMS,SOURCES,SPECS,SRPMS}
cat > "$WORK/rpm/SPECS/traymixer.spec" <<EOF
Name: traymixer
Version: $VERSION
Release: 1
Summary: Tray volume mixer for Linux
License: Apache-2.0
URL: https://github.com/sailxx/TrayMixer
Requires: pulseaudio-utils >= 15, libX11, libICE, libSM, fontconfig, freetype, libXext, libxcb, libXrandr, libXi, glibc >= 2.35, libstdc++, libicu, openssl-libs, zlib
AutoReqProv: no
%description
Separate volume and mute controls for outputs and application streams.
%install
mkdir -p %{buildroot}
cp -a "$WORK/root/." %{buildroot}/
%files
/usr/bin/traymixer
/usr/lib/traymixer
/usr/share/applications/traymixer.desktop
/usr/share/icons/hicolor/scalable/apps/traymixer.svg
%license /usr/share/licenses/traymixer/LICENSE
EOF
rpmbuild --define "_topdir $WORK/rpm" --define '_build_id_links none' -bb "$WORK/rpm/SPECS/traymixer.spec"
cp "$WORK/rpm/RPMS/x86_64/"*.rpm "$OUT/TrayMixer-${VERSION}-1.x86_64.rpm"
cp -a "$WORK/root" "$WORK/TrayMixer.AppDir"
cp linux/packaging/traymixer.desktop linux/packaging/traymixer.svg "$WORK/TrayMixer.AppDir/"
cp linux/packaging/AppRun "$WORK/TrayMixer.AppDir/AppRun"
chmod 755 "$WORK/TrayMixer.AppDir/AppRun"
# Download an exact asset and reject replacement or tampered bytes.
curl --fail --location --retry 3 https://github.com/AppImage/appimagetool/releases/download/continuous/appimagetool-x86_64.AppImage -o "$WORK/appimagetool"
echo "e8c1169c4af66c3184d7ee110713d4be04befe671fa358e2c1591ee840553b03  $WORK/appimagetool" | sha256sum -c -
chmod 755 "$WORK/appimagetool"
cd "$WORK"
./appimagetool --appimage-extract >/dev/null
ARCH=x86_64 ./squashfs-root/AppRun "$WORK/TrayMixer.AppDir" "$OUT/TrayMixer-${VERSION}-x86_64.AppImage"
cd "$OUT"
sha256sum ./*.deb ./*.rpm ./*.AppImage > SHA256SUMS
