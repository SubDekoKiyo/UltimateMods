using UltimateMods.Roles.Yakuza;
using static UltimateMods.Patches.PlayerControlFixedUpdatePatch;
using static UltimateMods.Roles.CrewmateRoles;

namespace UltimateMods.Roles.Patches
{
    public static class RolesUpdate
    {
        public static void SetTargetOutline()
        {
            if (PlayerControl.LocalPlayer.isRole(RoleType.Sheriff) && Sheriff.MaxShots > 0)
            {
                Sheriff.currentTarget = SetTarget();
                SetPlayerOutline(Sheriff.currentTarget, Sheriff.color);
            }
            if (PlayerControl.LocalPlayer.isRole(RoleType.YakuzaBoss) && YakuzaBoss.MaxShots > 0)
            {
                YakuzaBoss.currentTarget = SetTarget();
                SetPlayerOutline(YakuzaBoss.currentTarget, YakuzaBoss.color);
            }
            if (PlayerControl.LocalPlayer.isRole(RoleType.YakuzaStaff) && YakuzaStaff.MaxShots > 0)
            {
                YakuzaStaff.currentTarget = SetTarget();
                SetPlayerOutline(YakuzaStaff.currentTarget, YakuzaBoss.color);
            }
            if (PlayerControl.LocalPlayer.isRole(RoleType.YakuzaGun) && YakuzaGun.MaxShots > 0)
            {
                YakuzaGun.currentTarget = SetTarget();
                SetPlayerOutline(YakuzaGun.currentTarget, YakuzaBoss.color);
            }
        }
    }
}