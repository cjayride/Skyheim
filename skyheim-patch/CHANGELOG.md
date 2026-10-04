# Changelog

## 1.1.11

- Null-safe Rune Altar container update, so a missing SkyheimAltarPanel script does not break the inventory.
- Builds the rune cooldown overlay at runtime, so a missing SkyheimCooldownItem script does not leave cooldowns blank.

## 1.1.10

- Fixed a NullReferenceException in Skyheim's windfury damage hook when poison/AoE hits have no attacker, nview, or status manager.

## 1.1.9

- Thunderstore package uses the 1.0 COMPAT icon.

## 1.1.8

- Draws Skyheim rune cooldown seconds on SeneaL hotbar, action, and inventory slots without replacing SeneaL's HUD prefabs.
- If SeneaL UI is not installed, Skyheim keeps its original vanilla hotbar and inventory cooldown overlays.

## 1.1.7

- Stops Skyheim from rewriting the vanilla hotbar/inventory slot prefabs when SeneaL UI is installed, so food, action bar, and minimap can display.

## 1.1.5

- Fixed a stack-overflow crash when hovering enchanted armor in a chest.
- Inventory descriptions are populated directly at pointer-enter, before Valheim tries to show the tooltip.

## 1.1.1

- Fixed item names and descriptions in-game (no more missing/blank localization).
- Fixed a crash that occurred when trying to craft items (including with `devcommands`).
- Added configuration files for all the runes (`BepInEx/config/cjayride.skyheimcompat.cfg` after you load a world once).
- Enforces rune cooldown blocking on current Valheim.
