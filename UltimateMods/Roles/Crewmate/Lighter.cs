namespace UltimateMods.Roles
{
    [HarmonyPatch]
    public class Lighter : RoleBase<Lighter>
    {
        private static CustomButton LighterButton;
        public static Sprite LighterButtonSprite;

        public static float LighterModeLightsOnVision { get { return CustomRolesH.LighterModeLightsOnVision.getFloat(); } }
        public static float LighterModeLightsOffVision { get { return CustomRolesH.LighterModeLightsOffVision.getFloat(); } }
        public static float Cooldown { get { return CustomRolesH.LighterCooldown.getFloat(); } }
        public static float Duration { get { return CustomRolesH.LighterDuration.getFloat(); } }
        public static bool LightActive = false;

        public Lighter()
        {
            RoleType = roleId = RoleType.Lighter;
        }

        public static bool IsLightActive(PlayerControl player)
        {
            if (player.isRole(RoleType.Lighter) && player.IsAlive())
            {
                return LightActive;
            }
            return false;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static Sprite GetButtonSprite()
        {
            if (LighterButtonSprite) return LighterButtonSprite;
            LighterButtonSprite = Helpers.LoadSpriteFromTexture2D(LighterLight, 115f);
            return LighterButtonSprite;
        }

        public static void MakeButtons(HudManager hm)
        {
            LighterButton = new CustomButton(
                () =>
                {
                    LightActive = true;
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleType.Lighter) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () =>
                {
                    LightActive = false;

                    LighterButton.Timer = LighterButton.MaxTimer;
                    LighterButton.IsEffectActive = false;
                    LighterButton.actionButton.graphic.color = Palette.EnabledColor;
                },
                GetButtonSprite(),
                ButtonPositions.RightTop,
                hm,
                hm.KillButton,
                KeyCode.F,
                true,
                Duration,
                () =>
                {
                    LightActive = false;
                    LighterButton.Timer = LighterButton.MaxTimer;
                }
            );

            LighterButton.ButtonText = ModTranslation.getString("LighterText");
            LighterButton.EffectCancellable = true;
        }

        public static void SetButtonCooldowns()
        {
            LighterButton.MaxTimer = Cooldown;
            LighterButton.EffectDuration = Duration;
        }

        public static void Clear()
        {
            LightActive = false;
            players = new List<Lighter>();
        }
    }
}