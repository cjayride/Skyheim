# 🌌 Skyheim 1.0 Fix + Compatibility

**Bring Skyheim back to life in Valheim 1.0!**

A community compatibility update by **cjayride** for [Skyheim by makail](https://thunderstore.io/c/valheim/p/makail/Skyheim/), featuring updated game API support, boss Eitr shard drops, and working rune cooldown timers.

## 📦 Included Packages

### 🛠️ Skyheim 1.0 Fix — v1.3.36

A rewritten `skyheim.dll` updated for Valheim 1.0.

- 🔧 Fixes plugin-loading crashes caused by outdated game API references.
- 💎 Restores configured Eitr shard drops from bosses.
- 🧩 Updates bundle script bindings for compatibility.
- ⚠️ Requires **Skyheim Compat v1.1.28** (tested on Valheim 1.0.21).

### ⏱️ Skyheim Compat — v1.1.28

A companion plugin that restores rune cooldown timers to the vanilla UI.

- 🗡️ Displays rune cooldowns on the hotbar.
- 🎒 Displays rune cooldowns in the inventory.

## 📥 Installation

1. Install **Skyheim 1.0 Fix**.
2. Install **Skyheim Compat**.
3. Confirm that both packages are enabled.

> ⚠️ **Important:** Do not enable the original `makail-Skyheim` package alongside these replacement packages.

### 📁 Release Archives

- `dist/upload/SkyheimFix-1.3.36.zip`
- `dist/upload/SkyheimCompat-1.1.28.zip`

---

## 🔬 Technical Details

### 🛠️ Valheim 1.0 Compatibility Rebuild

The original Skyheim v1.3.12 DLL was rebuilt as **Cjayride Skyheim v1.3.13** to address API changes in the current Valheim version.

### 🐛 Crash Fix

The original plugin could fail during startup with this error:

```text
TypeLoadException: Could not load type of field 'SkyheimAltarPanel:_leftSlot' (7)
due to: Expected reference type but got type kind 17
```

Valheim no longer provides `InventoryGrid.Element`. Inventory slots now use `InventoryElement`, a `MonoBehaviour`. The original DLL still referenced the obsolete nested type, preventing BepInEx from loading the plugin.

### 🔄 Additional API Updates

The compatibility rebuild updates several game API references:

- `EffectList.Create` — Adds the `ZDOID` argument (`ZDOID.None`).
- `SEMan.AddStatusEffect` — Adds the `Int16` variant argument (`0`).
- `Humanoid.IsTeleportable(bool)` — Updates the method signature.
- `Character.Message` — Adds the extra `bool` argument (`false`).
- `InventoryGrid.m_elements` — Uses `List<InventoryElement>`.

### 🧪 Manual Installation for Playtesting

To test the rebuilt DLL manually:

1. Replace `skyheim.dll` inside the original `makail-Skyheim` installation with `dist/skyheim.dll`.
2. Keep `skyheim.json` in the same directory as the DLL.

### ✨ Rune Cooldowns, Names & Descriptions

Install `Cjayride.SkyheimCompat.dll` as a separate BepInEx plugin for the compatibility features.

---

⚔️ **Keep your runes ready, your cooldowns visible, and your adventures going!**