namespace UltimateMods.Roles
{
    [HarmonyPatch]
    public class Mayor : RoleBase<Mayor>
    {
        private static CustomButton MayorMeetingButton;
        public static TMP_Text MayorMeetingButtonText;
        public static Sprite MayorMeetingButtonSprite;
        public static int NumVotes { get { return Mathf.RoundToInt(CustomRolesH.MayorNumVotes.getFloat()); } }
        public static bool MeetingButton { get { return CustomRolesH.MayorMeetingButton.getBool(); } }
        public static int MaxButton { get { return Mathf.RoundToInt(CustomRolesH.MayorNumMeetingButton.getFloat()); } }
        public int NumButton = 2;


        public Mayor()
        {
            RoleType = roleId = RoleType.Mayor;
            NumButton = MaxButton;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static Sprite GetButtonSprite()
        {
            if (MayorMeetingButtonSprite) return MayorMeetingButtonSprite;
            MayorMeetingButtonSprite = Helpers.LoadSpriteFromTexture2D(MayorMeetingButtonSprite, 115f);
            return MayorMeetingButtonSprite;
        }

        public static void MakeButtons(HudManager hm)
        {
            MayorMeetingButton = new CustomButton(
                () =>
                {
                    if (local.NumButton <= 0)
                    {
                        return;
                    }
                    local.NumButton--;

                    PlayerControl.LocalPlayer.NetTransform.Halt(); // Stop current movement

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedCmdReportDeadBody, Hazel.SendOption.Reliable, -1);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    writer.Write(Byte.MaxValue);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.UncheckedCmdReportDeadBody(PlayerControl.LocalPlayer.PlayerId, Byte.MinValue);

                    MayorMeetingButton.Timer = MayorMeetingButton.MaxTimer;
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleType.Mayor) && !PlayerControl.LocalPlayer.Data.IsDead && MeetingButton && local.NumButton > 0; },
                () =>
                {
                    bool sabotageActive = false;
                    foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks.GetFastEnumerator())
                        if (task.TaskType is TaskTypes.FixLights or
                                            TaskTypes.RestoreOxy or
                                            TaskTypes.ResetReactor or
                                            TaskTypes.ResetSeismic or
                                            TaskTypes.FixComms or
                                            TaskTypes.StopCharles)
                            sabotageActive = true;
                    return !sabotageActive && PlayerControl.LocalPlayer.CanMove;
                },
                () => { MayorMeetingButton.Timer = MayorMeetingButton.MaxTimer; },
                GetButtonSprite(),
                ButtonPositions.RightTop,
                hm,
                hm.UseButton,
                KeyCode.F
            );
            MayorMeetingButton.ButtonText = ModTranslation.getString("MayorMeetingButtonText");
        }

        public static void SetButtonCooldowns()
        {
            MayorMeetingButton.MaxTimer = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.EmergencyCooldown);
        }

        public static void Clear()
        {
            players = new List<Mayor>();
        }
    }
}