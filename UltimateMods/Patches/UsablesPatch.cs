using HarmonyLib;
using UltimateMods.Roles;
using UltimateMods.Roles.Patches;

namespace UltimateMods.Patches
{
    public static class UsablesPatch
    {
        public static class ConsolePatch
        {
            [HarmonyPatch(typeof(Console), nameof(Console.CanUse))]
            public static class ConsoleCanUsePatch
            {
                public static bool Prefix(ref float __result, Console __instance, [HarmonyArgument(0)] GameData.PlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
                {
                    canUse = couldUse = false;
                    __result = float.MaxValue;

                    //if (IsBlocked(__instance, pc.Object)) return false;
                    if (__instance.AllowImpostor) return true;
                    if (!pc.Object.HasFakeTasks()) return true;

                    return false;
                }
            }

            [HarmonyPatch(typeof(Console), nameof(Console.Use))]
            public static class ConsoleUsePatch
            {
                public static bool Prefix(Console __instance)
                {
                    return !BlockButtonPatch.IsBlocked(__instance, PlayerControl.LocalPlayer);
                }
            }
        }

        public class SystemConsolePatch
        {
            [HarmonyPatch(typeof(SystemConsole), nameof(SystemConsole.CanUse))]
            public static class SystemConsoleCanUsePatch
            {
                public static bool Prefix(ref float __result, SystemConsole __instance, [HarmonyArgument(0)] GameData.PlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
                {
                    canUse = couldUse = false;
                    __result = float.MaxValue;
                    //if (IsBlocked(__instance, pc.Object)) return false;

                    return true;
                }
            }

            [HarmonyPatch(typeof(SystemConsole), nameof(SystemConsole.Use))]
            public static class SystemConsoleUsePatch
            {
                public static bool Prefix(SystemConsole __instance)
                {
                    return !BlockButtonPatch.IsBlocked(__instance, PlayerControl.LocalPlayer);
                }
            }
        }

        [HarmonyPatch(typeof(Vent), nameof(Vent.Use))]
        public static class VentUsePatch
        {
            public static bool Prefix(Vent __instance)
            {
                bool canUse;
                bool couldUse;
                __instance.CanUse(PlayerControl.LocalPlayer.Data, out canUse, out couldUse);
                bool CannotMoveInVents = (PlayerControl.LocalPlayer.isRole(RoleType.Madmate) && !Madmate.CanMoveInVents) ||
                                        (PlayerControl.LocalPlayer.isRole(RoleType.Jester) && !Jester.CanMoveInVents);
                if (!canUse) return false; // No need to execute the native method as using is disallowed anyways
                bool isEnter = !PlayerControl.LocalPlayer.inVent;

                if (isEnter)
                {
                    PlayerControl.LocalPlayer.MyPhysics.RpcEnterVent(__instance.Id);
                }
                else
                {
                    PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(__instance.Id);
                }
                __instance.SetButtons(isEnter && !CannotMoveInVents);
                return false;
            }
        }

        [HarmonyPatch(typeof(MedScanMinigame), nameof(MedScanMinigame.FixedUpdate))]
        class MedScanMinigameFixedUpdatePatch
        {
            static void Prefix(MedScanMinigame __instance)
            {
                if (MapOptions.allowParallelMedBayScans)
                {
                    __instance.medscan.CurrentUser = PlayerControl.LocalPlayer.PlayerId;
                    __instance.medscan.UsersList.Clear();
                }
            }
        }
    }
}