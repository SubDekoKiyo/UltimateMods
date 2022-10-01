// 今後復活予定

/*
using UnityEngine;
using HarmonyLib;
using UltimateMods.Utilities;
using static UltimateMods.ClassicAmongUs.ClassicMeetingStart;

namespace UltimateMods
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class ClassicAnimationPatch
    {
        public static bool WasCreated = false;
        public static bool IsTimerStart = false;
        public static int num;
        public static float Timer;
        public static void Prefix()
        {
            switch (AnimNum)
            {
                case 1:
                    if (Timer <= 0f && ClassicAnimation != null)
                    {
                        SetOne();
                    }
                    Timer = Mathf.Max(0f, Timer -= Time.deltaTime);
                    break;
                case 2:
                    if (Timer <= 0f && ClassicAnimation != null)
                    {
                        SetTwo();
                    }
                    Timer = Mathf.Max(0f, Timer -= Time.deltaTime);
                    break;
                case 3:
                    if (Timer <= 0f && ClassicAnimation != null)
                    {
                        SetThree();
                    }
                    Timer = Mathf.Max(0f, Timer -= Time.deltaTime);
                    break;
                case 4:
                    if (Timer <= 0f)
                    {
                        if (ClassicAnimation.transform.localScale.y > 0f && ClassicAnimation != null)
                        {
                            if (PushButton != null)
                            {
                                renderer2.sprite = null;
                            }
                            ClassicAnimation.transform.localScale -= new Vector3(0f, 0.25f, 0f);
                        }
                    }
                    Timer = Mathf.Max(0f, Timer -= Time.deltaTime);
                    break;
            }
        }
    }
}*/