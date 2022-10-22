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
                float num = float.MaxValue;
                PlayerControl @object = pc.Object;
                bool roleCouldUse = @object.RoleCanUseVents();

                var usableDistance = __instance.UsableDistance;
                if (__instance.name.StartsWith("SealedVent_"))
                {
                    canUse = couldUse = false;
                    __result = num;
                    return false;
                }

                // Submerged Compatability if needed:
                if (SubmergedCompatibility.IsSubmerged)
                {
                    // as submerged does, only change stuff for vents 9 and 14 of submerged. Code partially provided by AlexejheroYTB
                    if (SubmergedCompatibility.getInTransition())
                    {
                        __result = float.MaxValue;
                        return canUse = couldUse = false;
                    }
                    switch (__instance.Id)
                    {
                        case 9:  // Cannot enter vent 9 (Engine Room Exit Only Vent)!
                            if (PlayerControl.LocalPlayer.inVent) break;
                            __result = float.MaxValue;
                            return canUse = couldUse = false;
                        case 14: // Lower Central
                            __result = float.MaxValue;
                            couldUse = roleCouldUse && !pc.IsDead && (@object.CanMove || @object.inVent);
                            canUse = couldUse;
                            if (canUse)
                            {
                                Vector3 center = @object.Collider.bounds.center;
                                Vector3 position = __instance.transform.position;
                                __result = Vector2.Distance(center, position);
                                canUse &= __result <= __instance.UsableDistance;
                            }
                            return false;
                    }
                }

                couldUse = (@object.inVent || roleCouldUse) && !pc.IsDead && (@object.CanMove || @object.inVent);
                canUse = couldUse;
                if (canUse)
                {
                    Vector2 truePosition = @object.GetTruePosition();
                    Vector3 position = __instance.transform.position;
                    num = Vector2.Distance(truePosition, position);

                    canUse &= (num <= usableDistance && !PhysicsHelpers.AnythingBetween(truePosition, position, Constants.ShipOnlyMask, false));
                }
                __result = num;
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
                bool canMoveInVents = PlayerControl.LocalPlayer;
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
                __instance.SetButtons(isEnter && canMoveInVents);
                return false;
            }
        }
    }
}