namespace UltimateMods.Roles
{
    [HarmonyPatch]
    public class Sidekick : RoleBase<Sidekick>
    {
        public static PlayerControl CurrentTarget;
        private static CustomButton SidekickKillButton;

        public static float Cooldown { get { return CustomRolesH.JackalKillCooldown.getFloat(); } }
        public static bool CanUseVents { get { return CustomRolesH.SidekickCanUseVents.getBool(); } }
        public static bool CanKill { get { return CustomRolesH.SidekickCanKill.getBool(); } }
        public static bool PromotesToJackal { get { return CustomRolesH.SidekickPromotesToJackal.getBool(); } }
        public static bool HasImpostorVision { get { return CustomRolesH.JackalAndSidekickHaveImpostorVision.getBool(); } }

        public Sidekick()
        {
            RoleType = roleId = RoleType.Sidekick;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate()
        {
            foreach (var sidekick in Sidekick.allPlayers)
            {
                if (sidekick != PlayerControl.LocalPlayer) return;
                var BlockTarget = new List<PlayerControl>();
                foreach (var jackal in Jackal.allPlayers)
                    if (jackal != null) BlockTarget.Add(jackal);

                CurrentTarget = SetTarget(untargetablePlayers: BlockTarget);
                if (CanKill) SetPlayerOutline(CurrentTarget, ImpostorRed);
            }

            foreach (var jackal in Jackal.allPlayers)
            {
                if (Sidekick.PromotesToJackal &&
                    PlayerControl.LocalPlayer.isRole(RoleType.Sidekick) &&
                    PlayerControl.LocalPlayer.IsAlive() &&
                    !(Jackal.livingPlayers.Count > 0))
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SidekickPromotes, Hazel.SendOption.Reliable, -1);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.SidekickPromotes(PlayerControl.LocalPlayer.PlayerId);
                }
            }
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void MakeButtons(HudManager hm)
        {
            SidekickKillButton = new CustomButton(
                () =>
                {
                    if (Helpers.CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, CurrentTarget) == MurderAttemptResult.SuppressKill) return;

                    SidekickKillButton.Timer = SidekickKillButton.MaxTimer;
                    CurrentTarget = null;
                },
                () => { return CanKill && PlayerControl.LocalPlayer.isRole(RoleType.Jackal) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return CurrentTarget && PlayerControl.LocalPlayer.CanMove; },
                () => { SidekickKillButton.Timer = SidekickKillButton.MaxTimer; },
                hm.KillButton.graphic.sprite,
                ButtonPositions.RightTop,
                hm,
                hm.KillButton,
                KeyCode.Q,
                false
            );
        }

        public static void SetButtonCooldowns()
        {
            SidekickKillButton.MaxTimer = Cooldown;
        }

        public static void Clear()
        {
            CurrentTarget = null;
            players = new List<Sidekick>();
        }
    }
}