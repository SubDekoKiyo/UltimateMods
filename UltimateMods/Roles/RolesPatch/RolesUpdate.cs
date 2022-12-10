using HarmonyLib;

namespace UltimateMods.Roles.Patches
{
    public static class RolesUpdate
    {
        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
        class PlayerPhysicsPatch
        {
            public static void Postfix(PlayerPhysics __instance)
            {
                if (PlayerControl.LocalPlayer.isRole(RoleType.UnderTaker) && UnderTaker.DraggingBody)
                {
                    __instance.body.velocity *= UnderTaker.SpeedDown / 100f;
                }
            }
        }
    }
}