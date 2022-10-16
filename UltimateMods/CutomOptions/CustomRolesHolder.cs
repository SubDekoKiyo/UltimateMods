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
        // public static CustomOption EngineerVentCooldown;

        public static CustomRoleOption CustomImpostorRate;
        public static CustomOption CustomImpostorKillCooldown;
        public static CustomOption CustomImpostorCanUseVents;
        public static CustomOption CustomImpostorCanSabotage;

        public static CustomRoleOption UnderTakerRate;
        public static CustomOption UnderTakerKillCooldown;
        public static CustomOption UnderTakerButtonCooldown;
        public static CustomOption UnderTakerHasDuration;
        public static CustomOption UnderTakerDuration;
        public static CustomOption UnderTakerDraggingSpeed;
        public static CustomOption UnderTakerCanDumpBodyVents;

        public static CustomRoleOption BountyHunterRate;
        public static CustomOption BountyHunterSuccessKillCooldown;
        public static CustomOption BountyHunterAdditionalKillCooldown;
        public static CustomOption BountyHunterDuration;
        public static CustomOption BountyHunterShowArrow;
        public static CustomOption BountyHunterArrowUpdateCooldown;

        /* Modifiers */
        public static CustomRoleOption OpportunistRate;

        internal static Dictionary<byte, byte[]> blockedRolePairings = new();

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
            JesterRate = new CustomRoleOption(100, Neutral, ClearWhite, "Jester", JesterPink, 1);
            JesterCanEmergencyMeeting = CustomOption.Create(101, Neutral, JesterPink, "CanEmergencyMeeting", false, JesterRate);
            JesterCanUseVents = CustomOption.Create(102, Neutral, JesterPink, "CanUseVents", false, JesterRate);
            JesterCanSabotage = CustomOption.Create(103, Neutral, JesterPink, "CanSabotage", false, JesterRate);
            JesterHasImpostorVision = CustomOption.Create(104, Neutral, JesterPink, "HasImpostorVision", false, JesterRate);
            JesterMustFinishTasks = CustomOption.Create(105, Neutral, JesterPink, "JesterMustFinishTasks", false, JesterRate);
            JesterTasks = new CustomTasksOption(106, Neutral, JesterPink, 1, 1, 3, JesterMustFinishTasks);

            SheriffRate = new CustomRoleOption(110, Crewmate, ClearWhite, "Sheriff", SheriffYellow, 15);
            SheriffMaxShots = CustomOption.Create(111, Crewmate, SheriffYellow, "MaxShots", 2f, 1f, 15f, 1f, SheriffRate, format: "FormatShots");
            SheriffCooldowns = CustomOption.Create(112, Crewmate, SheriffYellow, "KillCooldowns", 30f, 2.5f, 60f, 2.5f, SheriffRate, format: "FormatSeconds");
            SheriffCanKillNeutral = CustomOption.Create(113, Crewmate, SheriffYellow, "CanKillNeutral", true, SheriffRate);
            SheriffMisfireKillsTarget = CustomOption.Create(114, Crewmate, SheriffYellow, "MisfireKillsTarget", false, SheriffRate);

            EngineerRate = new CustomRoleOption(120, Crewmate, ClearWhite, "Engineer", EngineerBlue, 15);
            EngineerCanFixSabo = CustomOption.Create(121, Crewmate, EngineerBlue, "EngineerCanFixSabo", true, EngineerRate);
            EngineerMaxFixCount = CustomOption.Create(122, Crewmate, EngineerBlue, "EngineerSaboFixCount", 2f, 1f, 15f, 1f, EngineerCanFixSabo, format: "FormatTimes");
            EngineerCanUseVents = CustomOption.Create(123, Crewmate, EngineerBlue, "CanUseVents", true, EngineerRate);
            // EngineerVentCooldown = CustomOption.Create(124, Crewmate, "EngineerVentCooldown", 20f, 0f, 60f, 2.5f, EngineerCanUseVents, format: "FormatSeconds");

            CustomImpostorRate = new CustomRoleOption(130, Impostor, ClearWhite, "CustomImpostor", ImpostorRed, 15);
            CustomImpostorKillCooldown = CustomOption.Create(131, Impostor, ClearWhite, "KillCooldowns", 30f, 2.5f, 60f, 2.5f, CustomImpostorRate, format: "FormatSeconds");
            CustomImpostorCanUseVents = CustomOption.Create(132, Impostor, ClearWhite, "CanUseVents", true, CustomImpostorRate);
            CustomImpostorCanSabotage = CustomOption.Create(133, Impostor, ClearWhite, "CanSabotage", true, CustomImpostorRate);

            UnderTakerRate = new CustomRoleOption(140, Impostor, ClearWhite, "UnderTaker", ImpostorRed, 1);
            UnderTakerKillCooldown = CustomOption.Create(141, Impostor, ClearWhite, "KillCooldowns", 35f, 5f, 60f, 2.5f, UnderTakerRate, format: "FormatSeconds");
            UnderTakerButtonCooldown = CustomOption.Create(142, Impostor, ClearWhite, "UnderTakerButtonCooldown", 30f, 2.5f, 60f, 2.5f, UnderTakerRate, format: "FormatSeconds");
            UnderTakerHasDuration = CustomOption.Create(143, Impostor, ClearWhite, "UnderTakerHasDuration", true, UnderTakerRate);
            UnderTakerDuration = CustomOption.Create(144, Impostor, ClearWhite, "UnderTakerDuration", 15f, 2.5f, 30f, 2.5f, UnderTakerHasDuration, format: "FormatSeconds");
            UnderTakerDraggingSpeed = CustomOption.Create(145, Impostor, ClearWhite, "UnderTakerDraggingSpeed", 80f, 75f, 100f, 2.5f, UnderTakerRate, format: "FormatPercent");
            UnderTakerCanDumpBodyVents = CustomOption.Create(146, Impostor, ClearWhite, "UnderTakerCanDumpDeadBodyInVent", false, UnderTakerRate);

            BountyHunterRate = new CustomRoleOption(150, Impostor, ClearWhite, "BountyHunter", ImpostorRed, 1);
            BountyHunterSuccessKillCooldown = CustomOption.Create(151, Impostor, ClearWhite, "BountyHunterSuccess", 5f, 2.5f, 30f, 2.5f, BountyHunterRate, format: "FormatSeconds");
            BountyHunterAdditionalKillCooldown = CustomOption.Create(152, Impostor, ClearWhite, "BountyHunterMiss", 20f, 5f, 45f, 2.5f, BountyHunterRate, format: "FormatSeconds");
            BountyHunterDuration = CustomOption.Create(153, Impostor, ClearWhite, "BountyHunterDuration", 30f, 2.5f, 60f, 2.5f, BountyHunterRate, format: "FormatSeconds");
            BountyHunterShowArrow = CustomOption.Create(154, Impostor, ClearWhite, "BountyHunterShowArrow", true, BountyHunterRate);
            BountyHunterArrowUpdateCooldown = CustomOption.Create(155, Impostor, ClearWhite, "BountyHunterArrowUpdateCooldown", 15f, 2.5f, 60f, 2.5f, BountyHunterShowArrow, format: "FormatSeconds");

            /* Modifiers */
            OpportunistRate = new CustomRoleOption(2000, Modifier, ClearWhite, "Opportunist", OpportunistGreen, 15);
        }
    }
}