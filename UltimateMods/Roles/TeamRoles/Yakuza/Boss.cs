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
    public static class YakuzaBoss
    {
        private static CustomButton BossKillButton;
        public static TMPro.TMP_Text BossNumShotsText;
        public static PlayerControl boss;
        public static PlayerControl currentTarget;
        public static bool CanKill = false;

        public static Color color = new Color32(46, 84, 245, byte.MaxValue);

        public static int MaxShots = 2;
        public static float Cooldowns = 30f;
        public static bool CanKillNeutral = true;
        public static bool MisfireKillsTarget = false;
        public static bool ShareShotsCounts = true;

        public static void ClearAndReload()
        {
            boss = null;
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
            // Boss Kill
            BossKillButton = new CustomButton(
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
                            targetId = YakuzaBoss.currentTarget.PlayerId;
                            misfire = false;
                        }
                        else
                        {
                            targetId = PlayerControl.LocalPlayer.PlayerId;
                            misfire = true;
                        }

                        MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.BossKill, Hazel.SendOption.Reliable, -1);
                        killWriter.Write(PlayerControl.LocalPlayer.Data.PlayerId);
                        killWriter.Write(targetId);
                        killWriter.Write(misfire);
                        AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                        RPCProcedure.BossKill(PlayerControl.LocalPlayer.Data.PlayerId, targetId, misfire);
                    }

                    BossKillButton.Timer = BossKillButton.MaxTimer;
                    currentTarget = null;
                },
                () =>
                {
                    if (ShareShotsCounts == false)
                        return PlayerControl.LocalPlayer.isRole(RoleType.YakuzaBoss) && MaxShots > 0 && !PlayerControl.LocalPlayer.Data.IsDead && CanKill;
                    else if (ShareShotsCounts == true)
                        return PlayerControl.LocalPlayer.isRole(RoleType.YakuzaBoss) && YakuzaGun.ShareShots > 0 && !PlayerControl.LocalPlayer.Data.IsDead && CanKill;
                    return true;
                },
                () =>
                {
                    if (ShareShotsCounts == false)
                    {
                        if (MaxShots > 0)
                            BossNumShotsText.text = String.Format(ModTranslation.getString("Shots"), MaxShots);
                        else
                            BossNumShotsText.text = "";
                    }
                    else if (ShareShotsCounts == true)
                    {
                        if (YakuzaGun.ShareShots > 0)
                            BossNumShotsText.text = String.Format(ModTranslation.getString("Shots"), YakuzaGun.ShareShots);
                        else
                            BossNumShotsText.text = "";
                    }
                    return currentTarget && PlayerControl.LocalPlayer.CanMove;
                },
                () => { BossKillButton.Timer = BossKillButton.MaxTimer; },
                hm.KillButton.graphic.sprite,
                new Vector3(0f, 1f, 0),
                hm,
                hm.KillButton,
                KeyCode.Q
            );

            BossNumShotsText = GameObject.Instantiate(BossKillButton.actionButton.cooldownTimerText, BossKillButton.actionButton.cooldownTimerText.transform.parent);
            BossNumShotsText.text = "";
            BossNumShotsText.enableWordWrapping = false;
            BossNumShotsText.transform.localScale = Vector3.one * 0.5f;
            BossNumShotsText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
        }

        public static void SetButtonCooldowns()
        {
            BossKillButton.MaxTimer = Cooldowns;
        }
    }
}