using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace Cjayride.SkyheimEitr
{
    /// <summary>
    /// Skill-screen icons are SkillDef.m_icon sprites. Painting the packed atlas can miss
    /// those if Unity already uploaded the atlas, so the four PNGs are also assigned
    /// directly onto the Nature/Holy/Fire/Frost skill defs.
    /// </summary>
    [HarmonyPatch(typeof(Skills), nameof(Skills.GetSkillDef))]
    internal static class SkillIconSprites
    {
        static readonly string[] Names =
        {
            "skill_nature_magic.png",
            "skill_holy_magic.png",
            "skill_fire_magic.png",
            "skill_frost_magic.png"
        };

        static Sprite[] _sprites;
        static bool _logged;

        internal static bool Ready => _sprites != null;

        internal static void Apply()
        {
            if (_sprites != null)
            {
                return;
            }

            Sprite[] loaded = new Sprite[4];
            for (int i = 0; i < Names.Length; i++)
            {
                loaded[i] = LoadPng(Names[i]);
                if (loaded[i] == null)
                {
                    Plugin.Log("Skill icon PNG missing: " + Names[i]);
                    return;
                }
            }

            _sprites = loaded;
            Plugin.Log("Skill icon PNGs loaded for Nature/Holy/Fire/Frost.");
        }

        static void Postfix(Skills.SkillType type, Skills.SkillDef __result)
        {
            if (__result == null)
            {
                return;
            }

            Apply();
            int index = (int)type - 200;
            if (_sprites == null || index < 0 || index > 3 || _sprites[index] == null)
            {
                return;
            }

            __result.m_icon = _sprites[index];
            if (!_logged)
            {
                _logged = true;
                Plugin.Log("Assigned new PNGs to Skyheim magic skill icons.");
            }
        }

        static Sprite LoadPng(string resourceName)
        {
            try
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        return null;
                    }

                    byte[] bytes = new byte[stream.Length];
                    int read = 0;
                    while (read < bytes.Length)
                    {
                        int n = stream.Read(bytes, read, bytes.Length - read);
                        if (n <= 0)
                        {
                            return null;
                        }

                        read += n;
                    }

                    Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    MethodInfo load = AccessTools.Method(
                        AccessTools.TypeByName("UnityEngine.ImageConversion"),
                        "LoadImage",
                        new[] { typeof(Texture2D), typeof(byte[]), typeof(bool) });
                    if (load == null || !(bool)load.Invoke(null, new object[] { texture, bytes, false }))
                    {
                        return null;
                    }

                    texture.name = resourceName;
                    texture.filterMode = FilterMode.Bilinear;
                    texture.wrapMode = TextureWrapMode.Clamp;
                    return Sprite.Create(
                        texture,
                        new Rect(0f, 0f, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f),
                        100f);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log("Skill icon PNG failed (" + resourceName + "): " + ex.Message);
                return null;
            }
        }
    }
}
