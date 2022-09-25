using UltimateMods.EndGame;
using static UltimateMods.GameHistory;
using UltimateMods.Roles.Yakuza;
using static UltimateMods.Roles.CrewmateRoles;

namespace UltimateMods.Roles.Patches
{
    public static class RolesMurder
    {
        public static void KillPlayer() { }

        public static void DeathPlayer(PlayerControl killer = null)
        {
            if (PlayerControl.LocalPlayer.isRole(RoleType.YakuzaBoss) && YakuzaBoss.boss.Data.IsDead)
            {
                foreach (var gun in PlayerControl.AllPlayerControls)
                {
                    if (gun.IsAlive())
                    {
                        if (killer == null)
                        {
                            gun.Exiled();
                        }
                        else
                        {
                            gun.MurderPlayer(gun);
                        }
                        finalStatuses[gun.PlayerId] = FinalStatus.Suicide;
                    }
                }
                foreach (var staff in PlayerControl.AllPlayerControls)
                {
                    if (staff.IsAlive())
                    {
                        if (killer == null)
                        {
                            staff.Exiled();
                        }
                        else
                        {
                            staff.MurderPlayer(staff);
                        }
                        finalStatuses[staff.PlayerId] = FinalStatus.Suicide;
                    }
                }
            }

            if (PlayerControl.LocalPlayer.isRole(RoleType.YakuzaGun) && YakuzaGun.gun.Data.IsDead)
            {
                YakuzaStaff.CanKill = true;

                if (PlayerControl.LocalPlayer.isRole(RoleType.YakuzaStaff) && YakuzaStaff.staff.Data.IsDead)
                {
                    YakuzaBoss.CanKill = true;
                }
            }
        }
    }
}