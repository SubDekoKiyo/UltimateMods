using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UltimateMods.Objects;
using UltimateMods.Utilities;
using UltimateMods.Modules;
using System;
using static UltimateMods.ColorDictionary;

namespace UltimateMods.Roles
{
    [HarmonyPatch]
    public class Snitch : RoleBase<Snitch>
    {
        public static List<CustomArrow> LocalArrows = new();
        public static int TaskCountForReveal { get { return Mathf.RoundToInt(CustomRolesH.SnitchLeftTasksForReveal.getFloat()); } }
        public static bool IncludeTeamJackal { get { return CustomRolesH.SnitchIncludeTeamJackal.getBool(); } }
        public static bool TeamJackalUseDifferentArrowColor { get { return CustomRolesH.SnitchTeamJackalUseDifferentArrowColor.getBool(); } }

        public Snitch()
        {
            RoleType = roleId = RoleType.Snitch;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate()
        {
            if (LocalArrows == null) return;

            foreach (CustomArrow arrow in LocalArrows) arrow.arrow.SetActive(false);

            foreach (var snitch in Snitch.allPlayers)
            {
                if (snitch == null) return;

                var (PlayerCompleted, PlayerTotal) = TasksHandler.taskInfo(snitch.Data);
                int NumberOfTasks = PlayerTotal - PlayerCompleted;

                if (NumberOfTasks <= TaskCountForReveal && (PlayerControl.LocalPlayer.Data.Role.IsImpostor || (IncludeTeamJackal && (PlayerControl.LocalPlayer.isRole(RoleType.Jackal) || PlayerControl.LocalPlayer.isRole(RoleType.Sidekick)))))
                {
                    if (LocalArrows.Count == 0) LocalArrows.Add(new(Color.blue));
                    if (LocalArrows.Count != 0 && LocalArrows[0] != null)
                    {
                        LocalArrows[0].arrow.SetActive(true);
                        LocalArrows[0].image.color = Color.blue;
                        LocalArrows[0].Update(snitch.transform.position);
                    }
                }
                else if (PlayerControl.LocalPlayer.isRole(RoleType.Snitch) && NumberOfTasks == 0)
                {
                    int arrowIndex = 0;
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls.GetFastEnumerator())
                    {
                        bool ArrowForImpostor = p.Data.Role.IsImpostor;
                        bool ArrowForTeamJackal = IncludeTeamJackal && (p.isRole(RoleType.Jackal) || p.isRole(RoleType.Sidekick));

                        // Update the arrows' color every time bc things go weird when you add a sidekick or someone dies
                        Color c = Palette.ImpostorRed;
                        if (ArrowForTeamJackal)
                        {
                            c = JackalBlue;
                        }
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
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void Clear()
        {
            if (LocalArrows != null)
            {
                foreach (CustomArrow Arrow in LocalArrows)
                    if (Arrow?.arrow != null)
                        UnityEngine.Object.Destroy(Arrow.arrow);
            }
            players = new List<Snitch>();
        }
    }
}