using System.Collections.Generic;
using UnityEngine;
using UltimateMods.Modules;
using UltimateMods.Roles;
using UltimateMods.Roles.Yakuza;
using static UltimateMods.Roles.NeutralRoles;
using static UltimateMods.Roles.CrewmateRoles;
using static UltimateMods.Modules.CustomOption.CustomOptionType;

namespace UltimateMods
{
    public class CustomRolesH
    {
        public static CustomRoleOption JesterRate;
        public static CustomOption JesterCanEmergencyMeeting;
        public static CustomOption JesterCanUseVents;
        public static CustomOption JesterCanSabotage;
        public static CustomOption JesterHasImpostorVision;

        public static CustomRoleOption SheriffRate;
        public static CustomOption SheriffMaxShots;
        public static CustomOption SheriffCooldowns;
        public static CustomOption SheriffCanKillNeutral;
        public static CustomOption SheriffMisfireKillsTarget;

        public static CustomOption YakuzaRate;
        public static CustomOption YakuzaMaxShots;
        public static CustomOption YakuzaCooldowns;
        public static CustomOption YakuzaCanKillNeutral;
        public static CustomOption YakuzaMisfireKillsTarget;
        public static CustomOption YakuzaShareShotsCount;

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
            JesterRate = new CustomRoleOption(100, Neutral, "Jester", Jester.color, 1);
            JesterCanEmergencyMeeting = CustomOption.Create(101, Neutral, "JesterCanEmergencyMeeting", false, JesterRate);
            JesterCanUseVents = CustomOption.Create(102, Neutral, "JesterCanUseVents", false, JesterRate);
            JesterCanSabotage = CustomOption.Create(103, Neutral, "JesterCanSabotage", false, JesterRate);
            JesterHasImpostorVision = CustomOption.Create(104, Neutral, "JesterHasImpostorVision", false, JesterRate);

            SheriffRate = new CustomRoleOption(110, Crewmate, "Sheriff", Sheriff.color, 15);
            SheriffMaxShots = CustomOption.Create(111, Crewmate, "MaxShots", 2f, 1f, 15f, 1f, SheriffRate, format: "FormatShots");
            SheriffCooldowns = CustomOption.Create(112, Crewmate, "KillCooldowns", 30f, 2.5f, 60f, 2.5f, SheriffRate, format: "FormatSeconds");
            SheriffCanKillNeutral = CustomOption.Create(113, Crewmate, "CanKillNeutral", true, SheriffRate);
            SheriffMisfireKillsTarget = CustomOption.Create(114, Crewmate, "MisfireKillsTarget", false, SheriffRate);

            YakuzaRate = new CustomRoleOption(120, Crewmate, "Yakuza", YakuzaBoss.color, 1);
            YakuzaMaxShots = CustomOption.Create(121, Crewmate, "MaxShots", 2f, 1f, 15f, 1f, YakuzaRate, format: "FormatShots");
            YakuzaCooldowns = CustomOption.Create(122, Crewmate, "KillCooldowns", 30f, 2.5f, 60f, 2.5f, YakuzaRate, format: "FormatShots");
            YakuzaCanKillNeutral = CustomOption.Create(123, Crewmate, "CanKillNeutral", true, YakuzaRate);
            YakuzaMisfireKillsTarget = CustomOption.Create(124, Crewmate, "MisfireKillsTarget", false, YakuzaRate);
            YakuzaShareShotsCount = CustomOption.Create(125, Crewmate, "YakuzaShareShots", true, YakuzaRate);
        }
    }
}