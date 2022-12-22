namespace UltimateMods.Localization;

public enum ModSupportedLangs
{
    Japanese = 0,
    English = 1,
    SChinese = 2,
    Indonesia = 3,
}

public static class LocalizationManager
{
    public static int DefaultLanguage = (int)ModSupportedLangs.English;
    public static Dictionary<string, Dictionary<int, string>> LocalizationData;
    private const string BlankText = "[BLANK]";

    public static void Load()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        Stream stream = assembly.GetManifestResourceStream("UltimateMods.Resources.LocalizationData.json");
        var byteArray = new byte[stream.Length];
        var read = stream.Read(byteArray, 0, (int)stream.Length);
        string json = Encoding.UTF8.GetString(byteArray);

        LocalizationData = new();
        JObject parsed = JObject.Parse(json);

        for (int i = 0; i < parsed.Count; i++)
        {
            JProperty token = parsed.ChildrenTokens[i].TryCast<JProperty>();
            if (token == null) continue;

            string stringName = token.Name;
            var val = token.Value.TryCast<JObject>();

            if (token.HasValues)
            {
                var strings = new Dictionary<int, string>();

                for (int j = 0; j < (int)ModSupportedLangs.Indonesia + 1; j++)
                {
                    string key = j.ToString();
                    var text = val[key]?.TryCast<JValue>().Value.ToString();

                    if (text != null && text.Length > 0)
                    {
                        if (text is BlankText) strings[j] = "";
                        else strings[j] = text;
                    }
                }

                LocalizationData[stringName] = strings;
            }
        }
    }

    public static string GetString(TransKey id, string def = null)
    {
        // Strip out color tags.
        string keyClean = Regex.Replace(id.ToString(), "<.*?>", "");
        keyClean = Regex.Replace(keyClean, "^-\\s*", "");
        keyClean = keyClean.Trim();

        def = def ?? id.ToString();
        if (!LocalizationData.ContainsKey(keyClean)) return def;

        var data = LocalizationData[keyClean];
        int lang = ModLanguageSelector.languageNum;

        if (data.ContainsKey(lang)) return id.ToString().Replace(keyClean, data[lang]);
        else if (data.ContainsKey(DefaultLanguage)) return id.ToString().Replace(keyClean, data[DefaultLanguage]);

        return id.ToString();
    }

    public static string GetString(string value, string def = null)
    {
        // Strip out color tags.
        string keyClean = Regex.Replace(value, "<.*?>", "");
        keyClean = Regex.Replace(keyClean, "^-\\s*", "");
        keyClean = keyClean.Trim();

        def = def ?? value;
        if (!LocalizationData.ContainsKey(keyClean)) return def;

        var data = LocalizationData[keyClean];
        int lang = ModLanguageSelector.languageNum;

        if (data.ContainsKey(lang)) return value.Replace(keyClean, data[lang]);
        else if (data.ContainsKey(DefaultLanguage)) return value.Replace(keyClean, data[DefaultLanguage]);

        return value;
    }
}

[HarmonyPatch(typeof(LanguageSetter), nameof(LanguageSetter.SetLanguage))]
class SetLanguagePatch
{
    static void Postfix()
    {
        try
        {
            ClientOptionsPatch.updateTranslations();
            VanillaOptionsPatch.updateTranslations();
        }
        catch
        {
            UltimateModsPlugin.Logger.LogError("Keys not found.");
        }
    }
}