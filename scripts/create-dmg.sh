#!/bin/bash
set -euo pipefail
cd "$(dirname "$0")/.."
app="$PWD/artifacts/PS5 PKG Tool.app"
dmg="$PWD/artifacts/PS5PKGTool-Mac-osx-arm64.dmg"
if [ ! -x "$app/Contents/MacOS/PS5PKGTool.Mac" ]; then
    echo "Build the app first: bash scripts/publish-macos.sh" >&2
    exit 1
fi
if [ -e "$dmg" ]; then
    echo "Move the existing $dmg before creating a new image." >&2
    exit 1
fi
staging="$(mktemp -d "${TMPDIR:-/tmp}/ps5pkgtool-dmg.XXXXXX")"
trap 'rm -rf "$staging"' EXIT
ditto "$app" "$staging/PS5 PKG Tool.app"
ln -s /Applications "$staging/Applications"
cat > "$staging/INSTALL.txt" <<'INSTALL'
Drag PS5 PKG Tool.app onto Applications, then eject this disk image.
Open PS5 PKG Tool from Applications. No .NET installation is required.

Apple Silicon, macOS 14 or newer. Ad hoc signed development build,
without Developer ID signing/notarization.
If macOS blocks launching, see System Settings > Privacy & Security > Open Anyway.
Apple instructions: https://support.apple.com/en-us/102445

Modified from pearlxcore/PS5PKGTool under GPL-3.0.
Licenses and notices are inside the app's Contents/Resources directory.
If sharing this build, also provide its corresponding modified source.
INSTALL
hdiutil create -volname "PS5 PKG Tool" -srcfolder "$staging" -fs HFS+ -format UDZO "$dmg"
hdiutil verify "$dmg"
echo "$dmg"
