# Third-party notices

PS5 PKG Tool is licensed under the GNU General Public License v3.0 (see `LICENSE`). The
components below are used under their own terms.

## ProsperoPkgTool

The PS5 PKG (FIH/CNT/PFS/NAPS) reading, verification, extraction and debug-package creation
implementation is provided by the clean-room MIT engine ProsperoPkgTool, consumed as a vendored
managed library at `PS5PKGTool/ThirdParty/ProsperoPkgTool/ProsperoPkgTool.dll`.

MIT License. Copyright (c) 2026 pearlxcore. No GPL, SDK, or decompiled code is included; the
engine is a clean-room reimplementation validated against independent oracles.

## LibProsperoPkg

The alternative package build/validate/extract backend is provided by LibProsperoPkg, loaded from a
vendored payload at `PS5PKGTool/ThirdParty/LibProsperoPkg12/` (version **1.2.0** - the version the
PPR-PKG / fpkg-gui builder ships; the newer 2.6.0 regressed the NAPS layout). It is loaded in an
isolated assembly-load context together with its bundled dependencies: `BCnEncoder.Net 2.3.0`,
`CommunityToolkit.HighPerformance 8.4.0`, `Magick.NET 14.15.0` (managed + its
`runtimes/win-x64/native/Magick.Native-Q8-x64.dll`), and `libScePubTools.dll`.

GNU General Public License v3.0 or later (GPL-3.0-or-later).
Copyright (c) SvenGDK 2026. https://github.com/SvenGDK/LibProsperoPkg

Because LibProsperoPkg is GPL-3.0-or-later and PS5 PKG Tool links it, the combined work is
distributed under the GPL-3.0 (see `LICENSE`). The full license text ships at
`PS5PKGTool/ThirdParty/LibProsperoPkg12/LICENSE`.

## UFS2Tool

The `PS5PKGTool.Ufs2` filesystem implementation is based on
[SvenGDK/UFS2Tool](https://github.com/SvenGDK/UFS2Tool), used and modified under the BSD 2-Clause
License. Copyright (c) 2026, SvenGDK. The original copyright and license text are retained in
`PS5PKGTool.Ufs2/LICENSE` and in the imported source files.

## DarkUI

The user interface is built on DarkUI, licensed under the MIT License.
Copyright (c) 2017 Robin (Robin Perris). https://github.com/RobinPerris/DarkUI

## BCnEncoder.Net

BCnEncoder.Net (and BCnEncoder.Net.ImageSharp) is distributed under the MIT License. It is used to
encode BC7 textures for PS5 CNT media. https://github.com/Nominom/BCnEncoder.NET

## Oodle.NET

Oodle.NET is distributed under the MIT License. Copyright (c) 2025 NotOfficer.
It is the managed wrapper used to load the native Oodle decoder.

## Oodle Data Compression

PS5PKGTool includes the 64 bit Oodle Data Compression 2.9.10 redistributable
`oo2core_9_win64.dll`. Oodle is proprietary Licensed Technology supplied by Epic Games
and RAD Game Tools and is used under the Unreal Engine End User License Agreement:
https://www.unrealengine.com/eula/unreal

Copyright Epic Games, Inc. and/or RAD Game Tools. All rights reserved.

The Oodle Licensed Technology is provided only as an incorporated object code component
of PS5PKGTool and only as needed to use PS5PKGTool. End users may not extract, reuse,
redistribute, or incorporate it into another product. To the maximum extent permitted by
applicable law, PS5PKGTool makes no representations or warranties and accepts no conditions
or liabilities relating to Epic Games' or RAD Game Tools' Licensed Technology.

Bundled file SHA256:
`6F5D41A7892EA6B2DB420F2458DAD2F84A63901C9A93CE9497337B16C195F457`

## PS4 PKG Tool assets

The file-type icons under `PS5PKGTool/Resources` are reused from the PS4 PKG Tool project
(https://github.com/pearlxcore/PS4-PKG-Tool), which is licensed under the GNU General Public
License v3.0. Copyright (c) pearlxcore.

## Image format references

The managed exFAT, UFS2/FFPKG, PFSC and PFS implementations were written from public format
documentation and validated against independent tools. Credit to the authors of the tools that
document and handle these formats:

- MkPFS, PSBrew / Renan Barreto (https://github.com/PSBrew/MkPFS), PFS and PFSC/FFPFSC format work.
- UFS2Tool and LibProsperoPkg, SvenGDK (https://github.com/SvenGDK/UFS2Tool), UFS2/FFPKG work. UFS2Tool is used under the BSD 2-Clause License.
- ps5-exfat-builder, kerrdec97 (https://github.com/PSBrew/ps5-exfat-builder), exFAT format work.
- PS5 Dump and Image Converter, strongt1me (https://github.com/strongt1me/PS5-Dump-Image-Converter).
