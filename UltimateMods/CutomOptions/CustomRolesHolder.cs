using System.Collections.Generic;
using UnityEngine;
using UltimateMods.Modules;
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
        public static CustomOption JesterCanMoveInVents;
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

        public static CustomRoleOption MadmateRate;
        public static CustomOption MadmateCanDieToSheriff;
        public static CustomOption MadmateCanEnterVents;
        public static CustomOption MadmateCanMoveInVents;
        public static CustomOption MadmateCanSabotage;
        public static CustomOption MadmateHasImpostorVision;
        public static CustomOption MadmateCanFixO2;
        public static CustomOption MadmateCanFixComms;
        public static CustomOption MadmateCanFixReactor;
        public static CustomOption MadmateCanFixBlackout;
        public static CustomOption MadmateHasTasks;
        public static CustomTasksOption MadmateTasksCount;
        public static CustomOption MadmateCanKnowImpostorWhenTasksEnded;
        public static CustomOption MadmateCanWinWhenTaskEnded;

        internal static Dictionary<byte, byte[]> BlockedRolePairings = new();

        private static byte ToByte(float f)
        {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }

        public static void Load()
        {
            /* Roles */
            JesterRate = new CustomRoleOption(100, Neutral, White, "Jester", JesterPink, 1);
            JesterCanEmergencyMeeting = CustomOption.Create(101, Neutral, JesterPink, "CanEmergencyMeeting", false, JesterRate);
            JesterCanUseVents = CustomOption.Create(102, Neutral, JesterPink, "CanUseVents", false, JesterRate);
            JesterCanMoveInVents = CustomOption.Create(103, Neutral, JesterPink, "CanMoveInVents", false, JesterCanUseVents);
            JesterCanSabotage = CustomOption.Create(104, Neutral, JesterPink, "CanSabotage", false, JesterRate);
            JesterHasImpostorVision = CustomOption.Create(105, Neutral, JesterPink, "HasImpostorVision", false, JesterRate);
            JesterMustFinishTasks = CustomOption.Create(106, Neutral, JesterPink, "JesterMustFinishTasks", false, JesterRate);
            JesterTasks = new CustomTasksOption(107, Neutral, JesterPink, 1, 1, 3, JesterMustFinishTasks);

            SheriffRate = new CustomRoleOption(110, Crewmate, White, "Sheriff", SheriffYellow, 15);
            SheriffMaxShots = CustomOption.Create(111, Crewmate, SheriffYellow, "MaxShots", 2f, 1f, 15f, 1f, SheriffRate, format: "FormatShots");
            SheriffCooldowns = CustomOption.Create(112, Crewmate, SheriffYellow, "KillCooldowns", 30f, 2.5f, 60f, 2.5f, SheriffRate, format: "FormatSeconds");
            SheriffCanKillNeutral = CustomOption.Create(113, Crewmate, SheriffYellow, "CanKillNeutral", true, SheriffRate);
            SheriffMisfireKillsTarget = CustomOption.Create(114, Crewmate, SheriffYellow, "MisfireKillsTarget", false, SheriffRate);

            EngineerRate = new CustomRoleOption(120, Crewmate, White, "Engineer", EngineerBlue, 15);
            EngineerCanFixSabo = CustomOption.Create(121, Crewmate, EngineerBlue, "EngineerCanFixSabo", true, EngineerRate);
            EngineerMaxFixCount = CustomOption.Create(122, Crewmate, EngineerBlue, "EngineerSaboFixCount", 2f, 1f, 15f, 1f, EngineerCanFixSabo, format: "FormatTimes");
            EngineerCanUseVents = CustomOption.Create(123, Crewmate, EngineerBlue, "CanUseVents", true, EngineerRate);
            // EngineerVentCooldown = CustomOption.Create(124, Crewmate, "EngineerVentCooldown", 20f, 0f, 60f, 2.5f, EngineerCanUseVents, format: "FormatSeconds");

            CustomImpostorRate = new CustomRoleOption(130, Impostor, White, "CustomImpostor", ImpostorRed, 15);
            CustomImpostorKillCooldown = CustomOption.Create(131, Impostor, ImpostorRed, "KillCooldowns", 30f, 2.5f, 60f, 2.5f, CustomImpostorRate, format: "FormatSeconds");
            CustomImpostorCanUseVents = CustomOption.Create(132, Impostor, ImpostorRed, "CanUseVents", true, CustomImpostorRate);
            CustomImpostorCanSabotage = CustomOption.Create(133, Impostor, ImpostorRed, "CanSabotage", true, CustomImpostorRate);

            UnderTakerRate = new CustomRoleOption(140, Impostor, White, "UnderTaker", ImpostorRed, 1);
            UnderTakerKillCooldown = CustomOption.Create(141, Impostor, ImpostorRed, "KillCooldowns", 35f, 5f, 60f, 2.5f, UnderTakerRate, format: "FormatSeconds");
            UnderTakerButtonCooldown = CustomOption.Create(142, Impostor, ImpostorRed, "UnderTakerButtonCooldown", 30f, 2.5f, 60f, 2.5f, UnderTakerRate, format: "FormatSeconds");
            UnderTakerHasDuration = CustomOption.Create(143, Impostor, ImpostorRed, "UnderTakerHasDuration", true, UnderTakerRate);
            UnderTakerDuration = CustomOption.Create(144, Impostor, ImpostorRed, "UnderTakerDuration", 15f, 2.5f, 30f, 2.5f, UnderTakerHasDuration, format: "FormatSeconds");
            UnderTakerDraggingSpeed = CustomOption.Create(145, Impostor, ImpostorRed, "UnderTakerDraggingSpeed", 80f, 75f, 100f, 2.5f, UnderTakerRate, format: "FormatPercent");
            UnderTakerCanDumpBodyVents = CustomOption.Create(146, Impostor, ImpostorRed, "UnderTakerCanDumpDeadBodyInVent", false, UnderTakerRate);

            BountyHunterRate = new CustomRoleOption(150, Impostor, White, "BountyHunter", ImpostorRed, 1);
            BountyHunterSuccessKillCooldown = CustomOption.Create(151, Impostor, ImpostorRed, "BountyHunterSuccess", 5f, 2.5f, 30f, 2.5f, BountyHunterRate, format: "FormatSeconds");
            BountyHunterAdditionalKillCooldown = CustomOption.Create(152, Impostor, ImpostorRed, "BountyHunterMiss", 20f, 5f, 45f, 2.5f, BountyHunterRate, format: "FormatSeconds");
            BountyHunterDuration = CustomOption.Create(153, Impostor, ImpostorRed, "BountyHunterDuration", 30f, 2.5f, 60f, 2.5f, BountyHunterRate, format: "FormatSeconds");
            BountyHunterShowArrow = CustomOption.Create(154, Impostor, ImpostorRed, "BountyHunterShowArrow", true, BountyHunterRate);
            BountyHunterArrowUpdateCooldown = CustomOption.Create(155, Impostor, ImpostorRed, "BountyHunterArrowUpdateCooldown", 15f, 2.5f, 60f, 2.5f, BountyHunterShowArrow, format: "FormatSeconds");

            /* Modifiers */
            OpportunistRate = new CustomRoleOption(2000, Modifier, White, "Opportunist", OpportunistGreen, 15);

            MadmateRate = new CustomRoleOption(2010, Modifier, White, "Madmate", ImpostorRed, 15);
            MadmateCanDieToSheriff = CustomOption.Create(2011, Modifier, ImpostorRed, "CanDieToSheriff", true, MadmateRate);
            MadmateCanEnterVents = CustomOption.Create(2012, Modifier, ImpostorRed, "CanUseVents", true, MadmateRate);
            MadmateCanMoveInVents = CustomOption.Create(2013, Modifier, ImpostorRed, "CanMoveInVents", false, MadmateCanEnterVents);
            MadmateCanSabotage = CustomOption.Create(2014, Modifier, ImpostorRed, "CanSabotage", false, MadmateRate);
            MadmateHasImpostorVision = CustomOption.Create(2015, Modifier, ImpostorRed, "HasImpostorVision", true, MadmateRate);
            MadmateCanFixO2 = CustomOption.Create(2016, Modifier, ImpostorRed, "CanFixO2", false, MadmateRate);
            MadmateCanFixComms = CustomOption.Create(2017, Modifier, ImpostorRed, "CanFixComms", false, MadmateRate);
            MadmateCanFixReactor = CustomOption.Create(2018, Modifier, ImpostorRed, "CanFixReactor", true, MadmateRate);
            MadmateCanFixBlackout = CustomOption.Create(2019, Modifier, ImpostorRed, "CanFixBlackout", true, MadmateRate);
            MadmateHasTasks = CustomOption.Create(2020, Modifier, ImpostorRed, "HasTasks", true, MadmateRate);
            MadmateTasksCount = new CustomTasksOption(2021, Modifier, ImpostorRed, 1, 2, 3, MadmateHasTasks);
            MadmateCanKnowImpostorWhenTasksEnded = CustomOption.Create(2022, Modifier, ImpostorRed, "MadmateKnowImpostorTaskEnd", true, MadmateHasTasks);
            MadmateCanWinWhenTaskEnded = CustomOption.Create(2023, Modifier, ImpostorRed, "MadmateCanWinWhenTaskEnd", true, MadmateHasTasks);
        }
    }
}