namespace UltimateMods.Roles
{
    [HarmonyPatch]
    public class Altruist : RoleBase<Altruist>
    {
        private static CustomButton AltruistButton;
        public static Sprite AltruistButtonSprite;
        public static bool Started = false;
        public static bool Ended = false;
        public static DeadBody Target;

        public static float Duration { get { return CustomRolesH.AltruistDuration.getFloat(); } }

        public Altruist()
        {
            RoleType = roleId = RoleType.Altruist;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate()
        {
            var TruePosition = PlayerControl.LocalPlayer.GetTruePosition();
            var MaxDistance = GameOptionsData.KillDistances[GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.KillDistance)];
            var flag = (GameOptionsManager.Instance.CurrentGameOptions.GetBool(BoolOptionNames.GhostsDoTasks) || !PlayerControl.LocalPlayer.Data.IsDead) &&
                        (!AmongUsClient.Instance || !AmongUsClient.Instance.IsGameOver) &&
                        PlayerControl.LocalPlayer.CanMove;
            var OverlapCircle = Physics2D.OverlapCircleAll(TruePosition, MaxDistance, LayerMask.GetMask(new[] { "Players", "Ghost" }));
            var ClosestDistance = float.MaxValue;

            foreach (var collider2D in OverlapCircle)
            {
                if (!flag || PlayerControl.LocalPlayer.Data.IsDead || collider2D.tag != "DeadBody" || Started) continue;
                Target = collider2D.GetComponent<DeadBody>();

                if (!(Vector2.Distance(TruePosition, Target.TruePosition) <= MaxDistance)) continue;

                var Distance = Vector2.Distance(TruePosition, Target.TruePosition);
                if (!(Distance < ClosestDistance)) continue;
                ClosestDistance = Distance;
            }
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static Sprite GetButtonSprite()
        {
            if (AltruistButtonSprite) return AltruistButtonSprite;
            AltruistButtonSprite = Helpers.LoadSpriteFromTexture2D(AltruistReviveButton, 115f);
            return AltruistButtonSprite;
        }

        public static void MakeButtons(HudManager hm)
        {
            AltruistButton = new CustomButton(
                () =>
                {
                    MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.AltruistKill, Hazel.SendOption.Reliable, -1);
                    killWriter.Write(PlayerControl.LocalPlayer.Data.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                    RPCProcedure.AltruistKill(PlayerControl.LocalPlayer.Data.PlayerId);
                    Started = true;
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleType.Altruist) && !Ended; },
                () =>
                {
                    bool CanRevive = false;
                    foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(), 1f, Constants.PlayersOnlyMask))
                        if (collider2D.tag == "DeadBody")
                            CanRevive = true;
                    return CanRevive && PlayerControl.LocalPlayer.CanMove;
                },
                () => { },
                GetButtonSprite(),
                ButtonPositions.RightTop,
                hm,
                hm.KillButton,
                KeyCode.F,
                true,
                Duration,
                () =>
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.AltruistRevive, Hazel.SendOption.Reliable, -1);
                    writer.Write(Target);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.AltruistRevive(Target, PlayerControl.LocalPlayer.PlayerId);
                    Ended = true;
                }
            );
            AltruistButton.ButtonText = ModTranslation.getString("AltruistReviveText");
            AltruistButton.EffectCancellable = false;
        }

        public static void SetButtonCooldowns()
        {
            AltruistButton.Timer = AltruistButton.MaxTimer = 0f;
        }

        public override void Clear()
        {
            Started = false;
            Ended = false;
            players = new List<Altruist>();
        }
    }
}