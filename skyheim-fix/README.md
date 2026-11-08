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

### Latest Update (1.4.4)

- Requires Skyheim Compat 1.2.6. Craft panel **Rune Altar** title follows the game language.

### Previous (1.4.3)

- Requires Skyheim Compat 1.2.5. Translation file update for craft/hammer **Rune Altar** label; install matching Compat.

### Previous (1.4.2)

- Requires Skyheim Compat 1.2.2. Rune Altar name is translated again and the piece is listed under Misc.

### Previous (1.4.1)

- Requires Skyheim Compat 1.2.1. Windfury proc swirls show on female player models.

### Previous (1.4.0)

- Requires Skyheim Compat 1.2.0. Install both packages together (altar, imbue, icons, i18n, healing, Epic Loot imbue).

### Previous (1.3.44)

- Requires Skyheim Compat 1.1.36. Healing rune tooltip and predicted heal line.

### Previous (1.3.43)

- Requires Skyheim Compat 1.1.35. Healing rune amount scales with Nature Magic; duration scales with rune quality.

### Previous (1.3.42)

- Requires Skyheim Compat 1.1.33. Enchanted legendary set armor can be Eitr-imbued at the Runic Chest.

### Previous (1.3.41)

- Requires Skyheim Compat 1.1.32. The new skill-icon PNGs are applied on the magic skills. Compat fixes the Runic Chest shard text and starting an imbue when you close the chest (SeneaL UI).

### Previous (1.3.40)

- Requires Skyheim Compat 1.1.31. Install both packages together.
- The Rune Altar click opens the imbue chest. Put armor and Eitr Shards in to imbue. Rune recipes craft only at the altar. The chest hover shows those directions. The new skill icons stay.

### Previous (1.3.39)

- Fixes the three problems 1.3.38 introduced, and keeps the Ukrainian translation:
  - The Rune Altar opens its own imbue panel again (1.3.38 opened a vanilla Craft panel listing every recipe).
  - The Runic Chest tooltip shows its directions again, not just the slot count.
  - The new skill icons now actually appear in the altar panel.
- Station and chest names are no longer renamed in the loaded assets, so recipes that bind to the literal "Rune Altar" (Wackys Database and other config-driven recipes) keep working, and old saves need no migration. The altar and chest titles read "Rune Altar" / "Runic Chest" in every language, as they always did before 1.3.38; all other Skyheim text still follows the translation file.
- The Ukrainian/Polish/English translation file and the contributor's icon artwork stay.
- The mod's asset bundle bytes are untouched.

### Previous (1.3.38)

- Requires Skyheim Compat 1.1.30 (the Ukrainian translation build). Install both packages together.
- The Rune Altar and its chest now read their names from the translation file, so the altar, its chest, and their tooltips follow the game's language (Ukrainian included). The building menu lists the altar under Crafting.
- Nine rune skill icons use the contributor's clearer artwork.
- Characters from older worlds keep their known Rune Altar; the saved station name is migrated on load.
- The mod's asset bundle bytes are untouched. The names and icons are applied at runtime, which keeps the build reproducible from source.

### Previous (1.3.37)

- Requires Skyheim Compat 1.1.29 (menu-return ObjectDB fix). Install both packages together.

### Previous (1.3.36)

- Tested on Valheim 1.0.21. Requires Skyheim Compat 1.1.28 and BepInExPack Valheim 5.4.2360.
- skyheim.dll behavior is unchanged from 1.3.34. This version exists so the package depends on the Compat build that shows rune cooldown timers on the vanilla hotbar.

### Latest Update (1.3.34)

- Requires Skyheim Compat 1.1.26. Compat lists no dependencies.
- skyheim.dll behavior is unchanged from 1.3.33. This version exists so the package depends on the Compat build that turns Skyheim spell sounds down to half volume.

### Latest Update (1.3.33)

- Requires Skyheim Compat 1.1.25. Compat lists no dependencies.
- skyheim.dll behavior is unchanged from 1.3.32. This version exists so the package depends on the Compat build that sends Recall to the bed and keeps the selected language.

### Latest Update (1.3.32)

- Requires Skyheim Compat 1.1.24. Compat lists no dependencies.
- skyheim.dll behavior is unchanged from 1.3.31. This version exists so the package depends on the Compat build that shows the Rune Altar shard cost, chest count, and wait time.

### Latest Update (1.3.31)

- Requires Skyheim Compat 1.1.19. Compat lists no dependencies.
- skyheim.dll behavior is unchanged from 1.3.30. This version exists so the package depends on the Compat build that heals the caster when the healing area misses them.

### Latest Update (1.3.30)

- Requires Skyheim Compat 1.1.18. Compat lists no dependencies.
- skyheim.dll behavior is unchanged from 1.3.29. This version exists so the package depends on the Compat build with the healing rune, immolation, and altar imbue changes.

### Latest Update (1.3.29)

- Requires Skyheim Compat 1.1.15. Compat lists no dependencies.
- skyheim.dll behavior is unchanged from 1.3.28. This version exists so the package depends on the Compat build that shows rune cooldown timers.

### Latest Update (1.3.28)

- Requires Skyheim Compat 1.1.13. Compat lists no dependencies.
- Status runes spend their eitr cost on cast. Rune magic skills have a config for experience gain and experience lost on death.

### Latest Update (1.3.27)

- Asset-bundle scripts bind again. The plugin log line is Skyheim 1.3.27.

### Latest Update (1.3.26)

- Bosses drop the configured `eitr_shard_drop` stack (Eikthyr is 5). Edit `BepInEx/config/cjayride.SkyheimEitr.cfg`.

### Latest Update (1.3.25)

- Boss eitr shard amounts (and stack size 50) are in `skyheim.dll`. Edit `BepInEx/config/cjayride.SkyheimEitr.cfg`. Compat stays localization/HUD/runes only.

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
