#!/bin/bash
set -euo pipefail
cd "$(dirname "$0")/.."
dotnet_command="${DOTNET:-dotnet}"
app="$PWD/artifacts/PS5 PKG Tool.app"
if [ -e "$app" ]; then
    echo "Move the existing $app before publishing a new bundle." >&2
    exit 1
fi
trap 'rm -rf "$app"' ERR
mkdir -p "$app/Contents/MacOS" "$app/Contents/Resources"
"$dotnet_command" publish PS5PKGTool.Mac/PS5PKGTool.Mac.csproj -c Release -r osx-arm64 \
    --self-contained true --disable-build-servers -p:UseSharedCompilation=false -o "$app/Contents/MacOS"
cat > "$app/Contents/Info.plist" <<'PLIST'
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0"><dict>
  <key>CFBundleExecutable</key><string>PS5PKGTool.Mac</string>
  <key>CFBundleIdentifier</key><string>org.ps5pkgtool.macos</string>
  <key>CFBundleName</key><string>PS5 PKG Tool</string>
  <key>CFBundleDisplayName</key><string>PS5 PKG Tool</string>
  <key>CFBundlePackageType</key><string>APPL</string>
  <key>CFBundleShortVersionString</key><string>0.1.0</string>
  <key>CFBundleVersion</key><string>1</string>
  <key>NSHighResolutionCapable</key><true/>
  <key>LSMinimumSystemVersion</key><string>14.0</string>
</dict></plist>
PLIST
cp LICENSE THIRD_PARTY_NOTICES.md "$app/Contents/Resources/"
cp PS5PKGTool/ThirdParty/ProsperoPkgTool/LICENSE "$app/Contents/Resources/ProsperoPkgTool-LICENSE"
cp PS5PKGTool.Ufs2/LICENSE "$app/Contents/Resources/Ufs2-LICENSE"
cp PS5PKGTool.Mac/AVALONIA-LICENSE "$app/Contents/Resources/"
cp PS5PKGTool.Mac/SKIA-LICENSE PS5PKGTool.Mac/HARFBUZZSHARP-LICENSE "$app/Contents/Resources/"
cat > "$app/Contents/Resources/MACOS-PORT.txt" <<'NOTICE'
Development macOS port of pearlxcore/PS5PKGTool, modified under GPL-3.0.
Source: https://github.com/foresthpark/PS5PKGTool-Mac
Original: https://github.com/pearlxcore/PS5PKGTool
Avalonia (MIT): https://github.com/AvaloniaUI/Avalonia/blob/master/licence.md
SkiaSharp (MIT): https://github.com/mono/SkiaSharp/blob/main/LICENSE.md
No proprietary Oodle native binary is included.
Distribute the corresponding modified source alongside any distributed build.
NOTICE
plutil -lint "$app/Contents/Info.plist"
codesign --force --deep --sign - "$app"
codesign --verify --deep --strict "$app"
trap - ERR
echo "$app"
