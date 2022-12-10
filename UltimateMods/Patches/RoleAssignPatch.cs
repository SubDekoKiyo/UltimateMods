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
        public static System.Random rnd = new ((int)DateTime.Now.Ticks);

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
            // Get the players that we want to assign the roles to. Crewmate and Neutral roles are assigned to natural Crewmates. Impostor roles to Impostors.
            List<PlayerControl> Crewmates = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            Crewmates.RemoveAll(x => x.Data.Role.IsImpostor);
            List<PlayerControl> Impostors = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            Impostors.RemoveAll(x => !x.Data.Role.IsImpostor);

            var CrewmateCount = CustomOptionsH.CrewmateRolesCount.getSelection();
            var NeutralCount = CustomOptionsH.NeutralRolesCount.getSelection();
            var ImpostorCount = CustomOptionsH.ImpostorRolesCount.getSelection();

            // Potentially lower the actual Maximum to the assignable players
            int CrewmateRoles = Mathf.Min(Crewmates.Count, CrewmateCount);
            int NeutralRoles = Mathf.Min(Crewmates.Count, NeutralCount);
            int ImpostorRoles = Mathf.Min(Impostors.Count, ImpostorCount);

            // Fill in the lists with the roles that should be assigned to players. Note that the special roles (like Mafia or Lovers) are NOT included in these lists
            Dictionary<byte, (bool enable, int count)> ImpSettings = new();
            Dictionary<byte, (bool enable, int count)> NeutralSettings = new();
            Dictionary<byte, (bool enable, int count)> CrewSettings = new();

            ImpSettings.Add((byte)RoleType.CustomImpostor, CustomRolesH.CustomImpostorRate.data);
            ImpSettings.Add((byte)RoleType.UnderTaker, CustomRolesH.UnderTakerRate.data);
            ImpSettings.Add((byte)RoleType.BountyHunter, CustomRolesH.BountyHunterRate.data);
            ImpSettings.Add((byte)RoleType.Teleporter, CustomRolesH.TeleporterRate.data);
            ImpSettings.Add((byte)RoleType.EvilHacker, CustomRolesH.EvilHackerRate.data);
            ImpSettings.Add((byte)RoleType.Adversity, CustomRolesH.AdversityRate.data);

            NeutralSettings.Add((byte)RoleType.Jester, CustomRolesH.JesterRate.data);
            NeutralSettings.Add((byte)RoleType.Jackal, CustomRolesH.JackalRate.data);
            NeutralSettings.Add((byte)RoleType.Arsonist, CustomRolesH.ArsonistRate.data);

            CrewSettings.Add((byte)RoleType.Sheriff, CustomRolesH.SheriffRate.data);
            CrewSettings.Add((byte)RoleType.Engineer, CustomRolesH.EngineerRate.data);
            CrewSettings.Add((byte)RoleType.Madmate, CustomRolesH.MadmateRate.data);
            CrewSettings.Add((byte)RoleType.Bakery, CustomRolesH.BakeryRate.data);
            CrewSettings.Add((byte)RoleType.Snitch, CustomRolesH.SnitchRate.data);
            CrewSettings.Add((byte)RoleType.Seer, CustomRolesH.SeerRate.data);
            // CrewSettings.Add((byte)RoleType.Altruist, CustomRolesH.AltruistRate.data);

            return new RoleAssignmentData
            {
                Crewmates = Crewmates,
                Impostors = Impostors,
                CrewSettings = CrewSettings,
                NeutralSettings = NeutralSettings,
                ImpSettings = ImpSettings,
                CrewmateRoles = CrewmateRoles,
                NeutralRoles = NeutralRoles,
                ImpostorRoles = ImpostorRoles
            };
        }

        private static void assignSpecialRoles(RoleAssignmentData data)
        {/*
            // Assign Lovers
            for (int i = 0; i < CustomOptionsH.loversNumCouples.getFloat(); i++)
            {
                var singleCrew = data.Crewmates.FindAll(x => !x.isLovers());
                var singleImps = data.Impostors.FindAll(x => !x.isLovers());

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

                            data.Impostors.RemoveAll(x => x.PlayerId == lover1);
                            data.Crewmates.RemoveAll(x => x.PlayerId == lover2);
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
                            data.Crewmates.RemoveAll(x => x.PlayerId == lover1);
                            data.Crewmates.RemoveAll(x => x.PlayerId == lover2);
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
            // Assign Guesser (chance to be Impostor based on setting)
            bool isEvilGuesser = rnd.Next(1, 101) <= CustomOptionsH.guesserIsImpGuesserRate.getSelection() * 10;
            if (CustomOptionsH.guesserSpawnBothRate.getSelection() > 0)
            {
                if (rnd.Next(1, 101) <= CustomOptionsH.guesserSpawnRate.getSelection() * 10)
                {
                    if (isEvilGuesser)
                    {
                        if (data.Impostors.Count > 0 && data.MaxImpostorRoles > 0)
                        {
                            byte evilGuesser = setRoleToRandomPlayer((byte)RoleType.EvilGuesser, data.Impostors);
                            data.Impostors.ToList().RemoveAll(x => x.PlayerId == evilGuesser);
                            data.MaxImpostorRoles--;
                            data.CrewSettings.Add((byte)RoleType.NiceGuesser, (CustomOptionsH.guesserSpawnBothRate.getSelection(), 1));
                        }
                    }
                    else if (data.Crewmates.Count > 0 && data.MaxCrewmateRoles > 0)
                    {
                        byte niceGuesser = setRoleToRandomPlayer((byte)RoleType.NiceGuesser, data.Crewmates);
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
                    data.CrewSettings.Add((byte)option.roleType, (option.enable, niceCount));

                if (evilCount > 0)
                    data.ImpSettings.Add((byte)option.roleType, (option.enable, evilCount));
            }
        }

        private static void assignEnsuredRoles(RoleAssignmentData data)
        {
            BlockedAssignments = 0;

            // Get all roles where the chance to occur is set to 100%
            List<byte> EnsuredCrewmateRoles = data.CrewSettings.Where(x => x.Value.enable).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).ToList();
            List<byte> EnsuredNeutralRoles = data.NeutralSettings.Where(x => x.Value.enable).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).ToList();
            List<byte> EnsuredImpostorRoles = data.ImpSettings.Where(x => x.Value.enable).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).ToList();

            // Assign roles until we run out of either players we can assign roles to or run out of roles we can assign to players
            while ((data.Impostors.Count > 0 && data.ImpostorRoles > 0 && EnsuredImpostorRoles.Count > 0) || (data.Crewmates.Count > 0 && ((data.CrewmateRoles > 0 && EnsuredCrewmateRoles.Count > 0) || (data.NeutralRoles > 0 && EnsuredNeutralRoles.Count > 0))))
            {
                Dictionary<TeamType, List<byte>> rolesToAssign = new();
                if (data.Crewmates.Count > 0 && data.CrewmateRoles > 0 && EnsuredCrewmateRoles.Count > 0) rolesToAssign.Add(TeamType.Crewmate, EnsuredCrewmateRoles);
                if (data.Crewmates.Count > 0 && data.NeutralRoles > 0 && EnsuredNeutralRoles.Count > 0) rolesToAssign.Add(TeamType.Neutral, EnsuredNeutralRoles);
                if (data.Impostors.Count > 0 && data.ImpostorRoles > 0 && EnsuredImpostorRoles.Count > 0) rolesToAssign.Add(TeamType.Impostor, EnsuredImpostorRoles);

                // Randomly select a pool of roles to assign a role from next (Crewmate role, Neutral role or Impostor role)
                // then select one of the roles from the selected pool to a player
                // and remove the role (and any potentially Blocked role pairings) from the pool(s)
                var roleType = rolesToAssign.Keys.ElementAt(rnd.Next(0, rolesToAssign.Keys.Count()));
                var players = roleType is TeamType.Crewmate or TeamType.Neutral ? data.Crewmates : data.Impostors;
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
                        if (data.ImpSettings.ContainsKey(BlockedRoleId)) data.ImpSettings[BlockedRoleId] = (false, 0);
                        if (data.NeutralSettings.ContainsKey(BlockedRoleId)) data.NeutralSettings[BlockedRoleId] = (false, 0);
                        if (data.CrewSettings.ContainsKey(BlockedRoleId)) data.CrewSettings[BlockedRoleId] = (false, 0);
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
                    case TeamType.Crewmate: data.CrewmateRoles--; break;
                    case TeamType.Neutral: data.NeutralRoles--; break;
                    case TeamType.Impostor: data.ImpostorRoles--; break;
                }
            }
        }


        private static void assignChanceRoles(RoleAssignmentData data)
        {
            BlockedAssignments = 0;

            // Get all roles where the chance to occur is set grater than 0% but not 100% and build a ticket pool based on their weight
            List<byte> CrewmateTickets = data.CrewSettings.Where(x => x.Value.enable is true).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).ToList();
            List<byte> NeutralTickets = data.NeutralSettings.Where(x => x.Value.enable is true).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).ToList();
            List<byte> ImpostorTickets = data.ImpSettings.Where(x => x.Value.enable is true).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).ToList();

            // Assign roles until we run out of either players we can assign roles to or run out of roles we can assign to players
            while (
                (data.Impostors.Count > 0 && data.ImpostorRoles > 0 && ImpostorTickets.Count > 0) ||
                (data.Crewmates.Count > 0 && (
                    (data.CrewmateRoles > 0 && CrewmateTickets.Count > 0) ||
                    (data.NeutralRoles > 0 && NeutralTickets.Count > 0)
                )))
            {

                Dictionary<TeamType, List<byte>> rolesToAssign = new();
                if (data.Crewmates.Count > 0 && data.CrewmateRoles > 0 && CrewmateTickets.Count > 0) rolesToAssign.Add(TeamType.Crewmate, CrewmateTickets);
                if (data.Crewmates.Count > 0 && data.NeutralRoles > 0 && NeutralTickets.Count > 0) rolesToAssign.Add(TeamType.Neutral, NeutralTickets);
                if (data.Impostors.Count > 0 && data.ImpostorRoles > 0 && ImpostorTickets.Count > 0) rolesToAssign.Add(TeamType.Impostor, ImpostorTickets);

                // Randomly select a pool of role tickets to assign a role from next (Crewmate role, Neutral role or Impostor role)
                // then select one of the roles from the selected pool to a player
                // and remove all tickets of this role (and any potentially Blocked role pairings) from the pool(s)
                var roleType = rolesToAssign.Keys.ElementAt(rnd.Next(0, rolesToAssign.Keys.Count()));
                var players = roleType is TeamType.Crewmate or TeamType.Neutral ? data.Crewmates : data.Impostors;
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
                        CrewmateTickets.RemoveAll(x => x == BlockedRoleId);
                        NeutralTickets.RemoveAll(x => x == BlockedRoleId);
                        ImpostorTickets.RemoveAll(x => x == BlockedRoleId);
                    }
                }

                // Adjust the role limit
                switch (roleType)
                {
                    case TeamType.Crewmate: data.CrewmateRoles--; break;
                    case TeamType.Neutral: data.NeutralRoles--; break;
                    case TeamType.Impostor: data.ImpostorRoles--; break;
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
                if (CustomRolesH.OpportunistRate.enable)
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
            public List<PlayerControl> Crewmates { get; set; }
            public List<PlayerControl> Impostors { get; set; }
            public Dictionary<byte, (bool enable, int count)> ImpSettings = new();
            public Dictionary<byte, (bool enable, int count)> NeutralSettings = new();
            public Dictionary<byte, (bool enable, int count)> CrewSettings = new();
            public int CrewmateRoles { get; set; }
            public int NeutralRoles { get; set; }
            public int ImpostorRoles { get; set; }
        }

        private enum TeamType
        {
            Crewmate = 0,
            Neutral = 1,
            Impostor = 2
        }
    }
}