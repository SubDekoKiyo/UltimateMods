using HarmonyLib;
using UnityEngine;

namespace UltimateMods.Roles.Patches
{
    public static class VentButtonPatch
    {
        [HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
        public static class VentCanUsePatch
        {
            public static bool Prefix(Vent __instance, ref float __result, [HarmonyArgument(0)] GameData.PlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
            {
                float Num = float.MaxValue;
                PlayerControl @object = pc.Object;
                bool roleCouldUse = @object.RoleCanUseVents();

                var usableDistance = __instance.UsableDistance;
                if (__instance.name.StartsWith("SealedVent_"))
                {
                    canUse = couldUse = false;
                    __result = Num;
                    return false;
                }

                couldUse = (@object.inVent || roleCouldUse) && !pc.IsDead && (@object.CanMove || @object.inVent);
                canUse = couldUse;
                if (canUse)
                {
                    Vector2 truePosition = @object.GetTruePosition();
                    Vector3 position = __instance.transform.position;
                    Num = Vector2.Distance(truePosition, position);

                    canUse &= (Num <= usableDistance && !PhysicsHelpers.AnythingBetween(truePosition, position, Constants.ShipOnlyMask, false));
                }
                __result = Num;
                return false;
            }
        }

        [HarmonyPatch(typeof(VentButton), nameof(VentButton.DoClick))]
        class VentButtonDoClickPatch
        {
            static bool Prefix(VentButton __instance)
            {
                // Manually modifying the VentButton to use Vent.Use again in order to trigger the Vent.Use prefix patch
                if (__instance.currentTarget != null) __instance.currentTarget.Use();
                return false;
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
    }
}