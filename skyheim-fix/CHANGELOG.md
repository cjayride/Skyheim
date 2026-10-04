# Changelog

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
