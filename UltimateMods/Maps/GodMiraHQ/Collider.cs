// 後日実装
/*
using HarmonyLib;
using UnityEngine;

namespace UltimateMods.Maps
{
    public static class GodMira
    {
        public static void ShipStatusAwake(ShipStatus __instance)
        {
            if (PlayerControl.LocalPlayer.IsMiraHQ() && CustomOptionsH.EnableGodMiraHQ.getBool())
            {
                SpriteRenderer renderer;
                GameObject HallNo1 = new("NewHallNo1");
                HallNo1.transform.SetParent(__instance.transform);
                HallNo1.transform.localPosition = new Vector3(6.5f, 15.5f, 0.5f);
                HallNo1.transform.localScale = new Vector3(1f, 1f, 1f);
                HallNo1.SetActive(true);

                renderer = HallNo1.AddComponent<SpriteRenderer>();
                renderer.sprite = Helpers.LoadSpriteFromResources("UltimateMods.Resources.GodMiraModeEntrance1.png", 200f);

                GameObject HallNo2 = new("NewHallNo2");
                HallNo2.transform.SetParent(__instance.transform);
                HallNo2.transform.localPosition = new Vector3(6.5f, 18.3f, 0.5f);
                HallNo2.transform.localScale = new Vector3(1f, 1f, 1f);
                HallNo2.SetActive(true);

                renderer = HallNo2.AddComponent<SpriteRenderer>();
                renderer.sprite = Helpers.LoadSpriteFromResources("UltimateMods.Resources.GodMiraModeHall1.png", 200f);
            }
        }

        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
        static class ShipStatusAwakePatch
        {
            static void Postfix(ShipStatus __instance)
            {
                ShipStatusAwake(__instance);
            }
        }
    }
}
*/

// メモ
// MiraのCloud Gen、外回りに謎の壁設置


//影y
//15.2f