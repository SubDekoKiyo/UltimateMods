namespace UltimateMods.Roles;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
public static class NeutralButtons
{
    private static CustomButton ArsonistButton;
    private static CustomButton IgniteButton;
    private static CustomButton JackalKillButton;
    private static CustomButton JackalMakeSidekickButton;
    private static CustomButton SidekickKillButton;

    public static void SetButtonCooldowns()
    {
        ArsonistButton.MaxTimer = Arsonist.Cooldown;
        IgniteButton.MaxTimer = 0f;
        JackalKillButton.MaxTimer = Jackal.Cooldown;
        JackalMakeSidekickButton.MaxTimer = Jackal.CreateSideKickCooldown;
        SidekickKillButton.MaxTimer = Sidekick.Cooldown;
    }

    public static void Postfix(HudManager __instance)
    {
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