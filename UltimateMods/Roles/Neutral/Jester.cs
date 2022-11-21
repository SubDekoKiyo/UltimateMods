using HarmonyLib;
using System.Collections.Generic;

namespace UltimateMods.Roles
{
    [HarmonyPatch]
    public class Jester : RoleBase<Jester>
    {
        public static bool TriggerJesterWin = false;
        public static bool CanCallEmergency { get { return CustomRolesH.JesterCanEmergencyMeeting.getBool(); } }
        public static bool CanUseVents { get { return CustomRolesH.JesterCanUseVents.getBool(); } }
        public static bool CanMoveInVents { get { return CustomRolesH.JesterCanMoveInVents.getBool(); } }
        public static bool CanSabotage { get { return CustomRolesH.JesterCanSabotage.getBool(); } }
        public static bool HasImpostorVision { get { return CustomRolesH.JesterHasImpostorVision.getBool(); } }
        public static bool HasTasks { get { return CustomRolesH.JesterMustFinishTasks.getBool(); } }
        public static int NumCommonTasks { get { return CustomRolesH.JesterTasks.CommonTasks; } }
        public static int NumLongTasks { get { return CustomRolesH.JesterTasks.LongTasks; } }
        public static int NumShortTasks { get { return CustomRolesH.JesterTasks.ShortTasks; } }

        public Jester()
        {
            RoleType = roleId = RoleType.Jester;
        }

        public void assignTasks()
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
                    local.assignTasks();
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

        public static void Clear()
        {
            TriggerJesterWin = false;
            players = new List<Jester>();
        }
    }
}