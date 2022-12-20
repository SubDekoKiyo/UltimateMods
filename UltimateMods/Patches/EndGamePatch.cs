namespace UltimateMods.EndGame
{
    public enum CustomGameOverReason
    {
        JesterExiled = 10,
        TeamJackalWin,
        ArsonistWin,

        SabotageReactor,
        SabotageO2,
        ForceEnd,
        EveryoneLose
    }

    public enum FinalStatus
    {
        Alive, // 生存
        Sabotage, // サボ
        Reactor, // リアクターサボ
        Exiled, // 追放
        Misfire, // 誤爆
        Suicide, // 自殺
        Dead, // 死亡(キル)
        LackO2, // 酸素不足
        Bomb, // 爆発
        Torched, // 焼殺
        Revival,
        Disconnected // 切断
    }

    public enum WinCondition
    {
        Default,

        CrewmateWin,
        ImpostorWin,
        JesterWin,
        OpportunistWin,
        JackalWin,
        ArsonistWin,

        ForceEnd,
        EveryoneLose
    }

    static class AdditionalTempData
    {
        public static WinCondition winCondition = WinCondition.Default;
        public static List<WinCondition> additionalWinConditions = new();
        public static List<PlayerRoleInfo> playerRoles = new();
        public static List<byte> WinningPlayers = new();
        public static GameOverReason gameOverReason;

        public static void clear()
        {
            playerRoles.Clear();
            additionalWinConditions.Clear();
            WinningPlayers.Clear();
            winCondition = WinCondition.Default;
        }

        internal class PlayerRoleInfo
        {
            public string WinOrLose { get; set; }
            public string PlayerName { get; set; }
            public string RoleString { get; set; }
            public int ColorId = 0;
            public int TasksCompleted { get; set; }
            public int TasksTotal { get; set; }
            public FinalStatus Status { get; set; }
            public int PlayerId { get; set; }
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    public static class OnGameEndPatch
    {
        public static void Prefix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
        {
            AdditionalTempData.gameOverReason = endGameResult.GameOverReason;
            if ((int)endGameResult.GameOverReason >= 10) endGameResult.GameOverReason = GameOverReason.ImpostorByKill;
        }

        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
        {
            var GameOverReason = AdditionalTempData.gameOverReason;
            AdditionalTempData.clear();

            var excludeRoles = new RoleId[] { };
            foreach (var p in GameData.Instance.AllPlayers)
            {
                var (tasksCompleted, tasksTotal) = TasksHandler.taskInfo(p);
                var finalStatus = finalStatuses[p.PlayerId] =
                    p.Disconnected == true ? FinalStatus.Disconnected :
                    finalStatuses.ContainsKey(p.PlayerId) ? finalStatuses[p.PlayerId] :
                    p.IsDead == true ? FinalStatus.Dead :
                    GameOverReason == (GameOverReason)CustomGameOverReason.SabotageReactor && !p.Role.IsImpostor ? FinalStatus.Reactor :
                    GameOverReason == (GameOverReason)CustomGameOverReason.SabotageO2 && !p.Role.IsImpostor ? FinalStatus.LackO2 :
                    FinalStatus.Alive;

                if (GameOverReason == GameOverReason.HumansByTask && p.Object.IsCrew()) tasksCompleted = tasksTotal;

                AdditionalTempData.playerRoles.Add(new AdditionalTempData.PlayerRoleInfo()
                {
                    PlayerName = p.PlayerName,
                    PlayerId = p.PlayerId,
                    ColorId = p.DefaultOutfit.ColorId,
                    // NameSuffix = Lovers.getIcon(p.Object),
                    RoleString = p.Object.GetRoleAndModString(p.Object.GetRoleId(), p.Object.GetModifierId()),
                    TasksTotal = tasksTotal,
                    TasksCompleted = tasksCompleted,
                    Status = finalStatus,
                });
            }

            List<PlayerControl> notWinners = new();
            notWinners.AddRange(Jester.allPlayers);
            notWinners.AddRange(Opportunist.allPlayers);
            notWinners.AddRange(Madmate.allPlayers);
            notWinners.AddRange(Jackal.allPlayers);
            notWinners.AddRange(Sidekick.allPlayers);
            notWinners.AddRange(Arsonist.allPlayers);

            List<WinningPlayerData> winnersToRemove = new();
            foreach (WinningPlayerData winner in TempData.winners)
            {
                if (notWinners.Any(x => x.Data.PlayerName == winner.PlayerName)) winnersToRemove.Add(winner);
            }
            foreach (var winner in winnersToRemove) TempData.winners.Remove(winner);


            bool JesterWin = Jester.exists && GameOverReason == (GameOverReason)CustomGameOverReason.JesterExiled;
            bool TeamJackalWin = GameOverReason == (GameOverReason)CustomGameOverReason.TeamJackalWin && (Jackal.livingPlayers.Count > 0 || ((Sidekick.allPlayers.Count > 0 && Sidekick.livingPlayers.Count >= 0) || Sidekick.allPlayers.Count <= 0));
            bool ArsonistWin = Arsonist.exists && GameOverReason == (GameOverReason)CustomGameOverReason.ArsonistWin;

            bool CrewmateWin = GameOverReason is GameOverReason.HumansByTask or GameOverReason.HumansByVote;
            bool ImpostorWin = GameOverReason is GameOverReason.ImpostorByKill or GameOverReason.ImpostorBySabotage or GameOverReason.ImpostorByVote;
            bool ForceEnd = GameOverReason is (GameOverReason)CustomGameOverReason.ForceEnd;
            bool SaboWin = GameOverReason is GameOverReason.ImpostorBySabotage;
            bool EveryoneLose = AdditionalTempData.playerRoles.All(x => x.Status != FinalStatus.Alive);

            if (JesterWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                foreach (var jester in Jester.players)
                {
                    WinningPlayerData wpd = new(jester.player.Data);
                    TempData.winners.Add(wpd);
                    jester.player.Data.IsDead = true;
                    AdditionalTempData.WinningPlayers.Add(jester.player.Data.PlayerId);
                }
                AdditionalTempData.winCondition = WinCondition.JesterWin;
            }

            else if (TeamJackalWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                foreach (var jackal in Jackal.players)
                {
                    WinningPlayerData wpd = new(jackal.player.Data);
                    wpd.IsImpostor = false;
                    TempData.winners.Add(wpd);
                    AdditionalTempData.WinningPlayers.Add(jackal.player.Data.PlayerId);
                }

                foreach (var sidekick in Sidekick.players)
                {
                    WinningPlayerData wpd = new(sidekick.player.Data);
                    wpd.IsImpostor = false;
                    TempData.winners.Add(wpd);
                    AdditionalTempData.WinningPlayers.Add(sidekick.player.Data.PlayerId);
                }
                AdditionalTempData.winCondition = WinCondition.JackalWin;
            }

            else if (ArsonistWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                foreach (var arsonist in Arsonist.players)
                {
                    WinningPlayerData wpd = new(arsonist.player.Data);
                    TempData.winners.Add(wpd);
                    AdditionalTempData.winCondition = WinCondition.ArsonistWin;
                    AdditionalTempData.WinningPlayers.Add(arsonist.player.Data.PlayerId);
                }
            }

            else if (CrewmateWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (player.IsCrew())
                    {
                        WinningPlayerData wpd = new(player.Data);
                        TempData.winners.Add(wpd);
                        AdditionalTempData.WinningPlayers.Add(player.Data.PlayerId);
                    }
                }
                AdditionalTempData.winCondition = WinCondition.CrewmateWin;
            }

            else if (ImpostorWin)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (player.IsImpostor())
                    {
                        WinningPlayerData wpd = new(player.Data);
                        TempData.winners.Add(wpd);
                        AdditionalTempData.WinningPlayers.Add(player.Data.PlayerId);
                    }
                }
                AdditionalTempData.winCondition = WinCondition.ImpostorWin;
            }

            else if (ForceEnd)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (player != null)
                    {
                        WinningPlayerData wpd = new(player.Data);
                        TempData.winners.Add(wpd);
                        player.Data.IsDead = false;
                    }
                }
                AdditionalTempData.winCondition = WinCondition.ForceEnd;
            }

            else if (EveryoneLose)
            {
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                AdditionalTempData.winCondition = WinCondition.EveryoneLose;
            }

            if (!SaboWin)
            {
                bool oppWin = false;
                foreach (var p in Opportunist.allPlayers)
                {
                    if (!TempData.winners.ToArray().Any(x => x.PlayerName == p.Data.PlayerName) && !p.Data.IsDead) TempData.winners.Add(new WinningPlayerData(p.Data));
                    oppWin = true;
                }
                if (oppWin) AdditionalTempData.additionalWinConditions.Add(WinCondition.OpportunistWin);
            }

            if (Madmate.allPlayers.Count > 0 && TempData.winners.ToArray().Any(x => x.IsImpostor))
            {
                if (!Madmate.CanWinTaskEnd || (Madmate.HasTasks && Madmate.CanWinTaskEnd && Madmate.TasksComplete(PlayerControl.LocalPlayer)))
                {
                    foreach (var p in Madmate.allPlayers)
                    {
                        WinningPlayerData wpd = new WinningPlayerData(p.Data);
                        TempData.winners.Add(wpd);
                        AdditionalTempData.WinningPlayers.Add(p.Data.PlayerId);
                    }
                }
            }

            foreach (WinningPlayerData wpd in TempData.winners)
            {
                wpd.IsDead = wpd.IsDead || AdditionalTempData.playerRoles.Any(x => x.PlayerName == wpd.PlayerName && x.Status != FinalStatus.Alive);
            }

            // Reset Settings
            RPCProcedure.ResetVariables();
        }

        public class EndGameNavigationPatch
        {
            public static TMP_Text textRenderer;

            [HarmonyPatch(typeof(EndGameNavigation), nameof(EndGameNavigation.ShowProgression))]
            public class ShowProgressionPatch
            {
                public static void Prefix()
                {
                    if (textRenderer != null)
                    {
                        textRenderer.gameObject.SetActive(false);
                    }
                }
            }

            [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
            public class EndGameManagerSetUpPatch
            {
                public static void Postfix(EndGameManager __instance)
                {
                    // Delete and readd PoolablePlayers always showing the name and role of the player
                    foreach (PoolablePlayer pb in __instance.transform.GetComponentsInChildren<PoolablePlayer>())
                    {
                        UnityEngine.Object.Destroy(pb.gameObject);
                    }
                    int Num = Mathf.CeilToInt(7.5f);
                    List<WinningPlayerData> list = TempData.winners.ToArray().ToList().OrderBy(delegate (WinningPlayerData b)
                    {
                        if (!b.IsYou)
                        {
                            return 0;
                        }
                        return -1;
                    }).ToList<WinningPlayerData>();
                    for (int i = 0; i < list.Count; i++)
                    {
                        WinningPlayerData winningPlayerData2 = list[i];
                        int Num2 = (i % 2 == 0) ? -1 : 1;
                        int Num3 = (i + 1) / 2;
                        float Num4 = (float)Num3 / (float)Num;
                        float Num5 = Mathf.Lerp(1f, 0.75f, Num4);
                        float Num6 = (float)((i == 0) ? -8 : -1);
                        PoolablePlayer poolablePlayer = UnityEngine.Object.Instantiate<PoolablePlayer>(__instance.PlayerPrefab, __instance.transform);
                        poolablePlayer.transform.localPosition = new Vector3(1f * (float)Num2 * (float)Num3 * Num5, FloatRange.SpreadToEdges(-1.125f, 0f, Num3, Num), Num6 + (float)Num3 * 0.01f) * 0.9f;
                        float Num7 = Mathf.Lerp(1f, 0.65f, Num4) * 0.9f;
                        Vector3 vector = new(Num7, Num7, 1f);
                        poolablePlayer.transform.localScale = vector;
                        poolablePlayer.UpdateFromPlayerOutfit((GameData.PlayerOutfit)winningPlayerData2, PlayerMaterial.MaskType.ComplexUI, winningPlayerData2.IsDead, true);
                        if (winningPlayerData2.IsDead)
                        {
                            poolablePlayer.cosmetics.currentBodySprite.BodySprite.sprite = poolablePlayer.cosmetics.currentBodySprite.GhostSprite;
                            poolablePlayer.SetDeadFlipX(i % 2 == 0);
                        }
                        else
                        {
                            poolablePlayer.SetFlipX(i % 2 == 0);
                        }

                        poolablePlayer.cosmetics.nameText.color = Color.white;
                        poolablePlayer.cosmetics.nameText.lineSpacing *= 0.7f;
                        poolablePlayer.cosmetics.nameText.transform.localScale = new Vector3(1f / vector.x, 1f / vector.y, 1f / vector.z);
                        poolablePlayer.cosmetics.nameText.transform.localPosition = new Vector3(poolablePlayer.cosmetics.nameText.transform.localPosition.x, poolablePlayer.cosmetics.nameText.transform.localPosition.y, -15f);
                        poolablePlayer.cosmetics.nameText.text = winningPlayerData2.PlayerName;

                        foreach (var data in AdditionalTempData.playerRoles)
                        {
                            if (data.PlayerName != winningPlayerData2.PlayerName) continue;
                            poolablePlayer.cosmetics.nameText.text += $"\n<size=80%>{data.RoleString}</size>";
                        }
                    }

                    // Additional code
                    GameObject bonusTextObject = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
                    bonusTextObject.transform.position = new Vector3(__instance.WinText.transform.position.x, __instance.WinText.transform.position.y - 0.5f, __instance.WinText.transform.position.z);
                    bonusTextObject.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
                    TMP_Text textRenderer = bonusTextObject.GetComponent<TMP_Text>();
                    textRenderer.text = "";

                    string bonusText = "";

                    if (AdditionalTempData.winCondition == WinCondition.JesterWin)
                    {
                        bonusText = ModTranslation.getString("JesterWin");
                        textRenderer.color = JesterPink;
                        __instance.BackgroundBar.material.SetColor("_Color", JesterPink);
                        if (Options.EnableCustomSounds)
                        {
                            SoundManager.Instance.StopSound(__instance.ImpostorStinger);
                            SoundManager.Instance.PlaySound(JesterWinSound, false, 0.8f);
                        }
                    }
                    else if (AdditionalTempData.winCondition == WinCondition.JackalWin)
                    {
                        bonusText = ModTranslation.getString("TeamJackalWin");
                        textRenderer.color = JackalBlue;
                        __instance.BackgroundBar.material.SetColor("_Color", JackalBlue);
                    }
                    else if (AdditionalTempData.winCondition == WinCondition.ArsonistWin)
                    {
                        bonusText = ModTranslation.getString("ArsonistWin");
                        textRenderer.color = ArsonistOrange;
                        __instance.BackgroundBar.material.SetColor("_Color", ArsonistOrange);
                    }
                    else if (AdditionalTempData.gameOverReason is GameOverReason.HumansByTask or GameOverReason.HumansByVote)
                    {
                        bonusText = ModTranslation.getString("CrewmateWin");
                        textRenderer.color = CrewmateBlue;
                    }
                    else if (AdditionalTempData.gameOverReason is GameOverReason.ImpostorByKill or GameOverReason.ImpostorByVote or GameOverReason.ImpostorBySabotage)
                    {
                        bonusText = ModTranslation.getString("ImpostorWin");
                        textRenderer.color = ImpostorRed;
                    }
                    else if (AdditionalTempData.winCondition == WinCondition.EveryoneLose)
                    {
                        bonusText = ModTranslation.getString("EveryoneLose");
                        textRenderer.color = DisabledGrey;
                        __instance.BackgroundBar.material.SetColor("_Color", DisabledGrey);
                        if (Options.EnableCustomSounds)
                        {
                            SoundManager.Instance.StopSound(__instance.ImpostorStinger);
                            SoundManager.Instance.PlaySound(EveryoneLoseSound, false, 0.8f);
                        }
                    }
                    else if (AdditionalTempData.winCondition == WinCondition.ForceEnd)
                    {
                        __instance.WinText.text = ModTranslation.getString("ForceEnd");
                        textRenderer.color = DisabledGrey;
                        __instance.BackgroundBar.material.SetColor("_Color", DisabledGrey);
                        SoundManager.Instance.StopSound(__instance.ImpostorStinger);
                        SoundManager.Instance.PlaySound(__instance.DisconnectStinger, false, 0.8f);
                    }

                    string extraText = "";
                    foreach (WinCondition w in AdditionalTempData.additionalWinConditions)
                    {
                        switch (w)
                        {
                            case WinCondition.OpportunistWin:
                                extraText += ModTranslation.getString("OpportunistExtra");
                                break;
                            default:
                                break;
                        }
                    }

                    if (extraText.Length > 0)
                    {
                        textRenderer.text = string.Format(ModTranslation.getString(bonusText + "Extra"), extraText);
                    }
                    else
                    {
                        textRenderer.text = ModTranslation.getString(bonusText);
                    }

                    if (AdditionalTempData.gameOverReason == (GameOverReason)CustomGameOverReason.SabotageO2)
                    {
                        textRenderer.text += ($"\n" + ModTranslation.getString("O2Win"));
                    }
                    else if (AdditionalTempData.gameOverReason == (GameOverReason)CustomGameOverReason.SabotageReactor)
                    {
                        textRenderer.text += ($"\n" + ModTranslation.getString("ReactorWin"));
                    }
                    else if (AdditionalTempData.gameOverReason == GameOverReason.HumansByTask)
                    {
                        textRenderer.text += ($"\n" + ModTranslation.getString("TaskWin"));
                    }
                    else if (AdditionalTempData.gameOverReason == (GameOverReason)CustomGameOverReason.ForceEnd)
                    {
                        textRenderer.text += ($"\n" + ModTranslation.getString("FinishedByHost"));
                    }

                    if (Options.ShowRoleSummary)
                    {
                        var position = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, Camera.main.nearClipPlane));
                        GameObject RoleSummary = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
                        RoleSummary.transform.position = new Vector3(__instance.Navigation.ExitButton.transform.position.x + 0.1f, position.y - 0.1f, -14f);
                        RoleSummary.transform.localScale = new Vector3(1f, 1f, 1f);

                        TMP_Text[] RoleSummaryText = new TMP_Text[5];
                        for (int i = 0; i < RoleSummaryText.Length; i++)
                        {
                            RoleSummaryText[i] = RoleSummary.GetComponent<TMP_Text>();
                            RoleSummaryText[i].alignment = TextAlignmentOptions.TopLeft;
                            RoleSummaryText[i].color = White;
                            RoleSummaryText[i].outlineColor = Color.black;
                            RoleSummaryText[i].outlineWidth *= 1.2f;
                            RoleSummaryText[i].fontSizeMin = 1.25f;
                            RoleSummaryText[i].fontSizeMax = 1.25f;
                            RoleSummaryText[i].fontSize = 1.25f;
                        }

                        // Reference - Nebula(https://github.com/Dolly1016/Nebula)
                        StringBuilder playerWin = new();
                        StringBuilder playerName = new();
                        StringBuilder playerFullRole = new();
                        StringBuilder playerFinalStatus = new();
                        StringBuilder playerRoleTaskStatus = new();

                        foreach (var data in AdditionalTempData.playerRoles)
                        {
                            bool IsWon = AdditionalTempData.WinningPlayers.Any(x => x == data.PlayerId);

                            if (IsWon) playerWin.AppendLine(Helpers.cs(CrewmateBlue, "★")); else playerWin.AppendLine(" ");
                            playerName.AppendLine(" " + data.PlayerName);
                            playerFullRole.AppendLine(" " + data.RoleString);
                            playerFinalStatus.AppendLine(" " + ModTranslation.getString("RoleSummary" + data.Status.ToString(), def: "-"));
                            playerRoleTaskStatus.AppendLine(" " + (data.TasksTotal > 0 ? $"<color=#FAD934FF>{data.TasksCompleted}/{data.TasksTotal}</color>" : ""));
                        }

                        RoleSummaryText[0].text = playerWin.ToString();
                        RoleSummaryText[1].text = playerName.ToString();
                        RoleSummaryText[2].text = playerFullRole.ToString();
                        RoleSummaryText[3].text = playerFinalStatus.ToString();
                        RoleSummaryText[4].text = playerRoleTaskStatus.ToString();

                        float width = 0.0f;
                        for (int i = 0; i < 5; i++)
                        {
                            RoleSummaryText[i].transform.position += new Vector3(width, 0f, 0f);
                            width += RoleSummaryText[i].preferredWidth - 0.05f;
                        }

                        AdditionalTempData.clear();
                    }
                }

                [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
                public class CheckEndCriteriaPatch
                {
                    public static bool Prefix()
                    {
                        if (!GameData.Instance) return false;
                        if (DestroyableSingleton<TutorialManager>.InstanceExists) return true; // InstanceExists | Don't check Custom Criteria when in Tutorial
                        if (FastDestroyableSingleton<HudManager>.Instance.IsIntroDisplayed) return false;

                        ShipStatus __instance = ShipStatus.Instance;
                        var statistics = new PlayerStatistics();
                        if (CheckAndEndGameForJackalWin(__instance, statistics)) return false;
                        if (CheckAndEndGameForSabotageWin(__instance)) return false;
                        if (CheckAndEndGameForTaskWin(__instance)) return false;
                        if (CheckAndEndGameForImpostorWin(__instance, statistics)) return false;
                        if (CheckAndEndGameForCrewmateWin(__instance, statistics)) return false;
                        return false;
                    }

                    private static bool CheckAndEndGameForJackalWin(ShipStatus __instance, PlayerStatistics statistics)
                    {
                        if (statistics.TeamJackalAlive >= statistics.TotalAlive - statistics.TeamJackalAlive &&
                            statistics.TeamImpostorsAlive == 0)
                        {
                            UncheckedEndGame(CustomGameOverReason.TeamJackalWin);
                            return true;
                        }
                        return false;
                    }

                    private static bool CheckAndEndGameForSabotageWin(ShipStatus __instance)
                    {
                        if (__instance.Systems == null) return false;
                        ISystemType systemType = __instance.Systems.ContainsKey(SystemTypes.LifeSupp) ? __instance.Systems[SystemTypes.LifeSupp] : null;
                        if (systemType != null)
                        {
                            LifeSuppSystemType lifeSuppSystemType = systemType.TryCast<LifeSuppSystemType>();
                            if (lifeSuppSystemType != null && lifeSuppSystemType.Countdown < 0f)
                            {
                                EndGameForO2(__instance);
                                lifeSuppSystemType.Countdown = 10000f;
                                return true;
                            }
                        }
                        ISystemType systemType2 = __instance.Systems.ContainsKey(SystemTypes.Reactor) ? __instance.Systems[SystemTypes.Reactor] : null;
                        if (systemType2 == null)
                        {
                            systemType2 = __instance.Systems.ContainsKey(SystemTypes.Laboratory) ? __instance.Systems[SystemTypes.Laboratory] : null;
                        }
                        if (systemType2 != null)
                        {
                            ICriticalSabotage criticalSystem = systemType2.TryCast<ICriticalSabotage>();
                            if (criticalSystem != null && criticalSystem.Countdown < 0f)
                            {
                                EndGameForReactor(__instance);
                                criticalSystem.ClearSabotage();
                                return true;
                            }
                        }
                        return false;
                    }

                    private static bool CheckAndEndGameForTaskWin(ShipStatus __instance)
                    {
                        if (GameData.Instance.TotalTasks > 0 && GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
                        {
                            UncheckedEndGame(GameOverReason.HumansByTask);
                            return true;
                        }
                        return false;
                    }

                    private static bool CheckAndEndGameForImpostorWin(ShipStatus __instance, PlayerStatistics statistics)
                    {
                        if (statistics.TeamImpostorsAlive >= statistics.TotalAlive - statistics.TeamImpostorsAlive &&
                            statistics.TeamJackalAlive == 0)
                        {
                            GameOverReason endReason;
                            switch (TempData.LastDeathReason)
                            {
                                case DeathReason.Exile:
                                    endReason = GameOverReason.ImpostorByVote;
                                    break;
                                case DeathReason.Kill:
                                    endReason = GameOverReason.ImpostorByKill;
                                    break;
                                default:
                                    endReason = GameOverReason.ImpostorByVote;
                                    break;
                            }
                            UncheckedEndGame(endReason);
                            return true;
                        }
                        return false;
                    }

                    private static bool CheckAndEndGameForCrewmateWin(ShipStatus __instance, PlayerStatistics statistics)
                    {
                        if (statistics.TeamCrew > 0 && statistics.TeamImpostorsAlive == 0 && statistics.TeamJackalAlive == 0)
                        {
                            UncheckedEndGame(GameOverReason.HumansByVote);
                            return true;
                        }
                        return false;
                    }

                    private static void EndGameForO2(ShipStatus __instance)
                    {
                        UncheckedEndGame(CustomGameOverReason.SabotageO2);
                        return;
                    }

                    private static void EndGameForReactor(ShipStatus __instance)
                    {
                        UncheckedEndGame(CustomGameOverReason.SabotageReactor);
                        return;
                    }

                    private static void UncheckedEndGame(GameOverReason reason)
                    {
                        GameManager.Instance.RpcEndGame(reason, false);
                    }

                    public static void UncheckedEndGame(CustomGameOverReason reason)
                    {
                        UncheckedEndGame((GameOverReason)reason);
                    }
                }

                internal class PlayerStatistics
                {
                    public int TeamImpostorsAlive { get; set; }
                    public int TeamCrew { get; set; }
                    public int TeamJackalAlive { get; set; }
                    public int NeutralAlive { get; set; }
                    public int TotalAlive { get; set; }

                    public PlayerStatistics()
                    {
                        GetPlayerCounts();
                    }

                    private void GetPlayerCounts()
                    {
                        int NumJackalAlive = 0;
                        int NumImpostorsAlive = 0;
                        int NumTotalAlive = 0;
                        int NumNeutralAlive = 0;
                        int NumCrew = 0;

                        foreach (var playerInfo in GameData.Instance.AllPlayers)
                        {
                            if (!playerInfo.Disconnected)
                            {
                                if (playerInfo.Object.IsCrew()) NumCrew++;
                                if (!playerInfo.IsDead)
                                {
                                    NumTotalAlive++;
                                    if (playerInfo.Role.IsImpostor) NumImpostorsAlive++;
                                    if (playerInfo.Object.IsNeutral()) NumNeutralAlive++;
                                    if (playerInfo.Object.IsTeamJackal()) NumJackalAlive++;
                                }
                            }
                        }

                        TeamCrew = NumCrew;
                        TeamImpostorsAlive = NumImpostorsAlive;
                        TeamJackalAlive = NumJackalAlive;
                        NeutralAlive = NumNeutralAlive;
                        TotalAlive = NumTotalAlive;
                    }
                }
            }
        }
    }
}