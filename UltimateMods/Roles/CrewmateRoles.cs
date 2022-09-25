using System.Linq;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using UltimateMods.Modules;
using Hazel;

namespace UltimateMods.Roles
{
    [HarmonyPatch]
    public static class CrewmateRoles
    {
        public static class Sheriff
        {
            public static PlayerControl currentTarget;
            public static PlayerControl sheriff;
            private static CustomButton SheriffKillButton;
            public static TMPro.TMP_Text SheriffNumShotsText;
            public static Color color = new Color32(248, 205, 70, byte.MaxValue);

            public static bool CanKillNeutrals = true;
            public static bool MisfireKillsTarget = false;
            public static float Cooldown = 30f;
            public static int MaxShots = 2;

            public static void ClearAndReload()
            {
                sheriff = null;
                currentTarget = null;
                CanKillNeutrals = CustomRolesH.SheriffCanKillNeutral.getBool();
                MisfireKillsTarget = CustomRolesH.SheriffMisfireKillsTarget.getBool();
                Cooldown = CustomRolesH.SheriffCooldowns.getFloat();
                MaxShots = (int)CustomRolesH.SheriffMaxShots.getFloat();
            }

            public static void MakeButtons(HudManager hm)
            {
                // Sheriff Kill
                SheriffKillButton = new CustomButton(
                    () =>
                    {
                        if (MaxShots <= 0)
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
                                (CanKillNeutrals && currentTarget.IsNeutral()))
                            {
                                targetId = Sheriff.currentTarget.PlayerId;
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
                        currentTarget = null;
                    },
                    () => { return PlayerControl.LocalPlayer.isRole(RoleType.Sheriff) && MaxShots > 0 && !PlayerControl.LocalPlayer.Data.IsDead; },
                    () =>
                    {
                        if (SheriffNumShotsText != null)
                        {
                            if (MaxShots > 0)
                                SheriffNumShotsText.text = String.Format(ModTranslation.getString("Shots"), MaxShots);
                            else
                                SheriffNumShotsText.text = "";
                        }
                        return currentTarget && PlayerControl.LocalPlayer.CanMove;
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
                SheriffKillButton.MaxTimer = Sheriff.Cooldown;
            }
        }
    }
}