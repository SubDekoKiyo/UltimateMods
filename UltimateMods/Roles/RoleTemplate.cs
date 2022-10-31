using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UltimateMods.Modules;
using TMPro;

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
            // TemplateButtonSprite = Helpers.LoadSpriteFromTexture2D(, 115f);
            return TemplateButtonSprite;
        }

        public static void MakeButtons(HudManager hm)
        {
            // Template
            TemplateButton = new CustomButton(
                () =>
                { }, // 押したとき
                () => { return PlayerControl.LocalPlayer.isRole(RoleType.NoRole) && !PlayerControl.LocalPlayer.Data.IsDead; }, //使用可能条件
                () => { return PlayerControl.LocalPlayer.CanMove; }, // 使用時の動作(Text変更等)
                () => { }, // 会議終了時
                Template.GetButtonSprite(), //絵
                new Vector3(-1.8f, -0.06f, 0), //位置
                hm, // HudManager
                hm.KillButton, // 基にするボタン
                KeyCode.F, //キー
                true, // 効果あり(ない場合ここから下記述不要)
                Duration, // 継続時間
                () => { } // // 効果終了時
            );

            TemplateButton.ButtonText = ModTranslation.getString("ButtonText"); // 文字
            TemplateButton.EffectCancellable = true; // 効果を途中で止められるか
            // ボタンの上に表示する小さい文字
            TemplateButtonText = GameObject.Instantiate(TemplateButton.actionButton.cooldownTimerText, TemplateButton.actionButton.cooldownTimerText.transform.parent); // TMP初期化
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