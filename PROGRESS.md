# Skyheim — Fix 1.4.2 / Compat 1.2.2

## Goal
Restore Rune Altar translation in the hammer menu and world hover, and put the piece on a build tab (Misc), without reopening the vanilla Craft panel or breaking rune station matching.

## Status
Completed. Packed.

## Files changed
- `Cjayride.SkyheimCompat/AltarChestText.cs` — Piece.m_name `$sh_piece_altar`, Misc category + Misc usage tag; CraftingStation hover uses the token. CraftingStation.m_name stays `Rune Altar`.
- Version bumps: Compat 1.2.2, Fix 1.4.2, manifests, changelogs, READMEs, pack.ps1.

## Commands
- `.\pack.ps1` — succeeded. Compat + Eitr build, ILRepack, zips written.

## Output
- `D:\AI-Agent-Projects\cjaycraft\Cjayride.Skyheim\dist\compat\Cjayride.SkyheimCompat.dll`
- `D:\AI-Agent-Projects\cjaycraft\Cjayride.Skyheim\dist\upload\SkyheimCompat-1.2.2.zip`
- `D:\AI-Agent-Projects\cjaycraft\Cjayride.Skyheim\dist\upload\SkyheimFix-1.4.2.zip`

## Important decisions
- Do not set `Piece.UsageTagFlags.Crafting` (that opened the vanilla Craft list in 1.3.38).
- Do not rename `CraftingStation.m_name` (recipes / Wacky / known stations bind to the literal `Rune Altar`).

## Next
- In-game: English / Ukrainian / Polish — hammer Misc tab, world hover, click still opens imbue chest, rune recipes still require the altar.
