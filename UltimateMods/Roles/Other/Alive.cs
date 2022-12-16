namespace UltimateMods.Roles
{
    [HarmonyPatch]
    public class AlivePlayer : RoleBase<AlivePlayer>
    {
        public static bool IsForceEnd = false;
        public AlivePlayer()
        {
            RoleType = roleId = RoleType.NoRole;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public override void Clear()
        {
            IsForceEnd = false;
            players = new List<AlivePlayer>();
        }
    }
}