namespace UltimateMods.Modules
{
    public enum ButtonPositions
    {
        ZoomIn, ZoomOut,
        LeftTop, CenterTop, RightTop,
        LeftBottom, CenterBottom, RightBottom
    }

    public class CustomButton
    {
        public static List<CustomButton> buttons = new();
        public ActionButton actionButton;
        public ButtonPositions ButtonPosition;
        public Vector3 LocalScale = Vector3.one;
        public float MaxTimer = float.MaxValue;
        public float Timer = 0f;
        public bool EffectCancellable = false;
        private Action OnClick;
        private Action OnMeetingEnds;
        private Func<bool> HasButton;
        private Func<bool> CouldUse;
        public Action OnEffectEnds;
        public bool HasEffect;
        public bool IsEffectActive = false;
        public bool ShowButtonText = true;
        public string ButtonText = null;
        public float EffectDuration;
        public Sprite Sprite;
        private HudManager hudManager;
        private bool mirror;
        private KeyCode? hotkey;

        public CustomButton(Action OnClick, Func<bool> HasButton, Func<bool> CouldUse, Action OnMeetingEnds, Sprite Sprite, ButtonPositions ButtonPosition, HudManager hudManager, ActionButton textTemplate, KeyCode? hotkey, bool HasEffect, float EffectDuration, Action OnEffectEnds, bool mirror = false, string ButtonText = null)
        {
            this.hudManager = hudManager;
            this.OnClick = OnClick;
            this.HasButton = HasButton;
            this.CouldUse = CouldUse;
            this.ButtonPosition = ButtonPosition;
            this.OnMeetingEnds = OnMeetingEnds;
            this.HasEffect = HasEffect;
            this.EffectDuration = EffectDuration;
            this.OnEffectEnds = OnEffectEnds;
            this.Sprite = Sprite;
            this.mirror = mirror;
            this.hotkey = hotkey;
            this.ButtonText = ButtonText;
            Timer = 16.2f;
            buttons.Add(this);
            actionButton = UnityEngine.Object.Instantiate(hudManager.KillButton, hudManager.KillButton.transform.parent);
            PassiveButton button = actionButton.GetComponent<PassiveButton>();
            button.OnClick = new Button.ButtonClickedEvent();
            button.OnClick.AddListener((UnityEngine.Events.UnityAction)onClickEvent);

            LocalScale = actionButton.transform.localScale;
            if (textTemplate)
            {
                UnityEngine.Object.Destroy(actionButton.buttonLabelText);
                actionButton.buttonLabelText = UnityEngine.Object.Instantiate(textTemplate.buttonLabelText, actionButton.transform);
            }

            setActive(false);
        }

        public CustomButton(Action OnClick, Func<bool> HasButton, Func<bool> CouldUse, Action OnMeetingEnds, Sprite Sprite, ButtonPositions ButtonPosition, HudManager hudManager, ActionButton textTemplate, KeyCode? hotkey, bool mirror = false, string ButtonText = null)
        : this(OnClick, HasButton, CouldUse, OnMeetingEnds, Sprite, ButtonPosition, hudManager, textTemplate, hotkey, false, 0f, () => { }, mirror, ButtonText) { }

        void onClickEvent()
        {
            if ((this.Timer < 0f && HasButton() && CouldUse()) || (this.HasEffect && this.IsEffectActive && this.EffectCancellable))
            {
                actionButton.graphic.color = new Color(1f, 1f, 1f, 0.3f);
                this.OnClick();

                if (this.HasEffect && !this.IsEffectActive)
                {
                    this.Timer = this.EffectDuration;
                    actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                    this.IsEffectActive = true;
                }
            }
        }

        public static void HudUpdate()
        {
            buttons.RemoveAll(item => item.actionButton == null);

            for (int i = 0; i < buttons.Count; i++)
            {
                try
                {
                    buttons[i].Update();
                }
                catch (NullReferenceException)
                {
                    System.Console.WriteLine("[WARNING] NullReferenceException from HudUpdate().HasButton(), if theres only one warning its fine");
                }
            }
        }

        public static void MeetingEndedUpdate()
        {
            buttons.RemoveAll(item => item.actionButton == null);
            for (int i = 0; i < buttons.Count; i++)
            {
                try
                {
                    buttons[i].OnMeetingEnds();
                    buttons[i].Update();
                }
                catch (NullReferenceException)
                {
                    System.Console.WriteLine("[WARNING] NullReferenceException from MeetingEndedUpdate().HasButton(), if theres only one warning its fine");
                }
            }
        }

        public static void ResetAllCooldowns()
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                try
                {
                    buttons[i].Timer = buttons[i].MaxTimer;
                    buttons[i].Update();
                }
                catch (NullReferenceException)
                {
                    System.Console.WriteLine("[WARNING] NullReferenceException from MeetingEndedUpdate().HasButton(), if theres only one warning its fine");
                }
            }
        }

        public void setActive(bool isActive)
        {
            if (isActive)
            {
                actionButton.gameObject.SetActive(true);
                actionButton.graphic.enabled = true;
            }
            else
            {
                actionButton.gameObject.SetActive(false);
                actionButton.graphic.enabled = false;
            }
        }

        private void Update()
        {
            if (PlayerControl.LocalPlayer.Data == null || MeetingHud.Instance || ExileController.Instance || !HasButton())
            {
                setActive(false);
                return;
            }
            setActive(hudManager.UseButton.isActiveAndEnabled || hudManager.PetButton.isActiveAndEnabled);

            actionButton.graphic.sprite = Sprite;
            if (ShowButtonText && ButtonText != null)
            {
                actionButton.OverrideText(ButtonText);
            }
            actionButton.buttonLabelText.enabled = ShowButtonText; // Only show the text if it's a kill button

            if (hudManager.UseButton != null)
            {
                Vector3 PositionOffset = new(0f, 0f, 0f);
                Vector3 pos = hudManager.UseButton.transform.localPosition;
                if (mirror) pos = new(-pos.x, pos.y, pos.z);

                switch (ButtonPosition)
                {
                    case ButtonPositions.ZoomIn:
                        PositionOffset = Vector3.zero + Vector3.up * 3.75f + Vector3.right * 0.2f;
                        break;
                    case ButtonPositions.ZoomOut:
                        PositionOffset = Vector3.zero + Vector3.up * 3.75f + Vector3.right * 0.55f;
                        break;
                    case ButtonPositions.LeftTop: // Kill Button
                        PositionOffset = new(-1.8f, 1f, 0f);
                        break;
                    case ButtonPositions.CenterTop: // Sabotage Button
                        PositionOffset = new(-0.9f, 1f, 0f);
                        break;
                    case ButtonPositions.RightTop: // ShapeShift Button
                        PositionOffset = new(0f, 1f, 0f);
                        break;
                    case ButtonPositions.LeftBottom: // Vent Button
                        PositionOffset = new(-1.8f, -0.06f, 0f);
                        break;
                    case ButtonPositions.CenterBottom: // Report Button
                        PositionOffset = new(-0.9f, -0.06f, 0f);
                        break;
                    case ButtonPositions.RightBottom: // Use/Pet Button
                        PositionOffset = new(0f, -0.06f, 0f);
                        break;
                }

                actionButton.transform.localPosition = pos + PositionOffset;
                actionButton.transform.localScale = LocalScale;
            }
            if (CouldUse())
            {
                actionButton.graphic.color = actionButton.buttonLabelText.color = Palette.EnabledColor;
                actionButton.graphic.material.SetFloat("_Desat", 0f);
            }
            else
            {
                actionButton.graphic.color = actionButton.buttonLabelText.color = Palette.DisabledClear;
                actionButton.graphic.material.SetFloat("_Desat", 1f);
            }

            if (Timer >= 0)
            {
                if (HasEffect && IsEffectActive)
                    Timer -= Time.deltaTime;
                else if (!PlayerControl.LocalPlayer.inVent && PlayerControl.LocalPlayer.moveable)
                    Timer -= Time.deltaTime;
            }

            if (Timer <= 0 && HasEffect && IsEffectActive)
            {
                IsEffectActive = false;
                actionButton.cooldownTimerText.color = Palette.EnabledColor;
                OnEffectEnds();
            }

            actionButton.SetCoolDown(Timer, (HasEffect && IsEffectActive) ? EffectDuration : MaxTimer);

            // Trigger OnClickEvent if the hotkey is being pressed down
            if (hotkey.HasValue && Input.GetKeyDown(hotkey.Value)) onClickEvent();
        }

        public void Destroy()
        {
            setActive(false);
            UnityEngine.Object.Destroy(actionButton);
            actionButton = null;
        }
    }
}