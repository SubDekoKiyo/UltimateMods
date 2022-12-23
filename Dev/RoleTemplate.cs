namespace UltimateMods.Roles
{
    [HarmonyPatch]
    public class Template : RoleBase<Template>
    {
        public Template()
        {
            RoleId = roleId = RoleId.NoRole;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void Clear()
        {
            players = new List<Template>();
        }
    }
}