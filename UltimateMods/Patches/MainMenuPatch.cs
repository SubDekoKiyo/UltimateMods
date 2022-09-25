using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System;
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
            if (template == null) return;

            var buttonDiscord = UnityEngine.Object.Instantiate(template, null);
            buttonDiscord.transform.localPosition = new Vector3(buttonDiscord.transform.localPosition.x, buttonDiscord.transform.localPosition.y + 0.6f, buttonDiscord.transform.localPosition.z);

            var textDiscord = buttonDiscord.transform.GetChild(0).GetComponent<TMPro.TMP_Text>();
            __instance.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) =>
            {
                textDiscord.SetText("Discord");
            })));

            PassiveButton passiveButtonDiscord = buttonDiscord.GetComponent<PassiveButton>();
            SpriteRenderer buttonSpriteDiscord = buttonDiscord.GetComponent<SpriteRenderer>();

            passiveButtonDiscord.OnClick = new Button.ButtonClickedEvent();
            passiveButtonDiscord.OnClick.AddListener((UnityEngine.Events.UnityAction)delegate
            {
                Application.OpenURL("https://discord.gg/kZwzNn9qRg");
            });

            Color discordColor = new Color32(88, 101, 242, byte.MaxValue);
            buttonSpriteDiscord.color = textDiscord.color = discordColor;
            passiveButtonDiscord.OnMouseOut.AddListener((UnityEngine.Events.UnityAction)delegate
            {
                buttonSpriteDiscord.color = textDiscord.color = discordColor;
            });

            var buttonGithub = UnityEngine.Object.Instantiate(template, null);
            buttonGithub.transform.localPosition = new Vector3(buttonGithub.transform.localPosition.x, buttonGithub.transform.localPosition.y + 1.2f, buttonGithub.transform.localPosition.z);

            var textGithub = buttonGithub.transform.GetChild(0).GetComponent<TMPro.TMP_Text>();
            __instance.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) =>
            {
                textGithub.SetText("Github");
            })));

            PassiveButton passiveButtonGithub = buttonGithub.GetComponent<PassiveButton>();
            SpriteRenderer buttonSpriteGithub = buttonGithub.GetComponent<SpriteRenderer>();

            passiveButtonGithub.OnClick = new Button.ButtonClickedEvent();
            passiveButtonGithub.OnClick.AddListener((UnityEngine.Events.UnityAction)delegate
            {
                Application.OpenURL("https://github.com/Dekokiyo/TheOtherRolesGM-KiyoMugi-Edition");
            });

            Color githubColor = new Color32(186, 187, 189, byte.MaxValue);
            buttonSpriteGithub.color = textGithub.color = githubColor;
            passiveButtonGithub.OnMouseOut.AddListener((UnityEngine.Events.UnityAction)delegate
            {
                buttonSpriteGithub.color = textGithub.color = githubColor;
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