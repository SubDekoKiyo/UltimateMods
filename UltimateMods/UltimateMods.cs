using HarmonyLib;
using System;
using UltimateMods.Roles;
using UltimateMods.EndGame;
using UltimateMods.Objects;
using static UltimateMods.Roles.CrewmateRoles;
using static UltimateMods.Roles.NeutralRoles;
using static UltimateMods.GameHistory;

namespace UltimateMods
{
    [HarmonyPatch]
    public static class UltimateMods
    {
        public static System.Random rnd = new System.Random((int)DateTime.Now.Ticks);
        public static void ClearAndReloadRoles()
        {
            Jester.ClearAndReload();
            Sheriff.ClearAndReload();
        }

        public static void FixedUpdate(PlayerControl player)
        {
            Modifier.allModifiers.DoIf(x => x.player == player, x => x.FixedUpdate());
        }

        public static void OnMeetingStart()
        {
            Modifier.allModifiers.Do(x => x.OnMeetingStart());
        }

        public static void OnMeetingEnd()
        {
            Modifier.allModifiers.Do(x => x.OnMeetingEnd());

            CustomOverlays.HideInfoOverlay();
            // CustomOverlays.HideBlackBG();
        }

        [HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), new Type[] { typeof(PlayerControl), typeof(DisconnectReasons) })]
        class HandleDisconnectPatch
        {
            public static void Postfix(GameData __instance, PlayerControl player, DisconnectReasons reason)
            {
                if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
                {
                    Role.allRoles.Do(x => x.HandleDisconnect(player, reason));
                    Modifier.allModifiers.Do(x => x.HandleDisconnect(player, reason));
                    finalStatuses[player.PlayerId] = FinalStatus.Disconnected;
                }
            }
        }
    }
}