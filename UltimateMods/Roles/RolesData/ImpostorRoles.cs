namespace UltimateMods.Roles;

[HarmonyPatch]
public static class ImpostorRoles
{
    public class Adversity : RoleBase<Adversity>
    {
        public Adversity()
        {
            RoleId = roleId = RoleId.Adversity;
        }

        public static bool IsLast = false;
        public static List<CustomArrow> Arrows = new();
        public static float UpdateTimer = 0f;
        public static float Cooldown { get { return CustomRolesH.AdversityAdversityStateKillCooldown.getFloat(); } }
        public static bool CanFindMad { get { return CustomRolesH.AdversityAdversityStateCanFindMadmate.getBool(); } }

        public static void CheckAndAdversityState()
        {
            List<PlayerControl> imps = new();
            foreach (var imp in PlayerControl.AllPlayerControls) if (imp.IsImpostor() && imp.IsAlive()) imps.Add(imp);

            if (imps.Count == 1) IsLast = true;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd()
        {
            if (PlayerControl.LocalPlayer.IsRole(RoleId.Adversity) && IsLast) PlayerControl.LocalPlayer.SetKillTimer(Cooldown);
        }
        public override void FixedUpdate()
        {
            if (PlayerControl.LocalPlayer.IsRole(RoleId.Adversity) && IsLast && CanFindMad && Madmate.players.Count > 0)
            {
                // 前フレームからの経過時間をマイナスする
                UpdateTimer -= Time.fixedDeltaTime;

                // 1秒経過したらArrowを更新
                if (UpdateTimer <= 0.0f)
                {
                    // 前回のArrowをすべて破棄する
                    foreach (CustomArrow arrow in Arrows)
                    {
                        if (arrow?.arrow != null)
                        {
                            arrow.arrow.SetActive(false);
                            UnityEngine.Object.Destroy(arrow.arrow);
                        }
                    }

                    // Arrorw一覧
                    Arrows = new();

                    // インポスターの位置を示すArrorwを描画
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    {
                        if (p.IsDead()) continue;
                        CustomArrow arrow;
                        if (p.IsRole(RoleId.Madmate))
                        {
                            if (p.Data.Role.IsImpostor) arrow = new CustomArrow(ImpostorRed);
                            else arrow = new CustomArrow(Palette.Black);

                            arrow.arrow.SetActive(true);
                            arrow.Update(p.transform.position);
                            Arrows.Add(arrow);
                        }
                    }
                }
            }
        }
        public override void OnKill(PlayerControl target)
        {
            if (PlayerControl.LocalPlayer.IsRole(RoleId.Adversity) && IsLast) PlayerControl.LocalPlayer.SetKillTimer(Cooldown);
        }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public override void Clear()
        {
            players = new List<Adversity>();
        }
    }

    public class BountyHunter : RoleBase<BountyHunter>
    {
        public BountyHunter()
        {
            RoleId = roleId = RoleId.BountyHunter;
        }

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

        public override void OnMeetingStart()
        {
            if (PlayerControl.LocalPlayer.IsRole(RoleId.BountyHunter)) KillCooldowns = player.killTimer;
        }
        public override void OnMeetingEnd()
        {
            if (PlayerControl.LocalPlayer.IsRole(RoleId.BountyHunter)) player.SetKillTimerUnchecked(KillCooldowns);
            if (exists) BountyUpdateTimer = 0f;
        }
        public override void FixedUpdate()
        {
            if (PlayerControl.LocalPlayer.IsRole(RoleId.BountyHunter))
            {
                if (player.Data.IsDead)
                {
                    if (Arrow != null || Arrow.arrow != null) Object.Destroy(Arrow.arrow);
                    Arrow = null;
                    if (CooldownTimer != null && CooldownTimer.gameObject != null) Object.Destroy(CooldownTimer.gameObject);
                    CooldownTimer = null;
                    Bounty = null;
                    foreach (PoolablePlayer p in Options.PlayerIcons.Values) if (p != null && p.gameObject != null) p.gameObject.SetActive(false);
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
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls) if (!p.Data.IsDead && !p.Data.Disconnected && p != p.Data.Role.IsImpostor) BountyList.Add(p);

                    Bounty = BountyList[UltimateMods.rnd.Next(0, BountyList.Count)];
                    if (Bounty == null) return;

                    // Show poolable player
                    if (FastDestroyableSingleton<HudManager>.Instance != null && FastDestroyableSingleton<HudManager>.Instance.UseButton != null)
                        foreach (PoolablePlayer pp in Options.PlayerIcons.Values) pp.gameObject.SetActive(false);
                    {
                        if (Options.PlayerIcons.ContainsKey(Bounty.PlayerId) && Options.PlayerIcons[Bounty.PlayerId].gameObject != null)
                            Options.PlayerIcons[Bounty.PlayerId].gameObject.SetActive(true);
                    }
                }

                // Update Cooldown Text
                if (CooldownTimer != null) CooldownTimer.text = Mathf.CeilToInt(Mathf.Clamp(BountyUpdateTimer, 0, Duration)).ToString();

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
        }
        public override void OnKill(PlayerControl target)
        {
            if (target == Bounty)
            {
                Bounty = null;
                player.SetKillTimer(SuccessCooldown);
                BountyUpdateTimer = 0f; // Force bounty update
            }
            else player.SetKillTimer(GameManager.Instance.LogicOptions.currentGameOptions.GetFloat(FloatOptionNames.KillCooldown) + AdditionalCooldown);
        }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public override void Clear()
        {
            players = new List<BountyHunter>();

            BountyPos = new();
            ArrowUpdateTimer = 0f;
            BountyUpdateTimer = 0f;
            Arrow = new(ImpostorRed);
            Bounty = null;
            if (Arrow != null && Arrow.arrow != null) UnityEngine.Object.Destroy(Arrow.arrow);
            Arrow = null;
            if (CooldownTimer != null && CooldownTimer.gameObject != null) UnityEngine.Object.Destroy(CooldownTimer.gameObject);
            CooldownTimer = null;
            foreach (PoolablePlayer p in Options.PlayerIcons.Values) if (p != null && p.gameObject != null) p.gameObject.SetActive(false);
            KillCooldowns = player.killTimer / 2;
        }
    }

    public class CustomImpostor : RoleBase<CustomImpostor>
    {
        public CustomImpostor()
        {
            RoleId = roleId = RoleId.CustomImpostor;
        }

        public static float KillCooldowns { get { return CustomRolesH.CustomImpostorKillCooldown.getFloat(); } }
        public static bool CanUseVents { get { return CustomRolesH.CustomImpostorCanUseVents.getBool(); } }
        public static bool CanSabotage { get { return CustomRolesH.CustomImpostorCanSabotage.getBool(); } }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd()
        {
            if (PlayerControl.LocalPlayer.IsRole(RoleId.CustomImpostor)) player.SetKillTimerUnchecked(KillCooldowns);
        }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target)
        {
            if (PlayerControl.LocalPlayer.IsRole(RoleId.CustomImpostor)) player.SetKillTimerUnchecked(KillCooldowns);
        }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public override void Clear()
        {
            players = new List<CustomImpostor>();
        }
    }

    public class EvilHacker : RoleBase<EvilHacker>
    {
        public EvilHacker()
        {
            RoleId = roleId = RoleId.EvilHacker;
        }

        public static bool CanHasBetterAdmin { get { return CustomRolesH.EvilHackerCanHasBetterAdmin.getBool(); } }

        public static Sprite GetButtonSprite()
        {
            byte mapId = GameOptionsManager.Instance.CurrentGameOptions.MapId;
            UseButtonSettings button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.PolusAdminButton]; // Polus
            if (mapId is 0 or 3) button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.AdminMapButton]; // Skeld || Dleks
            else if (mapId == 1) button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.MIRAAdminButton]; // Mira HQ
            else if (mapId == 4) button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.AirshipAdminButton]; // Airship
            return button.Image;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public override void Clear()
        {
            players = new List<EvilHacker>();
        }
    }

    public class Teleporter : RoleBase<Teleporter>
    {
        public Teleporter()
        {
            RoleId = roleId = RoleId.Teleporter;
        }

        public enum TeleportTarget
        {
            AliveAllPlayer = 0,
            Crewmate = 1,
        }

        public static Sprite TeleportButtonSprite;
        public static float Cooldown { get { return CustomRolesH.TeleporterButtonCooldown.getFloat(); } }
        public static TeleportTarget TeleportTo { get { return (TeleportTarget)CustomRolesH.TeleporterTeleportTo.getSelection(); } }

        public static Sprite GetButtonSprite()
        {
            if (TeleportButtonSprite) return TeleportButtonSprite;
            TeleportButtonSprite = Helpers.LoadSpriteFromTexture2D(TeleporterTeleportButton, 115f);
            return TeleportButtonSprite;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public override void Clear()
        {
            players = new List<Teleporter>();
        }
    }

    public class UnderTaker : RoleBase<UnderTaker>
    {
        public UnderTaker()
        {
            RoleId = roleId = RoleId.UnderTaker;
        }

        public static Sprite UnderTakerButtonSprite;

        public static float KillCooldown { get { return CustomRolesH.UnderTakerKillCooldown.getFloat(); } }
        public static float MoveCooldown { get { return CustomRolesH.UnderTakerDuration.getFloat(); } }
        public static bool HasDuration { get { return CustomRolesH.UnderTakerHasDuration.getBool(); } }
        public static float Duration { get { return CustomRolesH.UnderTakerDuration.getFloat(); } }
        public static float SpeedDown { get { return CustomRolesH.UnderTakerDraggingSpeed.getFloat(); } }
        public static bool CanDumpVent { get { return CustomRolesH.UnderTakerCanDumpBodyVents.getBool(); } }

        public static bool DraggingBody = false;
        public static byte BodyId = 0;

        public static Sprite GetButtonSprite()
        {
            if (UnderTakerButtonSprite) return UnderTakerButtonSprite;
            UnderTakerButtonSprite = Helpers.LoadSpriteFromTexture2D(UnderTakerMoveButton, 115f);
            return UnderTakerButtonSprite;
        }

        public static void UnderTakerResetValuesAtDead()
        {
            DraggingBody = false;
            BodyId = 0;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd()
        {
            if (PlayerControl.LocalPlayer.IsRole(RoleId.UnderTaker)) player.SetKillTimerUnchecked(KillCooldown);
        }
        public override void FixedUpdate()
        {
            if (DraggingBody)
            {
                DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
                for (int i = 0; i < array.Length; i++)
                {
                    if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == BodyId)
                    {
                        foreach (var underTaker in allPlayers)
                        {
                            var currentPosition = underTaker.GetTruePosition();
                            var velocity = underTaker.gameObject.GetComponent<Rigidbody2D>().velocity.normalized;
                            velocity *= SpeedDown / 100f;
                            var newPos = ((Vector2)underTaker.GetTruePosition()) - (velocity / 3) + new Vector2(0.15f, 0.25f) + array[i].myCollider.offset;
                            if (!PhysicsHelpers.AnythingBetween(
                                currentPosition,
                                newPos,
                                Constants.ShipAndObjectsMask,
                                false
                            ))
                            {
                                if (GameManager.Instance.LogicOptions.currentGameOptions.GetByte(ByteOptionNames.MapId) == 5)
                                {
                                    array[i].transform.position = newPos;
                                    array[i].transform.position += new Vector3(0, 0, -0.5f);
                                }
                                else array[i].transform.position = newPos;
                            }
                        }
                    }
                }
            }
        }
        public override void OnKill(PlayerControl target)
        {
            if (PlayerControl.LocalPlayer.IsRole(RoleId.UnderTaker)) player.SetKillTimerUnchecked(KillCooldown);
        }
        public override void OnDeath(PlayerControl killer = null)
        {
            if (DraggingBody) UnderTakerResetValuesAtDead();
        }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void OnEnterVent()
        {
            if (PlayerControl.LocalPlayer.IsRole(RoleId.UnderTaker) && CanDumpVent && DraggingBody)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CleanBody, Hazel.SendOption.Reliable, -1);
                writer.Write(BodyId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.CleanBody(BodyId);
                DraggingBody = false;
                if (HasDuration && RolesButtons.UnderTakerButton.IsEffectActive)
                {
                    RolesButtons.UnderTakerButton.Timer = 0f;
                    return;
                }
            }
        }

        public override void Clear()
        {
            players = new List<UnderTaker>();
        }
    }
}