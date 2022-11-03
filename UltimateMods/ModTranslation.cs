using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UltimateMods.Modules;
using AmongUs.Data;
using UltimateMods.Utilities;
namespace UltimateMods
{
    public class ModTranslation
    {
        public const string LanguageFolder = "Language";
        public static int defaultLanguage = (int)SupportedLangs.English;
        public static Dictionary<string, Dictionary<int, string>> TransData = new();

        private const string BlankText = "[BLANK]";

        public static void Load()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream("UltimateMods.Resources.Translate.json");
            var byteArray = new byte[stream.Length];
            var read = stream.Read(byteArray, 0, (int)stream.Length);
            string json = System.Text.Encoding.UTF8.GetString(byteArray);

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

                    for (int j = 0; j < (int)SupportedLangs.Irish + 1; j++)
                    {
                        string key = j.ToString();
                        var text = val[key]?.TryCast<JValue>().Value.ToString();

                        if (text != null && text.Length > 0)
                        {
                            if (text == BlankText) strings[j] = "";
                            else strings[j] = text;
                        }
                    }

                    TransData[stringName] = strings;
                }
            }

            foreach (var lang in Enum.GetValues(typeof(SupportedLangs)))
            {
                if (File.Exists(@$"./{LanguageFolder}/Language{lang}.dat"))
                {
                    LoadCustomTranslation($"Language{lang}.dat", (SupportedLangs)lang);
                }
                else if (lang is not SupportedLangs.English or SupportedLangs.Japanese && !File.Exists(@$"./{LanguageFolder}/Language{lang}.dat"))
                {
                    LoadCustomTranslation($"LanguageEnglish.dat", (SupportedLangs)lang);
                }
            }
        }

        public static string getString(string key, string def = null)
        {
            // Strip out color tags.
            string keyClean = Regex.Replace(key, "<.*?>", "");
            keyClean = Regex.Replace(keyClean, "^-\\s*", "");
            keyClean = keyClean.Trim();

            def = def ?? key;
            if (!TransData.ContainsKey(keyClean))
            {
                return def;
            }

            var data = TransData[keyClean];
            int lang = (int)DataManager.Settings.Language.CurrentLanguage;

            if (data.ContainsKey(lang))
            {
                return key.Replace(keyClean, data[lang]);
            }
            else if (data.ContainsKey(defaultLanguage))
            {
                return key.Replace(keyClean, data[defaultLanguage]);
            }

            return key;
        }

        public static void LoadCustomTranslation(string filename, SupportedLangs lang)
        {
            string path = @$"./{LanguageFolder}/{filename}";
            if (File.Exists(path))
            {
                UltimateModsPlugin.Logger.LogInfo($"Loading Translation File \"{filename}\"");
                using StreamReader sr = new(path, Encoding.GetEncoding("UTF-8"));
                string text;
                string[] tmp = { };
                while ((text = sr.ReadLine()) != null)
                {
                    tmp = text.Split(":");
                    if (tmp.Length > 1 && tmp[1] != "")
                    {
                        try
                        {
                            TransData[tmp[0]][(int)lang] = Regex.Unescape(tmp.Skip(1).Join(delimiter: ":").Replace("\\n", "\n").Replace("\\r", "\r"));
                        }
                        catch (KeyNotFoundException)
                        {
                            UltimateModsPlugin.Logger.LogWarning($"\"{tmp[0]}\" isn't a valid key.");
                        }
                    }
                }
            }
            else
            {
                UltimateModsPlugin.Logger.LogError($"Translation File \"{filename}\" isn't found.");
            }
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
}