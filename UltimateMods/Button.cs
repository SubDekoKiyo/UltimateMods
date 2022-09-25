using HarmonyLib;
using UltimateMods.Roles.Yakuza;
using static UltimateMods.Roles.CrewmateRoles;

namespace UltimateMods
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    public static class Buttons
    {
        public static void SetCustomButtonCooldowns()
        {
            Sheriff.SetButtonCooldowns();
            YakuzaBoss.SetButtonCooldowns();
            YakuzaStaff.SetButtonCooldowns();
            YakuzaGun.SetButtonCooldowns();
        }

        public static void MakeButtons(HudManager hm)
        {
            Sheriff.MakeButtons(hm);
            YakuzaBoss.MakeButtons(hm);
            YakuzaStaff.MakeButtons(hm);
            YakuzaGun.MakeButtons(hm);
        }

        public static void Postfix(HudManager __instance)
        {
            MakeButtons(__instance);
            SetCustomButtonCooldowns();
        }
    }
}