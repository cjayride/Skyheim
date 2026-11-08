# Skyheim — Fix 1.4.4 / Compat 1.2.6

## Goal
Craft panel title for the Rune Altar follows the game language (Ukrainian reported English "RUNE ALTAR").

## Status
Released (packed).

## Cause
`InventoryGui.UpdateRecipe` calls `Localization.Localize(CraftingStation.m_name)`. Station id stays the literal `Rune Altar` (WackyDB / recipe checks). Vanilla `Localize` only substitutes `$keys`, so the panel title never used `$sh_piece_altar`. World hover was already patched and showed Ukrainian.

## Files
- `Cjayride.SkyheimCompat/AltarChestText.cs` — rewrite leftover `Rune Altar` after Localize; set craft panel title
- Version bump Compat 1.2.6 / Fix 1.4.4

## Output
- `D:\AI-Agent-Projects\cjaycraft\Cjayride.Skyheim\dist\upload\SkyheimCompat-1.2.6.zip`
- `D:\AI-Agent-Projects\cjaycraft\Cjayride.Skyheim\dist\upload\SkyheimFix-1.4.4.zip`
- `D:\AI-Agent-Projects\cjaycraft\Cjayride.Skyheim\dist\compat\Cjayride.SkyheimCompat.dll`
