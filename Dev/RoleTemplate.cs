namespace UltimateMods.Roles
{
    [HarmonyPatch]
    public class Template : RoleBase<Template>
    {
        private static CustomButton TemplateButton;
        public static TMP_Text TemplateButtonText;
        public static Sprite TemplateButtonSprite;

        public static float Duration = 5f;

        public Template()
        {
            RoleType = roleId = RoleType.NoRole;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static Sprite GetButtonSprite()
        {
            if (TemplateButtonSprite) return TemplateButtonSprite;
            TemplateButtonSprite = Helpers.LoadSpriteFromTexture2D(TemplateButtonSprite, 115f);
            return TemplateButtonSprite;
        }

        public static void MakeButtons(HudManager hm)
        {
            TemplateButton = new CustomButton(
                () =>
                { },
                () => { return PlayerControl.LocalPlayer.isRole(RoleType.NoRole) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () => { },
                GetButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                hm,
                hm.KillButton,
                KeyCode.F,
                true,
                Duration,
                () => { }
            );

            TemplateButton.ButtonText = ModTranslation.getString("ButtonText");
            TemplateButton.EffectCancellable = true;
            TemplateButtonText = GameObject.Instantiate(TemplateButton.actionButton.cooldownTimerText, TemplateButton.actionButton.cooldownTimerText.transform.parent);
            TemplateButtonText.text = "";
            TemplateButtonText.enableWordWrapping = false;
            TemplateButtonText.transform.localScale = Vector3.one * 0.5f;
            TemplateButtonText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
        }

        public static void SetButtonCooldowns() { }

        public static void Clear()
        {
            players = new List<Template>();
        }
    }
}