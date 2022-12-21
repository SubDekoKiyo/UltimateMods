namespace UltimateMods.Roles;

public static class RoleManagement
{
    public static string GetRoleString(this PlayerControl player, RoleId roleId)
    {
        foreach (var role in Role.allRoles)
        {
            foreach (var t in RoleData.allRoleIds)
                if (roleId == t.Key) return role.RoleName();
        }
        if (player.IsImpostor()) return "Impostor";
        if (player.IsCrew()) return "Crewmate";
        return "NoData";
    }

    public static string GetRoleString(this PlayerControl player)
    {
        foreach (var role in Role.allRoles)
        {
            foreach (var t in RoleData.allRoleIds)
                if (player.GetRoleId() == t.Key) return role.RoleName();
        }
        if (player.IsImpostor()) return "Impostor";
        if (player.IsCrew()) return "Crewmate";
        return "NoData";
    }

    public static string GetTranslatedRoleString(this PlayerControl player, RoleId roleId)
    {
        return ModTranslation.getString(player.GetRoleString(roleId));
    }

    public static Color GetRoleColor(this PlayerControl player, RoleId roleId)
    {
        foreach (var role in Role.allRoles)
        {
            foreach (var t in RoleData.allRoleIds)
                if (roleId == t.Key) return role.RoleColor();
        }
        if (player.IsImpostor()) return ImpostorRed;
        if (player.IsCrew()) return CrewmateBlue;
        return White;
    }

    public static Color GetRoleColor(this PlayerControl player)
    {
        foreach (var role in Role.allRoles)
        {
            foreach (var t in RoleData.allRoleIds)
                if (player.GetRoleId() == t.Key) return role.RoleColor();
        }
        if (player.IsImpostor()) return ImpostorRed;
        if (player.IsCrew()) return CrewmateBlue;
        return White;
    }

    public static string GetRoleIntroDesc(RoleId roleId)
    {
        return ModTranslation.getString(PlayerControl.LocalPlayer.GetRoleString(roleId) + "Intro");
    }

    public static string GetRoleShortDesc(RoleId roleId)
    {
        return ModTranslation.getString(PlayerControl.LocalPlayer.GetRoleString(roleId) + "Short");
    }

    public static string GetRoleFullDesc(RoleId roleId)
    {
        return ModTranslation.getString(PlayerControl.LocalPlayer.GetRoleString(roleId) + "Full");
    }

    public static string GetRoleAndModString(this PlayerControl player, RoleId roleId, ModifierId modifierId = 0)
    {
        string data = player.GetTranslatedRoleString(roleId);
        if (modifierId > 0) data += player.GetTranslatedModifierString(modifierId);
        return data;
    }

    public static RoleId GetRoleId(this PlayerControl player)
    {
        foreach (var t in RoleData.allRoleIds)
            if (player.IsRole(t.Key)) return t.Key;
        return RoleId.NoRole;
    }

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