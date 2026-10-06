#!/bin/bash
set -euo pipefail
cd "$(dirname "$0")/.."
proof="$(mktemp -d "${TMPDIR:-/tmp}/ps5pkgtool-publish-check.XXXXXX")"
trap 'rm -rf "$proof"' EXIT
mkdir -p "$proof/scripts"
cp scripts/publish-macos.sh "$proof/scripts/"
if DOTNET=/usr/bin/false bash "$proof/scripts/publish-macos.sh"; then
    echo "FAIL: simulated publish failure returned success" >&2
    exit 1
fi
app="$proof/artifacts/PS5 PKG Tool.app"
test ! -e "$app"
mkdir -p "$app"
echo original > "$app/preserve.txt"
if DOTNET=/usr/bin/false bash "$proof/scripts/publish-macos.sh"; then
    echo "FAIL: existing bundle was accepted" >&2
    exit 1
fi
test "$(cat "$app/preserve.txt")" = original
echo "PASS: publish failure cleanup and existing-bundle preservation"
