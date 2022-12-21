namespace UltimateMods.Roles;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
public static class RolesButtons
{
    private static CustomButton AltruistButton;
    private static CustomButton EngineerRepairButton;
    private static CustomButton LighterButton;
    private static CustomButton MayorMeetingButton;
    private static CustomButton SheriffKillButton;
    private static CustomButton EvilHackerAdminButton;
    private static CustomButton TeleportButton;
    public static CustomButton UnderTakerButton;
    private static CustomButton ArsonistButton;
    private static CustomButton IgniteButton;
    private static CustomButton JackalKillButton;
    private static CustomButton JackalMakeSidekickButton;
    private static CustomButton SidekickKillButton;

    public static TMP_Text EngineerRepairButtonText;
    public static TMP_Text MayorMeetingButtonText;
    public static TMP_Text SheriffNumShotsText;

    public static void SetButtonCooldowns()
    {
        AltruistButton.Timer = AltruistButton.MaxTimer = 0f;
        EngineerRepairButton.MaxTimer = 0f;
        LighterButton.MaxTimer = Lighter.Cooldown; LighterButton.EffectDuration = Lighter.Duration;
        MayorMeetingButton.MaxTimer = GameManager.Instance.LogicOptions.currentGameOptions.GetInt(Int32OptionNames.EmergencyCooldown);
        SheriffKillButton.MaxTimer = Sheriff.Cooldown;
        EvilHackerAdminButton.MaxTimer = 0f;
        TeleportButton.MaxTimer = Teleporter.Cooldown;
        UnderTakerButton.MaxTimer = UnderTaker.MoveCooldown;
        UnderTakerButton.EffectDuration = UnderTaker.Duration;
        ArsonistButton.MaxTimer = Arsonist.Cooldown;
        IgniteButton.MaxTimer = 0f;
        JackalKillButton.MaxTimer = Jackal.Cooldown;
        JackalMakeSidekickButton.MaxTimer = Jackal.CreateSideKickCooldown;
        SidekickKillButton.MaxTimer = Sidekick.Cooldown;
    }

    public static void Postfix(HudManager __instance)
    {
        AltruistButton = new(
            () =>
            {
                MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.AltruistKill, Hazel.SendOption.Reliable, -1);
                killWriter.Write(PlayerControl.LocalPlayer.Data.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                RPCProcedure.AltruistKill(PlayerControl.LocalPlayer.Data.PlayerId);
                Altruist.Started = true;
            },
            () => { return PlayerControl.LocalPlayer.IsRole(RoleId.Altruist) && !Altruist.Ended; },
            () =>
            {
                bool CanRevive = false;
                foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(), 1f, Constants.PlayersOnlyMask)) if (collider2D.tag == "DeadBody") CanRevive = true;
                return CanRevive && PlayerControl.LocalPlayer.CanMove;
            },
            () => { },
            Altruist.GetButtonSprite(),
            ButtonPositions.RightTop,
            __instance,
            __instance.KillButton,
            KeyCode.F,
            true,
            Altruist.Duration,
            () =>
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.AltruistRevive, Hazel.SendOption.Reliable, -1);
                writer.Write(Altruist.Target);
                writer.Write(PlayerControl.LocalPlayer.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.AltruistRevive(Altruist.Target, PlayerControl.LocalPlayer.PlayerId);
                Altruist.Ended = true;
            }
        );
        AltruistButton.ButtonText = ModTranslation.getString("AltruistReviveText");
        AltruistButton.EffectCancellable = false;

        EngineerRepairButton = new(
            () =>
            {
                EngineerRepairButton.Timer = 0f;

                MessageWriter usedRepairWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EngineerUsedRepair, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(usedRepairWriter);
                RPCProcedure.EngineerUsedRepair(PlayerControl.LocalPlayer.Data.PlayerId);

                foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks.GetFastEnumerator())
                {
                    if (task.TaskType == TaskTypes.FixLights)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EngineerFixLights, Hazel.SendOption.Reliable, -1);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.EngineerFixLights();
                    }
                    else if (task.TaskType == TaskTypes.RestoreOxy)
                    {
                        MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.LifeSupp, 0 | 64);
                        MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.LifeSupp, 1 | 64);
                    }
                    else if (task.TaskType == TaskTypes.ResetReactor)
                    {
                        MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 16);
                    }
                    else if (task.TaskType == TaskTypes.ResetSeismic)
                    {
                        MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Laboratory, 16);
                    }
                    else if (task.TaskType == TaskTypes.FixComms)
                    {
                        MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Comms, 16 | 0);
                        MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Comms, 16 | 1);
                    }
                    else if (task.TaskType == TaskTypes.StopCharles)
                    {
                        MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 0 | 16);
                        MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 1 | 16);
                    }
                }
            },
            () =>
            {
                return PlayerControl.LocalPlayer.IsRole(RoleId.Engineer) && !PlayerControl.LocalPlayer.Data.IsDead && ProEngineer.ReamingCounts > 0 && ProEngineer.CanFixSabo;
            },
            () =>
            {
                bool sabotageActive = false;
                foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks.GetFastEnumerator())
                    if (task.TaskType == TaskTypes.FixLights || task.TaskType == TaskTypes.RestoreOxy || task.TaskType == TaskTypes.ResetReactor || task.TaskType == TaskTypes.ResetSeismic || task.TaskType == TaskTypes.FixComms || task.TaskType == TaskTypes.StopCharles)
                        sabotageActive = true;

                if (EngineerRepairButtonText != null)
                {
                    if (ProEngineer.ReamingCounts > 0)
                        EngineerRepairButtonText.text = String.Format(ModTranslation.getString("ReamingCount"), ProEngineer.ReamingCounts);
                    else
                        EngineerRepairButtonText.text = "";
                }

                return sabotageActive && ProEngineer.ReamingCounts > 0 && PlayerControl.LocalPlayer.CanMove;
            },
            () => { },
            ProEngineer.GetFixButtonSprite(),
            ButtonPositions.RightTop,
            __instance,
            __instance.UseButton,
            KeyCode.F
        );
        EngineerRepairButton.ButtonText = ModTranslation.getString("EngineerRepairButtonText");
        EngineerRepairButtonText = GameObject.Instantiate(EngineerRepairButton.actionButton.cooldownTimerText, EngineerRepairButton.actionButton.cooldownTimerText.transform.parent); // TMP初期化
        EngineerRepairButtonText.text = "";
        EngineerRepairButtonText.enableWordWrapping = false;
        EngineerRepairButtonText.transform.localScale = Vector3.one * 0.5f;
        EngineerRepairButtonText.transform.localPosition += new Vector3(-0.05f, 0.5f, 0);

        LighterButton = new(
            () =>
            {
                Lighter.LightActive = true;
            },
            () =>
            {
                return PlayerControl.LocalPlayer.IsRole(RoleId.Lighter) && !PlayerControl.LocalPlayer.Data.IsDead;
            },
            () => { return PlayerControl.LocalPlayer.CanMove; },
            () =>
            {
                Lighter.LightActive = false;
                LighterButton.Timer = LighterButton.MaxTimer;
                LighterButton.IsEffectActive = false;
                LighterButton.actionButton.graphic.color = Palette.EnabledColor;
            },
            Lighter.GetButtonSprite(),
            ButtonPositions.RightTop,
            __instance,
            __instance.UseButton,
            KeyCode.F,
            true,
            Lighter.Duration,
            () =>
            {
                Lighter.LightActive = false;
                LighterButton.Timer = LighterButton.MaxTimer;
            }
        );
        LighterButton.ButtonText = ModTranslation.getString("LighterText");
        LighterButton.EffectCancellable = true;

        MayorMeetingButton = new(
            () =>
            {
                if (Mayor.ReamingCount <= 0)
                {
                    return;
                }
                Mayor.ReamingCount--;

                PlayerControl.LocalPlayer.NetTransform.Halt(); // Stop current movement

                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedCmdReportDeadBody, Hazel.SendOption.Reliable, -1);
                writer.Write(PlayerControl.LocalPlayer.PlayerId);
                writer.Write(Byte.MaxValue);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.UncheckedCmdReportDeadBody(PlayerControl.LocalPlayer.PlayerId, Byte.MinValue);

                MayorMeetingButton.Timer = MayorMeetingButton.MaxTimer;
            },
            () =>
            {
                return PlayerControl.LocalPlayer.IsRole(RoleId.Mayor) && !PlayerControl.LocalPlayer.Data.IsDead && Mayor.HasMeetingButton && Mayor.ReamingCount > 0;
            },
            () =>
            {
                bool sabotageActive = false;
                foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks.GetFastEnumerator())
                    if (task.TaskType is TaskTypes.FixLights or
                                        TaskTypes.RestoreOxy or
                                        TaskTypes.ResetReactor or
                                        TaskTypes.ResetSeismic or
                                        TaskTypes.FixComms or
                                        TaskTypes.StopCharles) sabotageActive = true;

                if (MayorMeetingButtonText != null)
                {
                    if (Mayor.ReamingCount > 0) MayorMeetingButtonText.text = String.Format(ModTranslation.getString("ReamingCount"), Mayor.ReamingCount);
                    else MayorMeetingButtonText.text = "";
                }

                return !sabotageActive && PlayerControl.LocalPlayer.CanMove;
            },
            () => { MayorMeetingButton.Timer = MayorMeetingButton.MaxTimer; },
            Mayor.GetButtonSprite(),
            ButtonPositions.RightTop,
            __instance,
            __instance.UseButton,
            KeyCode.F
        );
        MayorMeetingButton.ButtonText = ModTranslation.getString("MayorMeetingButtonText");
        MayorMeetingButtonText = GameObject.Instantiate(MayorMeetingButton.actionButton.cooldownTimerText, MayorMeetingButton.actionButton.cooldownTimerText.transform.parent);
        MayorMeetingButtonText.text = "";
        MayorMeetingButtonText.enableWordWrapping = false;
        MayorMeetingButtonText.transform.localScale = Vector3.one * 0.5f;
        MayorMeetingButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

        SheriffKillButton = new(
            () =>
            {
                if (Sheriff.ReamingShots <= 0)
                {
                    return;
                }

                MurderAttemptResult murderAttemptResult = Helpers.CheckMurderAttempt(PlayerControl.LocalPlayer, Sheriff.currentTarget);
                if (murderAttemptResult == MurderAttemptResult.SuppressKill) return;

                if (murderAttemptResult == MurderAttemptResult.PerformKill)
                {
                    bool misfire = false;
                    byte targetId = Sheriff.currentTarget.PlayerId;
                    if ((Sheriff.currentTarget.Data.Role.IsImpostor) ||
                        (Sheriff.CanKillNeutrals && Sheriff.currentTarget.IsNeutral()) ||
                        (Madmate.CanDieToSheriffOrYakuza && Sheriff.currentTarget.IsRole(RoleId.Madmate)))
                    {
                        targetId = Sheriff.currentTarget.PlayerId;
                        misfire = false;
                    }
                    else
                    {
                        targetId = PlayerControl.LocalPlayer.PlayerId;
                        misfire = true;
                    }

                    MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SheriffKill, Hazel.SendOption.Reliable, -1);
                    killWriter.Write(PlayerControl.LocalPlayer.Data.PlayerId);
                    killWriter.Write(targetId);
                    killWriter.Write(misfire);
                    AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                    RPCProcedure.SheriffKill(PlayerControl.LocalPlayer.Data.PlayerId, targetId, misfire);
                }

                SheriffKillButton.Timer = SheriffKillButton.MaxTimer;
                Sheriff.currentTarget = null;
            },
            () => { return PlayerControl.LocalPlayer.IsRole(RoleId.Sheriff) && Sheriff.ReamingShots > 0 && !PlayerControl.LocalPlayer.Data.IsDead; },
            () =>
            {
                if (SheriffNumShotsText != null)
                {
                    if (Sheriff.ReamingShots > 0) SheriffNumShotsText.text = String.Format(ModTranslation.getString("ReamingShots"), Sheriff.ReamingShots);
                    else SheriffNumShotsText.text = "";
                }
                return Sheriff.currentTarget && PlayerControl.LocalPlayer.CanMove;
            },
            () => { SheriffKillButton.Timer = SheriffKillButton.MaxTimer; },
            __instance.KillButton.graphic.sprite,
            ButtonPositions.RightTop,
            __instance,
            __instance.KillButton,
            KeyCode.Q
        );
        SheriffNumShotsText = GameObject.Instantiate(SheriffKillButton.actionButton.cooldownTimerText, SheriffKillButton.actionButton.cooldownTimerText.transform.parent);
        SheriffNumShotsText.text = "";
        SheriffNumShotsText.enableWordWrapping = false;
        SheriffNumShotsText.transform.localScale = Vector3.one * 0.5f;
        SheriffNumShotsText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

        EvilHackerAdminButton = new(
            () =>
            {
                FastDestroyableSingleton<HudManager>.Instance.ToggleMapVisible(new MapOptions()
                {
                    Mode = MapOptions.Modes.CountOverlay,
                    AllowMovementWhileMapOpen = CustomRolesH.EvilHackerCanMoveEvenIfUsesAdmin.getBool(),
                    IncludeDeadBodies = true
                });
            },
            () =>
            {
                return PlayerControl.LocalPlayer.IsRole(RoleId.EvilHacker) && PlayerControl.LocalPlayer.IsAlive();
            },
            () => { return PlayerControl.LocalPlayer.CanMove; },
            () => { EvilHackerAdminButton.MaxTimer = 0f; },
            EvilHacker.GetButtonSprite(),
            ButtonPositions.LeftTop,
            __instance,
            __instance.KillButton,
            KeyCode.F
        );
        EvilHackerAdminButton.ButtonText = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Admin);

        TeleportButton = new(
            () =>
            {
                List<PlayerControl> Target = new();
                foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
                {
                    switch (Teleporter.TeleportTo)
                    {
                        case Teleporter.TeleportTarget.AliveAllPlayer:
                            if (pc.IsAlive() && pc.CanMove)
                            {
                                Target.Add(pc);
                            }
                            break;
                        case Teleporter.TeleportTarget.Crewmate:
                            if (pc.IsAlive() && pc.CanMove && pc.IsCrew())
                            {
                                Target.Add(pc);
                            }
                            break;
                    }
                }

                var player = Helpers.GetRandom(Target);
                MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.TeleporterTeleport, SendOption.Reliable, -1);
                Writer.Write(player.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(Writer);
                RPCProcedure.TeleporterTeleport(player.PlayerId);

                TeleportButton.Timer = Teleporter.Cooldown;
            },
            () =>
            {
                return PlayerControl.LocalPlayer.IsRole(RoleId.Teleporter) && !PlayerControl.LocalPlayer.Data.IsDead;
            },
            () => { return PlayerControl.LocalPlayer.CanMove; },
            () => { TeleportButton.Timer = TeleportButton.MaxTimer = Teleporter.Cooldown; },
            Teleporter.GetButtonSprite(),
            ButtonPositions.LeftTop,
            __instance,
            __instance.KillButton,
            KeyCode.F,
            false
        );
        TeleportButton.ButtonText = ModTranslation.getString("TeleportButtonText");

        UnderTakerButton = new(
            () =>
            {
                if (UnderTaker.DraggingBody)
                {
                    if (UnderTaker.HasDuration && UnderTakerButton.IsEffectActive)
                    {
                        UnderTakerButton.Timer = 0f;
                        return;
                    }
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DragPlaceBody, Hazel.SendOption.Reliable, -1);
                    writer.Write(UnderTaker.BodyId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.DragPlaceBody(UnderTaker.BodyId);

                    foreach (var underTaker in UnderTaker.allPlayers) underTaker.killTimer = UnderTaker.KillCooldown;
                    UnderTakerButton.Timer = UnderTakerButton.MaxTimer = UnderTaker.MoveCooldown;
                }
                else
                {
                    foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(), 1f, Constants.PlayersOnlyMask))
                    {
                        if (collider2D.tag == "DeadBody")
                        {
                            DeadBody component = collider2D.GetComponent<DeadBody>();
                            if (component && !component.Reported)
                            {
                                Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                                Vector2 truePosition2 = component.TruePosition;
                                if (Vector2.Distance(truePosition2, truePosition) <= PlayerControl.LocalPlayer.MaxReportDistance && PlayerControl.LocalPlayer.CanMove && !PhysicsHelpers.AnythingBetween(truePosition, truePosition2, Constants.ShipAndObjectsMask, false))
                                {
                                    GameData.PlayerInfo playerInfo = GameData.Instance.GetPlayerById(component.ParentId);
                                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DragPlaceBody, Hazel.SendOption.Reliable, -1);
                                    writer.Write(playerInfo.PlayerId);
                                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                                    RPCProcedure.DragPlaceBody(playerInfo.PlayerId);
                                    break;
                                }
                            }
                        }
                    }
                }
            },
            () =>
            {
                return PlayerControl.LocalPlayer.IsRole(RoleId.UnderTaker) && !PlayerControl.LocalPlayer.Data.IsDead;
            },
            () =>
            {
                if (UnderTaker.DraggingBody) UnderTakerButton.ButtonText = ModTranslation.getString("UnderTakerDropText");
                else UnderTakerButton.ButtonText = ModTranslation.getString("UnderTakerDragText");
                bool canDrag = false;
                foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(), 1f, Constants.PlayersOnlyMask)) if (collider2D.tag == "DeadBody") canDrag = true;
                return canDrag && PlayerControl.LocalPlayer.CanMove;
            },
            () =>
            {
                UnderTakerButton.Timer = UnderTakerButton.MaxTimer = UnderTaker.MoveCooldown;
                UnderTaker.UnderTakerResetValuesAtDead();
            },
            UnderTaker.GetButtonSprite(),
            ButtonPositions.LeftTop,
            __instance,
            __instance.KillButton,
            KeyCode.F,
            UnderTaker.HasDuration,
            UnderTaker.Duration,
            () =>
            {
                if (UnderTaker.DraggingBody)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DragPlaceBody, Hazel.SendOption.Reliable, -1);
                    writer.Write(UnderTaker.BodyId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.DragPlaceBody(UnderTaker.BodyId);
                }
                else
                {
                    UnderTaker.DraggingBody = false;
                    UnderTaker.BodyId = 0;
                    foreach (var underTaker in UnderTaker.allPlayers) underTaker.killTimer = UnderTaker.KillCooldown;
                }
                UnderTakerButton.Timer = UnderTakerButton.MaxTimer = UnderTaker.MoveCooldown;
            }
        );
        UnderTakerButton.ButtonText = ModTranslation.getString("UnderTakerDragText");
        UnderTakerButton.EffectCancellable = true;

        ArsonistButton = new(
            () =>
            {
                if (Arsonist.CurrentTarget != null)
                {
                    Arsonist.DouseTarget = Arsonist.CurrentTarget;
                }
            },
            () =>
            {
                return PlayerControl.LocalPlayer.IsRole(RoleId.Arsonist) && !Arsonist.DousedEveryone && !PlayerControl.LocalPlayer.Data.IsDead;
            },
            () =>
            {
                if (Arsonist.DousedEveryone) ArsonistButton.ButtonText = ModTranslation.getString("IgniteText");
                else ArsonistButton.ButtonText = ModTranslation.getString("DouseText");

                if (ArsonistButton.IsEffectActive && Arsonist.DouseTarget != Arsonist.CurrentTarget)
                {
                    Arsonist.DouseTarget = null;
                    ArsonistButton.Timer = 0f;
                    ArsonistButton.IsEffectActive = false;
                }

                return PlayerControl.LocalPlayer.CanMove && Arsonist.CurrentTarget != null;
            },
            () =>
            {
                ArsonistButton.Timer = ArsonistButton.MaxTimer;
                ArsonistButton.IsEffectActive = false;
                Arsonist.DouseTarget = null;
                Arsonist.UpdateStatus();
            },
            Arsonist.GetDouseSprite(),
            ButtonPositions.RightTop,
            __instance,
            __instance.KillButton,
            KeyCode.F,
            true,
            Arsonist.Duration,
            () =>
            {
                if (Arsonist.DouseTarget != null)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ArsonistDouse, Hazel.SendOption.Reliable, -1);
                    writer.Write(Arsonist.DouseTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.ArsonistDouse(Arsonist.DouseTarget.PlayerId);
                }

                Arsonist.DouseTarget = null;
                Arsonist.UpdateStatus();
                ArsonistButton.Timer = Arsonist.DousedEveryone ? 0 : ArsonistButton.MaxTimer;

                foreach (PlayerControl p in Arsonist.DousedPlayers)
                {
                    if (Options.PlayerIcons.ContainsKey(p.PlayerId))
                    {
                        Options.PlayerIcons[p.PlayerId].SetSemiTransparent(false);
                    }
                }
            }
        );
        ArsonistButton.ButtonText = ModTranslation.getString("DouseText");

        IgniteButton = new(
            () =>
            {
                if (Arsonist.DousedEveryone)
                {
                    MessageWriter winWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ArsonistWin, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(winWriter);
                    RPCProcedure.ArsonistWin();
                }
            },
            () => { return PlayerControl.LocalPlayer.IsRole(RoleId.Arsonist) && !PlayerControl.LocalPlayer.Data.IsDead && Arsonist.DousedEveryone; },
            () => { return PlayerControl.LocalPlayer.CanMove && Arsonist.DousedEveryone; },
            () => { },
            Arsonist.GetIgniteSprite(),
            ButtonPositions.RightTop,
            __instance,
            __instance.KillButton,
            KeyCode.F
        );
        IgniteButton.ButtonText = ModTranslation.getString("IgniteText");

        JackalKillButton = new(
            () =>
            {
                if (Helpers.CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, Jackal.CurrentTarget) == MurderAttemptResult.SuppressKill) return;

                JackalKillButton.Timer = JackalKillButton.MaxTimer;
                Jackal.CurrentTarget = null;
            },
            () =>
            {
                return PlayerControl.LocalPlayer.IsRole(RoleId.Jackal) && !PlayerControl.LocalPlayer.Data.IsDead;
            },
            () => { return Jackal.CurrentTarget && PlayerControl.LocalPlayer.CanMove; },
            () => { JackalKillButton.Timer = JackalKillButton.MaxTimer; },
            __instance.KillButton.graphic.sprite,
            ButtonPositions.RightTop,
            __instance,
            __instance.KillButton,
            KeyCode.Q,
            false
        );

        JackalMakeSidekickButton = new(
            () =>
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.JackalCreatesSidekick, Hazel.SendOption.Reliable, -1);
                writer.Write(Jackal.CurrentTarget.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.JackalCreatesSidekick(Jackal.CurrentTarget.PlayerId);
            },
            () => { return Jackal.CanSidekick && PlayerControl.LocalPlayer.IsRole(RoleId.Jackal) && !PlayerControl.LocalPlayer.Data.IsDead; },
            () =>
            { return Jackal.CanSidekick && Jackal.CurrentTarget && PlayerControl.LocalPlayer.CanMove; },
            () => { JackalMakeSidekickButton.Timer = JackalMakeSidekickButton.MaxTimer; },
            Jackal.GetButtonSprite(),
            ButtonPositions.LeftBottom,
            __instance,
            __instance.KillButton,
            KeyCode.F,
            false
        );
        JackalMakeSidekickButton.ButtonText = ModTranslation.getString("JackalSidekickText");

        SidekickKillButton = new(
            () =>
            {
                if (Helpers.CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, Sidekick.CurrentTarget) == MurderAttemptResult.SuppressKill) return;

                SidekickKillButton.Timer = SidekickKillButton.MaxTimer;
                Sidekick.CurrentTarget = null;
            },
            () =>
            {
                return Sidekick.CanKill && PlayerControl.LocalPlayer.IsRole(RoleId.Jackal) && !PlayerControl.LocalPlayer.Data.IsDead;
            },
            () => { return Sidekick.CurrentTarget && PlayerControl.LocalPlayer.CanMove; },
            () => { SidekickKillButton.Timer = SidekickKillButton.MaxTimer; },
            __instance.KillButton.graphic.sprite,
            ButtonPositions.RightTop,
            __instance,
            __instance.KillButton,
            KeyCode.Q,
            false
        );

        SetButtonCooldowns();
    }
}