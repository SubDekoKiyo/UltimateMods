// using UnityEngine;
// using HarmonyLib;
// using BepInEx;
// using UnityEngine.UI;
// using System.Collections;
// using System.Collections.Generic;
// using UltimateMods.Utilities;

// namespace UltimateMods.Menu
// {
//     public enum LanguageNum
//     {
//         English = 0,
//         Japanese = 1,
//         SChinese = 2,
//         Indonesia = 3
//     }

//     public static class LanguageMenuPatch
//     {
//         public static void Initialize()
//         {
//             LangDropDownMenu.value = UltimateModsPlugin.LanguageNum.Value;
//         }

//         [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
//         public static void Prefix()
//         {
//             UpdateDropDown();
//         }

//         public static void UpdateDropDown()
//         {
//             if (LangDropDownMenu.value == 0)
//             {
//                 UltimateModsPlugin.LanguageNum.Value = (int)LanguageNum.English;
//             }
//             else if (LangDropDownMenu.value == 1)
//             {
//                 UltimateModsPlugin.LanguageNum.Value = (int)LanguageNum.Japanese;
//             }
//             else if (LangDropDownMenu.value == 2)
//             {
//                 UltimateModsPlugin.LanguageNum.Value = (int)LanguageNum.SChinese;
//             }
//             else if (LangDropDownMenu.value == 3)
//             {
//                 UltimateModsPlugin.LanguageNum.Value = (int)LanguageNum.Indonesia;
//             }

//             LangDropDownMenu.gameObject.SetActive(FastDestroyableSingleton<OptionsMenuBehaviour>.Instance.GetComponentInChildren<LanguageButton>());
//         }
//     }
// }