namespace UltimateMods.Roles
{
    [HarmonyPatch]
    public class BountyHunter : RoleBase<BountyHunter>
    {
        public static CustomArrow Arrow;
        public static PlayerControl Bounty;
        public static TMP_Text CooldownTimer;
        public static Vector3 BountyPos;

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
                if (Arrow != null || Arrow.arrow != null) UnityEngine.Object.Destroy(Arrow.arrow);
                Arrow = null;
                if (CooldownTimer != null && CooldownTimer.gameObject != null) UnityEngine.Object.Destroy(CooldownTimer.gameObject);
                CooldownTimer = null;
                Bounty = null;
                foreach (PoolablePlayer p in Options.PlayerIcons.Values)
                {
                    if (p != null && p.gameObject != null) p.gameObject.SetActive(false);
                }
                return;
            }

            ArrowUpdateTimer -= Time.fixedDeltaTime;
            BountyUpdateTimer -= Time.fixedDeltaTime;

            if (Bounty == null || BountyUpdateTimer <= 0f)
            {
                // Set new bounty
                ArrowUpdateTimer = 0f; // Force arrow to update
                BountyUpdateTimer = Duration;
                var BountyList = new List<PlayerControl>();
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (!p.Data.IsDead && !p.Data.Disconnected && p != p.Data.Role.IsImpostor) BountyList.Add(p);
                }
                Bounty = BountyList[UltimateMods.rnd.Next(0, BountyList.Count)];
                if (Bounty == null) return;

                // Show poolable player
                if (FastDestroyableSingleton<HudManager>.Instance != null && FastDestroyableSingleton<HudManager>.Instance.UseButton != null)
                {
                    foreach (PoolablePlayer pp in Options.PlayerIcons.Values) pp.gameObject.SetActive(false);
                    if (Options.PlayerIcons.ContainsKey(Bounty.PlayerId) && Options.PlayerIcons[Bounty.PlayerId].gameObject != null)
                        Options.PlayerIcons[Bounty.PlayerId].gameObject.SetActive(true);
                }
            }

            // Update Cooldown Text
            if (CooldownTimer != null)
            {
                CooldownTimer.text = Mathf.CeilToInt(Mathf.Clamp(BountyUpdateTimer, 0, Duration)).ToString();
            }

            // Update Arrow
            if (ShowArrow && Bounty != null)
            {
                if (Arrow == null) Arrow = new CustomArrow(ImpostorRed);
                if (ArrowUpdateTimer <= 0f)
                {
                    BountyPos = Bounty.transform.position;
                    Arrow.Update(BountyPos);
                    ArrowUpdateTimer = ArrowUpdate;
                }
                Arrow.Update(BountyPos);
            }
        }
        public override void OnKill(PlayerControl target)
        {
            foreach (var bountyHunter in allPlayers)
            {
                if (target == Bounty)
                {
                    Bounty = null;
                    bountyHunter.SetKillTimer(SuccessCooldown);
                    BountyUpdateTimer = 0f; // Force bounty update
                }
                else
                    bountyHunter.SetKillTimer(GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown) + AdditionalCooldown);
            }
        }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public override void Clear()
        {
            players = new List<BountyHunter>();

            BountyPos = new();
            ArrowUpdateTimer = 0f;
            BountyUpdateTimer = 0f;
            Arrow = new CustomArrow(ImpostorRed);
            Bounty = null;
            if (Arrow != null && Arrow.arrow != null) UnityEngine.Object.Destroy(Arrow.arrow);
            Arrow = null;
            if (CooldownTimer != null && CooldownTimer.gameObject != null) UnityEngine.Object.Destroy(CooldownTimer.gameObject);
            CooldownTimer = null;
            foreach (PoolablePlayer p in Options.PlayerIcons.Values) if (p != null && p.gameObject != null) p.gameObject.SetActive(false);
            foreach (var bountyHunter in allPlayers) KillCooldowns = bountyHunter.killTimer / 2;
        }
    }
}