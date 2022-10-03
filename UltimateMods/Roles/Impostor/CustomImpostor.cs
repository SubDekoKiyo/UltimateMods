using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UltimateMods.Patches;
using UltimateMods.Roles;

namespace UltimateMods
{
    [HarmonyPatch]
    public class CustomImpostor : RoleBase<CustomImpostor>
    {
        public static Color color = Palette.ImpostorRed;

        public static float KillCooldowns { get { return CustomRolesH.CustomImpostorKillCooldown.getFloat(); } }
        public static bool CanUseVents { get { return CustomRolesH.CustomImpostorCanUseVents.getBool(); } }
        public static bool CanSabotage { get { return CustomRolesH.CustomImpostorCanSabotage.getBool(); } }

        public CustomImpostor()
        {
            RoleType = roleId = RoleType.CustomImpostor;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd()
        {
            if (PlayerControl.LocalPlayer == player)
                player.SetKillTimerUnchecked(KillCooldowns);
        }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target)
        {
            if (PlayerControl.LocalPlayer == player)
                player.SetKillTimerUnchecked(KillCooldowns);
        }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void MakeButtons(HudManager hm) { }
        public static void SetButtonCooldowns() { }

        public static void Clear()
        {
            players = new List<CustomImpostor>();
        }
    }
}