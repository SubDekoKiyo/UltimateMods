using System.Linq;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateMods.Roles
{
    [HarmonyPatch]
    public static class NeutralRoles
    {
        public static class Jester
        {
            public static PlayerControl jester;
            public static Color color = new Color32(236, 98, 165, byte.MaxValue);

            public static bool TriggerJesterWin = false;
            public static bool CanCallEmergency = true;
            public static bool CanUseVents = true;
            public static bool CanSabotage = true;
            public static bool HasImpostorVision = false;

            public static void ClearAndReload()
            {
                jester = null;
                TriggerJesterWin = false;
                CanCallEmergency = CustomRolesH.JesterCanEmergencyMeeting.getBool();
                CanUseVents = CustomRolesH.JesterCanUseVents.getBool();
                CanSabotage = CustomRolesH.JesterCanSabotage.getBool();
                HasImpostorVision = CustomRolesH.JesterHasImpostorVision.getBool();
            }
        }
    }
}