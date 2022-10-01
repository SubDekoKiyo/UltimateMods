using System.Linq;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using UltimateMods.Modules;
using Hazel;
using static UltimateMods.Patches.PlayerControlFixedUpdatePatch;

namespace UltimateMods.Roles
{
    [HarmonyPatch]
    public class Sheriff : RoleBase<Sheriff>
    {
        public PlayerControl currentTarget;
        private static CustomButton SheriffKillButton;
        public static TMPro.TMP_Text SheriffNumShotsText;
        public int NumShots = 2;
        public static Color color = new Color32(248, 205, 70, byte.MaxValue);

        public static float Cooldown { get { return CustomRolesH.SheriffCooldowns.getFloat(); } }
        public static int MaxShots { get { return Mathf.RoundToInt(CustomRolesH.SheriffMaxShots.getFloat()); } }
        public static bool CanKillNeutrals { get { return CustomRolesH.SheriffCanKillNeutral.getBool(); } }
        public static bool MisfireKillsTarget { get { return CustomRolesH.SheriffMisfireKillsTarget.getBool(); } }

        public Sheriff()
        {
            RoleType = roleId = RoleType.Sheriff;
            NumShots = MaxShots;
        }

        public static void MakeButtons(HudManager hm)
        {
            // Sheriff Kill
            SheriffKillButton = new CustomButton(
                () =>
                {
                    if (local.NumShots <= 0)
                    {
                        return;
                    }

                    MurderAttemptResult murderAttemptResult = Helpers.CheckMurderAttempt(PlayerControl.LocalPlayer, local.currentTarget);
                    if (murderAttemptResult == MurderAttemptResult.SuppressKill) return;

                    if (murderAttemptResult == MurderAttemptResult.PerformKill)
                    {
                        bool misfire = false;
                        byte targetId = local.currentTarget.PlayerId; ;
                        if ((local.currentTarget.Data.Role.IsImpostor) ||
                            (CanKillNeutrals && local.currentTarget.IsNeutral()))
                        {
                            targetId = local.currentTarget.PlayerId;
                            misfire = false;
                        }
                        else
                        {
                            targetId = PlayerControl.LocalPlayer.PlayerId;
                            misfire = true;
                        }

                        MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SheriffKill, Hazel.SendOption.Reliable, -1);
                        killWriter.Write(PlayerControl.LocalPlayer.Data.PlayerId);
                        killWriter.Write(targetId);
                        killWriter.Write(misfire);
                        AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                        RPCProcedure.SheriffKill(PlayerControl.LocalPlayer.Data.PlayerId, targetId, misfire);
                    }

                    SheriffKillButton.Timer = SheriffKillButton.MaxTimer;
                    local.currentTarget = null;
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleType.Sheriff) && local.NumShots > 0 && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (SheriffNumShotsText != null)
                    {
                        if (local.NumShots > 0)
                            SheriffNumShotsText.text = String.Format(ModTranslation.getString("Shots"), local.NumShots);
                        else
                            SheriffNumShotsText.text = "";
                    }
                    return local.currentTarget && PlayerControl.LocalPlayer.CanMove;
                },
                () => { SheriffKillButton.Timer = SheriffKillButton.MaxTimer; },
                hm.KillButton.graphic.sprite,
                new Vector3(0f, 1f, 0),
                hm,
                hm.KillButton,
                KeyCode.Q
            );

            SheriffNumShotsText = GameObject.Instantiate(SheriffKillButton.actionButton.cooldownTimerText, SheriffKillButton.actionButton.cooldownTimerText.transform.parent);
            SheriffNumShotsText.text = "";
            SheriffNumShotsText.enableWordWrapping = false;
            SheriffNumShotsText.transform.localScale = Vector3.one * 0.5f;
            SheriffNumShotsText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
        }

        public static void SetButtonCooldowns()
        {
            SheriffKillButton.MaxTimer = Cooldown;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate()
        {
            if (player == PlayerControl.LocalPlayer && NumShots > 0)
            {
                currentTarget = SetTarget();
                SetPlayerOutline(currentTarget, Sheriff.color);
            }
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void Clear()
        {
            players = new List<Sheriff>();
        }
    }
}