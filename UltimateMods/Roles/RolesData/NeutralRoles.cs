namespace UltimateMods.Roles;

public static class NeutralRoles
{
    public class Arsonist : RoleBase<Arsonist>
    {
        public override string RoleName() { return "Arsonist"; }
        public override Color RoleColor() { return ArsonistOrange; }

        public Arsonist()
        {
            RoleId = roleId = RoleId.Arsonist;
        }

        public static PlayerControl CurrentTarget;
        public static PlayerControl DouseTarget;
        public static List<PlayerControl> DousedPlayers = new();

        public static float Cooldown { get { return CustomRolesH.ArsonistCooldown.getFloat(); } }
        public static float Duration { get { return CustomRolesH.ArsonistDuration.getFloat(); } }
        public static bool DousedEveryone = false;

        private static Sprite DouseSprite;
        private static Sprite IgniteSprite;
        public static Sprite GetDouseSprite()
        {
            if (DouseSprite) return DouseSprite;
            DouseSprite = Helpers.LoadSpriteFromTexture2D(ArsonistDouseButton, 115f);
            return DouseSprite;
        }
        public static Sprite GetIgniteSprite()
        {
            if (IgniteSprite) return IgniteSprite;
            IgniteSprite = Helpers.LoadSpriteFromTexture2D(ArsonistIgniteButton, 115f);
            return IgniteSprite;
        }

        public static bool DousedEveryoneAlive()
        {
            return PlayerControl.AllPlayerControls.ToArray().All(x =>
            {
                return x == x.IsRole(RoleId.Arsonist) || x.Data.IsDead || x.Data.Disconnected || DousedPlayers.Any(y => y.PlayerId == x.PlayerId);
            });
        }

        public static void UpdateStatus()
        {
            if (PlayerControl.LocalPlayer.IsRole(RoleId.Arsonist))
            {
                DousedEveryone = DousedEveryoneAlive();
            }
        }

        public static void UpdateIcons()
        {
            foreach (PoolablePlayer pp in Options.PlayerIcons.Values)
            {
                pp.gameObject.SetActive(false);
            }

            if (PlayerControl.LocalPlayer.IsRole(RoleId.Arsonist))
            {
                int visibleCounter = 0;
                Vector3 bottomLeft = FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition;
                bottomLeft.x *= -1;
                bottomLeft += new Vector3(-0.25f, -0.25f, 0);

                foreach (PlayerControl p in PlayerControl.AllPlayerControls.GetFastEnumerator())
                {
                    if (p.PlayerId == PlayerControl.LocalPlayer.PlayerId) continue;
                    if (!Options.PlayerIcons.ContainsKey(p.PlayerId)) continue;

                    if (p.Data.IsDead || p.Data.Disconnected)
                    {
                        Options.PlayerIcons[p.PlayerId].gameObject.SetActive(false);
                    }
                    else
                    {
                        Options.PlayerIcons[p.PlayerId].gameObject.SetActive(true);
                        Options.PlayerIcons[p.PlayerId].transform.localScale = Vector3.one * 0.25f;
                        Options.PlayerIcons[p.PlayerId].transform.localPosition = bottomLeft + Vector3.right * visibleCounter * 0.45f;
                        visibleCounter++;
                    }
                    bool isDoused = DousedPlayers.Any(x => x.PlayerId == p.PlayerId);
                    Options.PlayerIcons[p.PlayerId].SetSemiTransparent(!isDoused);
                }
            }
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd()
        {
            Arsonist.UpdateIcons();
        }
        public override void FixedUpdate()
        {
            if (PlayerControl.LocalPlayer.IsRole(RoleId.Arsonist))
            {
                List<PlayerControl> UnTargetables;
                if (DouseTarget != null) UnTargetables = PlayerControl.AllPlayerControls.ToArray().Where(x => x.PlayerId != DouseTarget.PlayerId).ToList();
                else UnTargetables = DousedPlayers;
                CurrentTarget = SetTarget(untargetablePlayers: UnTargetables);
                if (CurrentTarget != null) SetPlayerOutline(CurrentTarget, ArsonistOrange);
            }
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public override void Clear()
        {
            players = new List<Arsonist>();

            CurrentTarget = null;
            DouseTarget = null;
            DousedEveryone = false;
            DousedPlayers = new();
            foreach (PoolablePlayer p in Options.PlayerIcons.Values) if (p != null && p.gameObject != null) p.gameObject.SetActive(false);
        }
    }

    public class Jackal : RoleBase<Jackal>
    {
        public override string RoleName() { return "Jackal"; }
        public override Color RoleColor() { return JackalBlue; }

        public Jackal()
        {
            RoleId = roleId = RoleId.Jackal;
            CanSidekick = CanCreateSidekick;
        }

        public static PlayerControl CurrentTarget;
        public static Sprite JackalSidekickButtonSprite;
        public static List<PlayerControl> BlockTarget = new();

        public static float Cooldown { get { return CustomRolesH.JackalKillCooldown.getFloat(); } }
        public static float CreateSideKickCooldown { get { return CustomRolesH.JackalCreateSidekickCooldown.getFloat(); } }
        public static bool CanUseVents { get { return CustomRolesH.JackalCanUseVents.getBool(); } }
        public static bool CanCreateSidekick { get { return CustomRolesH.JackalCanCreateSidekick.getBool(); } }
        public static bool JackalPromotedFromSidekickCanCreateSidekick { get { return CustomRolesH.JackalPromotedFromSidekickCanCreateSidekick.getBool(); } }
        public static bool HasImpostorVision { get { return CustomRolesH.JackalAndSidekickHaveImpostorVision.getBool(); } }
        public static bool CanSidekick = true;

        public static Sprite GetButtonSprite()
        {
            if (JackalSidekickButtonSprite) return JackalSidekickButtonSprite;
            JackalSidekickButtonSprite = Helpers.LoadSpriteFromTexture2D(JackalSidekickButton, 115f);
            return JackalSidekickButtonSprite;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate()
        {
            if (PlayerControl.LocalPlayer.IsRole(RoleId.Jackal))
            {
                foreach (var pc in PlayerControl.AllPlayerControls) if (pc.IsTeamJackal()) BlockTarget.Add(pc);

                CurrentTarget = SetTarget(untargetablePlayers: BlockTarget);
                SetPlayerOutline(CurrentTarget, JackalBlue);
            }
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public override void Clear()
        {
            players = new List<Jackal>();

            CurrentTarget = null;
        }
    }

    public class Sidekick : RoleBase<Sidekick>
    {
        public override string RoleName() { return "Sidekick"; }
        public override Color RoleColor() { return JackalBlue; }

        public Sidekick()
        {
            RoleId = roleId = RoleId.Sidekick;
        }

        public static PlayerControl CurrentTarget;

        public static float Cooldown { get { return CustomRolesH.JackalKillCooldown.getFloat(); } }
        public static bool CanUseVents { get { return CustomRolesH.SidekickCanUseVents.getBool(); } }
        public static bool CanKill { get { return CustomRolesH.SidekickCanKill.getBool(); } }
        public static bool PromotesToJackal { get { return CustomRolesH.SidekickPromotesToJackal.getBool(); } }
        public static bool HasImpostorVision { get { return CustomRolesH.JackalAndSidekickHaveImpostorVision.getBool(); } }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate()
        {
            if (PlayerControl.LocalPlayer.IsRole(RoleId.Sidekick))
            {
                var BlockTarget = new List<PlayerControl>();
                if (Jackal.players != null) foreach (var jackal in Jackal.allPlayers) BlockTarget.Add(jackal);

                Sidekick.CurrentTarget = SetTarget(untargetablePlayers: BlockTarget);
                if (Sidekick.CanKill) SetPlayerOutline(Sidekick.CurrentTarget, ImpostorRed);

                if (Sidekick.PromotesToJackal &&
                    PlayerControl.LocalPlayer.IsRole(RoleId.Sidekick) &&
                    PlayerControl.LocalPlayer.IsAlive() &&
                    !(Jackal.allPlayers.Count > 0))
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SidekickPromotes, Hazel.SendOption.Reliable, -1);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.SidekickPromotes(PlayerControl.LocalPlayer.PlayerId);
                }
            }
        }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public override void Clear()
        {
            players = new List<Sidekick>();

            CurrentTarget = null;
        }
    }

    public class Jester : RoleBase<Jester>
    {
        public override string RoleName() { return "Jester"; }
        public override Color RoleColor() { return JesterPink; }

        public Jester()
        {
            RoleId = roleId = RoleId.Jester;
        }

        public static bool CanCallEmergency { get { return CustomRolesH.JesterCanEmergencyMeeting.getBool(); } }
        public static bool CanUseVents { get { return CustomRolesH.JesterCanUseVents.getBool(); } }
        public static bool CanMoveInVents { get { return CustomRolesH.JesterCanMoveInVents.getBool(); } }
        public static bool CanSabotage { get { return CustomRolesH.JesterCanSabotage.getBool(); } }
        public static bool HasImpostorVision { get { return CustomRolesH.JesterHasImpostorVision.getBool(); } }
        public static bool HasTasks { get { return CustomRolesH.JesterMustFinishTasks.getBool(); } }
        public static int NumCommonTasks { get { return CustomRolesH.JesterTasks.CommonTasks; } }
        public static int NumLongTasks { get { return CustomRolesH.JesterTasks.LongTasks; } }
        public static int NumShortTasks { get { return CustomRolesH.JesterTasks.ShortTasks; } }

        public void AssignTasks()
        {
            player.GenerateAndAssignTasks(NumCommonTasks, NumShortTasks, NumLongTasks);
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
        class BeginCrewmatePatch
        {
            public static void Postfix(ShipStatus __instance)
            {
                if (HasTasks)
                {
                    local.AssignTasks();
                }
            }
        }

        public static bool TasksComplete(PlayerControl p)
        {
            int FinishedTasks = 0;
            int TasksCount = NumCommonTasks + NumLongTasks + NumShortTasks;
            if (TasksCount == 0) return true;
            foreach (var task in p.Data.Tasks)
            {
                if (task.Complete)
                {
                    FinishedTasks++;
                }
            }
            return FinishedTasks >= TasksCount;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public override void Clear()
        {
            players = new List<Jester>();
        }
    }
}