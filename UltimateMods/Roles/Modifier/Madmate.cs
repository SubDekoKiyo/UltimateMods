using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UltimateMods.Roles;
using UltimateMods.Modules;

namespace UltimateMods.Roles
{
    [HarmonyPatch]
    public class Madmate : ModifierBase<Madmate>
    {
        public static string Postfix { get { return ModTranslation.getString("MadmatePostfix"); } }

        public static List<PlayerControl> Candidates
        {
            get
            {
                List<PlayerControl> validPlayers = new();

                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (!player.hasModifier(ModifierType.Madmate))
                        validPlayers.Add(player);
                }

                return validPlayers;
            }
        }

        public Madmate()
        {
            ModType = modId = ModifierType.Madmate;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void Clear()
        {
            players = new List<Madmate>();
        }
    }
}