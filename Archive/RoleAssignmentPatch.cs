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
    class RoleManagerSelectRolesPatch
    {
        // private static List<byte> BlockLovers = new();
        public static int BlockedAssignments = 0;
        public static int MaxBlocks = 10;

        public static void Postfix()
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ResetVariables, Hazel.SendOption.Reliable, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.ResetVariables();

            if (!DestroyableSingleton<TutorialManager>.InstanceExists && CustomOptionsH.ActivateModRoles.getBool()) // Don't assign Roles in Tutorial or if deactivated
                assignRoles();
        }

        private static void assignRoles()
        {/*
            BlockLovers = new List<byte> {
                (byte)RoleType.Bait,
            };

            if (!Lovers.hasTasks)
            {
                BlockLovers.Add((byte)RoleType.Snitch);
                BlockLovers.Add((byte)RoleType.FortuneTeller);
                //BlockLovers.Add((byte)RoleType.Sunfish);
                BlockLovers.Add((byte)RoleType.Fox);
            }

            if (!CustomOptionsH.arsonistCanBeLovers.getBool())
            {
                BlockLovers.Add((byte)RoleType.Arsonist);
            }*/

            var data = getRoleAssignmentData();
            assignSpecialRoles(data);
            selectFactionForFactionIndependentRoles(data);
            assignEnsuredRoles(data);
            assignChanceRoles(data);
            assignRoleModifiers(data);
        }

        private static RoleAssignmentData getRoleAssignmentData()
        {
            // Get the players that we want to assign the roles to. Crewmate and Neutral roles are assigned to natural crewmates. Impostor roles to impostors.
            List<PlayerControl> crewmates = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            crewmates.RemoveAll(x => x.Data.Role.IsImpostor);
            List<PlayerControl> impostors = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            impostors.RemoveAll(x => !x.Data.Role.IsImpostor);

            var crewmateMin = CustomOptionsH.CrewmateRolesCountMin.getSelection();
            var crewmateMax = CustomOptionsH.CrewmateRolesCountMax.getSelection();
            var neutralMin = CustomOptionsH.NeutralRolesCountMin.getSelection();
            var neutralMax = CustomOptionsH.NeutralRolesCountMax.getSelection();
            var impostorMin = CustomOptionsH.ImpostorRolesCountMin.getSelection();
            var impostorMax = CustomOptionsH.ImpostorRolesCountMax.getSelection();

            // Make sure min is less or equal to Max
            if (crewmateMin > crewmateMax) crewmateMin = crewmateMax;
            if (neutralMin > neutralMax) neutralMin = neutralMax;
            if (impostorMin > impostorMax) impostorMin = impostorMax;

            // Get the Maximum allowed count of each role type based on the minimum and Maximum option
            int crewCountSettings = rnd.Next(crewmateMin, crewmateMax + 1);
            int neutralCountSettings = rnd.Next(neutralMin, neutralMax + 1);
            int impCountSettings = rnd.Next(impostorMin, impostorMax + 1);

            // Potentially lower the actual Maximum to the assignable players
            int MaxCrewmateRoles = Mathf.Min(crewmates.Count, crewCountSettings);
            int MaxNeutralRoles = Mathf.Min(crewmates.Count, neutralCountSettings);
            int MaxImpostorRoles = Mathf.Min(impostors.Count, impCountSettings);

            // Fill in the lists with the roles that should be assigned to players. Note that the special roles (like Mafia or Lovers) are NOT included in these lists
            Dictionary<byte, (int rate, int count)> impSettings = new();
            Dictionary<byte, (int rate, int count)> neutralSettings = new();
            Dictionary<byte, (int rate, int count)> crewSettings = new();

            impSettings.Add((byte)RoleType.CustomImpostor, CustomRolesH.CustomImpostorRate.data);
            impSettings.Add((byte)RoleType.UnderTaker, CustomRolesH.UnderTakerRate.data);
            impSettings.Add((byte)RoleType.BountyHunter, CustomRolesH.BountyHunterRate.data);
            impSettings.Add((byte)RoleType.Teleporter, CustomRolesH.TeleporterRate.data);

            neutralSettings.Add((byte)RoleType.Jester, CustomRolesH.JesterRate.data);

            crewSettings.Add((byte)RoleType.Sheriff, CustomRolesH.SheriffRate.data);
            crewSettings.Add((byte)RoleType.Engineer, CustomRolesH.EngineerRate.data);
            crewSettings.Add((byte)RoleType.Madmate, CustomRolesH.MadmateRate.data);
            crewSettings.Add((byte)RoleType.Bakery, CustomRolesH.BakeryRate.data);
            crewSettings.Add((byte)RoleType.Altruist, CustomRolesH.AltruistRate.data);

            return new RoleAssignmentData
            {
                crewmates = crewmates,
                impostors = impostors,
                crewSettings = crewSettings,
                neutralSettings = neutralSettings,
                impSettings = impSettings,
                MaxCrewmateRoles = MaxCrewmateRoles,
                MaxNeutralRoles = MaxNeutralRoles,
                MaxImpostorRoles = MaxImpostorRoles
            };
        }

        private static void assignSpecialRoles(RoleAssignmentData data)
        {/*
            // Assign Lovers
            for (int i = 0; i < CustomOptionsH.loversNumCouples.getFloat(); i++)
            {
                var singleCrew = data.crewmates.FindAll(x => !x.isLovers());
                var singleImps = data.impostors.FindAll(x => !x.isLovers());

                bool isOnlyRole = !CustomOptionsH.loversCanHaveAnotherRole.getBool();
                if (rnd.Next(1, 101) <= CustomOptionsH.loversSpawnRate.getSelection() * 10)
                {
                    int lover1 = -1;
                    int lover2 = -1;
                    int lover1Index = -1;
                    int lover2Index = -1;
                    if (singleImps.Count > 0 && singleCrew.Count > 0 && (!isOnlyRole || (data.MaxCrewmateRoles > 0 && data.MaxImpostorRoles > 0)) && rnd.Next(1, 101) <= CustomOptionsH.loversImpLoverRate.getSelection() * 10)
                    {
                        lover1Index = rnd.Next(0, singleImps.Count);
                        lover1 = singleImps[lover1Index].PlayerId;

                        lover2Index = rnd.Next(0, singleCrew.Count);
                        lover2 = singleCrew[lover2Index].PlayerId;

                        if (isOnlyRole)
                        {
                            data.MaxImpostorRoles--;
                            data.MaxCrewmateRoles--;

                            data.impostors.RemoveAll(x => x.PlayerId == lover1);
                            data.crewmates.RemoveAll(x => x.PlayerId == lover2);
                        }
                    }

                    else if (singleCrew.Count >= 2 && (isOnlyRole || data.MaxCrewmateRoles >= 2))
                    {
                        lover1Index = rnd.Next(0, singleCrew.Count);
                        while (lover2Index == lover1Index || lover2Index < 0) lover2Index = rnd.Next(0, singleCrew.Count);

                        lover1 = singleCrew[lover1Index].PlayerId;
                        lover2 = singleCrew[lover2Index].PlayerId;

                        if (isOnlyRole)
                        {
                            data.MaxCrewmateRoles -= 2;
                            data.crewmates.RemoveAll(x => x.PlayerId == lover1);
                            data.crewmates.RemoveAll(x => x.PlayerId == lover2);
                        }
                    }

                    if (lover1 >= 0 && lover2 >= 0)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetLovers, Hazel.SendOption.Reliable, -1);
                        writer.Write((byte)lover1);
                        writer.Write((byte)lover2);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.setLovers((byte)lover1, (byte)lover2);
                    }
                }
            }*/
        }

        private static void selectFactionForFactionIndependentRoles(RoleAssignmentData data)
        {/*
            // Assign Guesser (chance to be impostor based on setting)
            bool isEvilGuesser = rnd.Next(1, 101) <= CustomOptionsH.guesserIsImpGuesserRate.getSelection() * 10;
            if (CustomOptionsH.guesserSpawnBothRate.getSelection() > 0)
            {
                if (rnd.Next(1, 101) <= CustomOptionsH.guesserSpawnRate.getSelection() * 10)
                {
                    if (isEvilGuesser)
                    {
                        if (data.impostors.Count > 0 && data.MaxImpostorRoles > 0)
                        {
                            byte EvilGuesser = setRoleToRandomPlayer((byte)RoleType.EvilGuesser, data.impostors);
                            data.impostors.ToList().RemoveAll(x => x.PlayerId == EvilGuesser);
                            data.MaxImpostorRoles--;
                            data.crewSettings.Add((byte)RoleType.NiceGuesser, (CustomOptionsH.guesserSpawnBothRate.getSelection(), 1));
                        }
                    }
                    else if (data.crewmates.Count > 0 && data.MaxCrewmateRoles > 0)
                    {
                        byte NiceGuesser = setRoleToRandomPlayer((byte)RoleType.NiceGuesser, data.crewmates);
                        data.crewmates.ToList().RemoveAll(x => x.PlayerId == NiceGuesser);
                        data.MaxCrewmateRoles--;
                        data.impSettings.Add((byte)RoleType.EvilGuesser, (CustomOptionsH.guesserSpawnBothRate.getSelection(), 1));
                    }
                }
            }
            else
            {
                if (isEvilGuesser) data.impSettings.Add((byte)RoleType.EvilGuesser, (CustomOptionsH.guesserSpawnRate.getSelection(), 1));
                else data.crewSettings.Add((byte)RoleType.NiceGuesser, (CustomOptionsH.guesserSpawnRate.getSelection(), 1));
            }*/

            // Assign any dual role types
            foreach (var option in CustomDualRoleOption.dualRoles)
            {
                if (option.count <= 0 || !option.roleEnabled) continue;

                int NiceCount = 0;
                int EvilCount = 0;
                while (NiceCount + EvilCount < option.count)
                {
                    if (option.assignEqually)
                    {
                        NiceCount++;
                        EvilCount++;
                    }
                    else
                    {
                        bool isEvil = rnd.Next(1, 101) <= option.impChance * 10;
                        if (isEvil) EvilCount++;
                        else NiceCount++;
                    }
                }

                if (NiceCount > 0)
                    data.crewSettings.Add((byte)option.roleType, (option.rate, NiceCount));

                if (EvilCount > 0)
                    data.impSettings.Add((byte)option.roleType, (option.rate, EvilCount));
            }
        }

        private static void assignEnsuredRoles(RoleAssignmentData data)
        {
            BlockedAssignments = 0;

            // Get all roles where the chance to occur is set to 100%
            List<byte> EnsuredCrewmateRoles = data.crewSettings.Where(x => x.Value.rate == 10).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).ToList();
            List<byte> EnsuredNeutralRoles = data.neutralSettings.Where(x => x.Value.rate == 10).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).ToList();
            List<byte> EnsuredImpostorRoles = data.impSettings.Where(x => x.Value.rate == 10).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).ToList();

            // Assign roles until we run out of either players we can assign roles to or run out of roles we can assign to players
            while ((data.impostors.Count > 0 && data.MaxImpostorRoles > 0 && EnsuredImpostorRoles.Count > 0) || (data.crewmates.Count > 0 && ((data.MaxCrewmateRoles > 0 && EnsuredCrewmateRoles.Count > 0) || (data.MaxNeutralRoles > 0 && EnsuredNeutralRoles.Count > 0))))
            {
                Dictionary<TeamType, List<byte>> rolesToAssign = new();
                if (data.crewmates.Count > 0 && data.MaxCrewmateRoles > 0 && EnsuredCrewmateRoles.Count > 0) rolesToAssign.Add(TeamType.Crewmate, EnsuredCrewmateRoles);
                if (data.crewmates.Count > 0 && data.MaxNeutralRoles > 0 && EnsuredNeutralRoles.Count > 0) rolesToAssign.Add(TeamType.Neutral, EnsuredNeutralRoles);
                if (data.impostors.Count > 0 && data.MaxImpostorRoles > 0 && EnsuredImpostorRoles.Count > 0) rolesToAssign.Add(TeamType.Impostor, EnsuredImpostorRoles);

                // Randomly select a pool of roles to assign a role from next (Crewmate role, Neutral role or Impostor role)
                // then select one of the roles from the selected pool to a player
                // and remove the role (and any potentially Blocked role pairings) from the pool(s)
                var roleType = rolesToAssign.Keys.ElementAt(rnd.Next(0, rolesToAssign.Keys.Count()));
                var players = roleType is TeamType.Crewmate or TeamType.Neutral ? data.crewmates : data.impostors;
                var index = rnd.Next(0, rolesToAssign[roleType].Count);
                var roleId = rolesToAssign[roleType][index];
                var player = setRoleToRandomPlayer(rolesToAssign[roleType][index], players);
                if (player == byte.MaxValue && BlockedAssignments < MaxBlocks)
                {
                    BlockedAssignments++;
                    continue;
                }
                BlockedAssignments = 0;

                rolesToAssign[roleType].RemoveAt(index);

                if (CustomOptionsH.BlockedRolePairings.ContainsKey(roleId))
                {
                    foreach (var BlockedRoleId in CustomOptionsH.BlockedRolePairings[roleId])
                    {
                        // Set chance for the Blocked roles to 0 for chances less than 100%
                        if (data.impSettings.ContainsKey(BlockedRoleId)) data.impSettings[BlockedRoleId] = (0, 0);
                        if (data.neutralSettings.ContainsKey(BlockedRoleId)) data.neutralSettings[BlockedRoleId] = (0, 0);
                        if (data.crewSettings.ContainsKey(BlockedRoleId)) data.crewSettings[BlockedRoleId] = (0, 0);
                        // Remove Blocked roles even if the chance was 100%
                        foreach (var EnsuredRolesList in rolesToAssign.Values)
                        {
                            EnsuredRolesList.RemoveAll(x => x == BlockedRoleId);
                        }
                    }
                }

                // Adjust the role limit
                switch (roleType)
                {
                    case TeamType.Crewmate: data.MaxCrewmateRoles--; break;
                    case TeamType.Neutral: data.MaxNeutralRoles--; break;
                    case TeamType.Impostor: data.MaxImpostorRoles--; break;
                }
            }
        }


        private static void assignChanceRoles(RoleAssignmentData data)
        {
            BlockedAssignments = 0;

            // Get all roles where the chance to occur is set grater than 0% but not 100% and build a ticket pool based on their weight
            List<byte> crewmateTickets = data.crewSettings.Where(x => x.Value.rate is > 0 and < 10).Select(x => Enumerable.Repeat(x.Key, x.Value.rate * x.Value.count)).SelectMany(x => x).ToList();
            List<byte> neutralTickets = data.neutralSettings.Where(x => x.Value.rate is > 0 and < 10).Select(x => Enumerable.Repeat(x.Key, x.Value.rate * x.Value.count)).SelectMany(x => x).ToList();
            List<byte> impostorTickets = data.impSettings.Where(x => x.Value.rate is > 0 and < 10).Select(x => Enumerable.Repeat(x.Key, x.Value.rate * x.Value.count)).SelectMany(x => x).ToList();

            // Assign roles until we run out of either players we can assign roles to or run out of roles we can assign to players
            while (
                (data.impostors.Count > 0 && data.MaxImpostorRoles > 0 && impostorTickets.Count > 0) ||
                (data.crewmates.Count > 0 && (
                    (data.MaxCrewmateRoles > 0 && crewmateTickets.Count > 0) ||
                    (data.MaxNeutralRoles > 0 && neutralTickets.Count > 0)
                )))
            {

                Dictionary<TeamType, List<byte>> rolesToAssign = new();
                if (data.crewmates.Count > 0 && data.MaxCrewmateRoles > 0 && crewmateTickets.Count > 0) rolesToAssign.Add(TeamType.Crewmate, crewmateTickets);
                if (data.crewmates.Count > 0 && data.MaxNeutralRoles > 0 && neutralTickets.Count > 0) rolesToAssign.Add(TeamType.Neutral, neutralTickets);
                if (data.impostors.Count > 0 && data.MaxImpostorRoles > 0 && impostorTickets.Count > 0) rolesToAssign.Add(TeamType.Impostor, impostorTickets);

                // Randomly select a pool of role tickets to assign a role from next (Crewmate role, Neutral role or Impostor role)
                // then select one of the roles from the selected pool to a player
                // and remove all tickets of this role (and any potentially Blocked role pairings) from the pool(s)
                var roleType = rolesToAssign.Keys.ElementAt(rnd.Next(0, rolesToAssign.Keys.Count()));
                var players = roleType is TeamType.Crewmate or TeamType.Neutral ? data.crewmates : data.impostors;
                var index = rnd.Next(0, rolesToAssign[roleType].Count);
                var roleId = rolesToAssign[roleType][index];
                var player = setRoleToRandomPlayer(rolesToAssign[roleType][index], players);
                if (player == byte.MaxValue && BlockedAssignments < MaxBlocks)
                {
                    BlockedAssignments++;
                    continue;
                }
                BlockedAssignments = 0;

                rolesToAssign[roleType].RemoveAll(x => x == roleId);

                if (CustomOptionsH.BlockedRolePairings.ContainsKey(roleId))
                {
                    foreach (var BlockedRoleId in CustomOptionsH.BlockedRolePairings[roleId])
                    {
                        // Remove tickets of Blocked roles from all pools
                        crewmateTickets.RemoveAll(x => x == BlockedRoleId);
                        neutralTickets.RemoveAll(x => x == BlockedRoleId);
                        impostorTickets.RemoveAll(x => x == BlockedRoleId);
                    }
                }

                // Adjust the role limit
                switch (roleType)
                {
                    case TeamType.Crewmate: data.MaxCrewmateRoles--; break;
                    case TeamType.Neutral: data.MaxNeutralRoles--; break;
                    case TeamType.Impostor: data.MaxImpostorRoles--; break;
                }
            }
        }

        private static byte setRoleToHost(byte roleId, PlayerControl host, byte flag = 0)
        {
            byte playerId = host.PlayerId;

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetRole, Hazel.SendOption.Reliable, -1);
            writer.Write(roleId);
            writer.Write(playerId);
            writer.Write(flag);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.SetRole(roleId, playerId, flag);
            return playerId;
        }

        private static void assignRoleModifiers(RoleAssignmentData data)
        {/*
            // AntiTeleport
            for (int i = 0; i < CustomOptionsH.antiTeleportSpawnRate.count; i++)
            {
                if (rnd.Next(1, 100) <= CustomOptionsH.antiTeleportSpawnRate.rate * 10)
                {
                    var candidates = AntiTeleport.candidates;
                    if (candidates.Count <= 0)
                    {
                        break;
                    }
                    setModifierToRandomPlayer((byte)ModifierType.AntiTeleport, AntiTeleport.candidates);
                }
            }*/

            // Opportunist
            for (int i = 0; i < CustomRolesH.OpportunistRate.count; i++)
            {
                if (rnd.Next(1, 100) <= CustomRolesH.OpportunistRate.rate * 10)
                {
                    var candidates = Opportunist.Candidates;
                    if (candidates.Count <= 0)
                    {
                        break;
                    }
                    setModifierToRandomPlayer((byte)ModifierType.Opportunist, Opportunist.Candidates);
                }
            }
        }

        private static byte setRoleToRandomPlayer(byte roleId, List<PlayerControl> playerList, byte flag = 0, bool removePlayer = true)
        {
            var index = rnd.Next(0, playerList.Count);
            byte playerId = playerList[index].PlayerId;/*
            if (RoleInfo.lovers.enabled &&
                Helpers.playerById(playerId)?.isLovers() == true &&
                BlockLovers.Contains(roleId))
            {
                return byte.MaxValue;
            }*/

            if (removePlayer) playerList.RemoveAt(index);

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetRole, Hazel.SendOption.Reliable, -1);
            writer.Write(roleId);
            writer.Write(playerId);
            writer.Write(flag);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.SetRole(roleId, playerId, flag);
            return playerId;
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

        private class RoleAssignmentData
        {
            public List<PlayerControl> crewmates { get; set; }
            public List<PlayerControl> impostors { get; set; }
            public Dictionary<byte, (int rate, int count)> impSettings = new();
            public Dictionary<byte, (int rate, int count)> neutralSettings = new();
            public Dictionary<byte, (int rate, int count)> crewSettings = new();
            public int MaxCrewmateRoles { get; set; }
            public int MaxNeutralRoles { get; set; }
            public int MaxImpostorRoles { get; set; }
        }

        private enum TeamType
        {
            Crewmate = 0,
            Neutral = 1,
            Impostor = 2
        }

    }
}