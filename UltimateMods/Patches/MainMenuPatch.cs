using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System;
using static UltimateMods.ColorDictionary;
using static UnityEngine.UI.Button;
using Object = UnityEngine.Object;

namespace UltimateMods.Patches
{
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public class MainMenuPatch
    {
        private static GameObject bottomTemplate;
        private static AnnouncementPopUp popUp;
        private static void Prefix(MainMenuManager __instance)
        {
            // CustomHatLoader.LaunchHatFetcher();
            var template = GameObject.Find("ExitGameButton");
            // template.gameObject.SetActive(false);
            if (template == null) return;

            var ButtonDiscord = UnityEngine.Object.Instantiate(template, null);
            ButtonDiscord.transform.localPosition = new Vector3(ButtonDiscord.transform.localPosition.x, ButtonDiscord.transform.localPosition.y + 0.6f, ButtonDiscord.transform.localPosition.z);

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

            var ButtonGithub = UnityEngine.Object.Instantiate(template, null);
            ButtonGithub.transform.localPosition = new Vector3(ButtonGithub.transform.localPosition.x, ButtonGithub.transform.localPosition.y + 1.2f, ButtonGithub.transform.localPosition.z);

            var TextGithub = ButtonGithub.transform.GetChild(0).GetComponent<TMPro.TMP_Text>();
            __instance.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) =>
            {
                TextGithub.SetText("Github");
            })));

            PassiveButton PassiveButtonGithub = ButtonGithub.GetComponent<PassiveButton>();
            SpriteRenderer ButtonSpriteGithub = ButtonGithub.GetComponent<SpriteRenderer>();

            PassiveButtonGithub.OnClick = new Button.ButtonClickedEvent();
            PassiveButtonGithub.OnClick.AddListener((UnityEngine.Events.UnityAction)delegate
            {
                Application.OpenURL("https://github.com/Dekokiyo/UltimateMods");
            });

            ButtonSpriteGithub.color = TextGithub.color = GithubGray;
            PassiveButtonGithub.OnMouseOut.AddListener((UnityEngine.Events.UnityAction)delegate
            {
                ButtonSpriteGithub.color = TextGithub.color = GithubGray;
            });

            // UM credits button
            bottomTemplate = GameObject.Find("InventoryButton");
            if (bottomTemplate == null) return;
            var creditsButton = Object.Instantiate(bottomTemplate, bottomTemplate.transform.parent);
            var passiveCreditsButton = creditsButton.GetComponent<PassiveButton>();
            var spriteCreditsButton = creditsButton.GetComponent<SpriteRenderer>();

            spriteCreditsButton.sprite = Helpers.LoadSpriteFromResources("UltimateMods.Resources.CreditsButton.png", 75f);

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
                    bottomTemplate = GameObject.Find("InventoryButton");
                    foreach (Transform tf in bottomTemplate.transform.parent.GetComponentsInChildren<Transform>())
                    {
                        tf.localPosition = new Vector2(tf.localPosition.x * 0.8f, tf.localPosition.y);
                    }
                }
            })));
        }
    }
}