using HarmonyLib;
using UltimateMods.Utilities;
using UnityEngine;
using System.Collections.Generic;
using UltimateMods.Roles;
using static UltimateMods.ColorDictionary;

namespace UltimateMods.Patches
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    class PlayerNameColorPatch
    {
        static void ResetNameTagsAndColors()
        {
            var localPlayer = PlayerControl.LocalPlayer;
            var myData = localPlayer.Data;
            var amImpostor = myData.Role.IsImpostor;

            var dict = new Dictionary<byte, (string name, Color color)>();

            foreach (var data in GameData.Instance.AllPlayers.GetFastEnumerator())
            {
                var player = data.Object;
                string text = data.PlayerName;
                Color color;
                if (player)
                {
                    var playerName = text;
                    var nameText = player.cosmetics.nameText;

                    nameText.text = Helpers.HidePlayerName(localPlayer, player) ? "" : playerName;
                    nameText.color = color = amImpostor && data.Role.IsImpostor ? Palette.ImpostorRed : Color.white;
                }
                else
                    color = Color.white;

                dict.Add(data.PlayerId, (text, color));
            }

            if (MeetingHud.Instance != null)
                foreach (PlayerVoteArea playerVoteArea in MeetingHud.Instance.playerStates)
                {
                    var data = dict[playerVoteArea.TargetPlayerId];
                    var text = playerVoteArea.NameText;
                    text.text = data.name;
                    text.color = data.color;
                }
        }

        static void setPlayerNameColor(PlayerControl p, Color color)
        {
            p.cosmetics.nameText.color = color;
            if (MeetingHud.Instance != null)
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                    if (player.NameText != null && p.PlayerId == player.TargetPlayerId)
                        player.NameText.color = color;
        }

        static void setNameColors()
        {
            var p = PlayerControl.LocalPlayer;
            var role = p.isRole;
            if (role(RoleType.Jester))
                setPlayerNameColor(p, JesterPink);
            if (role(RoleType.Sheriff))
                setPlayerNameColor(p, SheriffYellow);
            if (role(RoleType.Engineer))
                setPlayerNameColor(p, EngineerBlue);
        }

        static void Postfix(HudManager __instance)
        {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;

            ResetNameTagsAndColors();
            setNameColors();
        }
    }
}