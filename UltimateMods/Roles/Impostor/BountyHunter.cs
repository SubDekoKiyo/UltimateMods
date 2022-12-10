using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static UltimateMods.ColorDictionary;
using UltimateMods.Patches;
using UltimateMods.Objects;
using AmongUs.GameOptions;
using UltimateMods.Utilities;

namespace UltimateMods.Roles
{
    [HarmonyPatch]
    public class BountyHunter : RoleBase<BountyHunter>
    {
        public static CustomArrow Arrow;
        public static PlayerControl Bounty;
        public static TMP_Text CooldownTimer;

        public static float SuccessCooldown { get { return CustomRolesH.BountyHunterSuccessKillCooldown.getFloat(); } }
        public static float AdditionalCooldown { get { return CustomRolesH.BountyHunterAdditionalKillCooldown.getFloat(); } }
        public static float Duration { get { return CustomRolesH.BountyHunterDuration.getFloat(); } }
        public static bool ShowArrow { get { return CustomRolesH.BountyHunterShowArrow.getBool(); } }
        public static float ArrowUpdate { get { return CustomRolesH.BountyHunterArrowUpdateCooldown.getFloat(); } }

        public static float KillCooldowns = 30f;
        public static float ArrowUpdateTimer = 0f;
        public static float BountyUpdateTimer = 0f;

        public BountyHunter()
        {
            RoleType = roleId = RoleType.BountyHunter;
        }

        public override void OnMeetingStart()
        {
            KillCooldowns = player.killTimer;
        }
        public override void OnMeetingEnd()
        {
            if (PlayerControl.LocalPlayer == player)
                player.SetKillTimerUnchecked(KillCooldowns);
        }
        public override void FixedUpdate()
        {
            if (PlayerControl.LocalPlayer != player) return;

            if (player.Data.IsDead)
            {
                if (BountyHunter.Arrow != null || BountyHunter.Arrow.arrow != null) UnityEngine.Object.Destroy(BountyHunter.Arrow.arrow);
                BountyHunter.Arrow = null;
                if (BountyHunter.CooldownTimer != null && BountyHunter.CooldownTimer.gameObject != null) UnityEngine.Object.Destroy(BountyHunter.CooldownTimer.gameObject);
                BountyHunter.CooldownTimer = null;
                BountyHunter.Bounty = null;
                foreach (PoolablePlayer p in Options.PlayerIcons.Values)
                {
                    if (p != null && p.gameObject != null) p.gameObject.SetActive(false);
                }
                return;
            }

            BountyHunter.ArrowUpdateTimer -= Time.fixedDeltaTime;
            BountyHunter.BountyUpdateTimer -= Time.fixedDeltaTime;

            if (BountyHunter.Bounty == null || BountyHunter.BountyUpdateTimer <= 0f)
            {
                // Set new bounty
                BountyHunter.Bounty = null;
                BountyHunter.ArrowUpdateTimer = 0f; // Force arrow to update
                BountyHunter.BountyUpdateTimer = BountyHunter.Duration;
                var BountyList = new List<PlayerControl>();
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (!p.Data.IsDead && !p.Data.Disconnected && p != p.Data.Role.IsImpostor) BountyList.Add(p);
                }
                BountyHunter.Bounty = BountyList[UltimateMods.rnd.Next(0, BountyList.Count)];
                if (BountyHunter.Bounty == null) return;

                // Show poolable player
                if (FastDestroyableSingleton<HudManager>.Instance != null && FastDestroyableSingleton<HudManager>.Instance.UseButton != null)
                {
                    foreach (PoolablePlayer pp in Options.PlayerIcons.Values) pp.gameObject.SetActive(false);
                    if (Options.PlayerIcons.ContainsKey(BountyHunter.Bounty.PlayerId) && Options.PlayerIcons[BountyHunter.Bounty.PlayerId].gameObject != null)
                        Options.PlayerIcons[BountyHunter.Bounty.PlayerId].gameObject.SetActive(true);
                }
            }

            // Update Cooldown Text
            if (BountyHunter.CooldownTimer != null)
            {
                BountyHunter.CooldownTimer.text = Mathf.CeilToInt(Mathf.Clamp(BountyHunter.BountyUpdateTimer, 0, BountyHunter.Duration)).ToString();
            }

            // Update Arrow
            if (BountyHunter.ShowArrow && BountyHunter.Bounty != null)
            {
                if (BountyHunter.Arrow == null) BountyHunter.Arrow = new CustomArrow(Color.red);
                if (BountyHunter.ArrowUpdateTimer <= 0f)
                {
                    BountyHunter.Arrow.Update(BountyHunter.Bounty.transform.position);
                    BountyHunter.ArrowUpdateTimer = BountyHunter.ArrowUpdate;
                }
                BountyHunter.Arrow.Update();
            }
        }
        public override void OnKill(PlayerControl target)
        {
            foreach (var bountyHunter in BountyHunter.allPlayers)
            {
                if (target == Bounty)
                {
                    bountyHunter.SetKillTimer(SuccessCooldown);
                    BountyHunter.BountyUpdateTimer = 0f; // Force bounty update
                }
                else
                    bountyHunter.SetKillTimer(GameOptionsManager.Instance.CurrentGameOptions.Cast<NormalGameOptionsV07>().KillCooldown + AdditionalCooldown);
            }
        }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void Clear()
        {
            players = new List<BountyHunter>();

            Arrow = new CustomArrow(ImpostorRed);
            Bounty = null;
            ArrowUpdateTimer = 0f;
            BountyUpdateTimer = 0f;
            if (Arrow != null && Arrow.arrow != null) UnityEngine.Object.Destroy(Arrow.arrow);
            Arrow = null;
            if (CooldownTimer != null && CooldownTimer.gameObject != null) UnityEngine.Object.Destroy(CooldownTimer.gameObject);
            CooldownTimer = null;
            foreach (PoolablePlayer p in Options.PlayerIcons.Values)
                if (p != null && p.gameObject != null) p.gameObject.SetActive(false);
            foreach (var bountyHunter in BountyHunter.allPlayers)
                KillCooldowns = bountyHunter.killTimer / 2;
        }
    }
}