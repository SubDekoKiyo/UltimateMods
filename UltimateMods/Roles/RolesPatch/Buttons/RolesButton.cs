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
            UnderTaker.SetButtonCooldowns();
            Teleporter.SetButtonCooldowns();
            Jackal.SetButtonCooldowns();
            Sidekick.SetButtonCooldowns();
            Arsonist.SetButtonCooldowns();
            Lighter.SetButtonCooldowns();
            Altruist.SetButtonCooldowns();
        }

        public static void MakeButtons(HudManager hm)
        {
            Sheriff.MakeButtons(hm);
            Engineer.MakeButtons(hm);
            UnderTaker.MakeButtons(hm);
            Teleporter.MakeButtons(hm);
            Jackal.MakeButtons(hm);
            Sidekick.MakeButtons(hm);
            Altruist.MakeButtons(hm);
            EvilHacker.MakeButtons(hm);
            Arsonist.MakeButtons(hm);
            Lighter.MakeButtons(hm);
        }

        public static void Postfix(HudManager __instance)
        {
            MakeButtons(__instance);
            SetCustomButtonCooldowns();
        }
    }
}