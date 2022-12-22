namespace UltimateMods.Modules
{
    [HarmonyPatch]
    public static class ModLanguageSelector
    {
        private static ToggleButtonBehaviour langOption;
        public static ToggleButtonBehaviour buttonPrefab;
        public static int languageNum = 0;
        public static string language;

        public static void Initialize()
        {
            languageNum = UltimateModsPlugin.LanguageNum.Value;
            language = LocalizationManager.GetString(TransKey.LanguageName);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
        public static void OptionsMenuBehaviour_StartPostfix(OptionsMenuBehaviour __instance)
        {
            if (!__instance.CensorChatButton) return;

            if (!buttonPrefab)
            {
                buttonPrefab = Object.Instantiate(__instance.CensorChatButton);
                Object.DontDestroyOnLoad(buttonPrefab);
                buttonPrefab.name = "CensorChatPrefab";
                buttonPrefab.gameObject.SetActive(false);
            }

            InitializeLangButton(__instance);
        }

        private static void InitializeLangButton(OptionsMenuBehaviour __instance)
        {
            langOption = Object.Instantiate(buttonPrefab, __instance.CensorChatButton.transform.parent);
            langOption.transform.localPosition = __instance.CensorChatButton.transform.localPosition + Vector3.down * 0.5f + Vector3.right * 1.35f;

            langOption.gameObject.SetActive(true);
            langOption.Text.text = String.Format(LocalizationManager.GetString(TransKey.Language), language);
            var langOptionButton = langOption.GetComponent<PassiveButton>();
            langOptionButton.OnClick = new ButtonClickedEvent();
            langOptionButton.OnClick.AddListener((Action)(() =>
            {
                ChangeNextLang();
            }));
        }

        private static void ChangeNextLang()
        {
            try
            {
                if (languageNum == (int)ModSupportedLangs.Indonesia) languageNum = UltimateModsPlugin.LanguageNum.Value = (int)ModSupportedLangs.Japanese;
                else languageNum++;

                ClientOptionsPatch.updateTranslations();
                VanillaOptionsPatch.updateTranslations();
                language = LocalizationManager.GetString(TransKey.LanguageName);
                langOption.Text.text = String.Format(LocalizationManager.GetString(TransKey.Language), language);
                UltimateModsPlugin.Logger.LogInfo($"Changed Languaged to {language}, Id is{languageNum}.");
                UltimateModsPlugin.LanguageNum.Value++;
            }
            catch (Exception e)
            {
                UltimateModsPlugin.Logger.LogError(e);
            }
        }
    }
}