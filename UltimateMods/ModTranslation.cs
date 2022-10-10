using HarmonyLib;
using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using UltimateMods.Modules;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Net;

namespace UltimateMods
{
    public class ModTranslation
    {
        public static int defaultLanguage = (int)SupportedLangs.English;
        public static Dictionary<string, Dictionary<int, string>> stringData;

        private const string BlankText = "[BLANK]";
        public const string TranslateFolder = "Translate";
        public static int lang = (int)SaveManager.LastLanguage;

        public ModTranslation() { }

        public static void Load()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream("UltimateMods.Resources.Translate.json");
            var byteArray = new byte[stream.Length];
            var read = stream.Read(byteArray, 0, (int)stream.Length);
            string json = System.Text.Encoding.UTF8.GetString(byteArray);

            stringData = new();
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

                    stringData[stringName] = strings;
                }
            }

            /* Source Code with TownOfHost */

            // Load Custom Translate File
            if (!Directory.Exists(TranslateFolder)) Directory.CreateDirectory(TranslateFolder);

            // Make Template of Translate File
            CreateTemplateFile();
            foreach (var lang in Enum.GetValues(typeof(SupportedLangs)))
            {
                if (File.Exists(@$"./{TranslateFolder}/{lang}.dat"))
                    LoadCustomTranslationFile($"{lang}.dat", (SupportedLangs)lang);
            }
        }

        // Source Code with TownOfHost
        public static void LoadCustomTranslationFile(string filename, SupportedLangs lang)
        {
            string path = @$"./{TranslateFolder}/{filename}";
            if (File.Exists(path))
            {
                Helpers.Log($"[Translate] Loading Custom Translate File. FileName is {filename}.");
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
                            stringData[tmp[0]][(int)lang] = tmp.Skip(1).Join(delimiter: ":").Replace("\\n", "\n").Replace("\\r", "\r");
                        }
                        catch (KeyNotFoundException)
                        {
                            UltimateModsPlugin.Logger.LogWarning($"[Translate] \"{tmp[0]}\"isn't a valid key.");
                        }
                    }
                }
            }
            else
            {
                UltimateModsPlugin.Logger.LogError($"[Translate] CustomTranslate \"{filename}\" was not found.");
            }
        }

        public static string getString(string key, string def = null)
        {
            // Strip out color tags.
            string keyClean = Regex.Replace(key, "<.*?>", "");
            keyClean = Regex.Replace(keyClean, "^-\\s*", "");
            keyClean = keyClean.Trim();

            def = def ?? key;
            if (!stringData.ContainsKey(keyClean))
            {
                return def;
            }

            var data = stringData[keyClean];

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

        private static void CreateTemplateFile()
        {
            var text = "";
            foreach (var title in stringData) text += $"{title.Key}:\n";
            File.WriteAllText(@$"./{TranslateFolder}/TranslateTemplate.dat", text);
            text = "";
            foreach (var title in stringData) text += $"{title.Key}:{title.Value[0].Replace("\n", "\\n").Replace("\r", "\\r")}\n";
            File.WriteAllText(@$"./{TranslateFolder}/Default_English.dat", text);
        }
    }/*

    class DownloadFiles
    {
        public static void DownloadTemplate()
        {
            WebClient wc = new WebClient();
            try
            {
                wc.DownloadFile("http://(画像ファイルのURL)", @$"./{ModTranslation.TranslateFolder}/NetaJapanese");
                Helpers.Log("[Download Translate] Completed!");
            }
            catch (WebException exc)
            {
                UltimateModsPlugin.Logger.LogError(exc.Message);
            }
        }
    }*/

    [HarmonyPatch(typeof(LanguageSetter), nameof(LanguageSetter.SetLanguage))]
    class SetLanguagePatch
    {
        static void Postfix()
        {
            ClientOptionsPatch.updateTranslations();
            VanillaOptionsPatch.updateTranslations();
        }
    }
}