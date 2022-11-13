using HarmonyLib;
using System;
using UltimateMods.Roles;
using UltimateMods.EndGame;
using UltimateMods.Objects;
using static UltimateMods.GameHistory;

namespace UltimateMods
{
    [HarmonyPatch]
    public static class UltimateMods
    {
        public static Random rnd = new MersenneTwister((int)DateTime.Now.Ticks);

        public static void ClearAndReloadRoles()
        {
            // Roles
            Sheriff.Clear();
            Jester.Clear();
            Engineer.Clear();
            CustomImpostor.Clear();
            UnderTaker.Clear();
            BountyHunter.Clear();
            Madmate.Clear();
            Bakery.Clear();
            Teleporter.Clear();
            // Altruist.Clear();

            // Modifiers
            Opportunist.Clear();

            AlivePlayer.Clear();
            Role.ClearAll();
        }

        public static void FixedUpdate(PlayerControl player)
        {
            Role.allRoles.DoIf(x => x.player == player, x => x.FixedUpdate());
            Modifiers.allModifiers.DoIf(x => x.player == player, x => x.FixedUpdate());
        }

        public static void OnMeetingStart()
        {
            Role.allRoles.Do(x => x.OnMeetingStart());
            Modifiers.allModifiers.Do(x => x.OnMeetingStart());
        }

        public static void OnMeetingEnd()
        {
            Role.allRoles.Do(x => x.OnMeetingEnd());
            Modifiers.allModifiers.Do(x => x.OnMeetingEnd());

            CustomOverlays.HideInfoOverlay();
        }

        [HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), new Type[] { typeof(PlayerControl), typeof(DisconnectReasons) })]
        class HandleDisconnectPatch
        {
            public static void Postfix(GameData __instance, PlayerControl player, DisconnectReasons reason)
            {
                if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
                {
                    Role.allRoles.Do(x => x.HandleDisconnect(player, reason));
                    Modifiers.allModifiers.Do(x => x.HandleDisconnect(player, reason));
                    finalStatuses[player.PlayerId] = FinalStatus.Disconnected;
                }
            }
        }
    }
}