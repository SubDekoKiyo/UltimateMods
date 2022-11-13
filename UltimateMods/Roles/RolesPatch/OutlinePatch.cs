using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UltimateMods.Utilities;

namespace UltimateMods.Roles.Patches
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public static class OutlinePatch
    {
        public static PlayerControl SetTarget(bool onlyCrewmates = false, bool targetPlayersInVents = false, List<PlayerControl> untargetablePlayers = null, PlayerControl targetingPlayer = null)
        {
            PlayerControl result = null;
            float num = GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];
            if (!MapUtilities.CachedShipStatus) return result;
            if (targetingPlayer == null) targetingPlayer = PlayerControl.LocalPlayer;

            if (untargetablePlayers == null)
            {
                untargetablePlayers = new List<PlayerControl>();
            }

            Vector2 truePosition = targetingPlayer.GetTruePosition();
            Il2CppSystem.Collections.Generic.List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
            for (int i = 0; i < allPlayers.Count; i++)
            {
                GameData.PlayerInfo playerInfo = allPlayers[i];
                if (!playerInfo.Disconnected && playerInfo.PlayerId != targetingPlayer.PlayerId && !playerInfo.IsDead && (!onlyCrewmates || !playerInfo.Role.IsImpostor))
                {
                    PlayerControl @object = playerInfo.Object;
                    if (untargetablePlayers.Any(x => x == @object))
                    {
                        // if that player is not targetable: skip check
                        continue;
                    }

                    if (@object && (!@object.inVent || targetPlayersInVents))
                    {
                        Vector2 vector = @object.GetTruePosition() - truePosition;
                        float magnitude = vector.magnitude;
                        if (magnitude <= num && !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask))
                        {
                            result = @object;
                            num = magnitude;
                        }
                    }
                }
            }
            return result;
        }

        public static void SetPlayerOutline(PlayerControl target, Color color)
        {
            if (target == null || target.cosmetics.currentBodySprite.BodySprite == null) return;

            target.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 1f);
            target.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", color);
        }

        // Update functions
        public static void SetBasePlayerOutlines()
        {
            foreach (PlayerControl target in PlayerControl.AllPlayerControls)
            {
                if (target == null || target.cosmetics.currentBodySprite.BodySprite == null) continue;

                target.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 0f);
            }
        }

        public static void ImpostorSetTarget()
        {
            if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor || !PlayerControl.LocalPlayer.CanMove || PlayerControl.LocalPlayer.Data.IsDead)
            { // !isImpostor || !canMove || isDead
                FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(null);
                return;
            }

            PlayerControl target = null;
            target = SetTarget(true, true, new List<PlayerControl>() { null });
            FastDestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(target); // Includes setPlayerOutline(target, Palette.ImpstorRed);
        }
    }
}