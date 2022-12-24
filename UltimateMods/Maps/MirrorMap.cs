namespace UltimateMods
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
    public class MirrorMap
    {
        public static GameObject skeld;
        public static GameObject miraHQ;
        public static GameObject polus;
        public static GameObject airShip;

        public static void Prefix(SpawnInMinigame.SpawnLocation __instance)
        {
            if (GameManager.Instance.LogicOptions.currentGameOptions.MapId == 0 && Helpers.IsMirrorMap)
            {
                skeld = GameObject.Find("SkeldShip(Clone)");
                skeld.transform.localScale = new Vector3(-1.2f, 1.2f, 1.2f);
                SkeldShipStatus.Instance.InitialSpawnCenter = new(0.8f, 0.6f);
                SkeldShipStatus.Instance.MeetingSpawnCenter = new(0.8f, 0.6f);
            }
            else if (GameManager.Instance.LogicOptions.currentGameOptions.MapId == 1 && Helpers.IsMirrorMap)
            {
                miraHQ = GameObject.Find("MiraShip(Clone)");
                miraHQ.transform.localScale = new Vector3(-1f, 1f, 1f);
                MiraShipStatus.Instance.InitialSpawnCenter = new(4.4f, 2.2f);
                MiraShipStatus.Instance.MeetingSpawnCenter = new(-25.3921f, 2.5626f);
                MiraShipStatus.Instance.MeetingSpawnCenter2 = new(-25.3921f, 2.5626f);
            }
            else if (GameManager.Instance.LogicOptions.currentGameOptions.MapId == 2 && Helpers.IsMirrorMap)
            {
                polus = GameObject.Find("PolusShip(Clone)");
                polus.transform.localScale = new Vector3(-1f, 1f, 1f);
                PolusShipStatus.Instance.InitialSpawnCenter = new(-16.7f, -2.1f);
                PolusShipStatus.Instance.MeetingSpawnCenter = new(-19.5f, -17f);
                PolusShipStatus.Instance.MeetingSpawnCenter2 = new(-19.5f, -17f);
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