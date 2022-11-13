using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UltimateMods.Modules;
using Hazel;
using static UltimateMods.ColorDictionary;
using static UltimateMods.Modules.Assets;
using static UltimateMods.Roles.Patches.OutlinePatch;

namespace UltimateMods.Roles
{
    [HarmonyPatch]
    public class Altruist : RoleBase<Altruist>
    {
        private static CustomButton AltruistButton;
        public static Sprite AltruistButtonSprite;
        public static byte BodyId = 0;
        public static bool Ended = false;
        public static DeadBody Target;

        public static float Duration { get { return CustomRolesH.AltruistDuration.getFloat(); } }

        public Altruist()
        {
            RoleType = roleId = RoleType.Altruist;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static Sprite GetButtonSprite()
        {
            if (AltruistButtonSprite) return AltruistButtonSprite;
            AltruistButtonSprite = Helpers.LoadSpriteFromTexture2D(AltruistReviveButton, 115f);
            return AltruistButtonSprite;
        }

        public static void MakeButtons(HudManager hm)
        {
            AltruistButton = new CustomButton(
                () =>
                {
                    MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.AltruistKill, Hazel.SendOption.Reliable, -1);
                    killWriter.Write(PlayerControl.LocalPlayer.Data.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                    RPCProcedure.AltruistKill(PlayerControl.LocalPlayer.Data.PlayerId);
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleType.Altruist) && !Ended; },
                () =>
                {
                    bool CanRevive = false;
                    foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(), 1f, Constants.PlayersOnlyMask))
                        if (collider2D.tag == "DeadBody")
                            CanRevive = true;
                    return CanRevive && PlayerControl.LocalPlayer.CanMove;
                },
                () => { },
                GetButtonSprite(),
                new Vector3(0f, 1f, 0),
                hm,
                hm.KillButton,
                KeyCode.F,
                true,
                Duration,
                () =>
                {
                    MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.AltruistRevive, Hazel.SendOption.Reliable, -1);
                    killWriter.Write(Target);
                    killWriter.Write(Altruist.BodyId);
                    AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                    RPCProcedure.AltruistRevive(Target, Altruist.BodyId);
                    Ended = true;
                }
            );
            AltruistButton.ButtonText = ModTranslation.getString("AltruistReviveText");
            AltruistButton.EffectCancellable = false;
        }

        public static void SetButtonCooldowns()
        {
            AltruistButton.Timer = AltruistButton.MaxTimer = 0f;
        }

        public static void Clear()
        {
            BodyId = 0;
            Ended = false;
            players = new List<Altruist>();
        }
    }
}