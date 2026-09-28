# Skyheim 1.0 Fix

**A temporary compatibility fix for Skyheim by makail**

---

## Important Credit & Disclaimer

**This is NOT an original mod.**

All credit for Skyheim goes to the original developer **makail** (also known as Makzimus).

- Original mod: [Skyheim by makail on Thunderstore](https://thunderstore.io/c/valheim/p/makail/Skyheim/)
- Original Nexus page: [Skyheim on Nexus Mods](https://www.nexusmods.com/valheim/mods/916)

This package is a **temporary community fix** created by **cjayride** because the original mod has not received updates in approximately 2 years.

The goal of this fix is simply to keep Skyheim working with the current version of Valheim (including the 1.0 / Deep North update) until the original author is able to update it (or until a better long-term solution appears).

If makail ever releases an official update, **please switch back to the original mod** and discontinue use of this fix.

---

## What is Skyheim?

Skyheim is a magic mod for Valheim that provides balanced progression and a reason to revisit bosses. Bosses drop unique crafting ingredients used to craft and upgrade runes that let you cast a variety of beneficial and destructive spells. The mod also allows early/mid-game access to Eitr by imbuing armor with Eitr Shards.

Features include:
- Rune Altar for crafting/upgrading runes and imbuing armor
- Multiple spell runes (offensive and supportive)
- Armor imbuement for Eitr capacity and regeneration
- Progression tied to boss kills and dungeon exploration

---

### Latest Update (1.3.24)

- Requires **Skyheim Compat 1.1.10** (poison/AoE NullReferenceException in Skyheim's damage postfix).
- Compat 1.1.5+ fixes enchanted-armor tooltip crashes and first-hover display.
- Fixed item names and descriptions in-game (no more missing/blank localization).
- Fixed a crash that occurred when trying to craft items (including with `devcommands`).
- Added a configuration file for all runes (enabled, cooldown, Eitr, stamina, range, damage).

---

## Installation

### Thunderstore / r2modman / Gale

Install **Skyheim 1.0 Fix**. **Skyheim Compat** is a required dependency and will install automatically.

**Do not install makail-Skyheim at the same time.** Gale/r2modman will still label the plugin `Skyheim 1.3.12`; the Fix package is the rewritten `skyheim.dll`. If the log shows `TypeLoadException` / `SkyheimAltarPanel:_leftSlot` / `Element`, you have the original DLL.

### Manual / Nexus

1. Make sure you have **BepInEx** installed.
2. Remove any previous version of Skyheim (original or other fixes).
3. Install **both** this package **and** **Skyheim Compat 1.1.10**.
4. Keep `skyheim.json` next to `skyheim.dll`.
5. Launch the game.

---

## Notes

- This is intended as a stop-gap solution only.
- Please support the original author if they return to the project.
- Report any issues with this specific fix to **cjayride**.

Thank you to **makail** for creating such a well-loved mod.  
— cjayride
