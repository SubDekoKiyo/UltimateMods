namespace UltimateMods.Roles;

public static class RoleManagement
{
    public static bool IsRole(this PlayerControl player, RoleId roleId)
    {
        foreach (var t in RoleData.allRoleIds)
            if (roleId == t.Key) return (bool)t.Value.GetMethod("IsRole", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });
        return false;
    }

    public static void SetRole(this PlayerControl player, RoleId roleId)
    {
        foreach (var t in RoleData.allRoleIds)
        {
            if (roleId == t.Key)
            {
                t.Value.GetMethod("SetRole", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });
                return;
            }
        }
    }

    public static void EraseAllRoles(this PlayerControl player)
    {
        foreach (var t in RoleData.allRoleIds)
        {
            t.Value.GetMethod("EraseRole", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });
        }
    }

    public static void OnKill(this PlayerControl player, PlayerControl target)
    {
        Role.allRoles.DoIf(x => x.player == player, x => x.OnKill(target));
        Modifier.allModifiers.DoIf(x => x.player == player, x => x.OnKill(target));
    }

    public static void OnDeath(this PlayerControl player, PlayerControl killer)
    {
        Role.allRoles.DoIf(x => x.player == player, x => x.OnDeath(killer));
        Modifier.allModifiers.DoIf(x => x.player == player, x => x.OnDeath(killer));

        RPCProcedure.UpdateMeeting(player.PlayerId, true);
    }
}