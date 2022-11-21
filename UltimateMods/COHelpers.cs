namespace UltimateMods
{
    public static class COHelpers
    {
        public static bool ShowButtons
        {
            get
            {
                return !(MapBehaviour.Instance && MapBehaviour.Instance.IsOpen) &&
                        !MeetingHud.Instance &&
                        !ExileController.Instance;
            }
        }

        public static bool ShowMeetingText
        {
            get
            {
                return MeetingHud.Instance != null &&
                    (MeetingHud.Instance.state == MeetingHud.VoteStates.Voted ||
                    MeetingHud.Instance.state == MeetingHud.VoteStates.NotVoted ||
                    MeetingHud.Instance.state == MeetingHud.VoteStates.Discussion);
            }
        }

        public static bool GameStarted
        {
            get
            {
                return AmongUsClient.Instance != null && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started;
            }
        }

        public static bool RolesEnabled
        {
            get
            {
                return CustomOptionsH.ActivateModRoles.getBool();
            }
        }

        public static bool RefundVotes
        {
            get
            {
                return CustomOptionsH.RefundVotesOnDeath.getBool();
            }
        }/*

        public static bool IsGodMiraHQ
        {
            get
            {
                return CustomOptionsH.EnableGodMiraHQ.getBool() && PlayerControl.GameOptions.MapId == 1;
            }
        }*/
    }
}