using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using UnityEngine;
using UltimateMods.Roles;
using UltimateMods.Modules;
using static UltimateMods.UltimateMods;

namespace UltimateMods.Patches
{
    [HarmonyPatch(typeof(RoleOptionsData), nameof(RoleOptionsData.GetNumPerGame))]
    class DisableVanillaRolePatch
    {
        public static void Postfix(ref int __result, ref RoleTypes role)
        {
            if (role is RoleTypes.Crewmate or RoleTypes.Impostor) return;

            if (CustomOptionsH.ActivateModRoles.getBool()) __result = 0; // Deactivate Vanilla Roles if the mod roles are active
        }
    }

    public class RoleAssignData
    {
        public RoleType RoleType;
        public int AssignCount;
        public Team RoleTeam;

        public RoleAssignData(RoleType RoleType, int Count, Team Team)
        {
            this.RoleType = RoleType;
            this.AssignCount = Count;
            this.RoleTeam = Team;
        }

        public static System.Random rnd = new((int)DateTime.Now.Ticks);
        public static List<RoleAssignData> AssignRoleList = new();

        public void ListUpEnabledRoles()
        {
            AssignRoleList.Clear();

            // Crewmate Roles
            AssignRoleList.Add(new(RoleType.Sheriff, CustomRolesH.SheriffRate.getCount(), Team.Crewmate));
            AssignRoleList.Add(new(RoleType.Bakery, CustomRolesH.BakeryRate.getCount(), Team.Crewmate));
            AssignRoleList.Add(new(RoleType.Engineer, CustomRolesH.EngineerRate.getCount(), Team.Crewmate));
            AssignRoleList.Add(new(RoleType.Madmate, CustomRolesH.MadmateRate.getCount(), Team.Crewmate));

            // Impostor Roles
            AssignRoleList.Add(new(RoleType.CustomImpostor, CustomRolesH.CustomImpostorRate.getCount(), Team.Impostor));
            AssignRoleList.Add(new(RoleType.Teleporter, CustomRolesH.TeleporterRate.getCount(), Team.Impostor));
            AssignRoleList.Add(new(RoleType.UnderTaker, CustomRolesH.UnderTakerRate.getCount(), Team.Impostor));
            AssignRoleList.Add(new(RoleType.BountyHunter, CustomRolesH.BountyHunterRate.getCount(), Team.Impostor));
            AssignRoleList.Add(new(RoleType.EvilHacker, CustomRolesH.EvilHackerRate.getCount(), Team.Impostor));

            // Neutral Roles
            AssignRoleList.Add(new(RoleType.Jester, CustomRolesH.JesterRate.getCount(), Team.Neutral));
        }

        public void AssignRolesAndModifiers()
        {
            List<PlayerControl> Crewmates = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            Crewmates.RemoveAll(x => x.Data.Role.IsImpostor);
            List<PlayerControl> Impostors = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            Impostors.RemoveAll(x => !x.Data.Role.IsImpostor);

            int CrewmateCount = CustomOptionsH.CrewmateRolesCount.getSelection();
            int ImpostorCount = CustomOptionsH.ImpostorRolesCount.getSelection();
            int NeutralCount = CustomOptionsH.NeutralRolesCount.getSelection();
            int ModifierCount = CustomOptionsH.ModifierCount.getSelection();

            while (Crewmates.Count > 0 && CrewmateCount > 0)
            {
                if (this.RoleTeam is not Team.Crewmate) continue;

                List<PlayerControl> TargetPlayers = new();
                // var AssignRole = EnabledCrewRoles[rnd.Next(0, EnabledCrewRoles.Count - 1)];
                var AssignRole = AssignRoleList[rnd.Next(0, AssignRoleList.Count - 1)];
                TargetPlayers.AddRange(Crewmates);
                var AssignedPlayer = SetRoleToRandomPlayer(AssignRole, TargetPlayers);
                CrewmateCount--;
            }

            while (Impostors.Count > 0 && ImpostorCount > 0)
            {
                if (this.RoleTeam is not Team.Impostor) continue;

                List<PlayerControl> TargetPlayers = new();
                // var AssignRole = EnabledImpRoles[rnd.Next(0, EnabledImpRoles.Count - 1)];
                var AssignRole = AssignRoleList[rnd.Next(0, AssignRoleList.Count - 1)];
                TargetPlayers.AddRange(Impostors);
                var AssignedPlayer = SetRoleToRandomPlayer(AssignRole, TargetPlayers);
                ImpostorCount--;
            }

            while (Crewmates.Count > 0 && NeutralCount > 0)
            {
                if (this.RoleTeam is not Team.Neutral) continue;

                List<PlayerControl> TargetPlayers = new();
                // var AssignRole = EnabledImpRoles[rnd.Next(0, EnabledImpRoles.Count - 1)];
                var AssignRole = AssignRoleList[rnd.Next(0, AssignRoleList.Count - 1)];
                TargetPlayers.AddRange(Crewmates);
                var AssignedPlayer = SetRoleToRandomPlayer(AssignRole, TargetPlayers);
                NeutralCount--;
            }
        }

        private byte SetRoleToRandomPlayer(RoleAssignData RoleData, List<PlayerControl> PlayerList, bool RemovePlayer = true)
        {
            byte RoleId = (byte)this.RoleType;

            if (RoleId != (byte)RoleData.RoleType)
            {
                return byte.MaxValue;
            }

            var Index = rnd.Next(0, PlayerList.Count);
            byte PlayerId = PlayerList[Index].PlayerId;

            if (RemovePlayer) PlayerList.RemoveAt(Index);

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetRole, Hazel.SendOption.Reliable, -1);
            writer.Write(RoleId);
            writer.Write(PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.SetRole(RoleId, PlayerId);
            return PlayerId;
        }
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    public static class AssignRoles
    {
        public static void Postfix(RoleAssignData __instance)
        {
            __instance.ListUpEnabledRoles();
            __instance.AssignRolesAndModifiers();
        }
    }

    public enum Team
    {
        Crewmate = 0,
        Impostor = 1,
        Neutral = 2,
    }
}