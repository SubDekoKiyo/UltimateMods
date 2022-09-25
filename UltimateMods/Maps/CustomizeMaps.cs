using HarmonyLib;

namespace UltimateMods.Maps
{
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
    public static class LoadCustomizedMap
    {
        public static void Prefix(IntroCutscene __instance)
        {
            if (PlayerControl.GameOptions.MapId == 1 && CustomOptionsH.EnableGodMiraHQ.getBool())
                GodMiraWireTask.MiraWireTaskPositionChange();
        }
    }
}