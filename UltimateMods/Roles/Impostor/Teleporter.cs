namespace UltimateMods.Roles
{
    [HarmonyPatch]
    public class Teleporter : RoleBase<Teleporter>
    {
        public enum TeleportTarget
        {
            AliveAllPlayer = 0,
            Crewmate = 1,
        }

        private static CustomButton TeleportButton;
        public static Sprite TeleportButtonSprite;

        public static float Cooldown { get { return CustomRolesH.TeleporterButtonCooldown.getFloat(); } }
        public static TeleportTarget TeleportTo { get { return (TeleportTarget)CustomRolesH.TeleporterTeleportTo.getSelection(); } }

        public Teleporter()
        {
            RoleType = roleId = RoleType.Teleporter;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static Sprite GetButtonSprite()
        {
            if (TeleportButtonSprite) return TeleportButtonSprite;
            TeleportButtonSprite = Helpers.LoadSpriteFromTexture2D(TeleporterTeleportButton, 115f);
            return TeleportButtonSprite;
        }

        public static void MakeButtons(HudManager hm)
        {
            // Template
            TeleportButton = new CustomButton(
                () =>
                {
                    List<PlayerControl> Target = new();
                    foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
                    {
                        switch (TeleportTo)
                        {
                            case TeleportTarget.AliveAllPlayer:
                                if (pc.IsAlive() && pc.CanMove)
                                {
                                    Target.Add(pc);
                                }
                                break;
                            case TeleportTarget.Crewmate:
                                if (pc.IsAlive() && pc.CanMove && pc.IsCrew())
                                {
                                    Target.Add(pc);
                                }
                                break;
                        }
                    }

                    var player = Helpers.GetRandom(Target);
                    MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.TeleporterTeleport, SendOption.Reliable, -1);
                    Writer.Write(player.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(Writer);
                    RPCProcedure.TeleporterTeleport(player.PlayerId);

                    TeleportButton.Timer = Cooldown;
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleType.Teleporter) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () => { TeleportButton.Timer = TeleportButton.MaxTimer = Cooldown; },
                GetButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                hm,
                hm.KillButton,
                KeyCode.F,
                false
            );
            TeleportButton.ButtonText = ModTranslation.getString("TeleportButtonText");
        }

        public static void SetButtonCooldowns()
        {
            TeleportButton.MaxTimer = Cooldown;
        }

        public static void Clear()
        {
            players = new List<Teleporter>();
        }
    }
}