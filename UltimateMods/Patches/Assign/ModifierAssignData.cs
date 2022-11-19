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
    public class ModifierAssignData
    {
        public ModifierType ModifierType;
        public int AssignCount;

        public ModifierAssignData(ModifierType ModifierType, int Count)
        {
            this.ModifierType = ModifierType;
            this.AssignCount = Count;
        }

        public static System.Random rnd = new((int)DateTime.Now.Ticks);
        public static List<ModifierAssignData> AssignModifierList = new();

        public void ListUpEnabledModifiers()
        {
            AssignModifierList.Clear();

            AssignModifierList.Add(new(ModifierType.Opportunist, CustomRolesH.OpportunistRate.getCount()));
        }

        public void AssignModifiersAndModifiers()
        {
            List<PlayerControl> Crewmates = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            Crewmates.RemoveAll(x => x.Data.Role.IsImpostor);
            List<PlayerControl> Impostors = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            Impostors.RemoveAll(x => !x.Data.Role.IsImpostor);

            int ModifierCount = CustomOptionsH.ModifierCount.getSelection();

            while (Crewmates.Count > 0 && ModifierCount > 0)
            {
                List<PlayerControl> TargetPlayers = new();
                // var AssignModifier = EnabledModModifiers[rnd.Next(0, EnabledModModifiers.Count - 1)];
                var AssignModifier = AssignModifierList[rnd.Next(0, AssignModifierList.Count - 1)];
                TargetPlayers.AddRange(Crewmates);
                var AssignedPlayer = SetModifierToRandomPlayer(AssignModifier, TargetPlayers);
                ModifierCount--;
            }
        }

        private byte SetModifierToRandomPlayer(ModifierAssignData ModifierData, List<PlayerControl> PlayerList)
        {
            byte ModifierId = (byte)this.ModifierType;

            if (PlayerList.Count <= 0 || ModifierId != (byte)ModifierData.ModifierType)
            {
                return byte.MaxValue;
            }

            var index = rnd.Next(0, PlayerList.Count);
            byte playerId = PlayerList[index].PlayerId;

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.AddModifier, Hazel.SendOption.Reliable, -1);
            writer.Write(ModifierId);
            writer.Write(playerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.AddModifier(ModifierId, playerId);
            return playerId;
        }
    }

    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    public static class AssignModifiers
    {
        public static void Postfix(ModifierAssignData __instance)
        {
            __instance.ListUpEnabledModifiers();
            __instance.AssignModifiersAndModifiers();
        }

    }
}