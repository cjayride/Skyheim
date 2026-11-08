# Changelog

## 1.2.6

- Craft panel title for the Rune Altar uses the translation file (Ukrainian/Polish/English). Station id stays `Rune Altar` for recipes.

## 1.2.5

- Fix Harmony patch on `VisEquipment.AttachBackItem` (correct parameter name `hash`) so Compat loads and hidden-rune VFX config works.

## 1.2.4

- Craft tab and hammer build UI translate **Rune Altar** via localization while `CraftingStation.m_name` stays the literal `Rune Altar` for WackyDB and recipe station checks.

## 1.2.3

- Config **Runes → Show Vfx When Weapon Hidden** (default **off**). Stops rune glow/particles on your back when you hide a rune with R; hand-held VFX unchanged. Set to true to restore the old shoulder/back VFX.

## 1.2.2

- Rune Altar name uses the translation file in the hammer menu and on world hover (English / Polish / Ukrainian).
- The altar is listed under the Misc build tab. It is not tagged as a vanilla Crafting-usage piece, so clicking it still opens the imbue chest.

## 1.2.1

- Windfury proc swirls and hit sound play on female player models as well as male. The original effect list was male-only (variant 0).

## 1.2.0

- Release line for altar/imbue restoration, Ukrainian + loose `skyheim.json`, SeneaL UI, Epic Loot imbue, and healing rune balance (Nature = amount, quality = duration) with full tooltip and predicted heal line.
- Install together with Skyheim Fix 1.4.0.

## 1.1.36

- Rune of Healing item tooltip again lists the 10-yard ally range and appends \"Heals for X over Y\" from your Nature Magic and rune quality.

## 1.1.35

- Rune of Healing: Nature Magic increases total HP healed (+1% per skill point). Rune quality lengthens the HoT (8s at quality 1, +2s per quality by default). Tooltip updated in English, Polish, and Ukrainian.

## 1.1.34

- Rune of Healing tooltip (EN/PL/UA): Nature Magic increases total healing, not duration, matching Compat HoT balance.

## 1.1.33

- Epic Loot enchanted and legendary set pieces can be Eitr-imbued. Enchanting is not the same as Skyheim imbue; the altar no longer requires the item to still look like an unenchanted vanilla prefab.
- Any helmet, chest, legs, or cape that is not already Eitr-imbued can be imbued (config: Altar imbue / Any Armor). Named Skyheim recipes still win when the base prefab is known.

## 1.1.32

- SeneaL's chest subtitle is rewritten every tick with the shard cost (it was immediately overwritten by "2 slots, used N").
- Imbue starts when you close the Runic Chest under SeneaL UI. SeneaL never marks the container in-use, so Skyheim never saw a close before.
- Closing the chest is still required to start. The subtitle says how many Eitr Shards that armor needs.

## 1.1.31

- Rune recipes require the Rune Altar crafting station (craft/upgrade only while standing at it). The altar click still opens the imbue chest, not the vanilla Craft list.
- The Runic Chest world hover and imbue panel title show the localized chest name plus the altar directions.
- English and Polish altar descriptions explain imbue-in-chest and craft-at-altar. Ukrainian text was already filled in.
- Eitr Shard counts in the altar chest match by prefab or item name, so imbue still starts when dropPrefab is missing.

## 1.1.30

- Ukrainian! A community contribution adds a Ukrainian translation and fills gaps in English and Polish: 78 new strings in all three languages. Run the game in Ukrainian and Skyheim speaks it.
- A skyheim.json placed next to skyheim.dll now overrides the built-in translations. Edit words or add a language without touching the mod. The file inside the mod stays as the fallback.
- If the game language has no block in the translation file, English words stay on screen (the file gets a warning in the log) instead of blank names.
- Hotbar and inventory rune cooldowns stop searching the UI tree every frame. The overlay is found once per slot; the attack path caches its rune lookup. Less UI lag on rigs that felt the old per-slot scan.

## 1.1.29

- Returning to the character select after leaving a server no longer throws a duplicate ObjectDB key for Slime Encrusted Bone (`bonemass_drop`). Skyheim LoadItems is skipped when that item is already registered. Recipes and status effects are not re-run from FejdStartup, so they are not guarded here.

## 1.1.28

- Tested on Valheim 1.0.21 (Steam build 26249040). No behavior changes.

## 1.1.27

- Rune cooldown timers show on the vanilla hotbar and inventory. They already showed on SeneaL slots.

## 1.1.26

- Each Skyheim spell sound has a volume slider under Audio. The default is half volume (0.5). Immolation still plays one explosion per tick.

## 1.1.25

- Rune of Recall uses the bed after the world has autosaved. It was taking the logout point from that save.
- Skyheim names are applied when the language loads. Later item wakes no longer write the English text back over that language.

## 1.1.24

- The Rune Altar status line no longer checks every altar every frame. It updates when the chest contents change, when the timer second changes, or once when an imbue finishes.

## 1.1.23

- After an imbue finishes, the Rune Altar says "Finished adding Eitr" while that piece is still in the chest. Opening it later with an imbued piece says "Item already imbued".

## 1.1.22

- The SeneaL chest subtitle for the Rune Altar is short enough to fit. A Fenris Coat with 12 shards reads "Need 35, have 12, 5 min".

## 1.1.21

- With SeneaL UI, the Rune Altar shard cost, how many shards are in the chest, and the wait time show on the chest subtitle. SeneaL hides Skyheim's own altar text.

## 1.1.20

- The Rune Altar status line says how many Eitr Shards the armor needs, how many are in the chest, and how long the imbue takes. The timer still starts only when the full amount is there and the chest is closed.

## 1.1.19

- If the Rune of Healing area misses its caster, the caster still receives the heal. A hit that already lands is left alone, and allies are unchanged.

## 1.1.18

- Rune of Healing is a configurable heal-over-time. Default is 90% of Skyheim's amount (about 36 HP at quality 1 before Elemental Magic skill), spread over 8 seconds, with the status icon held for at least 10 seconds.
- Heal Radius and Warmth Radius. Skyheim's areas are 5 m and 30 m. Warmth is an area by design.
- Optional Debug Logging for a Rune of Healing cast. It prints whether logging is on and which config file was read.
- Immolation aura hits do not build Epic Loot Eikthyr Shocking Charge. The aura explosion plays once per tick, with Aura Audio Volume Scale.
- The Rune Altar can eitr-imbue armor that Epic Loot has already enchanted. Extra Recipes adds the OdinHorse cloth sets and the Horse Trophy Helmet.

## 1.1.15

- Rune cooldown timers draw on the hotbar and in the inventory. The overlay was being created switched off, so the seconds never appeared.
- Destroyed cooldown overlays are removed before Skyheim updates them, which stops the fillAmount null-reference spam.

## 1.1.13

- Rune magic skills (Nature, Holy, Fire, Frost) have Skill Experience Gain Factor and Skill Experience Loss, with the same defaults as Blacksmithing (1 and 0).

## 1.1.12

- Skyheim buff runes spend their eitr cost when the cast is accepted. Warmth and the other status runes were checking the cost and then never taking it, so they could be clicked forever and the magic skill leveled to the cap.

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
