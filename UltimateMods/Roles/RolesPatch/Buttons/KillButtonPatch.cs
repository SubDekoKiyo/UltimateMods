using HarmonyLib;

namespace UltimateMods.Roles.Patches
{
    public static class KillButtonPatch
    {
        [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
        class KillButtonDoClickPatch
        {
            public static bool Prefix(KillButton __instance)
            {
                if (__instance.isActiveAndEnabled && __instance.currentTarget && !__instance.isCoolingDown && PlayerControl.LocalPlayer.IsAlive() && PlayerControl.LocalPlayer.CanMove)
                {
                    bool showAnimation = true;

                    // Use an unchecked kill command, to allow shorter kill cooldowns etc. without getting kicked
                    MurderAttemptResult res = Helpers.CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, __instance.currentTarget, showAnimation: showAnimation);
                    // Handle blank kill
                    if (res == MurderAttemptResult.BlankKill)
                    {
                        PlayerControl.LocalPlayer.killTimer = GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
                    }

                    __instance.SetTarget(null);
                }
                return false;
            }
        }
    }
}