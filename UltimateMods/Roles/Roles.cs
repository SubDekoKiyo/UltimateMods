using System.Linq;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UltimateMods.Roles.Yakuza;
using static UltimateMods.Roles.CrewmateRoles;
using static UltimateMods.Roles.NeutralRoles;

namespace UltimateMods.Roles
{
    public enum RoleType
    {
        // Crewmate Roles
        Crewmate = 0,
        Sheriff,
        YakuzaBoss,
        YakuzaStaff,
        YakuzaGun,

        // Impostor Roles
        Impostor = 100,
        Whopper,

        // Neutral Roles
        Jester = 200,

        // don't put anything below this
        NoRole = int.MaxValue
    }

    [HarmonyPatch]
    public static class RoleData
    {
        public static Dictionary<RoleType, Type> allRoleTypes = new();
    }

    public abstract class Role
    {
        public static List<Role> allRoles = new List<Role>();
        public PlayerControl player;
        public RoleType roleId;

        public abstract void OnMeetingStart();
        public abstract void OnMeetingEnd();
        public abstract void FixedUpdate();
        public abstract void OnKill(PlayerControl target);
        public abstract void OnDeath(PlayerControl killer = null);
        public abstract void HandleDisconnect(PlayerControl player, DisconnectReasons reason);
        public virtual void ResetRole() { }
        public virtual void PostInit() { }

        public static void ClearAll()
        {
            allRoles = new List<Role>();
        }
    }

    [HarmonyPatch]
    public abstract class RoleBase<T> : Role where T : RoleBase<T>, new()
    {
        public static List<T> players = new();
        public static RoleType RoleType;

        public void Init(PlayerControl player)
        {
            this.player = player;
            players.Add((T)this);
            allRoles.Add(this);
            PostInit();
        }

        public static T local
        {
            get
            {
                return players.FirstOrDefault(x => x.player == PlayerControl.LocalPlayer);
            }
        }

        public static List<PlayerControl> allPlayers
        {
            get
            {
                return players.Select(x => x.player).ToList();
            }
        }

        public static List<PlayerControl> livingPlayers
        {
            get
            {
                return players.Select(x => x.player).Where(x => x.IsAlive()).ToList();
            }
        }

        public static List<PlayerControl> deadPlayers
        {
            get
            {
                return players.Select(x => x.player).Where(x => !x.IsAlive()).ToList();
            }
        }

        public static bool exists
        {
            get { return COHelpers.RolesEnabled && players.Count > 0; }
        }

        public static T getRole(PlayerControl player = null)
        {
            player = player ?? PlayerControl.LocalPlayer;
            return players.FirstOrDefault(x => x.player == player);
        }

        public static bool isRole(PlayerControl player)
        {
            return players.Any(x => x.player == player);
        }
    }

    public static class RoleHelpers
    {
        public static bool isRole(this PlayerControl player, RoleType role)
        {
            foreach (var t in RoleData.allRoleTypes)
                if (role == t.Key)
                    return (bool)t.Value.GetMethod("isRole", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });

            switch (role)
            {
                case RoleType.Jester:
                    return Jester.jester == player;
                case RoleType.Sheriff:
                    return Sheriff.sheriff == player;
                case RoleType.YakuzaBoss:
                    return YakuzaBoss.boss == player;
                case RoleType.YakuzaStaff:
                    return YakuzaStaff.staff == player;
                case RoleType.YakuzaGun:
                    return YakuzaGun.gun == player;
                default:
                    UltimateModsPlugin.Logger.LogError("isRole: no method found for role type {role}");
                    break;
            }

            return false;
        }

        public static void setRole(this PlayerControl player, RoleType role)
        {
            foreach (var t in RoleData.allRoleTypes)
            {
                if (role == t.Key)
                {
                    t.Value.GetMethod("setRole", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });
                    return;
                }
            }

            switch (role)
            {
                case RoleType.Jester:
                    Jester.jester = player;
                    break;
                case RoleType.Sheriff:
                    Sheriff.sheriff = player;
                    break;
                case RoleType.YakuzaBoss:
                    YakuzaBoss.boss = player;
                    break;
                case RoleType.YakuzaStaff:
                    YakuzaStaff.staff = player;
                    break;
                case RoleType.YakuzaGun:
                    YakuzaGun.gun = player;
                    break;
                default:
                    UltimateModsPlugin.Logger.LogError("setRole: no method found for role type {role}");
                    return;
            }
        }

        public static void eraseRole(this PlayerControl player, RoleType role)
        {
            if (isRole(player, role))
            {
                foreach (var t in RoleData.allRoleTypes)
                {
                    if (role == t.Key)
                    {
                        t.Value.GetMethod("eraseRole", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });
                        return;
                    }
                }
                UltimateModsPlugin.Logger.LogError("eraseRole: no method found for role type {role}");
            }
        }

        public static void eraseAllRoles(this PlayerControl player)
        {
            foreach (var t in RoleData.allRoleTypes)
            {
                t.Value.GetMethod("eraseRole", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });
            }

            // Crewmate roles
            if (player.isRole(RoleType.Sheriff)) Sheriff.ClearAndReload();
            if (player.isRole(RoleType.YakuzaBoss)) YakuzaBoss.ClearAndReload();
            if (player.isRole(RoleType.YakuzaStaff)) YakuzaStaff.ClearAndReload();
            if (player.isRole(RoleType.YakuzaGun)) YakuzaGun.ClearAndReload();

            // Impostor roles

            // Neutral roles
            if (player.isRole(RoleType.Jester)) Jester.ClearAndReload();
        }

        public static void swapRoles(this PlayerControl player, PlayerControl target)
        {
            foreach (var t in RoleData.allRoleTypes)
            {
                if (player.isRole(t.Key))
                {
                    t.Value.GetMethod("swapRole", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player, target });
                }
            }

            if (player.isRole(RoleType.Jester)) Jester.jester = target;
            if (player.isRole(RoleType.Sheriff)) Sheriff.sheriff = target;
            if (player.isRole(RoleType.YakuzaBoss)) YakuzaBoss.boss = target;
            if (player.isRole(RoleType.YakuzaStaff)) YakuzaStaff.staff = target;
            if (player.isRole(RoleType.YakuzaGun)) YakuzaGun.gun = target;
        }
    }
}