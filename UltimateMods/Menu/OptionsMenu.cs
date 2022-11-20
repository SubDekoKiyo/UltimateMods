//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using BepInEx.Configuration;
using System.Diagnostics;
using System.Collections;
using UnityEngine.UI;
using TMPro;

namespace UltimateMods.Menu
{
    [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
    public static class OptionsMenu
    {
        public static void Postfix(OptionsMenuBehaviour __instance)
        {
            var Tabs = new List<TabGroup>(__instance.Tabs.ToArray());

            PassiveButton PassiveButton;

            // Add New Setting Tab
            GameObject UMTab = new("UMTab");
            UMTab.transform.SetParent(__instance.transform);
            UMTab.transform.localScale = new Vector3(1f, 1f, 1f);
            UMTab.SetActive(false);

            Tabs[Tabs.Count - 1] = (GameObject.Instantiate(Tabs[1], null));
            var UMButton = Tabs[Tabs.Count - 1];
            UMButton.gameObject.name = "UMButton";
            UMButton.transform.SetParent(Tabs[0].transform.parent);
            UMButton.transform.localScale = new Vector3(1f, 1f, 1f);
            UMButton.Content = UMTab;
            var TextObj = UMButton.transform.FindChild("Text_TMP").gameObject;
            TextObj.GetComponent<TextTranslatorTMP>().enabled = false;
            TextObj.GetComponent<TMP_Text>().text = "UM";

            PassiveButton = UMButton.gameObject.GetComponent<PassiveButton>();
            PassiveButton.OnClick = new Button.ButtonClickedEvent();
            PassiveButton.OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
            {
                __instance.OpenTabGroup(Tabs.Count - 2);

                PassiveButton.OnMouseOver.Invoke();
            }
            ));

            float y = Tabs[0].transform.localPosition.y, z = Tabs[0].transform.localPosition.z;
            if (Tabs.Count == 4)
                for (int i = 0; i < 3; i++) Tabs[i].transform.localPosition = new Vector3(1.7f * (float)(i - 1), y, z);
            else if (Tabs.Count == 5)
                for (int i = 0; i < 4; i++) Tabs[i].transform.localPosition = new Vector3(1.62f * ((float)i - 1.5f), y, z);

            __instance.Tabs = new Il2CppReferenceArray<TabGroup>(Tabs.ToArray());
        }
    }
}