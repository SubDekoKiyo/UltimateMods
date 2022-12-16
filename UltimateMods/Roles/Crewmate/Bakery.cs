namespace UltimateMods.Roles
{
    [HarmonyPatch]
    public class Bakery : RoleBase<Bakery>
    {
        public static float BombRate {get { return CustomRolesH.BakeryBombRate.getFloat(); } }

        public Bakery()
        {
            RoleType = roleId = RoleType.Bakery;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public override void Clear()
        {
            players = new List<Bakery>();
        }
    }
}