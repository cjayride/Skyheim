# Skyheim — Compat 1.2.9 released (2026-11-11)

## Status
Compat **1.2.9 released**: zip + commit + annotated tag + GitHub release, "the same way" as
SeneaLHudLayout 1.5.51 (user: "Do the same for Skyheim compat 1.2.9" — standing yes for this
release, same scope as the HudLayout 1.5.51 override of the no-push rule).

## What 1.2.9 is
- `[Cooldown overlay] OnSenealHotbar` default back to **true** (the 1.2.6 behaviour). The 1.2.7
  "off" was based on a guess; 1.2.8's self-reporting line settled it from the live log:
  `Skyheim cooldown overlays on SeneaL UI's hotbar/action slots: 16 of 16 slots carry one.`
  → Skyheim DOES attach on SeneaL's slots; the user confirmed "potions cool down looks okay".
- The gate + strip stay (set false to hand the space to SeneaL HUD Layout) and both paths log counts.

## Release
- zip: `dist\upload\SkyheimCompat-1.2.9.zip` 114,547 b
  sha256 `787A80ABDDEF0FDAF2524A379662AD44684BD628BD1E41C5B9CBBCEFA632BDC3`
  flat 5 entries copied from the accepted 1.2.6 zip: `Cjayride.SkyheimCompat.dll` (104,448 b,
  FileVersion 1.2.9.0, sha256 `53473B3FFAA3…` — byte-identical to `dist\compat\…` and the DLL
  installed in the Gale beta_test profile), `manifest.json` (name `SkyheimCompat`, version 1.2.9,
  description 120 chars — the same text that passed 1.2.6, deps []), `README.md`, `CHANGELOG.md`
  (top header `## 1.2.9`), `icon.png` (256×256 RGB — byte-identical to the accepted 1.2.6 zip's).
- built with `dotnet build Cjayride.SkyheimCompat -c Release` (OutputPath = `dist\compat\`), NOT
  pack.ps1 (that runs the Fix repack too; no Fix change this time, Fix stays 1.4.4).
- commit: source + skyheim-patch docs/manifest/DLL + dist zips (repo tracks dist\upload, per 6a75c3f).
- tag: annotated `compat-v1.2.9` (message "Skyheim Compat 1.2.9", matching compat-v1.2.6's type/format).
- release: tag compat-v1.2.9, title carries Compat 1.2.9, asset the zip above, **Latest**.
  (Note: compat releases get their OWN tag `compat-vX` here; Fix releases use `vX.Y.Z` +
  `dist/upload/SkyheimFix-*.zip`. v1.4.4 keeps the Fix zip; Latest moves to compat-v1.2.9.)

## Previous (Fix 1.4.4 / Compat 1.2.6, 2026-11-08)
Craft panel title for the Rune Altar follows the game language. `InventoryGui.UpdateRecipe` calls
`Localization.Localize(CraftingStation.m_name)`; station id stays the literal `Rune Altar`
(WackyDB/recipe checks); vanilla `Localize` only substitutes `$keys`, so the title never used
`$sh_piece_altar`. Files: `Cjayride.SkyheimCompat/AltarChestText.cs`.
Zips: `dist\upload\SkyheimCompat-1.2.6.zip`, `dist\upload\SkyheimFix-1.4.4.zip`.
