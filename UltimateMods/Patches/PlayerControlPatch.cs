using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UltimateMods.Utilities;
using UltimateMods.Modules;
using UltimateMods.Roles;

namespace UltimateMods.Patches
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public static class PlayerControlFixedUpdatePatch
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
        static void SetBasePlayerOutlines()
        {
            foreach (PlayerControl target in PlayerControl.AllPlayerControls)
            {
                if (target == null || target.cosmetics.currentBodySprite.BodySprite == null) continue;

                target.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 0f);
            }
        }
        public static void UpdatePlayerInfo()
        {
            bool commsActive = false;
            foreach (PlayerTask t in PlayerControl.LocalPlayer.myTasks)
            {
                if (t.TaskType == TaskTypes.FixComms)
                {
                    commsActive = true;
                    break;
                }
            }

            var canSeeEverything = PlayerControl.LocalPlayer.IsDead();
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (p == null) continue;

                var canSeeInfo = canSeeEverything || p == PlayerControl.LocalPlayer;

                if (canSeeInfo)
                {
                    Transform playerInfoTransform = p.cosmetics.nameText.transform.parent.FindChild("Info");
                    TMPro.TextMeshPro playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                    if (playerInfo == null)
                    {
                        playerInfo = UnityEngine.Object.Instantiate(p.cosmetics.nameText, p.cosmetics.nameText.transform.parent);
                        playerInfo.fontSize *= 0.75f;
                        playerInfo.gameObject.name = "Info";
                    }

                    // Set the position every time bc it sometimes ends up in the wrong place due to camoflauge
                    playerInfo.transform.localPosition = p.cosmetics.nameText.transform.localPosition + Vector3.up * 0.5f;

                    PlayerVoteArea playerVoteArea = MeetingHud.Instance?.playerStates?.FirstOrDefault(x => x.TargetPlayerId == p.PlayerId);
                    Transform meetingInfoTransform = playerVoteArea != null ? playerVoteArea.NameText.transform.parent.FindChild("Info") : null;
                    TMPro.TextMeshPro meetingInfo = meetingInfoTransform != null ? meetingInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                    if (meetingInfo == null && playerVoteArea != null)
                    {
                        meetingInfo = UnityEngine.Object.Instantiate(playerVoteArea.NameText, playerVoteArea.NameText.transform.parent);
                        meetingInfo.transform.localPosition += Vector3.down * 0.10f;
                        meetingInfo.fontSize *= 0.60f;
                        meetingInfo.gameObject.name = "Info";
                    }

                    // Set player name higher to align in middle
                    if (meetingInfo != null && playerVoteArea != null)
                    {
                        var playerName = playerVoteArea.NameText;
                        playerName.transform.localPosition = new Vector3(0.3384f, (0.0311f + 0.0683f), -0.1f);
                    }

                    var (tasksCompleted, tasksTotal) = TasksHandler.taskInfo(p.Data);
                    string roleNames = RoleInfo.GetRolesString(p, true);

                    var completedStr = commsActive ? "?" : tasksCompleted.ToString();
                    var color = commsActive ? "808080" : "FAD934FF";
                    string taskInfo = tasksTotal > 0 ? $"<color=#{color}>({completedStr}/{tasksTotal})</color>" : "";

                    string playerInfoText = "";
                    string meetingInfoText = "";
                    if (p == PlayerControl.LocalPlayer)
                    {
                        playerInfoText = $"{roleNames}";
                        if (TaskPanelBehaviour.InstanceExists)
                        {
                            TMPro.TextMeshPro tabText = TaskPanelBehaviour.Instance.tab.transform.FindChild("TabText_TMP").GetComponent<TMPro.TextMeshPro>();
                            tabText.SetText($"{TranslationController.Instance.GetString(StringNames.Tasks)} {taskInfo}");
                        }
                        meetingInfoText = $"{roleNames} {taskInfo}".Trim();
                    }
                    else if (MapOptions.GhostsSeeRoles && MapOptions.GhostsSeeTasks)
                    {
                        playerInfoText = $"{roleNames} {taskInfo}".Trim();
                        meetingInfoText = playerInfoText;
                    }
                    else if (MapOptions.GhostsSeeTasks)
                    {
                        playerInfoText = $"{taskInfo}".Trim();
                        meetingInfoText = playerInfoText;
                    }
                    else if (MapOptions.GhostsSeeRoles)
                    {
                        playerInfoText = $"{roleNames}";
                        meetingInfoText = playerInfoText;
                    }

                    playerInfo.text = playerInfoText;
                    playerInfo.gameObject.SetActive(p.Visible && !Helpers.HidePlayerName(p));
                    if (meetingInfo != null) meetingInfo.text = MeetingHud.Instance.state == MeetingHud.VoteStates.Results ? "" : meetingInfoText;
                }
            }
        }

        public static void Postfix(PlayerControl __instance)
        {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;

            if (PlayerControl.LocalPlayer == __instance)
            {
                // Update player outlines
                SetBasePlayerOutlines();

                // Update Role Description
                Helpers.RefreshRoleDescription(__instance);

                // Update Player Info
                UpdatePlayerInfo();
            }

            UltimateMods.FixedUpdate(__instance);
        }
    }

    [HarmonyPatch(typeof(KillAnimation), nameof(KillAnimation.CoPerformKill))]
    class KillAnimationCoPerformKillPatch
    {
        public static bool hideNextAnimation = false;

        public static void Prefix(KillAnimation __instance, [HarmonyArgument(0)] ref PlayerControl source, [HarmonyArgument(1)] ref PlayerControl target)
        {
            if (hideNextAnimation)
                source = target;
            hideNextAnimation = false;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    public static class MurderPlayerPatch
    {
        public static bool resetToCrewmate = false;
        public static bool resetToDead = false;

        public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            // Allow everyone to murder players
            resetToCrewmate = !__instance.Data.Role.IsImpostor;
            resetToDead = __instance.Data.IsDead;
            __instance.Data.Role.TeamType = RoleTeamTypes.Impostor;
            __instance.Data.IsDead = false;
        }

        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            // Collect dead player info
            DeadPlayer deadPlayer = new DeadPlayer(target, DateTime.UtcNow, DeathReason.Kill, __instance);
            GameHistory.deadPlayers.Add(deadPlayer);

            // Reset killer to crewmate if resetToCrewmate
            if (resetToCrewmate) __instance.Data.Role.TeamType = RoleTeamTypes.Crewmate;
            if (resetToDead) __instance.Data.IsDead = true;

            // Remove fake tasks when player dies
            if (target.HasFakeTasks())
                target.ClearAllTasks();

            __instance.OnKill(target);
            target.OnDeath(__instance);
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
    public static class ExilePlayerPatch
    {
        public static void Postfix(PlayerControl __instance)
        {
            // Collect dead player info
            DeadPlayer deadPlayer = new DeadPlayer(__instance, DateTime.UtcNow, DeathReason.Exile, null);
            GameHistory.deadPlayers.Add(deadPlayer);

            // Remove fake tasks when player dies
            if (__instance.HasFakeTasks())
                __instance.ClearAllTasks();

            __instance.OnDeath(killer: null);
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
    static class PlayerControlSetCoolDownPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] float time)
        {
            if (PlayerControl.GameOptions.KillCooldown <= 0f) return false;
            float multiplier = 1f;
            float addition = 0f;
            if (PlayerControl.LocalPlayer.isRole(RoleType.BountyHunter)) addition = BountyHunter.AdditionalCooldown;

            float max = Mathf.Max(PlayerControl.GameOptions.KillCooldown * multiplier + addition, __instance.killTimer);
            __instance.SetKillTimerUnchecked(Mathf.Clamp(time, 0f, max), max);
            return false;
        }

        public static void SetKillTimerUnchecked(this PlayerControl player, float time, float max = float.NegativeInfinity)
        {
            if (max == float.NegativeInfinity) max = time;

            player.killTimer = time;
            FastDestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(time, max);
        }
    }
}