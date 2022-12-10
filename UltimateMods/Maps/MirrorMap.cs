using HarmonyLib;
using UnityEngine;

namespace UltimateMods
{
    [HarmonyPatch(typeof(ShipStatus), nameof(GameStartManager.Start))]
    public class MirrorMap
    {
        public static GameObject Skeld;
        public static GameObject MiraHQ;
        public static GameObject Polus;
        public static GameObject AirShip;
        public static void Prefix(SpawnInMinigame.SpawnLocation __instance)
        {
            if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 0 && CustomOptionsH.EnableMirrorMap.getBool())
            {
                Skeld = GameObject.Find("SkeldShip(Clone)");
                Skeld.transform.localScale = new Vector3(-1.2f, 1.2f, 1.2f);
                SkeldShipStatus.Instance.InitialSpawnCenter = new Vector2(0.8f, 0.6f);
                SkeldShipStatus.Instance.MeetingSpawnCenter = new Vector2(0.8f, 0.6f);
            }
            else if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 1 && CustomOptionsH.EnableMirrorMap.getBool())
            {
                MiraHQ = GameObject.Find("MiraShip(Clone)");
                MiraHQ.transform.localScale = new Vector3(-1f, 1f, 1f);
                MiraShipStatus.Instance.InitialSpawnCenter = new Vector2(4.4f, 2.2f);
                MiraShipStatus.Instance.MeetingSpawnCenter = new Vector2(-25.3921f, 2.5626f);
                MiraShipStatus.Instance.MeetingSpawnCenter2 = new Vector2(-25.3921f, 2.5626f);
            }
            else if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 2 && CustomOptionsH.EnableMirrorMap.getBool())
            {
                Polus = GameObject.Find("PolusShip(Clone)");
                Polus.transform.localScale = new Vector3(-1f, 1f, 1f);
                PolusShipStatus.Instance.InitialSpawnCenter = new Vector2(-16.7f, -2.1f);
                PolusShipStatus.Instance.MeetingSpawnCenter = new Vector2(-19.5f, -17f);
                PolusShipStatus.Instance.MeetingSpawnCenter2 = new Vector2(-19.5f, -17f);
            }
        }
    }
}

/*めも
反転AirShip湧き位置
宿舎前 0.8 8.5
エンジン 0.7 -0.5
アーカイブ -19.8 10
メインホール -12 0
貨物室 -33 0
キッチン 7 -11
*/