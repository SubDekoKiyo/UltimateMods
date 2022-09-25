using UnityEngine;
using HarmonyLib;
using UltimateMods.Utilities;

namespace UltimateMods.ClassicAmongUs
{
    [HarmonyPatch]
    public static class ClassicAmongUs
    {
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
        class ClassicMeetingStart
        {
            public static void Postfix()
            {
                if (CustomOptionsH.RememberClassic.getBool())
                {
                    FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(PlayerControl.LocalPlayer.Data, PlayerControl.LocalPlayer.Data);

                    // Discussのやつが出てくるまでの時間
                    MeetingDiscuss.Timer = 6f;

                    // Discussのタイマー作動
                    MeetingDiscuss.EnableTimer = true;
                }
            }
        }

        [HarmonyPatch(typeof(MeetingCalledAnimation), nameof(MeetingCalledAnimation.CoShow))]
        class DisableMeetingAnimation
        {
            public static bool Prefix()
            {
                if (CustomOptionsH.RememberClassic.getBool())
                {
                    // 過去を思い出すときだけ無効化
                    // マジでホスト系Mod神だわ(SHR除く)
                    return false;
                }

                return true;
            }
        }
    }
}