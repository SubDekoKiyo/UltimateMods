namespace UltimateMods.Roles;

[HarmonyPatch]
public static class CrewmateRoles
{
    public class Altruist : RoleBase<Altruist>
    {
        public static Sprite AltruistButtonSprite;
        public static bool Started = false;
        public static bool Ended = false;
        public static DeadBody Target;
        public static float Duration { get { return CustomRolesH.AltruistDuration.getFloat(); } }

        public Altruist()
        {
            RoleId = roleId = RoleId.Altruist;
        }

        public static Sprite GetButtonSprite()
        {
            if (AltruistButtonSprite) return AltruistButtonSprite;
            AltruistButtonSprite = Helpers.LoadSpriteFromTexture2D(AltruistReviveButton, 115f);
            return AltruistButtonSprite;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate()
        {
            var TruePosition = PlayerControl.LocalPlayer.GetTruePosition();
            var MaxDistance = GameOptionsData.KillDistances[GameManager.Instance.LogicOptions.currentGameOptions.GetInt(Int32OptionNames.KillDistance)];
            var flag = (GameManager.Instance.LogicOptions.currentGameOptions.GetBool(BoolOptionNames.GhostsDoTasks) || !PlayerControl.LocalPlayer.Data.IsDead) &&
                        (!AmongUsClient.Instance || !AmongUsClient.Instance.IsGameOver) &&
                        PlayerControl.LocalPlayer.CanMove;
            var OverlapCircle = Physics2D.OverlapCircleAll(TruePosition, MaxDistance, LayerMask.GetMask(new[] { "Players", "Ghost" }));
            var ClosestDistance = float.MaxValue;

            foreach (var collider2D in OverlapCircle)
            {
                if (!flag || PlayerControl.LocalPlayer.Data.IsDead || collider2D.tag != "DeadBody" || Altruist.Started) continue;
                Altruist.Target = collider2D.GetComponent<DeadBody>();

                if (!(Vector2.Distance(TruePosition, Altruist.Target.TruePosition) <= MaxDistance)) continue;

                var Distance = Vector2.Distance(TruePosition, Altruist.Target.TruePosition);
                if (!(Distance < ClosestDistance)) continue;
                ClosestDistance = Distance;
            }
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public override void Clear()
        {
            players = new List<Altruist>();

            Started = false;
            Ended = false;
        }
    }

    public class Bakery : RoleBase<Bakery>
    {
        public static float BombRate { get { return CustomRolesH.BakeryBombRate.getFloat(); } }

        public Bakery()
        {
            RoleId = roleId = RoleId.Bakery;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public override void Clear()
        {
            players = new List<Bakery>();
        }
    }

    public class ProEngineer : RoleBase<ProEngineer>
    {
        private static Sprite EngineerRepairButtonSprite;

        public ProEngineer()
        {
            RoleId = roleId = RoleId.ProEngineer;
            ReamingCounts = FixCount;
        }

        public static bool CanFixSabo { get { return CustomRolesH.EngineerCanFixSabo.getBool(); } }
        public static int FixCount { get { return Mathf.RoundToInt(CustomRolesH.EngineerMaxFixCount.getFloat()); } }
        public static bool CanUseVents { get { return CustomRolesH.EngineerCanUseVents.getBool(); } }
        // public static float VentCooldown { get { return CustomRolesH.EngineerVentCooldown.getFloat(); } }
        public static int ReamingCounts = 1;

        public static Sprite GetFixButtonSprite()
        {
            if (EngineerRepairButtonSprite) return EngineerRepairButtonSprite;
            EngineerRepairButtonSprite = Helpers.LoadSpriteFromTexture2D(Modules.Assets.EngineerRepairButton, 115f);
            return EngineerRepairButtonSprite;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public override void Clear()
        {
            players = new List<ProEngineer>();
        }
    }

    public class Lighter : RoleBase<Lighter>
    {
        public Lighter()
        {
            RoleId = roleId = RoleId.Lighter;
        }

        public static Sprite LighterButtonSprite;
        public static float LighterModeLightsOnVision { get { return CustomRolesH.LighterModeLightsOnVision.getFloat(); } }
        public static float LighterModeLightsOffVision { get { return CustomRolesH.LighterModeLightsOffVision.getFloat(); } }
        public static float Cooldown { get { return CustomRolesH.LighterCooldown.getFloat(); } }
        public static float Duration { get { return CustomRolesH.LighterDuration.getFloat(); } }
        public static bool LightActive = false;

        public static bool IsLightActive(PlayerControl player)
        {
            if (player.IsRole(RoleId.Lighter) && player.IsAlive())
            {
                return LightActive;
            }
            return false;
        }

        public static Sprite GetButtonSprite()
        {
            if (LighterButtonSprite) return LighterButtonSprite;
            LighterButtonSprite = Helpers.LoadSpriteFromTexture2D(LighterLight, 115f);
            return LighterButtonSprite;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public override void Clear()
        {
            players = new List<Lighter>();

            LightActive = false;
        }
    }

    public class Madmate : RoleBase<Madmate>
    {
        public Madmate()
        {
            RoleId = roleId = RoleId.Madmate;
        }

        public static bool CanDieToSheriffOrYakuza { get { return CustomRolesH.MadmateCanDieToSheriffOrYakuza.getBool(); } }
        public static bool CanUseVents { get { return CustomRolesH.MadmateCanEnterVents.getBool(); } }
        public static bool CanMoveInVents { get { return CustomRolesH.MadmateCanMoveInVents.getBool(); } }
        public static bool CanSabotage { get { return CustomRolesH.MadmateCanSabotage.getBool(); } }
        public static bool HasImpostorVision { get { return CustomRolesH.MadmateHasImpostorVision.getBool(); } }
        public static bool CanFixO2 { get { return CustomRolesH.MadmateCanFixO2.getBool(); } }
        public static bool CanFixComms { get { return CustomRolesH.MadmateCanFixComms.getBool(); } }
        public static bool CanFixReactor { get { return CustomRolesH.MadmateCanFixReactor.getBool(); } }
        public static bool CanFixBlackout { get { return CustomRolesH.MadmateCanFixBlackout.getBool(); } }
        public static bool HasTasks { get { return CustomRolesH.MadmateHasTasks.getBool(); } }
        public static int CommonTasksCount { get { return CustomRolesH.MadmateTasksCount.CommonTasks; } }
        public static int ShortTasksCount { get { return CustomRolesH.MadmateTasksCount.ShortTasks; } }
        public static int LongTasksCount { get { return CustomRolesH.MadmateTasksCount.LongTasks; } }
        public static bool CanKnowImpostorsTaskEnd { get { return CustomRolesH.MadmateCanKnowImpostorWhenTasksEnded.getBool(); } }
        public static bool CanWinTaskEnd { get { return CustomRolesH.MadmateCanWinWhenTaskEnded.getBool(); } }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
        class BeginCrewmatePatch
        {
            public static void Postfix(ShipStatus __instance)
            {
                if (HasTasks && PlayerControl.LocalPlayer.IsRole(RoleId.Madmate))
                {
                    local.AssignTasks();
                }
            }
        }

        public void AssignTasks()
        {
            player.GenerateAndAssignTasks(CommonTasksCount, ShortTasksCount, LongTasksCount);
        }

        public static bool KnowsImpostors(PlayerControl player)
        {
            return CanKnowImpostorsTaskEnd && TasksComplete(player);
        }

        public static bool TasksComplete(PlayerControl player)
        {
            if (!HasTasks) return false;

            int counter = 0;
            int totalTasks = CommonTasksCount + LongTasksCount + ShortTasksCount;
            if (totalTasks == 0) return true;
            foreach (var task in player.Data.Tasks)
            {
                if (task.Complete)
                {
                    counter++;
                }
            }
            return counter >= totalTasks;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public override void Clear()
        {
            players = new List<Madmate>();
        }
    }

    public class Mayor : RoleBase<Mayor>
    {
        public Mayor()
        {
            RoleId = roleId = RoleId.Mayor;
            ReamingCount = MaxButton;
        }

        public static Sprite MayorMeetingButtonSprite;
        public static int NumVotes { get { return Mathf.RoundToInt(CustomRolesH.MayorNumVotes.getFloat()); } }
        public static bool HasMeetingButton { get { return CustomRolesH.MayorMeetingButton.getBool(); } }
        public static int MaxButton { get { return Mathf.RoundToInt(CustomRolesH.MayorNumMeetingButton.getFloat()); } }
        public static int ReamingCount = 1;

        public static Sprite GetButtonSprite()
        {
            if (MayorMeetingButtonSprite) return MayorMeetingButtonSprite;
            MayorMeetingButtonSprite = Helpers.LoadSpriteFromTexture2D(MeetingButton, 550f);
            return MayorMeetingButtonSprite;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public override void Clear()
        {
            players = new List<Mayor>();
        }
    }

    public class Seer : RoleBase<Seer>
    {
        public Seer()
        {
            RoleId = roleId = RoleId.Seer;
        }

        private static Sprite SoulSprite;
        public static List<Vector3> DeadBodyPositions = new();
        public static float SoulDuration { get { return CustomRolesH.SeerSoulDuration.getFloat(); } }
        public static bool LimitSoulDuration { get { return CustomRolesH.SeerLimitSoulDuration.getBool(); } }
        public static int Mode { get { return CustomRolesH.SeerMode.getSelection(); } }

        public static Sprite GetSoulSprite()
        {
            if (SoulSprite) return SoulSprite;
            SoulSprite = Helpers.LoadSpriteFromTexture2D(Soul, 500f);
            return SoulSprite;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd()
        {
            if (DeadBodyPositions != null && PlayerControl.LocalPlayer.IsRole(RoleId.Seer) && (Mode == 0 || Mode == 2))
            {
                foreach (Vector3 pos in DeadBodyPositions)
                {
                    GameObject soul = new();
                    // soul.transform.position = pos;
                    soul.transform.position = new Vector3(pos.x, pos.y, pos.y / 1000 - 1f);
                    soul.layer = 5;
                    var rend = soul.AddComponent<SpriteRenderer>();
                    rend.sprite = GetSoulSprite();

                    if (LimitSoulDuration)
                    {
                        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(SoulDuration, new Action<float>((p) =>
                        {
                            if (rend != null)
                            {
                                var tmp = rend.color;
                                tmp.a = Mathf.Clamp01(1 - p);
                                rend.color = tmp;
                            }
                            if (p == 1f && rend != null && rend.gameObject != null) UnityEngine.Object.Destroy(rend.gameObject);
                        })));
                    }
                }
                DeadBodyPositions = new List<Vector3>();
            }
        }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public override void Clear()
        {
            players = new List<Seer>();
        }
    }

    public class Sheriff : RoleBase<Sheriff>
    {
        public Sheriff()
        {
            RoleId = roleId = RoleId.Sheriff;
            ReamingShots = MaxShots;
        }

        public static PlayerControl currentTarget;
        public static float Cooldown { get { return CustomRolesH.SheriffCooldowns.getFloat(); } }
        public static int MaxShots { get { return Mathf.RoundToInt(CustomRolesH.SheriffMaxShots.getFloat()); } }
        public static bool CanKillNeutrals { get { return CustomRolesH.SheriffCanKillNeutral.getBool(); } }
        public static bool MisfireKillsTarget { get { return CustomRolesH.SheriffMisfireKillsTarget.getBool(); } }
        public static int ReamingShots = 1;

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate()
        {
            if (PlayerControl.LocalPlayer.IsRole(RoleId.Sheriff) && Sheriff.MaxShots > 0)
            {
                Sheriff.currentTarget = SetTarget();
                SetPlayerOutline(Sheriff.currentTarget, SheriffYellow);
            }
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public override void Clear()
        {
            players = new List<Sheriff>();
        }
    }

    public class Snitch : RoleBase<Snitch>
    {
        public Snitch()
        {
            RoleId = roleId = RoleId.Snitch;
        }

        public static List<CustomArrow> LocalArrows = new();
        public static int TaskCountForReveal { get { return Mathf.RoundToInt(CustomRolesH.SnitchLeftTasksForReveal.getFloat()); } }
        public static bool IncludeTeamJackal { get { return CustomRolesH.SnitchIncludeTeamJackal.getBool(); } }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate()
        {
            if (LocalArrows == null) return;

            foreach (CustomArrow arrow in LocalArrows) arrow.arrow.SetActive(false);

            foreach (var snitch in Snitch.allPlayers)
            {
                var (PlayerCompleted, PlayerTotal) = TasksHandler.taskInfo(snitch.Data);
                int NumberOfTasks = PlayerTotal - PlayerCompleted;

                if (NumberOfTasks <= TaskCountForReveal)
                {
                    if (PlayerControl.LocalPlayer.Data.Role.IsImpostor || (IncludeTeamJackal && (PlayerControl.LocalPlayer.IsRole(RoleId.Jackal) || PlayerControl.LocalPlayer.IsRole(RoleId.Sidekick))))
                    {
                        if (LocalArrows.Count == 0) LocalArrows.Add(new(SnitchGreen));
                        if (LocalArrows.Count != 0 && LocalArrows[0] != null)
                        {
                            LocalArrows[0].arrow.SetActive(true);
                            LocalArrows[0].image.color = SnitchGreen;
                            LocalArrows[0].Update(snitch.transform.position);
                        }
                    }
                    else if (PlayerControl.LocalPlayer.IsRole(RoleId.Snitch))
                    {
                        int arrowIndex = 0;
                        foreach (PlayerControl p in PlayerControl.AllPlayerControls.GetFastEnumerator())
                        {
                            bool ArrowForImpostor = p.Data.Role.IsImpostor;
                            bool ArrowForTeamJackal = IncludeTeamJackal && (p.IsRole(RoleId.Jackal) || p.IsRole(RoleId.Sidekick));

                            // Update the arrows' color every time bc things go weird when you add a sidekick or someone dies
                            Color c = ImpostorRed;
                            if (ArrowForTeamJackal) c = JackalBlue;

                            if (!p.Data.IsDead && (ArrowForImpostor || ArrowForTeamJackal))
                            {
                                if (arrowIndex >= LocalArrows.Count)
                                {
                                    LocalArrows.Add(new(c));
                                }
                                if (arrowIndex < LocalArrows.Count && LocalArrows[arrowIndex] != null)
                                {
                                    LocalArrows[arrowIndex].image.color = c;
                                    LocalArrows[arrowIndex].arrow.SetActive(true);
                                    LocalArrows[arrowIndex].Update(p.transform.position, c);
                                }
                                arrowIndex++;
                            }
                        }
                    }
                }
            }
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public override void Clear()
        {
            players = new List<Snitch>();

            if (LocalArrows != null) foreach (CustomArrow Arrow in LocalArrows)
                    if (Arrow?.arrow != null) UnityEngine.Object.Destroy(Arrow.arrow);
        }
    }
}