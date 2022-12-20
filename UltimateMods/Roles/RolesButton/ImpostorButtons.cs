namespace UltimateMods.Roles;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
public static class ImpostorButtons
{
    private static CustomButton EvilHackerAdminButton;
    private static CustomButton TeleportButton;
    public static CustomButton UnderTakerButton;

    public static void SetButtonCooldowns()
    {
        TeleportButton.MaxTimer = Teleporter.Cooldown;
        UnderTakerButton.MaxTimer = UnderTaker.MoveCooldown;
        UnderTakerButton.EffectDuration = UnderTaker.Duration;
    }

    public static void Postfix(HudManager __instance)
    {
        EvilHackerAdminButton = new(
            () =>
            {
                PlayerControl.LocalPlayer.NetTransform.Halt();
                HudManager.Instance.ToggleMapVisible(new MapOptions()
                {
                    Mode = MapOptions.Modes.CountOverlay,
                    AllowMovementWhileMapOpen = CustomRolesH.EvilHackerCanMoveEvenIfUsesAdmin.getBool(),
                    IncludeDeadBodies = true
                });
            },
            () =>
            {
                return PlayerControl.LocalPlayer.IsRole(RoleId.EvilHacker) &&
                    PlayerControl.LocalPlayer.IsAlive();
            },
            () => { return PlayerControl.LocalPlayer.CanMove; },
            () => { },
            EvilHacker.GetButtonSprite(),
            ButtonPositions.LeftTop,
            __instance,
            __instance.KillButton,
            KeyCode.F,
            false,
            0f,
            () => { },
            GameManager.Instance.LogicOptions.currentGameOptions.GetByte(ByteOptionNames.MapId) == 3,
            DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Admin)
        );

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

        SetButtonCooldowns();
    }
}