using HarmonyLib;
using System;
using UnityEngine;
using UltimateMods.Utilities;
using static UltimateMods.Modules.Assets;

namespace UltimateMods.Patches
{
    [HarmonyPatch]
    public static class CredentialsPatch
    {
        public static string baseCredentials;

        [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
        private static class VersionShowerPatch
        {
            static void Postfix(VersionShower __instance)
            {
                var amongUsLogo = GameObject.Find("bannerLogo_AmongUs");
                if (amongUsLogo == null) return;

                var version = UnityEngine.Object.Instantiate<TMPro.TextMeshPro>(__instance.text);
                version.transform.position = new Vector3(0, -0.25f, 0);
                if (UltimateModsPlugin.isBeta)
                    version.SetText(string.Format(ModTranslation.getString("CreditsBetaVersion"), UltimateModsPlugin.Version.ToString()));
                else
                    version.SetText(string.Format(ModTranslation.getString("CreditsVersion"), UltimateModsPlugin.Version.ToString()));

                version.transform.SetParent(amongUsLogo.transform);

                if (UltimateModsPlugin.isBeta)
                {
                    baseCredentials = $@"<size=130%><color=#0094ff>Ultimate Mods</color></size> Ver.Beta{UltimateModsPlugin.Version.ToString()}";
                    __instance.text.text += $"\n<color=#0094ff>UltimateMods</color> BuildNum: " + ModTranslation.getString("BuildNum");
                }
                else
                    baseCredentials = $@"<size=130%><color=#0094ff>Ultimate Mods</color></size> Ver.{UltimateModsPlugin.Version.ToString()}";

                __instance.transform.localPosition = new Vector3(__instance.transform.localPosition.x, __instance.transform.localPosition.y - 0.1f, __instance.transform.localPosition.z);
            }
        }

        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        internal static class PingTrackerPatch
        {
            static void Postfix(PingTracker __instance)
            {
                __instance.text.alignment = TMPro.TextAlignmentOptions.TopRight;
                if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
                {
                    if (UltimateModsPlugin.DebugMode.Value && AmongUsClient.Instance.AmHost)
                        __instance.text.text = $"{baseCredentials}\n" + ModTranslation.getString("Position") + PlayerControl.LocalPlayer.GetTruePosition().ToString() + $"\n{__instance.text.text}";
                    else
                        __instance.text.text = $"{baseCredentials}\n{__instance.text.text}";

                    if (PlayerControl.LocalPlayer.Data.IsDead)
                        __instance.gameObject.GetComponent<AspectPosition>().DistanceFromEdge = new Vector3(2.0f, 0.1f, 0.5f);
                    else
                        __instance.gameObject.GetComponent<AspectPosition>().DistanceFromEdge = new Vector3(1.2f, 0.1f, 0.5f);
                }
                else
                {
                    __instance.text.text = $"{baseCredentials}\n{__instance.text.text}";
                    __instance.gameObject.GetComponent<AspectPosition>().DistanceFromEdge = new Vector3(2.8f, 0.1f, 0.5f);
                }
            }
        }

        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
        public static class MainMenuButtonPatch
        {
            public static SpriteRenderer renderer;
            public static Sprite bannerSprite;
            public static Sprite horseBannerSprite;
            private static PingTracker instance;
            static void Postfix(PingTracker __instance)
            {
                FastDestroyableSingleton<ModManager>.Instance.ShowModStamp();

                var amongUsLogo = GameObject.Find("bannerLogo_AmongUs");
                if (amongUsLogo != null)
                {
                    amongUsLogo.transform.localScale *= 0.6f;
                    amongUsLogo.transform.position += Vector3.up * 0.25f;
                }

                var umLogo = new GameObject("bannerLogo_UM");
                umLogo.transform.position = Vector3.up;
                renderer = umLogo.AddComponent<SpriteRenderer>();
                loadSprites();
                renderer.sprite = Helpers.LoadSpriteFromTexture2D(NormalBanner, 200f);

                instance = __instance;
                loadSprites();
                renderer.sprite = MapOptions.enableHorseMode ? horseBannerSprite : bannerSprite;
            }

            public static void loadSprites()
            {
                if (bannerSprite == null) bannerSprite = Helpers.LoadSpriteFromTexture2D(NormalBanner, 200f);
                if (horseBannerSprite == null) horseBannerSprite = Helpers.LoadSpriteFromTexture2D(HorseBanner, 200f);
            }

            public static void updateSprite()
            {
                loadSprites();
                if (renderer != null)
                {
                    float fadeDuration = 1f;
                    instance.StartCoroutine(Effects.Lerp(fadeDuration, new Action<float>((p) =>
                    {
                        renderer.color = new Color(1, 1, 1, 1 - p);
                        if (p == 1)
                        {
                            renderer.sprite = MapOptions.enableHorseMode ? horseBannerSprite : bannerSprite;
                            instance.StartCoroutine(Effects.Lerp(fadeDuration, new Action<float>((p) =>
                            {
                                renderer.color = new Color(1, 1, 1, p);
                            })));
                        }
                    })));
                }
            }
        }
    }
}