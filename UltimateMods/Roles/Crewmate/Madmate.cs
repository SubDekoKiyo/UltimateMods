using HarmonyLib;
using System.Collections.Generic;

namespace UltimateMods.Roles
{
    [HarmonyPatch]
    public class Madmate : RoleBase<Madmate>
    {
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

        public Madmate()
        {
            RoleType = roleId = RoleType.Madmate;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
        class BeginCrewmatePatch
        {
            public static void Postfix(ShipStatus __instance)
            {
                if (HasTasks && isRole(PlayerControl.LocalPlayer))
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

        public static void Clear()
        {
            players = new List<Madmate>();
        }
    }
}