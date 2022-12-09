// Source Code from TownOfHost

using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using System.Text.RegularExpressions;
using UltimateMods.Modules;
using AmongUs.Data;

namespace UltimateMods
{
    public static class ModTranslation
    {
        public static int DefaultLanguage = (int)SupportedLang.Japanese;
        public static Dictionary<string, Dictionary<int, string>> TransData = new();

        private const string BlankText = "[BLANK]";

        public static void Load()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream("UltimateMods.Translate.StringData.csv");
            var streamReader = new StreamReader(stream);

            string[] Header = streamReader.ReadLine().Split(',');
            int CurrentLine = 1;

            while (!streamReader.EndOfStream)
            {
                CurrentLine++;
                string line = streamReader.ReadLine();
                if (line == "" || line[0] == ',' || (line[0] == '#' && line[1] == '"')) continue;
                string[] values = line.Split(',');
                List<string> fields = new(values);
                Dictionary<int, string> tmp = new();
                try
                {
                    for (var i = 1; i < fields.Count; ++i)
                    {
                        if (fields[i] != string.Empty && fields[i].TrimStart()[0] == '"')
                        {
                            while (fields[i].TrimEnd()[^1] != '"')
                            {
                                fields[i] = fields[i] + "," + fields[i + 1];
                                fields.RemoveAt(i + 1);
                            }
                        }
                    }
                    for (var i = 1; i < fields.Count; i++)
                    {
                        var tmp_str = fields[i].Replace("\\n", "\n").Trim('"');
                        tmp.Add(Int32.Parse(Header[i]), tmp_str);
                    }
                    if (TransData.ContainsKey(fields[0])) { UltimateModsPlugin.Logger.LogWarning($"翻訳用CSVに重複があります。{CurrentLine}行目: \"{fields[0]}\""); continue; }
                    TransData.Add(fields[0], tmp);
                }
                catch
                {
                    var err = $"翻訳用CSVファイルに誤りがあります。{CurrentLine}行目:";
                    foreach (var c in fields) err += $" [{c}]";
                    UltimateModsPlugin.Logger.LogError(err);
                    continue;
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
            int lang = ModLanguageSelector.languageNum;

            if (data.ContainsKey(lang))
            {
                return key.Replace(keyClean, data[lang]);
            }
            else if (data.ContainsKey(DefaultLanguage))
            {
                return key.Replace(keyClean, data[DefaultLanguage]);
            }

            return key;
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