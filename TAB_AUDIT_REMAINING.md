# Overview/detail tabs audit — remaining work

The bulk of the source-adaptation audit is implemented and committed. This file records what is
**not yet done** so it is tracked rather than lost. None of it is broken or partially wired; these
are additive/structural items only.

## Implemented (for reference)
- Cross-tab: async selection guards, populated-after-success, scoped section errors,
  structure-without-content, refreshed Overview.
- Size semantics split (`ContainerFileLength` / `ContainerLogicalSize` / `GameRootBytes` / stored).
- Typed `SectionState`/`SectionStatus` + Overview diagnostics.
- Artwork availability/encoding/dimensions in section headers.
- Trophy set version + languages + failure-vs-absent; filter "showing X of N" and scope text.
- UDS event selection by group + name.
- Files list columns attached + `Origin` column (PFS/CNT/Host/exFAT/UFS2/PFSC); honest summaries.
- Executable ELF-vs-SELF rendering, decoded machine, "Name offset", on-demand eboot SHA-256.
- Raw param.json Formatted/Original views + inner-PFS fallback.
- Container rename; second-level **Metadata** tab (param.sfo / Keystone / PlayGo); PKG-only
  Segments/CNT/SI hidden for other sources.
- Structure-only listing for sources without a readable `param.json` (no longer dropped).
- Regression smoke: details, source round-trips (exFAT/UFS2/FFPFSC/PPT PKG), edge cases
  (corrupt UCP, missing UDS), structure-only.

## Remaining

| Item | Notes |
|---|---|
| Lazy per-section detail loading (CT-5) | `Ps5DetailsLoader` still reads artwork/trophies/UDS/executable/inventory in one pass; only the UI is lazy. Real fix: per-section load methods + a shared disposable source session. |
| Per-root selection UI (CT-6) | A source with several `sce_sys/param.json` roots is listed structure-only with a warning; there is no chooser to pick a root. |
| Structural view when the volume cannot be opened (CT-6) | If the image/UFS2/PFSC volume itself fails to open, the source is still reported as a scan error. |
| Artwork export labeling (§2) | Label converted PNG exports (DDS is decoded/re-encoded, original PNG can be copied unchanged). |
| Multiple trophy-archive selection (§3) | Reader checks only `sce_sys/trophy2/trophy00.ucp`; scope is now stated but no archive selection is offered. |
| Full UDS enum values view/export + unresolved-reference warnings (§4) | Enum tab shows a 4-value sample only. |
| Files: wrapper → inner filesystem → game root coordinate labeling; encrypted-entry read actions (§5) | Metadata-only summary now names encrypted entries; per-entry disabled read actions are not wired. |
| LPP PKG round-trip fixture (tests) | LibProsperoPkg always fake-signs loose executables, so the fixture needs a real loadable-segment ELF. |
| Missing-CNT-JSON fixture (tests) | Needs a package whose outer CNT entry `0x2000` is absent/encrypted while the inner PFS has `param.json`; the engine builder always writes `0x2000`, so this needs binary CNT surgery. |
