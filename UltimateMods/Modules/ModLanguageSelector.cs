using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Events;
using UltimateMods.Patches;
using UltimateMods.Utilities;
using UnityEngine.UI;
using static UnityEngine.UI.Button;
using Object = UnityEngine.Object;

namespace UltimateMods.Modules
{
    public enum SupportedLang
    {
        Japanese = 0,
        English = 1,
        SChinese = 2,
        Indonesia = 3,
    }

    [HarmonyPatch]
    public static class ModLanguageSelector
    {
        private static ToggleButtonBehaviour LangOption;
        public static ToggleButtonBehaviour ButtonPrefab;
        public static int LanguageNum;
        public static string Language;

        public static void Initialize()
        {
            LanguageNum = UltimateModsPlugin.LanguageNum.Value;
            Language = ModTranslation.getString("AllLanguage");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
        public static void OptionsMenuBehaviour_StartPostfix(OptionsMenuBehaviour __instance)
        {
            if (!__instance.CensorChatButton) return;

            if (!ButtonPrefab)
            {
                ButtonPrefab = Object.Instantiate(__instance.CensorChatButton);
                Object.DontDestroyOnLoad(ButtonPrefab);
                ButtonPrefab.name = "CensorChatPrefab";
                ButtonPrefab.gameObject.SetActive(false);
            }

            InitializeLangButton(__instance);
        }

        private static void InitializeLangButton(OptionsMenuBehaviour __instance)
        {
            LangOption = Object.Instantiate(ButtonPrefab, __instance.CensorChatButton.transform.parent);
            LangOption.transform.localPosition = __instance.CensorChatButton.transform.localPosition + Vector3.down * 0.5f + Vector3.right * 1.35f;

            LangOption.gameObject.SetActive(true);
            LangOption.Text.text = String.Format(ModTranslation.getString("Language"), Language);
            var LangOptionButton = LangOption.GetComponent<PassiveButton>();
            LangOptionButton.OnClick = new ButtonClickedEvent();
            LangOptionButton.OnClick.AddListener((Action)(() =>
            {
                ChangeNextLang();
            }));
        }

        private static void ChangeNextLang()
        {
            if (LanguageNum == (int)SupportedLang.Indonesia)
            {
                LanguageNum = UltimateModsPlugin.LanguageNum.Value = (int)SupportedLang.Japanese;
            }
            else
            {
                LanguageNum++;
            }
            ClientOptionsPatch.updateTranslations();
            VanillaOptionsPatch.updateTranslations();
            Language = ModTranslation.getString("AllLanguage");
            LangOption.Text.text = String.Format(ModTranslation.getString("Language"), Language);
            UltimateModsPlugin.Logger.LogInfo("Changed Language");
            UltimateModsPlugin.LanguageNum.Value++;
        }
    }
}