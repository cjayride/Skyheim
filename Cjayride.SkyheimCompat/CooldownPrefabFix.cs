using HarmonyLib;
using skyheim;
using UnityEngine;
using UnityEngine.UI;

namespace Cjayride.SkyheimCompat
{
    internal static class CooldownPrefabFix
    {
        internal static void Replace()
        {
            if (SkyheimCooldown.Instance == null)
            {
                return;
            }

            GameObject prefab = Build();
            AccessTools.Field(typeof(SkyheimCooldown), "_cooldownPrefab")
                ?.SetValue(SkyheimCooldown.Instance, prefab);
            Plugin.Log("Replaced Skyheim item_cooldown prefab with a runtime overlay (fixes missing SkyheimCooldownItem).");
        }

        static GameObject Build()
        {
            GameObject root = new GameObject("item_cooldown", typeof(RectTransform));
            Object.DontDestroyOnLoad(root);
            root.SetActive(false);

            RectTransform rootRt = root.GetComponent<RectTransform>();
            Stretch(rootRt);

            GameObject swipeGo = new GameObject("swipe", typeof(RectTransform), typeof(Image));
            swipeGo.transform.SetParent(root.transform, false);
            Stretch(swipeGo.transform as RectTransform);
            Image swipe = swipeGo.GetComponent<Image>();
            swipe.color = new Color(0f, 0f, 0f, 0.55f);
            swipe.raycastTarget = false;
            swipe.sprite = WhiteSprite();
            swipe.type = Image.Type.Filled;
            swipe.fillMethod = Image.FillMethod.Radial360;
            swipe.fillOrigin = (int)Image.Origin360.Top;
            swipe.fillClockwise = false;
            swipe.fillAmount = 0f;

            GameObject textGo = new GameObject("text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(root.transform, false);
            Stretch(textGo.transform as RectTransform);
            Text text = textGo.GetComponent<Text>();
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 14;
            text.color = Color.white;
            text.raycastTarget = false;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.text = "";

            SkyheimCooldownItem item = root.AddComponent<SkyheimCooldownItem>();
            AccessTools.Field(typeof(SkyheimCooldownItem), "_text")?.SetValue(item, text);
            AccessTools.Field(typeof(SkyheimCooldownItem), "_swipe")?.SetValue(item, swipe);
            return root;
        }

        static void Stretch(RectTransform rt)
        {
            if (!rt)
            {
                return;
            }

            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;
        }

        static Sprite WhiteSprite()
        {
            Texture2D tex = Texture2D.whiteTexture;
            return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
