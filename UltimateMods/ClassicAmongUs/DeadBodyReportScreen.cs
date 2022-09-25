// SourceCode with SuperNewRoles
// Thanks to ykundesu, This Source Code was Completed.

using HarmonyLib;

namespace UltimateMods.ClassicAmongUs
{
    [HarmonyPatch(typeof(KillOverlay), "ShowKillAnimation")]
    public static class DeadBodyReportScreen
    {
        public static void Prefix(KillOverlay __instance, [HarmonyArgument(0)] GameData.PlayerInfo killer, [HarmonyArgument(1)] GameData.PlayerInfo victim, ref OverlayKillAnimation[] __state)
        {
            if (killer.PlayerId == victim.PlayerId)
            {
                // PreFix
                __state = __instance.KillAnims;
                // int index = Helpers.GetRandomIndex(__state.ToList());
                // Logger.Info(__state.Length.ToString() + ":" + index.ToString());

                /* 0を変えることで強制的にキルアニメーションが変わる */

                var anim = __state[3];
                __instance.KillAnims = new OverlayKillAnimation[1] { anim };
                // Logger.Info(__instance.KillAnims.Length.ToString());
            }
        }

        public static void Postfix(KillOverlay __instance, [HarmonyArgument(0)] GameData.PlayerInfo killer, [HarmonyArgument(1)] GameData.PlayerInfo victim, OverlayKillAnimation[] __state)
        {
            if (killer.PlayerId == victim.PlayerId)
            {
                // Postfix
                if (!Constants.ShouldHorseAround())
                {
                    var anim = __instance.transform.FindChild("PunchShootKill(Clone)");
                    anim.transform.FindChild("Impostor").gameObject.SetActive(false);
                    anim.transform.FindChild("killstabknife").gameObject.SetActive(false);
                    anim.transform.FindChild("killstabknifehand").gameObject.SetActive(false);
                    anim.transform.FindChild("Victim").gameObject.SetActive(false);
                    anim.transform.FindChild("PetSlot").gameObject.SetActive(false);

                    __instance.KillAnims = __state;
                }
            }
        }
    }
}