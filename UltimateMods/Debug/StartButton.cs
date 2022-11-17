using HarmonyLib;

namespace UltimateMods.Debug
{
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        public static class StartButton
        {
            public static void Prefix(GameStartManager __instance)
            {
                if (UltimateModsPlugin.isBeta) __instance.MinPlayers = 1; //One Player start
            }
        }
}