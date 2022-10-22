using System.Linq;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace UltimateMods.Roles
{
    public enum RoleType
    {
        // Crewmate Roles
        Crewmate = 0,
        Sheriff,
        Engineer,
        Madmate,

        // Impostor Roles
        Impostor = 100,
        CustomImpostor,
        UnderTaker,
        BountyHunter,

        // Neutral Roles
        Jester = 200,
        // don't put anything below this
        NoRole = int.MaxValue
    }

    [HarmonyPatch]
    public static class RoleData
    {
        public static Dictionary<RoleType, Type> allRoleTypes = new()
        {
            // Crewmate
            { RoleType.Sheriff, typeof(RoleBase<Sheriff>) },
            { RoleType.Engineer, typeof(RoleBase<Engineer>) },
            { RoleType.Madmate, typeof(RoleBase<Madmate>) },

            // Neutral
            { RoleType.Jester, typeof(RoleBase<Jester>) },

            // Impostor
            { RoleType.CustomImpostor, typeof(RoleBase<CustomImpostor>) },
            { RoleType.UnderTaker, typeof(RoleBase<UnderTaker>) },
            { RoleType.BountyHunter, typeof(RoleBase<BountyHunter>) },
        };
    }

    public abstract class Role
    {
        public static List<Role> allRoles = new();
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

        public static void setRole(PlayerControl player)
        {
            if (!isRole(player))
            {
                T role = new T();
                role.Init(player);
            }
        }

        public static void eraseRole(PlayerControl player)
        {
            players.DoIf(x => x.player == player, x => x.ResetRole());
            players.RemoveAll(x => x.player == player && x.roleId == RoleType);
            allRoles.RemoveAll(x => x.player == player && x.roleId == RoleType);
        }

        public static void swapRole(PlayerControl p1, PlayerControl p2)
        {
            var index = players.FindIndex(x => x.player == p1);
            if (index >= 0)
                players[index].player = p2;
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
                default:
                    UltimateModsPlugin.Logger.LogError($"IsRole: no method found for role type {role}");
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

                default:
                    UltimateModsPlugin.Logger.LogError($"SetRole: no method found for role type {role}");
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
                UltimateModsPlugin.Logger.LogError($"eraseRole: no method found for role type {role}");
            }
        }

        public static void eraseAllRoles(this PlayerControl player)
        {
            foreach (var t in RoleData.allRoleTypes)
            {
                t.Value.GetMethod("eraseRole", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });
            }

            // Crewmate roles

            // Impostor roles

            // Neutral roles
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
        }

        public static void OnKill(this PlayerControl player, PlayerControl target)
        {
            Role.allRoles.DoIf(x => x.player == player, x => x.OnKill(target));
            Modifiers.allModifiers.DoIf(x => x.player == player, x => x.OnKill(target));
        }

        public static void OnDeath(this PlayerControl player, PlayerControl killer)
        {
            Role.allRoles.DoIf(x => x.player == player, x => x.OnDeath(killer));
            Modifiers.allModifiers.DoIf(x => x.player == player, x => x.OnDeath(killer));

            RPCProcedure.UpdateMeeting(player.PlayerId, true);
        }
    }
}