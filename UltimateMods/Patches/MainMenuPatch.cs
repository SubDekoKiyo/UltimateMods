namespace UltimateMods.Patches
{
    public class MainMenuPatch
    {
        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
        public static class MainMenuObjects
        {
            private static GameObject buttonTemplate;
            private static AnnouncementPopUp popUp;
            private static GameObject buttonDiscord;
            private static GameObject buttonTwitter;
            private static GameObject tmpButton;

            public static SpriteRenderer renderer;
            public static Sprite bannerSprite;
            public static Sprite horseBannerSprite;
            private static PingTracker instance;

            private static void Prefix(MainMenuManager __instance)
            {
                // CustomHatLoader.LaunchHatFetcher();
                tmpButton = GameObject.Find("ExitGameButton");
                // tmpButton.gameObject.SetActive(false);
                if (tmpButton == null) return;

                buttonDiscord = UnityEngine.Object.Instantiate(tmpButton, null);

                var TextDiscord = buttonDiscord.transform.GetChild(0).GetComponent<TMP_Text>();
                __instance.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) =>
                {
                    TextDiscord.SetText("Discord");
                })));

                PassiveButton PassiveButtonDiscord = buttonDiscord.GetComponent<PassiveButton>();
                SpriteRenderer buttonSpriteDiscord = buttonDiscord.GetComponent<SpriteRenderer>();

                PassiveButtonDiscord.OnClick = new ButtonClickedEvent();
                PassiveButtonDiscord.OnClick.AddListener((UnityAction)delegate
                {
                    Application.OpenURL("https://discord.gg/kZwzNn9qRg");
                });

                buttonSpriteDiscord.color = TextDiscord.color = DiscordPurple;
                PassiveButtonDiscord.OnMouseOut.AddListener((UnityAction)delegate
                {
                    buttonSpriteDiscord.color = TextDiscord.color = DiscordPurple;
                });

                buttonTwitter = UnityEngine.Object.Instantiate(tmpButton, null);

                var TextTwitter = buttonTwitter.transform.GetChild(0).GetComponent<TMP_Text>();
                __instance.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) =>
                {
                    TextTwitter.SetText("Twitter");
                })));

                PassiveButton PassiveButtonTwitter = buttonTwitter.GetComponent<PassiveButton>();
                SpriteRenderer buttonSpriteTwitter = buttonTwitter.GetComponent<SpriteRenderer>();

                PassiveButtonTwitter.OnClick = new ButtonClickedEvent();
                PassiveButtonTwitter.OnClick.AddListener((UnityAction)delegate
                {
                    Application.OpenURL("https://twitter.com/DekoKiyomori");
                });

                buttonSpriteTwitter.color = TextTwitter.color = TwitterLightBlue;
                PassiveButtonTwitter.OnMouseOut.AddListener((UnityAction)delegate
                {
                    buttonSpriteTwitter.color = TextTwitter.color = TwitterLightBlue;
                });

                SetButtonPosition();

                // UM credits Button
                buttonTemplate = GameObject.Find("InventoryButton");
                if (buttonTemplate == null) return;
                var creditsButton = Object.Instantiate(buttonTemplate, buttonTemplate.transform.parent);
                var passiveCreditsButton = creditsButton.GetComponent<PassiveButton>();
                var spriteCreditsButton = creditsButton.GetComponent<SpriteRenderer>();

                spriteCreditsButton.sprite = Helpers.LoadSpriteFromTexture2D(CreditsButton, 75f);

                passiveCreditsButton.OnClick = new ButtonClickedEvent();

                passiveCreditsButton.OnClick.AddListener((System.Action)delegate
                {
                    // do stuff
                    if (popUp != null) Object.Destroy(popUp);
                    popUp = Object.Instantiate(Object.FindObjectOfType<AnnouncementPopUp>(true));
                    popUp.gameObject.SetActive(true);
                    popUp.Init();
                    // SelectableHyperLinkHelper.DestroyGOs(popUp.selectableHyperLinks, "test");
                    string creditsString = ModTranslation.getString("DevName");
                    creditsString += ModTranslation.getString("CreditsText");
                    popUp.AnnounceTextMeshPro.text = creditsString;
                    __instance.StartCoroutine(Effects.Lerp(0.01f, new Action<float>((p) =>
                    {
                        if (p == 1)
                        {
                            var titleText = GameObject.Find("Title_Text").GetComponent<TextMeshPro>();
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
                        buttonTemplate = GameObject.Find("InventoryButton");
                        foreach (Transform tf in buttonTemplate.transform.parent.GetComponentsInChildren<Transform>())
                        {
                            tf.localPosition = new Vector2(tf.localPosition.x * 0.8f, tf.localPosition.y);
                        }
                    }
                })));
            }

            public static void SetButtonPosition()
            {
                if (buttonDiscord != null)
                    buttonDiscord.transform.localPosition = new Vector3(tmpButton.transform.localPosition.x, tmpButton.transform.localPosition.y + 0.6f, tmpButton.transform.localPosition.z);
                if (buttonTwitter != null)
                    buttonTwitter.transform.localPosition = new Vector3(tmpButton.transform.localPosition.x, tmpButton.transform.localPosition.y + 1.2f, tmpButton.transform.localPosition.z);
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
                renderer.sprite = Options.enableHorseMode ? horseBannerSprite : bannerSprite;
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
                            renderer.sprite = Options.enableHorseMode ? horseBannerSprite : bannerSprite;
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