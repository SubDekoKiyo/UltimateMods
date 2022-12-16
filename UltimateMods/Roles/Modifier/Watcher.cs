namespace UltimateMods.Roles
{
    [HarmonyPatch]
    public class Watcher : ModifierBase<Watcher>
    {
        public static string Postfix { get { return "WT"; } }

        public static List<PlayerControl> Candidates
        {
            get
            {
                List<PlayerControl> validPlayers = new();

                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (!player.hasModifier(ModifierType.Watcher))
                        validPlayers.Add(player);
                }

                return validPlayers;
            }
        }

        public Watcher()
        {
            ModType = modId = ModifierType.Watcher;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public override void Clear()
        {
            players = new List<Watcher>();
        }
    }
}