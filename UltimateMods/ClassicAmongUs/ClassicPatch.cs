using UnityEngine;
using HarmonyLib;
using UltimateMods.Utilities;
using System;
using System.Threading.Tasks;
using static UltimateMods.ClassicAnimationPatch;

namespace UltimateMods
{
    [HarmonyPatch]
    public static class ClassicAmongUs
    {
        public static float DeleteTimer;

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
        public static class ClassicMeetingStart
        {
            public static GameObject ClassicAnimation;
            public static GameObject PushButton;
            public static SpriteRenderer renderer;
            public static SpriteRenderer renderer2;
            public static float AnimTimer;
            public static int AnimNum = 0;
            public static void Prefix()
            {
                if (CustomOptionsH.RememberClassic.getBool())
                {
                    GameObject Text = GameObject.Find("Main Camera/Hud/KillOverlay/EmergencyAnimation(Clone)/Text (TMP)");
                    GameObject TextBg = GameObject.Find("Main Camera/Hud/KillOverlay/EmergencyAnimation(Clone)/TextBg");
                    GameObject SpeedLines = GameObject.Find("Main Camera/Hud/KillOverlay/EmergencyAnimation(Clone)/SpeedLines");
                    GameObject YellowTape = GameObject.Find("Main Camera/Hud/KillOverlay/EmergencyAnimation(Clone)/yellowtape");
                    GameObject Player = GameObject.Find("Main Camera/Hud/KillOverlay/EmergencyAnimation(Clone)/killstabplayerstill");
                    Text.gameObject.SetActive(false);
                    TextBg.gameObject.SetActive(false);
                    SpeedLines.gameObject.SetActive(false);
                    YellowTape.gameObject.SetActive(false);
                    Player.gameObject.SetActive(false);

                    // Discussのやつが出てくるまでの時間
                    MeetingDiscuss.Timer = 6f;

                    // Discussのタイマー作動
                    MeetingDiscuss.EnableTimer = true;

                    ClassicAnimation = new("ClassicAnimation");
                    ClassicAnimation.transform.SetParent(FastDestroyableSingleton<HudManager>.Instance.transform);
                    ClassicAnimation.transform.localPosition = new Vector3(0f, 0f, 0f);
                    ClassicAnimation.transform.localScale = new Vector3(1.3f, 1.25f, 1f);
                    ClassicAnimation.SetActive(true);

                    PushButton = new("PushButton");
                    PushButton.transform.SetParent(ClassicAnimation.transform);
                    PushButton.transform.localPosition = new Vector3(0f, 0.4f, -0.1f);
                    PushButton.transform.localScale = new Vector3(0.35f, 0.35f, 1f);
                    PushButton.SetActive(true);

                    renderer = ClassicAnimation.AddComponent<SpriteRenderer>();
                    renderer.sprite = Helpers.LoadSpriteFromResources($"UltimateMods.Resources.ClassicAmongUs.EmergencyMeeting.Animation0.png", 115f);
                    AnimNum = 0;
                }
            }

            public static void SetOne()
            {
                if (renderer != null)
                {
                    renderer.sprite = Helpers.LoadSpriteFromResources($"UltimateMods.Resources.ClassicAmongUs.EmergencyMeeting.Animation1.png", 115f);
                    Timer = 0.1f;
                    AnimNum++;
                }
            }

            public static void SetTwo()
            {
                if (renderer != null)
                {
                    renderer.sprite = Helpers.LoadSpriteFromResources($"UltimateMods.Resources.ClassicAmongUs.EmergencyMeeting.Animation2.png", 115f);
                    Timer = 0.1f;
                    AnimNum++;
                }
            }

            public static void SetThree()
            {
                if (renderer != null)
                {
                    renderer.sprite = Helpers.LoadSpriteFromResources($"UltimateMods.Resources.ClassicAmongUs.EmergencyMeeting.Animation3.png", 115f);

                    renderer2 = PushButton.AddComponent<SpriteRenderer>();
                    renderer2.sprite = Helpers.LoadSpriteFromResources($"UltimateMods.Resources.ClassicAmongUs.EmergencyMeeting.PushButton{ModTranslation.lang}.png", 115f);
                    Timer = 2.2f;
                    AnimNum++;
                }
            }
        }

        public static void DestroyObject()
        {
            if (ClassicMeetingStart.ClassicAnimation != null)
            {
                UnityEngine.Object.Destroy(ClassicMeetingStart.ClassicAnimation);
                ClassicMeetingStart.AnimNum = 0;
            }
        }

        // [HarmonyPatch(typeof(MeetingCalledAnimation), nameof(MeetingCalledAnimation.CoShow))]
        // class DisableMeetingAnimation
        // {
        //     public static bool Prefix()
        //     {
        //         if (CustomOptionsH.RememberClassic.getBool())
        //         {
        //             // 過去を思い出すときだけ無効化
        //             return false;
        //         }

        //         return true;
        //     }
        // }
    }
}