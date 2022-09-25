using HarmonyLib;
using UnityEngine;

namespace UltimateMods.ClassicAmongUs
{
    [HarmonyPatch]
    public static class MeetingDiscuss
    {
        // Discussのやつの場所
        // これをModで有効化して表示している
        static GameObject Discuss;
        public static float Timer;
        public static bool DisableTimer = false;
        public static bool EnableTimer = false;

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
        public static void Postfix()
        {
            if (!CustomOptionsH.RememberClassic.getBool()) return;

            // Discussのタイマー軌道・終了
            GameObject Discuss = GameObject.Find("Main Camera/Hud/Emblems/DiscussEmblem");

            if (Discuss != null && EnableTimer)
            {
                Timer = Mathf.Max(0f, Timer -= Time.deltaTime);
                if (Timer <= 0f)
                {
                    Discuss.SetActive(true);
                    EnableTimer = false;
                    Timer = 3f;
                    DisableTimer = true;
                }
            }

            if (Discuss != null && DisableTimer)
            {
                Timer = Mathf.Max(0f, Timer -= Time.deltaTime);
                if (Timer <= 0f)
                {
                    Discuss.SetActive(false);
                    DisableTimer = false;
                }
            }
        }

        public static void OnMeetingEnd()
        {
            Timer = 3f;
            EnableTimer = false;
            DisableTimer = false;
            if (Discuss != null && Discuss.active)
            {
                Discuss.SetActive(false);
            }
        }
    }
}