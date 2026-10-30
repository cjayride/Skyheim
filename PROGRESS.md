# Skyheim Compat 1.1.29 — ObjectDB duplicate key on menu return

## Goal
- Stop ArgumentException (duplicate ObjectDB key -1871759496 / bonemass_drop) when returning to character select after a multiplayer disconnect.
- Guard AssetManager.LoadItems in Compat; do not rewrite skyheim.dll.
- First cold load must still register items.

## Acceptance
- Harmony prefix skips LoadItems when ObjectDB already has prefab `bonemass_drop`.
- FejdStartup postfix only calls LoadItems; recipes/SE are not re-run there.
- Compat 1.1.29 built and zipped. SkyheimFix version stays 1.3.36 (dependency string updated).

## Status
- Completed: compile, pack, modpack dependency bump.
- Not done in this session: join/disconnect in-game check.

## Files changed
- `Cjayride.SkyheimCompat/AssetManagerLoadItemsGuard.cs` — skip + log when sentinel exists.
- Version 1.1.29: Plugin.cs, csproj, skyheim-patch manifest/README/CHANGELOG, pack.ps1, root README.
- `skyheim-fix/manifest.json` (and README/CHANGELOG) Compat dependency 1.1.29; Fix version unchanged.
- `beta_cjaycraft/manifest.json` and `beta_cjaycraft-server/manifest.json` dependency bump (server already listed Compat).
- BepInEx HintPath fallback to `D:\AI-Agent-Projects\Mods\libs` (game install has no BepInEx\core).

## Commands
- `dotnet build Cjayride.SkyheimCompat -c Release` — succeeded, 0 errors.
- `pack.ps1` — ILRepack + zips.

## Output
- `D:\AI-Agent-Projects\cjaycraft\Cjayride.Skyheim\dist\compat\Cjayride.SkyheimCompat.dll` (FileVersion 1.1.29.0)
- `D:\AI-Agent-Projects\cjaycraft\Cjayride.Skyheim\dist\repack\skyheim.dll` (Fix 1.3.36, unchanged version)
- `D:\AI-Agent-Projects\cjaycraft\Cjayride.Skyheim\dist\upload\SkyheimCompat-1.1.29.zip`
- `D:\AI-Agent-Projects\cjaycraft\Cjayride.Skyheim\dist\upload\SkyheimFix-1.3.36.zip`

## Next steps
1. Install Compat 1.1.29 and join/disconnect to confirm no duplicate-key exception.
2. Confirm cold start still registers Skyheim items.
