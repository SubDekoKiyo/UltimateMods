namespace UltimateMods.Roles
{
    [HarmonyPatch]
    public class Sunglasses : ModifierBase<Sunglasses>
    {
        public static string Postfix { get { return "SG"; } }
        public static int Vision { get { return Mathf.RoundToInt(CustomRolesH.Sunglass.getFloat()); } }

        public static List<PlayerControl> Candidates
        {
            get
            {
                List<PlayerControl> validPlayers = new();

                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (!player.hasModifier(ModifierType.Sunglasses))
                        validPlayers.Add(player);
                }

                return validPlayers;
            }
        }

        public Sunglasses()
        {
            ModType = modId = ModifierType.Sunglasses;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public override void Clear()
        {
            players = new List<Sunglasses>();
        }
    }
}