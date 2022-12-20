namespace UltimateMods.Patches
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public static class PlayerControlFixedUpdatePatch
    {
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
                    TextMeshPro playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TextMeshPro>() : null;
                    if (playerInfo == null)
                    {
                        playerInfo = UnityEngine.Object.Instantiate(p.cosmetics.nameText, p.cosmetics.nameText.transform.parent);
                        playerInfo.fontSize *= 0.75f;
                        playerInfo.gameObject.name = "Info";
                    }

                    // Set the position every time bc it sometimes ends up in the wrong place due to camoflauge
                    playerInfo.transform.localPosition = p.cosmetics.nameText.transform.localPosition + Vector3.up * 0.25f;

                    PlayerVoteArea playerVoteArea = MeetingHud.Instance?.playerStates?.FirstOrDefault(x => x.TargetPlayerId == p.PlayerId);
                    Transform meetingInfoTransform = playerVoteArea != null ? playerVoteArea.NameText.transform.parent.FindChild("Info") : null;
                    TextMeshPro meetingInfo = meetingInfoTransform != null ? meetingInfoTransform.GetComponent<TextMeshPro>() : null;
                    if (meetingInfo == null && playerVoteArea != null)
                    {
                        meetingInfo = UnityEngine.Object.Instantiate(playerVoteArea.NameText, playerVoteArea.NameText.transform.parent);
                        meetingInfo.transform.localPosition += Vector3.down * 0.05f;
                        meetingInfo.fontSize *= 0.60f;
                        meetingInfo.gameObject.name = "Info";
                    }

                    // Set player name higher to align in middle
                    if (meetingInfo != null && playerVoteArea != null)
                    {
                        var playerName = playerVoteArea.NameText;
                        playerName.transform.localPosition = new Vector3(0.3384f, (0.0311f + 0.0683f + 0.05f), -0.1f);
                    }

                    var (tasksCompleted, tasksTotal) = TasksHandler.taskInfo(p.Data);
                    string roleNames = p.GetRoleAndModString(p.GetRoleId(), p.GetModifierId());
                    bool WasTaskEnd = tasksCompleted == tasksTotal;

                    var completedStr = commsActive ? "?" : tasksCompleted.ToString();
                    var color = commsActive ? "808080" : WasTaskEnd ? "00FF00" : "FAD934FF";
                    string taskInfo = tasksTotal > 0 ? $"<color=#{color}>({completedStr}/{tasksTotal})</color>" : "";

                    string playerInfoText = "";
                    string meetingInfoText = "";
                    if (p == PlayerControl.LocalPlayer)
                    {
                        playerInfoText = $"{roleNames}";
                        if (DestroyableSingleton<TaskPanelBehaviour>.InstanceExists)
                        {
                            TextMeshPro tabText = FastDestroyableSingleton<TaskPanelBehaviour>.Instance.tab.transform.FindChild("TabText_TMP").GetComponent<TextMeshPro>();
                            tabText.SetText($"{TranslationController.Instance.GetString(StringNames.Tasks)} {taskInfo}");
                        }
                        meetingInfoText = $"{roleNames} {taskInfo}".Trim();
                    }
                    else if (Options.GhostsSeeRoles && Options.GhostsSeeTasks && !Altruist.exists)
                    {
                        playerInfoText = $"{roleNames} {taskInfo}".Trim();
                        meetingInfoText = playerInfoText;
                    }
                    else if (Options.GhostsSeeTasks && !Altruist.exists)
                    {
                        playerInfoText = $"{taskInfo}".Trim();
                        meetingInfoText = playerInfoText;
                    }
                    else if (Options.GhostsSeeRoles && !Altruist.exists)
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
                Roles.Patches.OutlinePatch.SetBasePlayerOutlines();
                Roles.Patches.OutlinePatch.ImpostorSetTarget();

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
            if (hideNextAnimation) source = target;
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

            if (target.IsImpostor() && AmongUsClient.Instance.AmHost)
            {
                Adversity.CheckAndAdversityState();
            }

            // Seer show flash and add dead player position
            foreach (var seer in Seer.allPlayers)
            {
                if (PlayerControl.LocalPlayer.IsRole(RoleId.Seer) && !seer.Data.IsDead && seer != target && Seer.Mode <= 1)
                {
                    Helpers.ShowFlash(new Color(42f / 255f, 187f / 255f, 245f / 255f));
                }
                if (Seer.DeadBodyPositions != null) Seer.DeadBodyPositions.Add(target.transform.position);
            }

            Arsonist.UpdateStatus();

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
            DeadPlayer deadPlayer = new(__instance, DateTime.UtcNow, DeathReason.Exile, null);
            GameHistory.deadPlayers.Add(deadPlayer);

            // Remove fake tasks when player dies
            if (__instance.HasFakeTasks()) __instance.ClearAllTasks();

            if (__instance.IsImpostor()) Adversity.CheckAndAdversityState();

            __instance.OnDeath(killer: null);
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
    static class PlayerControlSetCoolDownPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] float time)
        {
            if (GameManager.Instance.LogicOptions.currentGameOptions.GetFloat(FloatOptionNames.KillCooldown) <= 0f) return false;
            float multiplier = 1f;
            float addition = 0f;
            if (PlayerControl.LocalPlayer.IsRole(RoleId.BountyHunter)) addition = BountyHunter.AdditionalCooldown;

            float Max = Mathf.Max(GameManager.Instance.LogicOptions.currentGameOptions.GetFloat(FloatOptionNames.KillCooldown) * multiplier + addition, __instance.killTimer);
            __instance.SetKillTimerUnchecked(Mathf.Clamp(time, 0f, Max), Max);
            return false;
        }

        public static void SetKillTimerUnchecked(this PlayerControl player, float time, float Max = float.NegativeInfinity)
        {
            if (Max == float.NegativeInfinity) Max = time;

            player.killTimer = time;
            FastDestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(time, Max);
        }
    }
}