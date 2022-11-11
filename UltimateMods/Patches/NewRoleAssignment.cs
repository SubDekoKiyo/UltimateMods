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

            if (CustomOptionsH.ActivateModRoles.getBool()) __result = 0; // Deactivate Vanilla Roles if the mod Roles are active
        }
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    class RoleManagerSelectRolesPatch
    {
        // private static List<byte> blockLovers = new();
        public static int BlockedAssignments = 0;
        public static int MaxBlocks = 10;

        public static void Postfix()
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ResetVariables, Hazel.SendOption.Reliable, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.ResetVariables();

            if (!DestroyableSingleton<TutorialManager>.InstanceExists && CustomOptionsH.ActivateModRoles.getBool()) // Don't assign Roles in Tutorial or if deactivated
            {
                AssignRoles();
            }
        }

        private static void AssignRoles()
        {
            var data = GetRoleAndModifierAssignmentData();
            SelectFactionForFactionIndependentRoles(data);
            AssignEnsuredRoles(data);
            AssignChanceRoles(data);
            AssignEnsuredModifier(data);
            AssignChanceModifier(data);
        }

        private static RoleAssignmentData GetRoleAndModifierAssignmentData()
        {
            // Get the players that we want to assign the Roles to. Crewmate and Neutral Roles are assigned to natural Crewmates. Impostor Roles to Impostors.
            List<PlayerControl> Crewmates = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            Crewmates.RemoveAll(x => x.Data.Role.IsImpostor);
            List<PlayerControl> Impostors = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            Impostors.RemoveAll(x => !x.Data.Role.IsImpostor);

            var CrewmateMin = CustomOptionsH.CrewmateRolesCountMin.getSelection();
            var CrewmateMax = CustomOptionsH.CrewmateRolesCountMax.getSelection();
            var NeutralMin = CustomOptionsH.NeutralRolesCountMin.getSelection();
            var NeutralMax = CustomOptionsH.NeutralRolesCountMax.getSelection();
            var ImpostorMin = CustomOptionsH.ImpostorRolesCountMin.getSelection();
            var ImpostorMax = CustomOptionsH.ImpostorRolesCountMax.getSelection();
            var ModifierMin = CustomOptionsH.ModifierCountMin.getSelection();
            var ModifierMax = CustomOptionsH.ModifierCountMax.getSelection();

            // Make sure Min is less or equal to Max
            if (CrewmateMin > CrewmateMax) CrewmateMin = CrewmateMax;
            if (NeutralMin > NeutralMax) NeutralMin = NeutralMax;
            if (ImpostorMin > ImpostorMax) ImpostorMin = ImpostorMax;
            if (ModifierMin > ModifierMax) ModifierMin = ModifierMax;

            // Get the Maximum allowed count of each Role type based on the Minimum and Maximum option
            int CrewCountSettings = rnd.Next(CrewmateMin, CrewmateMax + 1);
            int NeutralCountSettings = rnd.Next(NeutralMin, NeutralMax + 1);
            int ImpCountSettings = rnd.Next(ImpostorMin, ImpostorMax + 1);
            int ModifierCountSettings = rnd.Next(ModifierMin, ModifierMax + 1);

            // Potentially lower the actual Maximum to the assignable players
            int MaxCrewmateRoles = Mathf.Min(Crewmates.Count, CrewCountSettings);
            int MaxNeutralRoles = Mathf.Min(Crewmates.Count, NeutralCountSettings);
            int MaxImpostorRoles = Mathf.Min(Impostors.Count, ImpCountSettings);
            int MaxModifierCount = Mathf.Min(Crewmates.Count, ModifierCountSettings);

            // Fill in the lists with the Roles that should be assigned to players. Note that the special Roles (like Mafia or Lovers) are NOT included in these lists
            Dictionary<byte, (int rate, int count)> ImpSettings = new();
            Dictionary<byte, (int rate, int count)> NeutralSettings = new();
            Dictionary<byte, (int rate, int count)> CrewSettings = new();
            Dictionary<byte, (int rate, int count)> ModifierSettings = new();

            ImpSettings.Add((byte)RoleType.CustomImpostor, CustomRolesH.CustomImpostorRate.data);
            ImpSettings.Add((byte)RoleType.UnderTaker, CustomRolesH.UnderTakerRate.data);
            ImpSettings.Add((byte)RoleType.BountyHunter, CustomRolesH.BountyHunterRate.data);
            ImpSettings.Add((byte)RoleType.Teleporter, CustomRolesH.TeleporterRate.data);

            NeutralSettings.Add((byte)RoleType.Jester, CustomRolesH.JesterRate.data);

            CrewSettings.Add((byte)RoleType.Sheriff, CustomRolesH.SheriffRate.data);
            CrewSettings.Add((byte)RoleType.Engineer, CustomRolesH.EngineerRate.data);
            CrewSettings.Add((byte)RoleType.Madmate, CustomRolesH.MadmateRate.data);
            CrewSettings.Add((byte)RoleType.Bakery, CustomRolesH.BakeryRate.data);
            CrewSettings.Add((byte)RoleType.Altruist, CustomRolesH.AltruistRate.data);

            ModifierSettings.Add((byte)ModifierType.Opportunist, CustomRolesH.OpportunistRate.data);

            return new RoleAssignmentData
            {
                Crewmates = Crewmates,
                Impostors = Impostors,
                CrewSettings = CrewSettings,
                NeutralSettings = NeutralSettings,
                ImpSettings = ImpSettings,
                ModifierSettings = ModifierSettings,
                MaxCrewmateRoles = MaxCrewmateRoles,
                MaxNeutralRoles = MaxNeutralRoles,
                MaxImpostorRoles = MaxImpostorRoles,
                MaxModifier = MaxModifierCount
            };
        }

        private static void SelectFactionForFactionIndependentRoles(RoleAssignmentData data)
        {/*
            // Assign Guesser (chance to be Impostor based on Setting)
            bool isEvilGuesser = rnd.Next(1, 101) <= CustomOptionsH.guesserIsImpGuesserRate.getSelection() * 10;
            if (CustomOptionsH.guesserSpawnBothRate.getSelection() > 0)
            {
                if (rnd.Next(1, 101) <= CustomOptionsH.guesserSpawnRate.getSelection() * 10)
                {
                    if (isEvilGuesser)
                    {
                        if (data.Impostors.Count > 0 && data.MaxImpostorRoles > 0)
                        {
                            byte evilGuesser = SetRoleToRandomPlayer((byte)RoleType.EvilGuesser, data.Impostors);
                            data.Impostors.ToList().RemoveAll(x => x.PlayerId == evilGuesser);
                            data.MaxImpostorRoles--;
                            data.CrewSettings.Add((byte)RoleType.NiceGuesser, (CustomOptionsH.guesserSpawnBothRate.getSelection(), 1));
                        }
                    }
                    else if (data.Crewmates.Count > 0 && data.MaxCrewmateRoles > 0)
                    {
                        byte niceGuesser = SetRoleToRandomPlayer((byte)RoleType.NiceGuesser, data.Crewmates);
                        data.Crewmates.ToList().RemoveAll(x => x.PlayerId == niceGuesser);
                        data.MaxCrewmateRoles--;
                        data.ImpSettings.Add((byte)RoleType.EvilGuesser, (CustomOptionsH.guesserSpawnBothRate.getSelection(), 1));
                    }
                }
            }
            else
            {
                if (isEvilGuesser) data.ImpSettings.Add((byte)RoleType.EvilGuesser, (CustomOptionsH.guesserSpawnRate.getSelection(), 1));
                else data.CrewSettings.Add((byte)RoleType.NiceGuesser, (CustomOptionsH.guesserSpawnRate.getSelection(), 1));
            }*/

            // Assign any dual Role types
            foreach (var option in CustomDualRoleOption.dualRoles)
            {
                if (option.count <= 0 || !option.roleEnabled) continue;

                int niceCount = 0;
                int evilCount = 0;
                while (niceCount + evilCount < option.count)
                {
                    if (option.assignEqually)
                    {
                        niceCount++;
                        evilCount++;
                    }
                    else
                    {
                        bool isEvil = rnd.Next(1, 101) <= option.impChance * 10;
                        if (isEvil) evilCount++;
                        else niceCount++;
                    }
                }

                if (niceCount > 0)
                    data.CrewSettings.Add((byte)option.roleType, (option.rate, niceCount));

                if (evilCount > 0)
                    data.ImpSettings.Add((byte)option.roleType, (option.rate, evilCount));
            }
        }

        private static void AssignEnsuredRoles(RoleAssignmentData data)
        {
            BlockedAssignments = 0;

            // Get all Roles where the chance to occur is Set to 100%
            List<byte> EnsuredCrewmateRoles = data.CrewSettings.Where(x => x.Value.rate == 10).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).ToList();
            List<byte> EnsuredNeutralRoles = data.NeutralSettings.Where(x => x.Value.rate == 10).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).ToList();
            List<byte> EnsuredImpostorRoles = data.ImpSettings.Where(x => x.Value.rate == 10).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).ToList();

            // Assign Roles until we run out of either players we can assign Roles to or run out of Roles we can assign to players
            while ((data.Impostors.Count > 0 && data.MaxImpostorRoles > 0 && EnsuredImpostorRoles.Count > 0) || (data.Crewmates.Count > 0 && ((data.MaxCrewmateRoles > 0 && EnsuredCrewmateRoles.Count > 0) || (data.MaxNeutralRoles > 0 && EnsuredNeutralRoles.Count > 0))))
            {
                Dictionary<TeamType, List<byte>> RolesToAssign = new();
                if (data.Crewmates.Count > 0 && data.MaxCrewmateRoles > 0 && EnsuredCrewmateRoles.Count > 0) RolesToAssign.Add(TeamType.Crewmate, EnsuredCrewmateRoles);
                if (data.Crewmates.Count > 0 && data.MaxNeutralRoles > 0 && EnsuredNeutralRoles.Count > 0) RolesToAssign.Add(TeamType.Neutral, EnsuredNeutralRoles);
                if (data.Impostors.Count > 0 && data.MaxImpostorRoles > 0 && EnsuredImpostorRoles.Count > 0) RolesToAssign.Add(TeamType.Impostor, EnsuredImpostorRoles);

                // Randomly select a pool of Roles to assign a Role from next (Crewmate Role, Neutral Role or Impostor Role)
                // then select one of the Roles from the selected pool to a player
                // and remove the Role (and any potentially Blocked Role pairings) from the pool(s)
                var RoleType = RolesToAssign.Keys.ElementAt(rnd.Next(0, RolesToAssign.Keys.Count()));
                var players = RoleType is TeamType.Crewmate or TeamType.Neutral ? data.Crewmates : data.Impostors;
                var Index = rnd.Next(0, RolesToAssign[RoleType].Count);
                var RoleId = RolesToAssign[RoleType][Index];
                var player = SetRoleToRandomPlayer(RolesToAssign[RoleType][Index], players);
                if (player == byte.MaxValue && BlockedAssignments < MaxBlocks)
                {
                    BlockedAssignments++;
                    continue;
                }
                BlockedAssignments = 0;

                RolesToAssign[RoleType].RemoveAt(Index);

                if (CustomOptionsH.blockedRolePairings.ContainsKey(RoleId))
                {
                    foreach (var BlockedRoleId in CustomOptionsH.blockedRolePairings[RoleId])
                    {
                        // Set chance for the Blocked Roles to 0 for chances less than 100%
                        if (data.ImpSettings.ContainsKey(BlockedRoleId)) data.ImpSettings[BlockedRoleId] = (0, 0);
                        if (data.NeutralSettings.ContainsKey(BlockedRoleId)) data.NeutralSettings[BlockedRoleId] = (0, 0);
                        if (data.CrewSettings.ContainsKey(BlockedRoleId)) data.CrewSettings[BlockedRoleId] = (0, 0);
                        // Remove Blocked Roles even if the chance was 100%
                        foreach (var EnsuredRolesList in RolesToAssign.Values)
                        {
                            EnsuredRolesList.RemoveAll(x => x == BlockedRoleId);
                        }
                    }
                }

                // Adjust the Role limit
                switch (RoleType)
                {
                    case TeamType.Crewmate: data.MaxCrewmateRoles--; break;
                    case TeamType.Neutral: data.MaxNeutralRoles--; break;
                    case TeamType.Impostor: data.MaxImpostorRoles--; break;
                }
            }
        }

        private static void AssignChanceRoles(RoleAssignmentData data)
        {
            BlockedAssignments = 0;

            // Get all Roles where the chance to occur is Set grater than 0% but not 100% and build a ticket pool based on their weight
            List<byte> CrewmateTickets = data.CrewSettings.Where(x => x.Value.rate is > 0 and < 10).Select(x => Enumerable.Repeat(x.Key, x.Value.rate * x.Value.count)).SelectMany(x => x).ToList();
            List<byte> NeutralTickets = data.NeutralSettings.Where(x => x.Value.rate is > 0 and < 10).Select(x => Enumerable.Repeat(x.Key, x.Value.rate * x.Value.count)).SelectMany(x => x).ToList();
            List<byte> ImpostorTickets = data.ImpSettings.Where(x => x.Value.rate is > 0 and < 10).Select(x => Enumerable.Repeat(x.Key, x.Value.rate * x.Value.count)).SelectMany(x => x).ToList();

            // Assign Roles until we run out of either players we can assign Roles to or run out of Roles we can assign to players
            while (
                (data.Impostors.Count > 0 && data.MaxImpostorRoles > 0 && ImpostorTickets.Count > 0) ||
                (data.Crewmates.Count > 0 && (
                    (data.MaxCrewmateRoles > 0 && CrewmateTickets.Count > 0) ||
                    (data.MaxNeutralRoles > 0 && NeutralTickets.Count > 0)
                )))
            {

                Dictionary<TeamType, List<byte>> RolesToAssign = new();
                if (data.Crewmates.Count > 0 && data.MaxCrewmateRoles > 0 && CrewmateTickets.Count > 0) RolesToAssign.Add(TeamType.Crewmate, CrewmateTickets);
                if (data.Crewmates.Count > 0 && data.MaxNeutralRoles > 0 && NeutralTickets.Count > 0) RolesToAssign.Add(TeamType.Neutral, NeutralTickets);
                if (data.Impostors.Count > 0 && data.MaxImpostorRoles > 0 && ImpostorTickets.Count > 0) RolesToAssign.Add(TeamType.Impostor, ImpostorTickets);

                // Randomly select a pool of Role tickets to assign a Role from next (Crewmate Role, Neutral Role or Impostor Role)
                // then select one of the Roles from the selected pool to a player
                // and remove all tickets of this Role (and any potentially Blocked Role pairings) from the pool(s)
                var RoleType = RolesToAssign.Keys.ElementAt(rnd.Next(0, RolesToAssign.Keys.Count()));
                var players = RoleType is TeamType.Crewmate or TeamType.Neutral ? data.Crewmates : data.Impostors;
                var Index = rnd.Next(0, RolesToAssign[RoleType].Count);
                var RoleId = RolesToAssign[RoleType][Index];
                var player = SetRoleToRandomPlayer(RolesToAssign[RoleType][Index], players);
                if (player == byte.MaxValue && BlockedAssignments < MaxBlocks)
                {
                    BlockedAssignments++;
                    continue;
                }
                BlockedAssignments = 0;

                RolesToAssign[RoleType].RemoveAll(x => x == RoleId);

                if (CustomOptionsH.blockedRolePairings.ContainsKey(RoleId))
                {
                    foreach (var BlockedRoleId in CustomOptionsH.blockedRolePairings[RoleId])
                    {
                        // Remove tickets of Blocked Roles from all pools
                        CrewmateTickets.RemoveAll(x => x == BlockedRoleId);
                        NeutralTickets.RemoveAll(x => x == BlockedRoleId);
                        ImpostorTickets.RemoveAll(x => x == BlockedRoleId);
                    }
                }

                // Adjust the Role limit
                switch (RoleType)
                {
                    case TeamType.Crewmate: data.MaxCrewmateRoles--; break;
                    case TeamType.Neutral: data.MaxNeutralRoles--; break;
                    case TeamType.Impostor: data.MaxImpostorRoles--; break;
                }
            }
        }

        private static void AssignEnsuredModifier(RoleAssignmentData data)
        {
            List<byte> EnsuredModifier = data.ModifierSettings.Where(x => x.Value.rate == 10).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).ToList();

            while (data.Crewmates.Count > 0 || (data.MaxModifier > 0 && EnsuredModifier.Count > 0))
            {
                Dictionary<TeamType, List<byte>> ModifierToAssign = new();
                if (data.Crewmates.Count > 0 && data.MaxModifier > 0 && EnsuredModifier.Count > 0) ModifierToAssign.Add(TeamType.Modifier, EnsuredModifier);

                var ModifierType = ModifierToAssign.Keys.ElementAt(rnd.Next(0, ModifierToAssign.Keys.Count()));
                var Index = rnd.Next(0, ModifierToAssign[ModifierType].Count);
                var ModifierId = ModifierToAssign[ModifierType][Index];
                List<PlayerControl> player = PlayerControl.AllPlayerControls.ToArray().ToList();
                SetModifierToRandomPlayer(ModifierId, player);
                ModifierToAssign[ModifierType].RemoveAt(Index);
                data.MaxModifier--;
            }
        }

        private static void AssignChanceModifier(RoleAssignmentData data)
        {
            List<byte> ModifierTickets = data.ModifierSettings.Where(x => x.Value.rate is > 0 and < 10).Select(x => Enumerable.Repeat(x.Key, x.Value.rate * x.Value.count)).SelectMany(x => x).ToList();

            while ((data.Impostors.Count > 0 && data.Crewmates.Count > 0) && (data.MaxModifier > 0 && ModifierTickets.Count > 0))
            {
                Dictionary<TeamType, List<byte>> ModifierToAssign = new();
                if (data.Crewmates.Count > 0 && data.MaxModifier > 0 && ModifierTickets.Count > 0) ModifierToAssign.Add(TeamType.Modifier, ModifierTickets);

                var ModifierType = ModifierToAssign.Keys.ElementAt(rnd.Next(0, ModifierToAssign.Keys.Count()));
                var Index = rnd.Next(0, ModifierToAssign[ModifierType].Count);
                var ModifierId = ModifierToAssign[ModifierType][Index];
                List<PlayerControl> player = PlayerControl.AllPlayerControls.ToArray().ToList();
                SetModifierToRandomPlayer(ModifierId, player);
                ModifierToAssign[ModifierType].RemoveAll(x => x == ModifierId);
                data.MaxModifier--;
            }
        }

        private static byte SetRoleToHost(byte RoleId, PlayerControl host, byte flag = 0)
        {
            byte playerId = host.PlayerId;

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetRole, Hazel.SendOption.Reliable, -1);
            writer.Write(RoleId);
            writer.Write(playerId);
            writer.Write(flag);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.SetRole(RoleId, playerId, flag);
            return playerId;
        }

        private static byte SetRoleToRandomPlayer(byte RoleId, List<PlayerControl> playerList, byte flag = 0, bool removePlayer = true)
        {
            var Index = rnd.Next(0, playerList.Count);
            byte playerId = playerList[Index].PlayerId;

            if (removePlayer) playerList.RemoveAt(Index);

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetRole, Hazel.SendOption.Reliable, -1);
            writer.Write(RoleId);
            writer.Write(playerId);
            writer.Write(flag);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.SetRole(RoleId, playerId, flag);
            return playerId;
        }

        private static byte SetModifierToRandomPlayer(byte modId, List<PlayerControl> playerList)
        {
            if (playerList.Count <= 0)
            {
                return byte.MaxValue;
            }

            var Index = rnd.Next(0, playerList.Count);
            byte playerId = playerList[Index].PlayerId;

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.AddModifier, Hazel.SendOption.Reliable, -1);
            writer.Write(modId);
            writer.Write(playerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.AddModifier(modId, playerId);
            return playerId;
        }

        private class RoleAssignmentData
        {
            public List<PlayerControl> Crewmates { get; set; }
            public List<PlayerControl> Impostors { get; set; }
            public Dictionary<byte, (int rate, int count)> ImpSettings = new();
            public Dictionary<byte, (int rate, int count)> NeutralSettings = new();
            public Dictionary<byte, (int rate, int count)> CrewSettings = new();
            public Dictionary<byte, (int rate, int count)> ModifierSettings = new();
            public int MaxCrewmateRoles { get; set; }
            public int MaxNeutralRoles { get; set; }
            public int MaxImpostorRoles { get; set; }
            public int MaxModifier { get; set; }
        }

        private enum TeamType
        {
            Crewmate = 0,
            Neutral = 1,
            Impostor = 2,
            Modifier = 3,
        }
    }
}