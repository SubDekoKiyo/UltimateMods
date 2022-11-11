using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System;
using static UltimateMods.ColorDictionary;
using static UnityEngine.UI.Button;
using Object = UnityEngine.Object;
using UltimateMods.Utilities;
using static UltimateMods.Modules.Assets;

namespace UltimateMods.Patches
{
    public class MainMenuPatch
    {
        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
        public static class MainMenuObjects
        {
            private static GameObject ButtonTemplate;
            private static AnnouncementPopUp PopUp;
            private static GameObject ButtonDiscord;
            private static GameObject ButtonTwitter;
            private static GameObject TmpButton;

            public static SpriteRenderer renderer;
            public static Sprite bannerSprite;
            public static Sprite horseBannerSprite;
            private static PingTracker instance;

            private static void Prefix(MainMenuManager __instance)
            {
                // CustomHatLoader.LaunchHatFetcher();
                TmpButton = GameObject.Find("ExitGameButton");
                // TmpButton.gameObject.SetActive(false);
                if (TmpButton == null) return;

                ButtonDiscord = UnityEngine.Object.Instantiate(TmpButton, null);

                var TextDiscord = ButtonDiscord.transform.GetChild(0).GetComponent<TMPro.TMP_Text>();
                __instance.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) =>
                {
                    TextDiscord.SetText("Discord");
                })));

                PassiveButton PassiveButtonDiscord = ButtonDiscord.GetComponent<PassiveButton>();
                SpriteRenderer ButtonSpriteDiscord = ButtonDiscord.GetComponent<SpriteRenderer>();

                PassiveButtonDiscord.OnClick = new Button.ButtonClickedEvent();
                PassiveButtonDiscord.OnClick.AddListener((UnityEngine.Events.UnityAction)delegate
                {
                    Application.OpenURL("https://discord.gg/kZwzNn9qRg");
                });

                ButtonSpriteDiscord.color = TextDiscord.color = DiscordPurple;
                PassiveButtonDiscord.OnMouseOut.AddListener((UnityEngine.Events.UnityAction)delegate
                {
                    ButtonSpriteDiscord.color = TextDiscord.color = DiscordPurple;
                });

                ButtonTwitter = UnityEngine.Object.Instantiate(TmpButton, null);

                var TextTwitter = ButtonTwitter.transform.GetChild(0).GetComponent<TMPro.TMP_Text>();
                __instance.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) =>
                {
                    TextTwitter.SetText("Twitter");
                })));

                PassiveButton PassiveButtonTwitter = ButtonTwitter.GetComponent<PassiveButton>();
                SpriteRenderer ButtonSpriteTwitter = ButtonTwitter.GetComponent<SpriteRenderer>();

                PassiveButtonTwitter.OnClick = new Button.ButtonClickedEvent();
                PassiveButtonTwitter.OnClick.AddListener((UnityEngine.Events.UnityAction)delegate
                {
                    Application.OpenURL("https://twitter.com/DekoKiyomori");
                });

                ButtonSpriteTwitter.color = TextTwitter.color = TwitterLightBlue;
                PassiveButtonTwitter.OnMouseOut.AddListener((UnityEngine.Events.UnityAction)delegate
                {
                    ButtonSpriteTwitter.color = TextTwitter.color = TwitterLightBlue;
                });

                SetButtonPosition();

                // UM credits button
                ButtonTemplate = GameObject.Find("InventoryButton");
                if (ButtonTemplate == null) return;
                var creditsButton = Object.Instantiate(ButtonTemplate, ButtonTemplate.transform.parent);
                var passiveCreditsButton = creditsButton.GetComponent<PassiveButton>();
                var spriteCreditsButton = creditsButton.GetComponent<SpriteRenderer>();

                spriteCreditsButton.sprite = Helpers.LoadSpriteFromTexture2D(CreditsButton, 75f);

                passiveCreditsButton.OnClick = new ButtonClickedEvent();

                passiveCreditsButton.OnClick.AddListener((System.Action)delegate
                {
                    // do stuff
                    if (PopUp != null) Object.Destroy(PopUp);
                    PopUp = Object.Instantiate(Object.FindObjectOfType<AnnouncementPopUp>(true));
                    PopUp.gameObject.SetActive(true);
                    PopUp.Init();
                    // SelectableHyperLinkHelper.DestroyGOs(PopUp.selectableHyperLinks, "test");
                    string creditsString = ModTranslation.getString("DevName");
                    creditsString += ModTranslation.getString("CreditsText");
                    PopUp.AnnounceTextMeshPro.text = creditsString;
                    __instance.StartCoroutine(Effects.Lerp(0.01f, new Action<float>((p) =>
                    {
                        if (p == 1)
                        {
                            var titleText = GameObject.Find("Title_Text").GetComponent<TMPro.TextMeshPro>();
                            if (titleText != null) titleText.text = ModTranslation.getString("CreditsTitle");
                        }
                    })));
                });
            }

            public static void Postfix(MainMenuManager __instance)
            {
                __instance.StartCoroutine(Effects.Lerp(0.01f, new Action<float>((p) =>
                {
                    if (p == 1)
                    {
                        ButtonTemplate = GameObject.Find("InventoryButton");
                        foreach (Transform tf in ButtonTemplate.transform.parent.GetComponentsInChildren<Transform>())
                        {
                            tf.localPosition = new Vector2(tf.localPosition.x * 0.8f, tf.localPosition.y);
                        }
                    }
                })));
            }

            public static void SetButtonPosition()
            {
                if (ButtonDiscord != null)
                    ButtonDiscord.transform.localPosition = new Vector3(TmpButton.transform.localPosition.x, TmpButton.transform.localPosition.y + 0.6f, TmpButton.transform.localPosition.z);
                if (ButtonTwitter != null)
                    ButtonTwitter.transform.localPosition = new Vector3(TmpButton.transform.localPosition.x, TmpButton.transform.localPosition.y + 1.2f, TmpButton.transform.localPosition.z);
            }

            public static void Postfix(PingTracker __instance)
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
                LoadSprites();
                renderer.sprite = Helpers.LoadSpriteFromTexture2D(NormalBanner, 200f);

                instance = __instance;
                LoadSprites();
                renderer.sprite = MapOptions.enableHorseMode ? horseBannerSprite : bannerSprite;
            }

            public static void LoadSprites()
            {
                if (bannerSprite == null) bannerSprite = Helpers.LoadSpriteFromTexture2D(NormalBanner, 200f);
                if (horseBannerSprite == null) horseBannerSprite = Helpers.LoadSpriteFromTexture2D(HorseBanner, 200f);
            }

            public static void UpdateSprite()
            {
                LoadSprites();
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

        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.LateUpdate))]
        public static class ObjectPositionFixer
        {
            public static void Prefix()
            {
                MainMenuObjects.SetButtonPosition();
            }
        }
    }
}