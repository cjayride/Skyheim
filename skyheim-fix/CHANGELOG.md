# Changelog

## 1.4.2

- Requires Skyheim Compat 1.2.2 (translated Rune Altar name and Misc build tab).

## 1.4.1

- Requires Skyheim Compat 1.2.1 (Windfury proc VFX on female characters).

## 1.4.0

- Requires Skyheim Compat 1.2.0. Paired release: Rune Altar imbue chest, rune crafting at the altar, skill icons, translation/i18n assets, and Compat fixes through healing tooltips.
- Fix version moves to 1.4.x while Compat moves to 1.2.0 (same release, different version lines).

## 1.3.44

- Requires Skyheim Compat 1.1.36 (healing rune tooltip text + predicted heal line).

## 1.3.43

- Requires Skyheim Compat 1.1.35 (healing rune: amount from Nature Magic, duration from rune quality).

## 1.3.42

- Requires Skyheim Compat 1.1.33 (Epic Loot enchanted / legendary set armor can be Eitr-imbued).

## 1.3.41

- Skill icons (Fire/Frost/Holy/Nature) are assigned from the new PNGs onto the skill defs, and the atlas paint retries if the packed texture is not ready on the first load.
- Requires Skyheim Compat 1.1.32 (SeneaL chest text and imbue start).

## 1.3.40

- Requires Skyheim Compat 1.1.31.
- Clicking the Rune Altar opens the imbue chest again (not a vanilla Craft panel of every recipe). Put armor and Eitr Shards in the chest to imbue.
- Rune recipes are bound to the Rune Altar station, so they craft only while you are at the altar.
- The Runic Chest hover text shows the directions (imbue in the chest, craft runes at the altar). English and Polish descriptions match the Ukrainian ones instead of repeating the piece name.
- Skill icons still use the contributor artwork painted onto the loaded rune atlas.

## 1.3.39

- Fixes the three problems the 1.3.38 translation build introduced, keeping the Ukrainian translation itself:
  - The Rune Altar opens its own imbue panel again. 1.3.38 had marked it a vanilla crafting station, so clicking it opened a Craft panel listing every recipe and the gear-plus-shard imbue flow stopped working.
  - The Runic Chest tooltip shows its directions again. Renaming the chest had left only "x slots, used x slots"; the station and chest names now follow the translation file the normal way, with the literal names kept where recipes and tooltips bind to them.
  - The new skill icons actually appear. The shipped icon patch compared the texture name exactly and never matched the baked bundle name, so it silently did nothing; it now matches and paints the loaded rune atlas.
- Old worlds are untouched: station names are no longer renamed, so saved "Rune Altar" entries keep working with no migration.
- The Ukrainian/Polish/English translation file and the contributor's icon artwork stay.

## 1.3.38

- Requires Skyheim Compat 1.1.30 (the Ukrainian translation build).
- The Rune Altar, its chest, and their tooltips now follow the translation file. Ukrainian shows "Рунічний вівтар" / "Рунічна скринька"; every language can name them.
- The building menu lists the Rune Altar under Crafting.
- Nine rune skill icons (fire, frost, holy, nature and friends) get the contributor's clearer artwork. The art ships as patch data and paints the loaded texture; the game's asset bundle bytes are untouched.- Old worlds keep their known Rune Altar. The saved station name is migrated when the character loads, so the altar does not need to be re-discovered.
- No other behavior changes; the embedded asset bundle is byte-for-byte the shipped one.

## 1.3.37

- Requires Skyheim Compat 1.1.29 (fixes duplicate ObjectDB key when returning to the main menu after disconnect).
- No change to skyheim.dll behavior.

## 1.3.36

- Tested on Valheim 1.0.21 (Steam build 26249040).
- Requires Skyheim Compat 1.1.28 and BepInExPack Valheim 5.4.2360.
- No change to skyheim.dll behavior.

## 1.3.35

- Requires Skyheim Compat 1.1.27. Compat has no dependencies of its own.
- No change to skyheim.dll behavior. The version moved so this package depends on the Compat build that shows rune cooldown timers on the vanilla hotbar.

## 1.3.34

- Requires Skyheim Compat 1.1.26. Compat has no dependencies of its own.
- No change to skyheim.dll behavior. The version moved so this package depends on the Compat build that turns Skyheim spell sounds down to half volume.

## 1.3.33

- Requires Skyheim Compat 1.1.25. Compat has no dependencies of its own.
- No change to skyheim.dll behavior. The version moved so this package depends on the Compat build that sends Recall to the bed and keeps the selected language.

## 1.3.32

- Requires Skyheim Compat 1.1.24. Compat has no dependencies of its own.
- No change to skyheim.dll behavior. The version moved so this package depends on the Compat build that shows the Rune Altar shard cost, chest count, and wait time.

## 1.3.31

- Requires Skyheim Compat 1.1.19. Compat has no dependencies of its own.
- No change to skyheim.dll behavior. The version moved so this package depends on the Compat build that heals the caster when the healing area misses them.

## 1.3.30

- Requires Skyheim Compat 1.1.18. Compat has no dependencies of its own.
- No change to skyheim.dll behavior. The version moved so this package depends on the Compat build with the healing rune, immolation, and altar imbue changes.

## 1.3.29

- Requires Skyheim Compat 1.1.15. Compat has no dependencies of its own.
- No change to skyheim.dll behavior. The version moved so this package depends on the Compat build that shows rune cooldown timers.

## 1.3.28

- Requires Skyheim Compat 1.1.13. Compat has no dependencies of its own.
- Status runes spend their eitr cost when the cast is accepted, so Warmth and the other buff runes can no longer be clicked for free skill experience.
- Rune magic skills have Skill Experience Gain Factor (default 1) and Skill Experience Loss (default 0).

## 1.3.27

- Keep the merged DLL's assembly name as `skyheim`, so the asset bundle can attach SkyheimAltarPanel, recipes, status effects, and the other scripts. The log reports Skyheim 1.3.27 instead of 1.3.12.

## 1.3.26

- Bosses drop `eitr_shard_drop` in the configured stack (Eikthyr 5, and the rest of `cjayride.SkyheimEitr.cfg`). The stack is written onto the loot list the ragdoll saves, so boss loot chests receive the full amount instead of Skyheim's built-in 1.

## 1.3.25

- Boss eitr shards stack to 50. Per-player amounts are in `cjayride.SkyheimEitr.cfg` (built into `skyheim.dll`). Dedicated server sends those numbers to clients.

## 1.3.24

- Requires Skyheim Compat 1.1.10 (guards Skyheim's Character.Damage windfury hook so poison/AoE hits no longer NRE).

## 1.3.23

- Requires Skyheim Compat 1.1.9.
- Thunderstore package uses the 1.0 FIX icon.

## 1.3.22

- Requires Skyheim Compat 1.1.8 (SeneaL UI HUD plus rune cooldown overlay).

## 1.3.21

- Valheim 1.0 / InventoryElement rewrite of skyheim.dll.
- Localization, craft crash fix, and rune config live in the Compat companion.
