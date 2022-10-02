using HarmonyLib;
using UltimateMods.Roles;

namespace UltimateMods
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    public static class Buttons
    {
        public static void SetCustomButtonCooldowns()
        {
            Sheriff.SetButtonCooldowns();
            Engineer.SetButtonCooldowns();
        }

        public static void MakeButtons(HudManager hm)
        {
            Sheriff.MakeButtons(hm);
            Engineer.MakeButtons(hm);
        }

        public static void Postfix(HudManager __instance)
        {
            MakeButtons(__instance);
            SetCustomButtonCooldowns();
        }
    }
}