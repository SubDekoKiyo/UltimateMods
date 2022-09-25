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
    public static class YakuzaGun
    {
        private static CustomButton GunKillButton;
        public static TMPro.TMP_Text GunNumShotsText;
        public static PlayerControl gun;
        public static PlayerControl currentTarget;

        public static int MaxShots = 2;
        public static int ShareShots = 2;
        public static float Cooldowns = 30f;
        public static bool CanKillNeutral = true;
        public static bool MisfireKillsTarget = false;
        public static bool ShareShotsCounts = true;

        public static void ClearAndReload()
        {
            gun = null;
            currentTarget = null;
            MaxShots = (int)CustomRolesH.YakuzaMaxShots.getFloat();
            Cooldowns = CustomRolesH.YakuzaCooldowns.getFloat();
            CanKillNeutral = CustomRolesH.YakuzaCanKillNeutral.getBool();
            MisfireKillsTarget = CustomRolesH.YakuzaMisfireKillsTarget.getBool();
            ShareShotsCounts = CustomRolesH.YakuzaShareShotsCount.getBool();

            if (ShareShotsCounts == true)
                YakuzaGun.ShareShots = MaxShots;
        }

        public static void MakeButtons(HudManager hm)
        {
            // Gun Kill
            GunKillButton = new CustomButton(
                () =>
                {
                    if (MaxShots <= 0 || ShareShots <= 0)
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

                        MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.GunKill, Hazel.SendOption.Reliable, -1);
                        killWriter.Write(PlayerControl.LocalPlayer.Data.PlayerId);
                        killWriter.Write(targetId);
                        killWriter.Write(misfire);
                        AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                        RPCProcedure.GunKill(PlayerControl.LocalPlayer.Data.PlayerId, targetId, misfire);
                    }

                    GunKillButton.Timer = GunKillButton.MaxTimer;
                    currentTarget = null;
                },
                () =>
                {
                    if (ShareShotsCounts == false)
                        return PlayerControl.LocalPlayer.isRole(RoleType.YakuzaGun) && MaxShots > 0 && !PlayerControl.LocalPlayer.Data.IsDead;
                    else if (ShareShotsCounts == true)
                        return PlayerControl.LocalPlayer.isRole(RoleType.YakuzaGun) && YakuzaGun.ShareShots > 0 && !PlayerControl.LocalPlayer.Data.IsDead;
                    return true;
                },
                () =>
                {
                    if (ShareShotsCounts == false)
                    {
                        if (MaxShots > 0)
                            GunNumShotsText.text = String.Format(ModTranslation.getString("Shots"), MaxShots);
                        else
                            GunNumShotsText.text = "";
                    }
                    else if (ShareShotsCounts == true)
                    {
                        if (ShareShots > 0)
                            GunNumShotsText.text = String.Format(ModTranslation.getString("Shots"), ShareShots);
                        else
                            GunNumShotsText.text = "";
                    }
                    return currentTarget && PlayerControl.LocalPlayer.CanMove;
                },
                () => { GunKillButton.Timer = GunKillButton.MaxTimer; },
                hm.KillButton.graphic.sprite,
                new Vector3(0f, 1f, 0),
                hm,
                hm.KillButton,
                KeyCode.Q
            );

            GunNumShotsText = GameObject.Instantiate(GunKillButton.actionButton.cooldownTimerText, GunKillButton.actionButton.cooldownTimerText.transform.parent);
            GunNumShotsText.text = "";
            GunNumShotsText.enableWordWrapping = false;
            GunNumShotsText.transform.localScale = Vector3.one * 0.5f;
            GunNumShotsText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
        }

        public static void SetButtonCooldowns()
        {
            GunKillButton.MaxTimer = Cooldowns;
        }
    }
}