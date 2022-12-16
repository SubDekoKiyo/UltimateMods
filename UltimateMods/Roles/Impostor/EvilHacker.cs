namespace UltimateMods.Roles
{
    [HarmonyPatch]
    public class EvilHacker : RoleBase<EvilHacker>
    {
        private static CustomButton AdminButton;
        public static Sprite AdminButtonSprite;

        public static bool canHasBetterAdmin { get { return CustomRolesH.EvilHackerCanHasBetterAdmin.getBool(); } }

        public EvilHacker()
        {
            RoleType = roleId = RoleType.EvilHacker;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static Sprite GetButtonSprite()
        {
            byte mapId = GameOptionsManager.Instance.CurrentGameOptions.GetByte(ByteOptionNames.MapId);
            UseButtonSettings button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.PolusAdminButton]; // Polus
            if (mapId == 0 || mapId == 3) button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.AdminMapButton]; // Skeld
            else if (mapId == 1) button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.MIRAAdminButton]; // Mira HQ
            else if (mapId == 4) button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.AirshipAdminButton]; // Airship
            AdminButtonSprite = button.Image;
            return AdminButtonSprite;
        }

        public static void MakeButtons(HudManager hm)
        {
            AdminButton = new CustomButton(
                () =>
                {
                    PlayerControl.LocalPlayer.NetTransform.Halt();
                    HudManager.Instance.ToggleMapVisible(new MapOptions()
                    {
                        Mode = MapOptions.Modes.CountOverlay,
                        AllowMovementWhileMapOpen = true,
                        IncludeDeadBodies = true
                    });
                },
                () =>
                {
                    return PlayerControl.LocalPlayer.isRole(RoleType.EvilHacker) &&
                    PlayerControl.LocalPlayer.IsAlive();
                },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () => { },
                GetButtonSprite(),
                ButtonPositions.LeftTop,
                hm,
                hm.KillButton,
                KeyCode.F,
                false,
                0f,
                () => { },
                GameOptionsManager.Instance.CurrentGameOptions.GetByte(ByteOptionNames.MapId) == 3,
                DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Admin)
            );
        }

        public static void SetButtonCooldowns() { }

        public override void Clear()
        {
            players = new List<EvilHacker>();
        }
    }
}