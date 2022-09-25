using System.Linq;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using UltimateMods.Modules;
using Hazel;

namespace UltimateMods.Roles.Yakuza
{
    [HarmonyPatch]
    public static class YakuzaStaff
    {
        private static CustomButton StaffKillButton;
        public static TMPro.TMP_Text StaffNumShotsText;
        public static PlayerControl staff;
        public static PlayerControl currentTarget;
        public static bool CanKill = false;

        public static int MaxShots = 2;
        public static float Cooldowns = 30f;
        public static bool CanKillNeutral = true;
        public static bool MisfireKillsTarget = false;
        public static bool ShareShotsCounts = true;

        public static void ClearAndReload()
        {
            staff = null;
            currentTarget = null;
            CanKill = false;
            MaxShots = (int)CustomRolesH.YakuzaMaxShots.getFloat();
            Cooldowns = CustomRolesH.YakuzaCooldowns.getFloat();
            CanKillNeutral = CustomRolesH.YakuzaCanKillNeutral.getBool();
            MisfireKillsTarget = CustomRolesH.YakuzaMisfireKillsTarget.getBool();
            ShareShotsCounts = CustomRolesH.YakuzaShareShotsCount.getBool();
        }

        public static void MakeButtons(HudManager hm)
        {
            // Staff Kill
            StaffKillButton = new CustomButton(
                () =>
                {
                    if (MaxShots <= 0 || YakuzaGun.ShareShots <= 0)
                    {
                        return;
                    }

                    MurderAttemptResult murderAttemptResult = Helpers.CheckMurderAttempt(PlayerControl.LocalPlayer, currentTarget);
                    if (murderAttemptResult == MurderAttemptResult.SuppressKill) return;

                    if (murderAttemptResult == MurderAttemptResult.PerformKill)
                    {
                        bool misfire = false;
                        byte targetId = currentTarget.PlayerId; ;
                        if ((currentTarget.Data.Role.IsImpostor) ||
                            (CanKillNeutral && currentTarget.IsNeutral()))
                        {
                            targetId = currentTarget.PlayerId;
                            misfire = false;
                        }
                        else
                        {
                            targetId = PlayerControl.LocalPlayer.PlayerId;
                            misfire = true;
                        }

                        MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.StaffKill, Hazel.SendOption.Reliable, -1);
                        killWriter.Write(PlayerControl.LocalPlayer.Data.PlayerId);
                        killWriter.Write(targetId);
                        killWriter.Write(misfire);
                        AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                        RPCProcedure.StaffKill(PlayerControl.LocalPlayer.Data.PlayerId, targetId, misfire);
                    }

                    StaffKillButton.Timer = StaffKillButton.MaxTimer;
                    currentTarget = null;
                },
                () =>
                {
                    if (ShareShotsCounts == false)
                        return PlayerControl.LocalPlayer.isRole(RoleType.YakuzaStaff) && MaxShots > 0 && !PlayerControl.LocalPlayer.Data.IsDead && CanKill;
                    else if (ShareShotsCounts == true)
                        return PlayerControl.LocalPlayer.isRole(RoleType.YakuzaStaff) && YakuzaGun.ShareShots > 0 && !PlayerControl.LocalPlayer.Data.IsDead && CanKill;
                    return true;
                },
                () =>
                {
                    if (StaffNumShotsText != null)
                    {
                        if (ShareShotsCounts == false)
                        {
                            if (MaxShots > 0)
                                StaffNumShotsText.text = String.Format(ModTranslation.getString("Shots"), MaxShots);
                            else
                                StaffNumShotsText.text = "";
                        }
                        else if (ShareShotsCounts == true)
                        {
                            if (YakuzaGun.ShareShots > 0)
                                StaffNumShotsText.text = String.Format(ModTranslation.getString("Shots"), YakuzaGun.ShareShots);
                            else
                                StaffNumShotsText.text = "";
                        }
                    }
                    return currentTarget && PlayerControl.LocalPlayer.CanMove;
                },
                () => { StaffKillButton.Timer = StaffKillButton.MaxTimer; },
                hm.KillButton.graphic.sprite,
                new Vector3(0f, 1f, 0),
                hm,
                hm.KillButton,
                KeyCode.Q
            );

            StaffNumShotsText = GameObject.Instantiate(StaffKillButton.actionButton.cooldownTimerText, StaffKillButton.actionButton.cooldownTimerText.transform.parent);
            StaffNumShotsText.text = "";
            StaffNumShotsText.enableWordWrapping = false;
            StaffNumShotsText.transform.localScale = Vector3.one * 0.5f;
            StaffNumShotsText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
        }

        public static void SetButtonCooldowns()
        {
            StaffKillButton.MaxTimer = Cooldowns;
        }
    }
}