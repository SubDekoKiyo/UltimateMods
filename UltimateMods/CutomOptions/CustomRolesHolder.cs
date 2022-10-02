using System.Collections.Generic;
using UnityEngine;
using UltimateMods.Modules;
using UltimateMods.Roles;
using static UltimateMods.ColorDictionary;
using static UltimateMods.Modules.CustomOption.CustomOptionType;

namespace UltimateMods
{
    public class CustomRolesH
    {
        /* Roles */
        public static CustomRoleOption JesterRate;
        public static CustomOption JesterCanEmergencyMeeting;
        public static CustomOption JesterCanUseVents;
        public static CustomOption JesterCanSabotage;
        public static CustomOption JesterHasImpostorVision;
        public static CustomOption JesterMustFinishTasks;
        public static CustomTasksOption JesterTasks;

        public static CustomRoleOption SheriffRate;
        public static CustomOption SheriffMaxShots;
        public static CustomOption SheriffCooldowns;
        public static CustomOption SheriffCanKillNeutral;
        public static CustomOption SheriffMisfireKillsTarget;

        public static CustomRoleOption EngineerRate;
        public static CustomOption EngineerCanFixSabo;
        public static CustomOption EngineerMaxFixCount;
        public static CustomOption EngineerCanUseVents;
        public static CustomOption EngineerVentCooldown;

        /* Modifiers */
        public static CustomRoleOption OpportunistRate;

        internal static Dictionary<byte, byte[]> blockedRolePairings = new Dictionary<byte, byte[]>();

        public static string cs(Color c, string s)
        {
            return string.Format("<color=#{0:X2}{1:X2}{2:X2}{3:X2}>{4}</color>", ToByte(c.r), ToByte(c.g), ToByte(c.b), ToByte(c.a), s);
        }

        private static byte ToByte(float f)
        {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }

        public static void Load()
        {
            /* Roles */
            JesterRate = new CustomRoleOption(100, TypeNeutral, ClearWhite, "Jester", JesterPink, 1);
            JesterCanEmergencyMeeting = CustomOption.Create(101, TypeNeutral, JesterPink, "CanEmergencyMeeting", false, JesterRate);
            JesterCanUseVents = CustomOption.Create(102, TypeNeutral, JesterPink, "CanUseVents", false, JesterRate);
            JesterCanSabotage = CustomOption.Create(103, TypeNeutral, JesterPink, "CanSabotage", false, JesterRate);
            JesterHasImpostorVision = CustomOption.Create(104, TypeNeutral, JesterPink, "HasImpostorVision", false, JesterRate);
            JesterMustFinishTasks = CustomOption.Create(105, TypeNeutral, JesterPink, "JesterMustFinishTasks", false, JesterRate);
            JesterTasks = new CustomTasksOption(106, TypeNeutral, JesterPink, 1, 1, 3, JesterTasks);

            SheriffRate = new CustomRoleOption(110, TypeCrewmate, ClearWhite, "Sheriff", SheriffYellow, 15);
            SheriffMaxShots = CustomOption.Create(111, TypeCrewmate, SheriffYellow, "MaxShots", 2f, 1f, 15f, 1f, SheriffRate, format: "FormatShots");
            SheriffCooldowns = CustomOption.Create(112, TypeCrewmate, SheriffYellow, "KillCooldowns", 30f, 2.5f, 60f, 2.5f, SheriffRate, format: "FormatSeconds");
            SheriffCanKillNeutral = CustomOption.Create(113, TypeCrewmate, SheriffYellow, "CanKillNeutral", true, SheriffRate);
            SheriffMisfireKillsTarget = CustomOption.Create(114, TypeCrewmate, SheriffYellow, "MisfireKillsTarget", false, SheriffRate);

            EngineerRate = new CustomRoleOption(120, TypeCrewmate, ClearWhite, "Engineer", EngineerBlue, 15);
            EngineerCanFixSabo = CustomOption.Create(121, TypeCrewmate, EngineerBlue, "EngineerCanFixSabo", true, EngineerRate);
            EngineerMaxFixCount = CustomOption.Create(122, TypeCrewmate, EngineerBlue, "EngineerSaboFixCount", 2f, 1f, 15f, 1f, EngineerCanFixSabo, format: "FormatTimes");
            EngineerCanUseVents = CustomOption.Create(123, TypeCrewmate, EngineerBlue, "CanUseVents", true, EngineerRate);
            // EngineerVentCooldown = CustomOption.Create(124, TypeCrewmate, "EngineerVentCooldown", 20f, 0f, 60f, 2.5f, EngineerCanUseVents, format: "FormatSeconds");

            /* Modifiers */
            OpportunistRate = new CustomRoleOption(2000, TypeModifier, ClearWhite, "Opportunist", OpportunistGreen, 15);
        }
    }
}