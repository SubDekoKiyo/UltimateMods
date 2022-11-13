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
    class RoleOptionsDataGetNumPerGamePatch
    {
        public static void Postfix(ref int __result, ref RoleTypes role)
        {
            if (role is RoleTypes.Crewmate or RoleTypes.Impostor) return;

            if (CustomOptionsH.ActivateModRoles.getBool()) __result = 0; // Deactivate Vanilla Roles if the mod roles are active
        }
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    public static class RoleAssignmentPatch
    {
        public static System.Random rnd = new((int)DateTime.Now.Ticks);
        public static List<RoleType> EnabledCrewRoles = new();
        public static List<RoleType> EnabledImpRoles = new();
        public static List<RoleType> EnabledNeuRoles = new();
        public static List<ModifierType> EnabledModRoles = new();

        public static void ListUpEnabledRoles()
        {
            EnabledCrewRoles.Clear();
            EnabledImpRoles.Clear();
            EnabledNeuRoles.Clear();
            EnabledModRoles.Clear();

            // Crewmates
            // if (CustomRolesH.AltruistRate.getBool()) EnabledCrewRoles.Add(RoleType.Altruist);
            if (CustomRolesH.BakeryRate.getBool()) EnabledCrewRoles.Add(RoleType.Bakery);
            if (CustomRolesH.SheriffRate.getBool()) EnabledCrewRoles.Add(RoleType.Sheriff);
            if (CustomRolesH.EngineerRate.getBool()) EnabledCrewRoles.Add(RoleType.Engineer);
            if (CustomRolesH.MadmateRate.getBool()) EnabledCrewRoles.Add(RoleType.Madmate);

            // Impostors
            if (CustomRolesH.UnderTakerRate.getBool()) EnabledImpRoles.Add(RoleType.UnderTaker);
            if (CustomRolesH.TeleporterRate.getBool()) EnabledImpRoles.Add(RoleType.Teleporter);
            if (CustomRolesH.CustomImpostorRate.getBool()) EnabledImpRoles.Add(RoleType.CustomImpostor);
            if (CustomRolesH.BountyHunterRate.getBool()) EnabledImpRoles.Add(RoleType.BountyHunter);

            // Neutrals
            if (CustomRolesH.JesterRate.getBool()) EnabledNeuRoles.Add(RoleType.Jester);

            // Modifiers
            if (CustomRolesH.OpportunistRate.getBool()) EnabledModRoles.Add(ModifierType.Opportunist);
        }

        public static void AssignRolesAndModifiers()
        {
            List<PlayerControl> Crewmates = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            Crewmates.RemoveAll(x => x.Data.Role.IsImpostor);
            List<PlayerControl> Impostors = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            Impostors.RemoveAll(x => !x.Data.Role.IsImpostor);

            var CrewmateCount = CustomOptionsH.CrewmateRolesCount.getSelection();
            var ImpostorCount = CustomOptionsH.ImpostorRolesCount.getSelection();
            var NeutralCount = CustomOptionsH.NeutralRolesCount.getSelection();
            var ModifierCount = CustomOptionsH.ModifierCount.getSelection();

            while (Crewmates.Count > 0 && CrewmateCount > 0)
            {
                List<PlayerControl> TargetPlayers = new();
                var AssignRole = EnabledCrewRoles[rnd.Next(0, EnabledCrewRoles.Count - 1)];
                TargetPlayers.AddRange(Crewmates);
                var AssignedPlayer = SetRoleToRandomPlayer((byte)AssignRole, TargetPlayers);
                CrewmateCount--;
            }

            while (Impostors.Count > 0 && ImpostorCount > 0)
            {
                List<PlayerControl> TargetPlayers = new();
                var AssignRole = EnabledImpRoles[rnd.Next(0, EnabledImpRoles.Count - 1)];
                TargetPlayers.AddRange(Impostors);
                var AssignedPlayer = SetRoleToRandomPlayer((byte)AssignRole, TargetPlayers);
                ImpostorCount--;
            }

            while (Crewmates.Count > 0 && NeutralCount > 0)
            {
                List<PlayerControl> TargetPlayers = new();
                var AssignRole = EnabledNeuRoles[rnd.Next(0, EnabledNeuRoles.Count - 1)];
                TargetPlayers.AddRange(Crewmates);
                var AssignedPlayer = SetRoleToRandomPlayer((byte)AssignRole, TargetPlayers);
                NeutralCount--;
            }

            while (Crewmates.Count > 0 && ModifierCount > 0)
            {
                List<PlayerControl> TargetPlayers = new();
                var AssignRole = EnabledModRoles[rnd.Next(0, EnabledModRoles.Count - 1)];
                TargetPlayers.AddRange(Crewmates);
                var AssignedPlayer = SetRoleToRandomPlayer((byte)AssignRole, TargetPlayers);
                ModifierCount--;
            }
        }

        private static byte SetRoleToRandomPlayer(byte RoleId, List<PlayerControl> PlayerList, byte Flag = 0, bool RemovePlayer = true)
        {
            var Index = rnd.Next(0, PlayerList.Count);
            byte PlayerId = PlayerList[Index].PlayerId;

            if (RemovePlayer) PlayerList.RemoveAt(Index);

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetRole, Hazel.SendOption.Reliable, -1);
            writer.Write(RoleId);
            writer.Write(PlayerId);
            writer.Write(Flag);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.SetRole(RoleId, PlayerId, Flag);
            return PlayerId;
        }

        private static byte setModifierToRandomPlayer(byte modId, List<PlayerControl> playerList)
        {
            if (playerList.Count <= 0)
            {
                return byte.MaxValue;
            }

            var index = rnd.Next(0, playerList.Count);
            byte playerId = playerList[index].PlayerId;

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.AddModifier, Hazel.SendOption.Reliable, -1);
            writer.Write(modId);
            writer.Write(playerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.AddModifier(modId, playerId);
            return playerId;
        }

        public static void Postfix()
        {
            ListUpEnabledRoles();
            AssignRolesAndModifiers();
        }
    }
}