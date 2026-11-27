# Skyheim — Thunderstore versions

**Shipping:**
- Compat **1.2.8** — imbued capes/backpacks/SeneaL extra cape cells apply max eitr + regen (`CapeImbueStats`). Zip: `D:\AI-Agent-Projects\cjaycraft\Cjayride.Skyheim\dist\upload\SkyheimCompat-1.2.8.zip`
- Fix **1.4.6** — depends on Compat 1.2.8; same `skyheim.dll` as 1.4.5. Zip: `D:\AI-Agent-Projects\cjaycraft\Cjayride.Skyheim\dist\upload\SkyheimFix-1.4.6.zip`

Cause: `SkyheimArmor.Player_SetMaxEitr_Prefix` and regen postfix only sum `m_chestItem`, `m_helmetItem`, `m_legItem`. Capes are `m_shoulderItem`.