namespace UltimateMods.Roles
{
    [HarmonyPatch]
    public class ModifierT : ModifierBase<ModifierT>
    {
        public override string ModifierPostfix() { return ""; }

        public static List<PlayerControl> Candidates
        {
            get
            {
                List<PlayerControl> validPlayers = new();

                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (!player.hasModifier(ModifierType.NoModifier))
                        validPlayers.Add(player);
                }

                return validPlayers;
            }
        }

        public ModifierT()
        {
            ModType = modId = ModifierType.NoModifier;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void Clear()
        {
            players = new List<ModifierT>();
        }
    }
}