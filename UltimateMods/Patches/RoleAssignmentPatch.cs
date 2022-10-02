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
        // private static List<byte> blockLovers = new();
        public static int blockedAssignments = 0;
        public static int maxBlocks = 10;

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
            blockLovers = new List<byte> {
                (byte)RoleType.Bait,
            };

            if (!Lovers.hasTasks)
            {
                blockLovers.Add((byte)RoleType.Snitch);
                blockLovers.Add((byte)RoleType.FortuneTeller);
                //blockLovers.Add((byte)RoleType.Sunfish);
                blockLovers.Add((byte)RoleType.Fox);
            }

            if (!CustomOptionsH.arsonistCanBeLovers.getBool())
            {
                blockLovers.Add((byte)RoleType.Arsonist);
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

            // Make sure min is less or equal to max
            if (crewmateMin > crewmateMax) crewmateMin = crewmateMax;
            if (neutralMin > neutralMax) neutralMin = neutralMax;
            if (impostorMin > impostorMax) impostorMin = impostorMax;

            // Get the maximum allowed count of each role type based on the minimum and maximum option
            int crewCountSettings = rnd.Next(crewmateMin, crewmateMax + 1);
            int neutralCountSettings = rnd.Next(neutralMin, neutralMax + 1);
            int impCountSettings = rnd.Next(impostorMin, impostorMax + 1);

            // Potentially lower the actual maximum to the assignable players
            int maxCrewmateRoles = Mathf.Min(crewmates.Count, crewCountSettings);
            int maxNeutralRoles = Mathf.Min(crewmates.Count, neutralCountSettings);
            int maxImpostorRoles = Mathf.Min(impostors.Count, impCountSettings);

            // Fill in the lists with the roles that should be assigned to players. Note that the special roles (like Mafia or Lovers) are NOT included in these lists
            Dictionary<byte, (int rate, int count)> impSettings = new();
            Dictionary<byte, (int rate, int count)> neutralSettings = new();
            Dictionary<byte, (int rate, int count)> crewSettings = new();

            // impSettings.Add((byte)RoleType.Morphling, CustomRolesH.MorphlingRate.data);

            neutralSettings.Add((byte)RoleType.Jester, CustomRolesH.JesterRate.data);

            crewSettings.Add((byte)RoleType.Sheriff, CustomRolesH.SheriffRate.data);
            crewSettings.Add((byte)RoleType.Engineer, CustomRolesH.EngineerRate.data);

            return new RoleAssignmentData
            {
                crewmates = crewmates,
                impostors = impostors,
                crewSettings = crewSettings,
                neutralSettings = neutralSettings,
                impSettings = impSettings,
                maxCrewmateRoles = maxCrewmateRoles,
                maxNeutralRoles = maxNeutralRoles,
                maxImpostorRoles = maxImpostorRoles
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
                    if (singleImps.Count > 0 && singleCrew.Count > 0 && (!isOnlyRole || (data.maxCrewmateRoles > 0 && data.maxImpostorRoles > 0)) && rnd.Next(1, 101) <= CustomOptionsH.loversImpLoverRate.getSelection() * 10)
                    {
                        lover1Index = rnd.Next(0, singleImps.Count);
                        lover1 = singleImps[lover1Index].PlayerId;

                        lover2Index = rnd.Next(0, singleCrew.Count);
                        lover2 = singleCrew[lover2Index].PlayerId;

                        if (isOnlyRole)
                        {
                            data.maxImpostorRoles--;
                            data.maxCrewmateRoles--;

                            data.impostors.RemoveAll(x => x.PlayerId == lover1);
                            data.crewmates.RemoveAll(x => x.PlayerId == lover2);
                        }
                    }

                    else if (singleCrew.Count >= 2 && (isOnlyRole || data.maxCrewmateRoles >= 2))
                    {
                        lover1Index = rnd.Next(0, singleCrew.Count);
                        while (lover2Index == lover1Index || lover2Index < 0) lover2Index = rnd.Next(0, singleCrew.Count);

                        lover1 = singleCrew[lover1Index].PlayerId;
                        lover2 = singleCrew[lover2Index].PlayerId;

                        if (isOnlyRole)
                        {
                            data.maxCrewmateRoles -= 2;
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
                        if (data.impostors.Count > 0 && data.maxImpostorRoles > 0)
                        {
                            byte evilGuesser = setRoleToRandomPlayer((byte)RoleType.EvilGuesser, data.impostors);
                            data.impostors.ToList().RemoveAll(x => x.PlayerId == evilGuesser);
                            data.maxImpostorRoles--;
                            data.crewSettings.Add((byte)RoleType.NiceGuesser, (CustomOptionsH.guesserSpawnBothRate.getSelection(), 1));
                        }
                    }
                    else if (data.crewmates.Count > 0 && data.maxCrewmateRoles > 0)
                    {
                        byte niceGuesser = setRoleToRandomPlayer((byte)RoleType.NiceGuesser, data.crewmates);
                        data.crewmates.ToList().RemoveAll(x => x.PlayerId == niceGuesser);
                        data.maxCrewmateRoles--;
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
                    data.crewSettings.Add((byte)option.roleType, (option.rate, niceCount));

                if (evilCount > 0)
                    data.impSettings.Add((byte)option.roleType, (option.rate, evilCount));
            }
        }

        private static void assignEnsuredRoles(RoleAssignmentData data)
        {
            blockedAssignments = 0;

            // Get all roles where the chance to occur is set to 100%
            List<byte> ensuredCrewmateRoles = data.crewSettings.Where(x => x.Value.rate == 10).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).ToList();
            List<byte> ensuredNeutralRoles = data.neutralSettings.Where(x => x.Value.rate == 10).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).ToList();
            List<byte> ensuredImpostorRoles = data.impSettings.Where(x => x.Value.rate == 10).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).ToList();

            // Assign roles until we run out of either players we can assign roles to or run out of roles we can assign to players
            while ((data.impostors.Count > 0 && data.maxImpostorRoles > 0 && ensuredImpostorRoles.Count > 0) || (data.crewmates.Count > 0 && ((data.maxCrewmateRoles > 0 && ensuredCrewmateRoles.Count > 0) || (data.maxNeutralRoles > 0 && ensuredNeutralRoles.Count > 0))))
            {
                Dictionary<TeamType, List<byte>> rolesToAssign = new();
                if (data.crewmates.Count > 0 && data.maxCrewmateRoles > 0 && ensuredCrewmateRoles.Count > 0) rolesToAssign.Add(TeamType.Crewmate, ensuredCrewmateRoles);
                if (data.crewmates.Count > 0 && data.maxNeutralRoles > 0 && ensuredNeutralRoles.Count > 0) rolesToAssign.Add(TeamType.Neutral, ensuredNeutralRoles);
                if (data.impostors.Count > 0 && data.maxImpostorRoles > 0 && ensuredImpostorRoles.Count > 0) rolesToAssign.Add(TeamType.Impostor, ensuredImpostorRoles);

                // Randomly select a pool of roles to assign a role from next (Crewmate role, Neutral role or Impostor role)
                // then select one of the roles from the selected pool to a player
                // and remove the role (and any potentially blocked role pairings) from the pool(s)
                var roleType = rolesToAssign.Keys.ElementAt(rnd.Next(0, rolesToAssign.Keys.Count()));
                var players = roleType is TeamType.Crewmate or TeamType.Neutral ? data.crewmates : data.impostors;
                var index = rnd.Next(0, rolesToAssign[roleType].Count);
                var roleId = rolesToAssign[roleType][index];
                var player = setRoleToRandomPlayer(rolesToAssign[roleType][index], players);
                if (player == byte.MaxValue && blockedAssignments < maxBlocks)
                {
                    blockedAssignments++;
                    continue;
                }
                blockedAssignments = 0;

                rolesToAssign[roleType].RemoveAt(index);

                if (CustomOptionsH.blockedRolePairings.ContainsKey(roleId))
                {
                    foreach (var blockedRoleId in CustomOptionsH.blockedRolePairings[roleId])
                    {
                        // Set chance for the blocked roles to 0 for chances less than 100%
                        if (data.impSettings.ContainsKey(blockedRoleId)) data.impSettings[blockedRoleId] = (0, 0);
                        if (data.neutralSettings.ContainsKey(blockedRoleId)) data.neutralSettings[blockedRoleId] = (0, 0);
                        if (data.crewSettings.ContainsKey(blockedRoleId)) data.crewSettings[blockedRoleId] = (0, 0);
                        // Remove blocked roles even if the chance was 100%
                        foreach (var ensuredRolesList in rolesToAssign.Values)
                        {
                            ensuredRolesList.RemoveAll(x => x == blockedRoleId);
                        }
                    }
                }

                // Adjust the role limit
                switch (roleType)
                {
                    case TeamType.Crewmate: data.maxCrewmateRoles--; break;
                    case TeamType.Neutral: data.maxNeutralRoles--; break;
                    case TeamType.Impostor: data.maxImpostorRoles--; break;
                }
            }
        }


        private static void assignChanceRoles(RoleAssignmentData data)
        {
            blockedAssignments = 0;

            // Get all roles where the chance to occur is set grater than 0% but not 100% and build a ticket pool based on their weight
            List<byte> crewmateTickets = data.crewSettings.Where(x => x.Value.rate is > 0 and < 10).Select(x => Enumerable.Repeat(x.Key, x.Value.rate * x.Value.count)).SelectMany(x => x).ToList();
            List<byte> neutralTickets = data.neutralSettings.Where(x => x.Value.rate is > 0 and < 10).Select(x => Enumerable.Repeat(x.Key, x.Value.rate * x.Value.count)).SelectMany(x => x).ToList();
            List<byte> impostorTickets = data.impSettings.Where(x => x.Value.rate is > 0 and < 10).Select(x => Enumerable.Repeat(x.Key, x.Value.rate * x.Value.count)).SelectMany(x => x).ToList();

            // Assign roles until we run out of either players we can assign roles to or run out of roles we can assign to players
            while (
                (data.impostors.Count > 0 && data.maxImpostorRoles > 0 && impostorTickets.Count > 0) ||
                (data.crewmates.Count > 0 && (
                    (data.maxCrewmateRoles > 0 && crewmateTickets.Count > 0) ||
                    (data.maxNeutralRoles > 0 && neutralTickets.Count > 0)
                )))
            {

                Dictionary<TeamType, List<byte>> rolesToAssign = new();
                if (data.crewmates.Count > 0 && data.maxCrewmateRoles > 0 && crewmateTickets.Count > 0) rolesToAssign.Add(TeamType.Crewmate, crewmateTickets);
                if (data.crewmates.Count > 0 && data.maxNeutralRoles > 0 && neutralTickets.Count > 0) rolesToAssign.Add(TeamType.Neutral, neutralTickets);
                if (data.impostors.Count > 0 && data.maxImpostorRoles > 0 && impostorTickets.Count > 0) rolesToAssign.Add(TeamType.Impostor, impostorTickets);

                // Randomly select a pool of role tickets to assign a role from next (Crewmate role, Neutral role or Impostor role)
                // then select one of the roles from the selected pool to a player
                // and remove all tickets of this role (and any potentially blocked role pairings) from the pool(s)
                var roleType = rolesToAssign.Keys.ElementAt(rnd.Next(0, rolesToAssign.Keys.Count()));
                var players = roleType is TeamType.Crewmate or TeamType.Neutral ? data.crewmates : data.impostors;
                var index = rnd.Next(0, rolesToAssign[roleType].Count);
                var roleId = rolesToAssign[roleType][index];
                var player = setRoleToRandomPlayer(rolesToAssign[roleType][index], players);
                if (player == byte.MaxValue && blockedAssignments < maxBlocks)
                {
                    blockedAssignments++;
                    continue;
                }
                blockedAssignments = 0;

                rolesToAssign[roleType].RemoveAll(x => x == roleId);

                if (CustomOptionsH.blockedRolePairings.ContainsKey(roleId))
                {
                    foreach (var blockedRoleId in CustomOptionsH.blockedRolePairings[roleId])
                    {
                        // Remove tickets of blocked roles from all pools
                        crewmateTickets.RemoveAll(x => x == blockedRoleId);
                        neutralTickets.RemoveAll(x => x == blockedRoleId);
                        impostorTickets.RemoveAll(x => x == blockedRoleId);
                    }
                }

                // Adjust the role limit
                switch (roleType)
                {
                    case TeamType.Crewmate: data.maxCrewmateRoles--; break;
                    case TeamType.Neutral: data.maxNeutralRoles--; break;
                    case TeamType.Impostor: data.maxImpostorRoles--; break;
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
            RPCProcedure.setRole(roleId, playerId, flag);
            return playerId;
        }

        private static void assignRoleModifiers(RoleAssignmentData data)
        {/*
            // Madmate
            for (int i = 0; i < CustomOptionsH.madmateSpawnRate.count; i++)
            {
                if (rnd.Next(1, 100) <= CustomOptionsH.madmateSpawnRate.rate * 10)
                {
                    var candidates = Madmate.candidates;
                    if (candidates.Count <= 0)
                    {
                        break;
                    }

                    if (Madmate.madmateType == Madmate.MadmateType.Simple)
                    {
                        if (data.maxCrewmateRoles <= 0) break;
                        setModifierToRandomPlayer((byte)ModifierType.Madmate, Madmate.candidates);
                        data.maxCrewmateRoles--;
                    }
                    else
                    {
                        setModifierToRandomPlayer((byte)ModifierType.Madmate, Madmate.candidates);
                    }
                }
            }

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
                    var candidates = Opportunist.candidates;
                    if (candidates.Count <= 0)
                    {
                        break;
                    }
                    setModifierToRandomPlayer((byte)ModifierType.Opportunist, Opportunist.candidates);
                }
            }
        }

        private static byte setRoleToRandomPlayer(byte roleId, List<PlayerControl> playerList, byte flag = 0, bool removePlayer = true)
        {
            var index = rnd.Next(0, playerList.Count);
            byte playerId = playerList[index].PlayerId;/*
            if (RoleInfo.lovers.enabled &&
                Helpers.playerById(playerId)?.isLovers() == true &&
                blockLovers.Contains(roleId))
            {
                return byte.MaxValue;
            }*/

            if (removePlayer) playerList.RemoveAt(index);

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetRole, Hazel.SendOption.Reliable, -1);
            writer.Write(roleId);
            writer.Write(playerId);
            writer.Write(flag);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.setRole(roleId, playerId, flag);
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
            public int maxCrewmateRoles { get; set; }
            public int maxNeutralRoles { get; set; }
            public int maxImpostorRoles { get; set; }
        }

        private enum TeamType
        {
            Crewmate = 0,
            Neutral = 1,
            Impostor = 2
        }

    }
}