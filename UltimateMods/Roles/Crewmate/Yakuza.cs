namespace UltimateMods.Roles;

[HarmonyPatch]
public static class Yakuza
{
    public static float Cooldown { get { return CustomRolesH.YakuzaKillCooldown.getFloat(); } }
    public static int MaxShots { get { return Mathf.RoundToInt(CustomRolesH.YakuzaNumShots.getFloat()); } }
    public static bool CanKillNeutrals { get { return CustomRolesH.YakuzaCanKillNeutrals.getBool(); } }
    public static bool MisfireKillsTarget { get { return CustomRolesH.YakuzaMisfireKillsTarget.getBool(); } }
    public static bool ShareShots { get { return CustomRolesH.YakuzaShareShots.getBool(); } }
    public static int PublicShots = 2;
    public static int NumShots = 2;

    public class Boss
    {
        public static PlayerControl boss;
        private static CustomButton bossKillButton;
        public static TMPro.TMP_Text bossNumShotsText;
        public static PlayerControl currentTarget;

        public static void FixedUpdate()
        {
            if (boss == PlayerControl.LocalPlayer && NumShots > 0)
            {
                currentTarget = SetTarget();
                SetPlayerOutline(currentTarget, YakuzaBlue);
            }
        }

        public static void OnDeath(PlayerControl killer = null)
        {
            if (boss.IsDead())
            {
                if (Gun.gun.IsAlive())
                {
                    if (killer == null)
                    {
                        Gun.gun.Exiled();
                    }
                    else
                    {
                        Gun.gun.MurderPlayer(Gun.gun);
                    }
                    finalStatuses[Gun.gun.PlayerId] = FinalStatus.Suicide;
                }

                if (Executives.executives.IsAlive())
                {
                    if (killer == null)
                    {
                        Executives.executives.Exiled();
                    }
                    else
                    {
                        Executives.executives.MurderPlayer(Executives.executives);
                    }
                    finalStatuses[Executives.executives.PlayerId] = FinalStatus.Suicide;
                }
            }
        }

        public static void MakeButtons(HudManager hm)
        {
            bossKillButton = new CustomButton(
                () =>
                {
                    if (NumShots <= 0 || Yakuza.PublicShots <= 0)
                    {
                        return;
                    }

                    MurderAttemptResult murderAttemptResult = Helpers.CheckMurderAttempt(PlayerControl.LocalPlayer, currentTarget);
                    if (murderAttemptResult == MurderAttemptResult.SuppressKill) return;

                    if (murderAttemptResult == MurderAttemptResult.PerformKill)
                    {
                        bool misfire = false;
                        byte targetId = currentTarget.PlayerId;
                        if ((currentTarget.Data.Role.IsImpostor) ||
                            (CanKillNeutrals && currentTarget.IsNeutral()) ||
                            (Madmate.CanDieToSheriffOrYakuza && currentTarget.isRole(RoleType.Madmate)))
                        {
                            targetId = currentTarget.PlayerId;
                            misfire = false;
                        }
                        else
                        {
                            targetId = PlayerControl.LocalPlayer.PlayerId;
                            misfire = true;
                        }

                        MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.YakuzaKill, Hazel.SendOption.Reliable, -1);
                        killWriter.Write(PlayerControl.LocalPlayer.Data.PlayerId);
                        killWriter.Write(targetId);
                        killWriter.Write(misfire);
                        AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                        RPCProcedure.YakuzaKill(PlayerControl.LocalPlayer.Data.PlayerId, targetId, misfire);
                    }

                    bossKillButton.Timer = bossKillButton.MaxTimer;
                    currentTarget = null;
                },
                () =>
                {
                    if (Yakuza.ShareShots == false)
                        return PlayerControl.LocalPlayer.isRole(RoleType.Boss) && NumShots > 0 && !PlayerControl.LocalPlayer.Data.IsDead && Gun.gun.IsDead() && Executives.executives.IsDead();
                    else if (Yakuza.ShareShots == true)
                        return PlayerControl.LocalPlayer.isRole(RoleType.Boss) && Yakuza.PublicShots > 0 && !PlayerControl.LocalPlayer.Data.IsDead && Gun.gun.IsDead() && Executives.executives.IsDead();
                    return true;
                },
                () =>
                {
                    if (Yakuza.ShareShots == false)
                    {
                        if (NumShots > 0)
                            bossNumShotsText.text = String.Format(ModTranslation.getString("ReamingShots"), NumShots);
                        else
                            bossNumShotsText.text = "";
                    }
                    else if (Yakuza.ShareShots == true)
                    {
                        if (Yakuza.PublicShots > 0)
                            bossNumShotsText.text = String.Format(ModTranslation.getString("ReamingShots"), Yakuza.PublicShots);
                        else
                            bossNumShotsText.text = "";
                    }
                    return currentTarget && PlayerControl.LocalPlayer.CanMove;
                },
                () => { bossKillButton.Timer = bossKillButton.MaxTimer; },
                hm.KillButton.graphic.sprite,
                ButtonPositions.RightTop,
                hm,
                hm.KillButton,
                KeyCode.Q
            );

            bossNumShotsText = GameObject.Instantiate(bossKillButton.actionButton.cooldownTimerText, bossKillButton.actionButton.cooldownTimerText.transform.parent);
            bossNumShotsText.text = "";
            bossNumShotsText.enableWordWrapping = false;
            bossNumShotsText.transform.localScale = Vector3.one * 0.5f;
            bossNumShotsText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
        }

        public static void SetButtonCooldowns()
        {
            bossKillButton.MaxTimer = Yakuza.Cooldown;
        }
    }

    public static class Executives
    {
        private static CustomButton executivesKillButton;
        public static TMPro.TMP_Text executivesNumShotsText;
        public static PlayerControl executives;
        public static PlayerControl currentTarget;

        public static void FixedUpdate()
        {
            if (Executives.executives == PlayerControl.LocalPlayer && NumShots > 0)
            {
                currentTarget = SetTarget();
                SetPlayerOutline(currentTarget, YakuzaBlue);
            }
        }

        public static void MakeButtons(HudManager hm)
        {
            executivesKillButton = new CustomButton(
                () =>
                {
                    if (NumShots <= 0 || Yakuza.PublicShots <= 0)
                    {
                        return;
                    }

                    MurderAttemptResult murderAttemptResult = Helpers.CheckMurderAttempt(PlayerControl.LocalPlayer, currentTarget);
                    if (murderAttemptResult == MurderAttemptResult.SuppressKill) return;

                    if (murderAttemptResult == MurderAttemptResult.PerformKill)
                    {
                        bool misfire = false;
                        byte targetId = currentTarget.PlayerId;
                        if ((currentTarget.Data.Role.IsImpostor) ||
                            (CanKillNeutrals && currentTarget.IsNeutral()) ||
                            (Madmate.CanDieToSheriffOrYakuza && currentTarget.isRole(RoleType.Madmate)))
                        {
                            targetId = currentTarget.PlayerId;
                            misfire = false;
                        }
                        else
                        {
                            targetId = PlayerControl.LocalPlayer.PlayerId;
                            misfire = true;
                        }

                        MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.YakuzaKill, Hazel.SendOption.Reliable, -1);
                        killWriter.Write(PlayerControl.LocalPlayer.Data.PlayerId);
                        killWriter.Write(targetId);
                        killWriter.Write(misfire);
                        AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                        RPCProcedure.YakuzaKill(PlayerControl.LocalPlayer.Data.PlayerId, targetId, misfire);
                    }

                    executivesKillButton.Timer = executivesKillButton.MaxTimer;
                    currentTarget = null;
                },
                () =>
                {
                    if (Yakuza.ShareShots == false)
                        return PlayerControl.LocalPlayer.isRole(RoleType.Executives) && NumShots > 0 && !PlayerControl.LocalPlayer.Data.IsDead && Gun.gun.IsDead();
                    else if (Yakuza.ShareShots == true)
                        return PlayerControl.LocalPlayer.isRole(RoleType.Executives) && Yakuza.PublicShots > 0 && !PlayerControl.LocalPlayer.Data.IsDead && Gun.gun.IsDead();
                    return true;
                },
                () =>
                {
                    if (executivesNumShotsText != null)
                    {
                        if (Yakuza.ShareShots == false)
                        {
                            if (NumShots > 0)
                                executivesNumShotsText.text = String.Format(ModTranslation.getString("ReamingShots"), NumShots);
                            else
                                executivesNumShotsText.text = "";
                        }
                        else if (Yakuza.ShareShots == true)
                        {
                            if (Yakuza.PublicShots > 0)
                                executivesNumShotsText.text = String.Format(ModTranslation.getString("ReamingShots"), Yakuza.PublicShots);
                            else
                                executivesNumShotsText.text = "";
                        }
                    }
                    return currentTarget && PlayerControl.LocalPlayer.CanMove;
                },
                () => { executivesKillButton.Timer = executivesKillButton.MaxTimer; },
                hm.KillButton.graphic.sprite,
                ButtonPositions.RightTop,
                hm,
                hm.KillButton,
                KeyCode.Q
            );

            executivesNumShotsText = GameObject.Instantiate(executivesKillButton.actionButton.cooldownTimerText, executivesKillButton.actionButton.cooldownTimerText.transform.parent);
            executivesNumShotsText.text = "";
            executivesNumShotsText.enableWordWrapping = false;
            executivesNumShotsText.transform.localScale = Vector3.one * 0.5f;
            executivesNumShotsText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
        }

        public static void SetButtonCooldowns()
        {
            executivesKillButton.MaxTimer = Yakuza.Cooldown;
        }
    }

    public class Gun
    {
        private static CustomButton gunKillButton;
        public static TMPro.TMP_Text gunNumShotsText;
        public static PlayerControl gun;

        public static PlayerControl currentTarget;

        public static void FixedUpdate()
        {
            if (Gun.gun == PlayerControl.LocalPlayer && NumShots > 0)
            {
                currentTarget = SetTarget();
                SetPlayerOutline(currentTarget, YakuzaBlue);
            }
        }

        public static void MakeButtons(HudManager hm)
        {
            Gun.gunKillButton = new CustomButton(
                () =>
                {
                    if (NumShots <= 0 || Yakuza.PublicShots <= 0)
                    {
                        return;
                    }

                    MurderAttemptResult murderAttemptResult = Helpers.CheckMurderAttempt(PlayerControl.LocalPlayer, currentTarget);
                    if (murderAttemptResult == MurderAttemptResult.SuppressKill) return;

                    if (murderAttemptResult == MurderAttemptResult.PerformKill)
                    {
                        bool misfire = false;
                        byte targetId = currentTarget.PlayerId;
                        if ((currentTarget.Data.Role.IsImpostor) ||
                            (CanKillNeutrals && currentTarget.IsNeutral()) ||
                            (Madmate.CanDieToSheriffOrYakuza && currentTarget.isRole(RoleType.Madmate)))
                        {
                            targetId = currentTarget.PlayerId;
                            misfire = false;
                        }
                        else
                        {
                            targetId = PlayerControl.LocalPlayer.PlayerId;
                            misfire = true;
                        }

                        MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.YakuzaKill, Hazel.SendOption.Reliable, -1);
                        killWriter.Write(PlayerControl.LocalPlayer.Data.PlayerId);
                        killWriter.Write(targetId);
                        killWriter.Write(misfire);
                        AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                        RPCProcedure.YakuzaKill(PlayerControl.LocalPlayer.Data.PlayerId, targetId, misfire);
                    }

                    Gun.gunKillButton.Timer = Gun.gunKillButton.MaxTimer;
                    currentTarget = null;
                },
                () =>
                {
                    if (Yakuza.ShareShots == false)
                        return PlayerControl.LocalPlayer.isRole(RoleType.Gun) && NumShots > 0 && !PlayerControl.LocalPlayer.Data.IsDead;
                    else if (Yakuza.ShareShots == true)
                        return PlayerControl.LocalPlayer.isRole(RoleType.Gun) && Yakuza.PublicShots > 0 && !PlayerControl.LocalPlayer.Data.IsDead;
                    return true;
                },
                () =>
                {
                    if (Yakuza.ShareShots == false)
                    {
                        if (NumShots > 0)
                            Gun.gunNumShotsText.text = String.Format(ModTranslation.getString("ReamingShots"), NumShots);
                        else
                            Gun.gunNumShotsText.text = "";
                    }
                    else if (Yakuza.ShareShots == true)
                    {
                        if (Yakuza.PublicShots > 0)
                            Gun.gunNumShotsText.text = String.Format(ModTranslation.getString("ReamingShots"), Yakuza.PublicShots);
                        else
                            Gun.gunNumShotsText.text = "";
                    }
                    return currentTarget && PlayerControl.LocalPlayer.CanMove;
                },
                () => { Gun.gunKillButton.Timer = Gun.gunKillButton.MaxTimer; },
                hm.KillButton.graphic.sprite,
                ButtonPositions.RightTop,
                hm,
                hm.KillButton,
                KeyCode.Q
            );

            Gun.gunNumShotsText = GameObject.Instantiate(Gun.gunKillButton.actionButton.cooldownTimerText, Gun.gunKillButton.actionButton.cooldownTimerText.transform.parent);
            Gun.gunNumShotsText.text = "";
            Gun.gunNumShotsText.enableWordWrapping = false;
            Gun.gunNumShotsText.transform.localScale = Vector3.one * 0.5f;
            Gun.gunNumShotsText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
        }

        public static void SetButtonCooldowns()
        {
            Gun.gunKillButton.MaxTimer = Yakuza.Cooldown;
        }
    }

    public static void Clear()
    {
        if (ShareShots == true)
            PublicShots = MaxShots;
        else
            NumShots = MaxShots;
    }

    public static void MakeButtons(HudManager hm)
    {
        Boss.MakeButtons(hm);
        Executives.MakeButtons(hm);
        Gun.MakeButtons(hm);
    }

    public static void SetButtonCooldowns()
    {
        Boss.SetButtonCooldowns();
        Executives.SetButtonCooldowns();
        Gun.SetButtonCooldowns();
    }

    public static void FixedUpdate()
    {
        Boss.FixedUpdate();
        Executives.FixedUpdate();
        Gun.FixedUpdate();
    }
}