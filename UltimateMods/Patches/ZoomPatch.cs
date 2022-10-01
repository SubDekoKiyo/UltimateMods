using HarmonyLib;
using UnityEngine;
using UltimateMods.Utilities;

namespace UltimateMods.Patches
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class Zoom
    {
        public static bool flag = true;
        public static void Postfix(HudManager __instance)
        {
            if (CustomOptionsH.EnableDiePlayerZoomInOut.getBool() && PlayerControl.LocalPlayer.IsDead())
            {
                if ((AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started || AmongUsClient.Instance.GameMode == GameModes.FreePlay)
                && (PlayerControl.LocalPlayer.CanMove)
                && !(MapBehaviour.Instance && MapBehaviour.Instance.IsOpen)
                && !(MeetingHud.Instance))
                {
                    if (Input.GetAxis("Mouse ScrollWheel") < 0)
                    {
                        if (Camera.main.orthographicSize < 18.0f)
                        {
                            Camera.main.orthographicSize *= 1.5f;
                            __instance.transform.localScale *= 1.5f;
                            __instance.UICamera.orthographicSize *= 1.5f;
                        }
                    }
                    if (Input.GetAxis("Mouse ScrollWheel") > 0)
                    {
                        if (Camera.main.orthographicSize > 3.0f)
                        {
                            Camera.main.orthographicSize /= 1.5f;
                            __instance.transform.localScale /= 1.5f;
                            __instance.UICamera.orthographicSize /= 1.5f;
                        }
                    }
                }
                flag = false;
            }
            else
            {
                if (flag == false)
                {
                    Reset.Zoom();
                    flag = true;
                }
            }
        }

        public static class Reset
        {
            public static void Zoom()
            {
                Camera.main.orthographicSize = 3.0f;
                FastDestroyableSingleton<HudManager>.Instance.UICamera.orthographicSize = 3.0f;
                FastDestroyableSingleton<HudManager>.Instance.transform.localScale = Vector3.one;
                if (MeetingHud.Instance != null) MeetingHud.Instance.transform.localScale = Vector3.one;
                FastDestroyableSingleton<HudManager>.Instance.Chat.transform.localScale = Vector3.one;
            }
        }
    }
}