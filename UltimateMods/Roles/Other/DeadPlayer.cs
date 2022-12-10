using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UltimateMods.Modules;
using TMPro;
using UltimateMods.Utilities;
using AmongUs.GameOptions;
using static UltimateMods.Modules.Assets;

namespace UltimateMods.Roles
{
    [HarmonyPatch]
    public class DeadPlayer : RoleBase<DeadPlayer>
    {
        private static CustomButton ZoomIn;
        private static CustomButton ZoomOut;
        private static bool EnableZoomInOut { get { return CustomOptionsH.CanZoomInOutWhenPlayerIsDead.getBool(); } }
        public static Sprite ZoomInIcon;
        public static Sprite ZoomOutIcon;

        public static void ResetZoom()
        {
            Camera.main.orthographicSize = 3.0f;
            FastDestroyableSingleton<HudManager>.Instance.UICamera.orthographicSize = 3.0f;
            FastDestroyableSingleton<HudManager>.Instance.transform.localScale = Vector3.one;
            FastDestroyableSingleton<HudManager>.Instance.ShadowQuad.gameObject.SetActive(false);
        }

        public DeadPlayer()
        {
            RoleType = roleId = RoleType.NoRole;
        }

        public override void OnMeetingStart()
        {
            ResetZoom();
        }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate()
        {
            if (FastDestroyableSingleton<HudManager>.Instance.ShadowQuad.gameObject.active)
            {
                FastDestroyableSingleton<HudManager>.Instance.ShadowQuad.gameObject.SetActive(false);
            }
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void MakeButtons(HudManager hm)
        {
            ZoomIn = new CustomButton(
                () =>
                {
                    if (Camera.main.orthographicSize > 3.0f)
                    {
                        Camera.main.orthographicSize /= 1.5f;
                        hm.UICamera.orthographicSize /= 1.5f;
                    }

                    if (hm.transform.localScale.x > 1.0f)
                    {
                        hm.transform.localScale /= 1.5f;
                    }
                },
                () => { return EnableZoomInOut && PlayerControl.LocalPlayer.IsDead(); },
                () => { return PlayerControl.LocalPlayer.IsDead(); },
                () => { },
                GetZoomInSprite(),
                Vector3.zero + Vector3.up * 3.75f + Vector3.right * 0.2f,
                hm,
                hm.UseButton,
                KeyCode.PageUp,
                false
            );
            ZoomIn.Timer = 0.0f;
            ZoomIn.MaxTimer = 0.0f;
            ZoomIn.ShowButtonText = false;
            ZoomIn.LocalScale = Vector3.one * 0.5f;

            ZoomOut = new CustomButton(
                () =>
                {
                    if (Camera.main.orthographicSize < 18.0f)
                    {
                        Camera.main.orthographicSize *= 1.5f;
                        hm.UICamera.orthographicSize *= 1.5f;
                    }

                    if (hm.transform.localScale.x < 6.0f)
                    {
                        hm.transform.localScale *= 1.5f;
                    }
                },
                () => { return EnableZoomInOut && PlayerControl.LocalPlayer.IsDead(); },
                () => { return PlayerControl.LocalPlayer.IsDead(); },
                () => { },
                GetZoomOutSprite(),
                Vector3.zero + Vector3.up * 3.75f + Vector3.right * 0.55f,
                hm,
                hm.UseButton,
                KeyCode.PageDown,
                false
            );
            ZoomOut.Timer = 0.0f;
            ZoomOut.MaxTimer = 0.0f;
            ZoomOut.ShowButtonText = false;
            ZoomOut.LocalScale = Vector3.one * 0.5f;
        }

        public static Sprite GetZoomInSprite()
        {
            if (ZoomInIcon) return ZoomInIcon;
            ZoomInIcon = Helpers.LoadSpriteFromTexture2D(ZoomInButton, 115f);
            return ZoomInIcon;
        }

        public static Sprite GetZoomOutSprite()
        {
            if (ZoomOutIcon) return ZoomOutIcon;
            ZoomOutIcon = Helpers.LoadSpriteFromTexture2D(ZoomOutButton, 115f);
            return ZoomOutIcon;
        }

        public static void Clear()
        {
            players = new List<DeadPlayer>();
        }

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        public static class Zoom
        {
            public static bool flag = true;
            public static void Postfix(HudManager __instance)
            {
                if (EnableZoomInOut && PlayerControl.LocalPlayer.IsDead())
                {
                    if ((AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started
                        // || AmongUsClient.Instance.GameMode == GameModes.FreePlay)
                        && (PlayerControl.LocalPlayer.CanMove)
                        && !(MapBehaviour.Instance && MapBehaviour.Instance.IsOpen)
                        && !(MeetingHud.Instance)))
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