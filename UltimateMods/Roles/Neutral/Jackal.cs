namespace UltimateMods.Roles
{
    [HarmonyPatch]
    public class Jackal : RoleBase<Jackal>
    {
        public static PlayerControl CurrentTarget;
        private static CustomButton JackalKillButton;
        private static CustomButton JackalMakeSidekickButton;
        public static Sprite JackalSidekickButtonSprite;
        public static List<PlayerControl> BlockTarget = new();

        public static float Cooldown { get { return CustomRolesH.JackalKillCooldown.getFloat(); } }
        public static float CreateSideKickCooldown { get { return CustomRolesH.JackalCreateSidekickCooldown.getFloat(); } }
        public static bool CanUseVents { get { return CustomRolesH.JackalCanUseVents.getBool(); } }
        public static bool CanCreateSidekick { get { return CustomRolesH.JackalCanCreateSidekick.getBool(); } }
        public static bool JackalPromotedFromSidekickCanCreateSidekick { get { return CustomRolesH.JackalPromotedFromSidekickCanCreateSidekick.getBool(); } }
        public static bool HasImpostorVision { get { return CustomRolesH.JackalAndSidekickHaveImpostorVision.getBool(); } }
        public static bool CanSidekick = true;

        public Jackal()
        {
            RoleType = roleId = RoleType.Jackal;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate()
        {
            if (player == PlayerControl.LocalPlayer)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.IsTeamJackal())
                        BlockTarget.Add(pc);
                }

                CurrentTarget = SetTarget(untargetablePlayers: BlockTarget);
                SetPlayerOutline(CurrentTarget, JackalBlue);
            }
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static Sprite GetButtonSprite()
        {
            if (JackalSidekickButtonSprite) return JackalSidekickButtonSprite;
            JackalSidekickButtonSprite = Helpers.LoadSpriteFromTexture2D(JackalSidekickButton, 115f);
            return JackalSidekickButtonSprite;
        }

        public static void MakeButtons(HudManager hm)
        {
            JackalKillButton = new CustomButton(
                () =>
                {
                    if (Helpers.CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, CurrentTarget) == MurderAttemptResult.SuppressKill) return;

                    JackalKillButton.Timer = JackalKillButton.MaxTimer;
                    CurrentTarget = null;
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleType.Jackal) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return CurrentTarget && PlayerControl.LocalPlayer.CanMove; },
                () => { JackalKillButton.Timer = JackalKillButton.MaxTimer; },
                hm.KillButton.graphic.sprite,
                ButtonPositions.RightTop,
                hm,
                hm.KillButton,
                KeyCode.Q,
                false
            );

            JackalMakeSidekickButton = new CustomButton(
                () =>
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.JackalCreatesSidekick, Hazel.SendOption.Reliable, -1);
                    writer.Write(CurrentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.JackalCreatesSidekick(CurrentTarget.PlayerId);
                },
                () => { return CanSidekick && PlayerControl.LocalPlayer.isRole(RoleType.Jackal) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                { return CanSidekick && CurrentTarget && PlayerControl.LocalPlayer.CanMove; },
                () => { JackalMakeSidekickButton.Timer = JackalMakeSidekickButton.MaxTimer; },
                GetButtonSprite(),
                ButtonPositions.LeftBottom,
                hm,
                hm.KillButton,
                KeyCode.F,
                false
            );
            JackalMakeSidekickButton.ButtonText = ModTranslation.getString("JackalSidekickText");
        }

        public static void SetButtonCooldowns()
        {
            JackalKillButton.MaxTimer = Cooldown;
            JackalMakeSidekickButton.MaxTimer = CreateSideKickCooldown;
        }

        public static void Clear()
        {
            CanSidekick = CanCreateSidekick;
            CurrentTarget = null;
            players = new List<Jackal>();
        }
    }
}