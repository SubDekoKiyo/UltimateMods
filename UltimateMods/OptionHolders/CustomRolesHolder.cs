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

        public static CustomRoleOption BakeryRate;
        public static CustomOption BakeryBombRate;

        public static CustomRoleOption TeleporterRate;
        public static CustomOption TeleporterButtonCooldown;
        public static CustomOption TeleporterTeleportTo;

        public static CustomRoleOption AltruistRate;
        public static CustomOption AltruistDuration;

        public static CustomRoleOption EvilHackerRate;
        public static CustomOption EvilHackerCanMoveEvenIfUsesAdmin;
        public static CustomOption EvilHackerCanHasBetterAdmin;

        public static CustomRoleOption AdversityRate;
        public static CustomOption AdversityAdversityStateKillCooldown;
        public static CustomOption AdversityAdversityStateCanFindMadmate;
        public static CustomOption AdversityAdversityStateCanSeeVotes;

        public static CustomRoleOption SnitchRate;
        public static CustomOption SnitchLeftTasksForReveal;
        public static CustomOption SnitchIncludeTeamJackal;
        public static CustomOption SnitchTeamJackalUseDifferentArrowColor;

        public static CustomRoleOption JackalRate;
        public static CustomOption JackalKillCooldown;
        public static CustomOption JackalCreateSidekickCooldown;
        public static CustomOption JackalCanUseVents;
        public static CustomOption JackalCanCreateSidekick;
        public static CustomOption SidekickPromotesToJackal;
        public static CustomOption SidekickCanKill;
        public static CustomOption SidekickCanUseVents;
        public static CustomOption JackalPromotedFromSidekickCanCreateSidekick;
        public static CustomOption JackalAndSidekickHaveImpostorVision;

        public static CustomRoleOption SeerRate;
        public static CustomOption SeerMode;
        public static CustomOption SeerSoulDuration;
        public static CustomOption SeerLimitSoulDuration;

        public static CustomRoleOption ArsonistRate;
        public static CustomOption ArsonistCooldown;
        public static CustomOption ArsonistDuration;

        /* Modifiers */
        public static CustomRoleOption OpportunistRate;

        internal static Dictionary<byte, byte[]> BlockedRolePairings = new();

        public static void Load()
        {
            /* Roles */
            JesterRate = new(100, Neutral, White, "Jester", JesterPink, 1);
            JesterCanEmergencyMeeting = Create(101, Neutral, JesterPink, "CanEmergencyMeeting", false, JesterRate);
            JesterCanUseVents = Create(102, Neutral, JesterPink, "CanUseVents", false, JesterRate);
            JesterCanMoveInVents = Create(103, Neutral, JesterPink, "CanMoveInVents", false, JesterCanUseVents);
            JesterCanSabotage = Create(104, Neutral, JesterPink, "CanSabotage", false, JesterRate);
            JesterHasImpostorVision = Create(105, Neutral, JesterPink, "HasImpostorVision", false, JesterRate);
            JesterMustFinishTasks = Create(106, Neutral, JesterPink, "JesterMustFinishTasks", false, JesterRate);
            JesterTasks = new CustomTasksOption(107, Neutral, JesterPink, 1, 1, 3, JesterMustFinishTasks);

            SheriffRate = new(110, Crewmate, White, "Sheriff", SheriffYellow, 15);
            SheriffMaxShots = Create(111, Crewmate, SheriffYellow, "MaxShots", 2f, 1f, 15f, 1f, SheriffRate, format: "FormatShots");
            SheriffCooldowns = Create(112, Crewmate, SheriffYellow, "KillCooldowns", 30f, 2.5f, 60f, 2.5f, SheriffRate, format: "FormatSeconds");
            SheriffCanKillNeutral = Create(113, Crewmate, SheriffYellow, "CanKillNeutral", true, SheriffRate);
            SheriffMisfireKillsTarget = Create(114, Crewmate, SheriffYellow, "MisfireKillsTarget", false, SheriffRate);

            EngineerRate = new(120, Crewmate, White, "Engineer", EngineerBlue, 15);
            EngineerCanFixSabo = Create(121, Crewmate, EngineerBlue, "EngineerCanFixSabo", true, EngineerRate);
            EngineerMaxFixCount = Create(122, Crewmate, EngineerBlue, "EngineerSaboFixCount", 2f, 1f, 15f, 1f, EngineerCanFixSabo, format: "FormatTimes");
            EngineerCanUseVents = Create(123, Crewmate, EngineerBlue, "CanUseVents", true, EngineerRate);
            // EngineerVentCooldown = Create(124, Crewmate, "EngineerVentCooldown", 20f, 0f, 60f, 2.5f, EngineerCanUseVents, format: "FormatSeconds");

            CustomImpostorRate = new(130, Impostor, White, "CustomImpostor", ImpostorRed, 15);
            CustomImpostorKillCooldown = Create(131, Impostor, ImpostorRed, "KillCooldowns", 30f, 2.5f, 60f, 2.5f, CustomImpostorRate, format: "FormatSeconds");
            CustomImpostorCanUseVents = Create(132, Impostor, ImpostorRed, "CanUseVents", true, CustomImpostorRate);
            CustomImpostorCanSabotage = Create(133, Impostor, ImpostorRed, "CanSabotage", true, CustomImpostorRate);

            UnderTakerRate = new(140, Impostor, White, "UnderTaker", ImpostorRed, 1);
            UnderTakerKillCooldown = Create(141, Impostor, ImpostorRed, "KillCooldowns", 35f, 5f, 60f, 2.5f, UnderTakerRate, format: "FormatSeconds");
            UnderTakerButtonCooldown = Create(142, Impostor, ImpostorRed, "UnderTakerButtonCooldown", 30f, 2.5f, 60f, 2.5f, UnderTakerRate, format: "FormatSeconds");
            UnderTakerHasDuration = Create(143, Impostor, ImpostorRed, "UnderTakerHasDuration", true, UnderTakerRate);
            UnderTakerDuration = Create(144, Impostor, ImpostorRed, "UnderTakerDuration", 15f, 2.5f, 30f, 2.5f, UnderTakerHasDuration, format: "FormatSeconds");
            UnderTakerDraggingSpeed = Create(145, Impostor, ImpostorRed, "UnderTakerDraggingSpeed", 80f, 75f, 100f, 2.5f, UnderTakerRate, format: "FormatPercent");
            UnderTakerCanDumpBodyVents = Create(146, Impostor, ImpostorRed, "UnderTakerCanDumpDeadBodyInVent", false, UnderTakerRate);

            BountyHunterRate = new(150, Impostor, White, "BountyHunter", ImpostorRed, 1);
            BountyHunterSuccessKillCooldown = Create(151, Impostor, ImpostorRed, "BountyHunterSuccess", 5f, 2.5f, 30f, 2.5f, BountyHunterRate, format: "FormatSeconds");
            BountyHunterAdditionalKillCooldown = Create(152, Impostor, ImpostorRed, "BountyHunterMiss", 20f, 5f, 45f, 2.5f, BountyHunterRate, format: "FormatSeconds");
            BountyHunterDuration = Create(153, Impostor, ImpostorRed, "BountyHunterDuration", 30f, 2.5f, 60f, 2.5f, BountyHunterRate, format: "FormatSeconds");
            BountyHunterShowArrow = Create(154, Impostor, ImpostorRed, "BountyHunterShowArrow", true, BountyHunterRate);
            BountyHunterArrowUpdateCooldown = Create(155, Impostor, ImpostorRed, "BountyHunterArrowUpdateCooldown", 15f, 2.5f, 60f, 2.5f, BountyHunterShowArrow, format: "FormatSeconds");

            MadmateRate = new(160, Crewmate, White, "Madmate", ImpostorRed, 15);
            MadmateCanDieToSheriff = Create(161, Crewmate, ImpostorRed, "CanDieToSheriff", true, MadmateRate);
            MadmateCanEnterVents = Create(162, Crewmate, ImpostorRed, "CanUseVents", true, MadmateRate);
            MadmateCanMoveInVents = Create(163, Crewmate, ImpostorRed, "CanMoveInVents", false, MadmateCanEnterVents);
            MadmateCanSabotage = Create(164, Crewmate, ImpostorRed, "CanSabotage", false, MadmateRate);
            MadmateHasImpostorVision = Create(165, Crewmate, ImpostorRed, "HasImpostorVision", true, MadmateRate);
            MadmateCanFixO2 = Create(166, Crewmate, ImpostorRed, "CanFixO2", false, MadmateRate);
            MadmateCanFixComms = Create(167, Crewmate, ImpostorRed, "CanFixComms", false, MadmateRate);
            MadmateCanFixReactor = Create(168, Crewmate, ImpostorRed, "CanFixReactor", true, MadmateRate);
            MadmateCanFixBlackout = Create(169, Crewmate, ImpostorRed, "CanFixBlackout", true, MadmateRate);
            MadmateHasTasks = Create(170, Crewmate, ImpostorRed, "HasTasks", true, MadmateRate);
            MadmateTasksCount = new CustomTasksOption(171, Crewmate, ImpostorRed, 1, 2, 3, MadmateHasTasks);
            MadmateCanKnowImpostorWhenTasksEnded = Create(172, Crewmate, ImpostorRed, "MadmateKnowImpostorTaskEnd", true, MadmateHasTasks);
            MadmateCanWinWhenTaskEnded = Create(173, Crewmate, ImpostorRed, "MadmateCanWinWhenTaskEnd", true, MadmateHasTasks);

            BakeryRate = new(175, Crewmate, White, "Bakery", BakeryYellow, 1);
            BakeryBombRate = Create(176, Crewmate, BakeryYellow, "BakeryBombRate", 10f, 0f, 100f, 5f, BakeryRate, format: "FormatPercent");

            TeleporterRate = new(180, Impostor, White, "Teleporter", ImpostorRed, 15);
            TeleporterButtonCooldown = Create(181, Impostor, ImpostorRed, "TeleporterButtonCooldown", 40f, 10f, 80f, 2.5f, TeleporterRate, format: "FormatSeconds");
            TeleporterTeleportTo = Create(182, Impostor, ImpostorRed, "TeleporterTeleportTo", new string[] { "TeleporterAllRandom", "OnlyCrewmate" }, TeleporterRate);

            AltruistRate = new(185, Crewmate, White, "Altruist", ImpostorRed, 1);
            AltruistDuration = Create(186, Crewmate, AltruistRed, "AltruistDuration", 7.5f, 2.5f, 20f, 2.5f, AltruistRate, format: "FormatSeconds");

            EvilHackerRate = new(190, Impostor, White, "EvilHacker", ImpostorRed, 1);
            EvilHackerCanMoveEvenIfUsesAdmin = Create(191, Impostor, ImpostorRed, "EvilHackerCanMoveEvenIfUsesAdmin", false, EvilHackerRate);
            EvilHackerCanHasBetterAdmin = Create(192, Impostor, ImpostorRed, "EvilHackerCanHasBetterAdmin", false, EvilHackerRate);

            AdversityRate = new(195, Impostor, White, "Adversity", ImpostorRed, 1);
            AdversityAdversityStateKillCooldown = Create(196, Impostor, ImpostorRed, "AdversityStateKillCooldown", 20f, 2.5f, 60f, 2.5f, AdversityRate, format: "FormatSeconds");
            AdversityAdversityStateCanFindMadmate = Create(197, Impostor, ImpostorRed, "AdversityStateCanFindMadmate", false, AdversityRate);
            AdversityAdversityStateCanSeeVotes = Create(198, Impostor, ImpostorRed, "AdversityStateCanSeeVotes", false, AdversityRate);

            SnitchRate = new(200, Crewmate, White, "Snitch", SnitchGreen, 15);
            SnitchLeftTasksForReveal = Create(201, Crewmate, SnitchGreen, "SnitchLeftTasksForReveal", 1f, 0f, 5f, 1f, SnitchRate);
            SnitchIncludeTeamJackal = Create(202, Crewmate, SnitchGreen, "SnitchIncludeTeamJackal", false, SnitchRate);

            JackalRate = new(210, Neutral, White, "Jackal", JackalBlue, 1);
            JackalKillCooldown = Create(211, Neutral, JackalBlue, "KillCooldowns", 30f, 2.5f, 60f, 2.5f, JackalRate, format: "FormatSeconds");
            JackalCanUseVents = Create(212, Neutral, JackalBlue, "JackalCanUseVents", true, JackalRate);
            JackalAndSidekickHaveImpostorVision = Create(213, Neutral, JackalBlue, "JackalAndSidekickHaveImpostorVision", false, JackalRate);
            JackalCanCreateSidekick = Create(214, Neutral, JackalBlue, "JackalCanCreateSidekick", false, JackalRate);
            JackalCreateSidekickCooldown = Create(215, Neutral, JackalBlue, "JackalCreateSidekickCooldown", 30f, 2.5f, 60f, 2.5f, JackalCanCreateSidekick, format: "FormatSeconds");
            SidekickPromotesToJackal = Create(216, Neutral, JackalBlue, "SidekickPromotesToJackal", true, JackalCanCreateSidekick);
            SidekickCanKill = Create(217, Neutral, JackalBlue, "SidekickCanKill", false, JackalCanCreateSidekick);
            SidekickCanUseVents = Create(218, Neutral, JackalBlue, "SidekickCanUseVents", true, JackalCanCreateSidekick);
            JackalPromotedFromSidekickCanCreateSidekick = Create(219, Neutral, JackalBlue, "JackalPromotedFromSidekickCanCreateSidekick", true, JackalCanCreateSidekick);

            SeerRate = new(230, Crewmate, White, "Seer", SeerGreen, 15);
            SeerMode = Create(231, Crewmate, SeerGreen, "SeerMode", new string[] { "SeerModeBoth", "SeerModeFlash", "SeerModeSouls" }, SeerRate);
            SeerLimitSoulDuration = Create(232, Crewmate, SeerGreen, "SeerLimitSoulDuration", false, SeerRate);
            SeerSoulDuration = Create(233, Crewmate, SeerGreen, "SeerSoulDuration", 15f, 0f, 120f, 5f, SeerLimitSoulDuration, format: "FormatSeconds");

            ArsonistRate = new(240, Neutral, White, "Arsonist", ArsonistOrange, 1);
            ArsonistCooldown = Create(241, Neutral, ArsonistOrange, "ArsonistCooldown", 12.5f, 2.5f, 60f, 2.5f, ArsonistRate, format: "FormatSeconds");
            ArsonistDuration = Create(242, Neutral, ArsonistOrange, "ArsonistDuration", 3f, 0f, 10f, 1f, ArsonistRate, format: "FormatSeconds");

            /* Modifiers */
            OpportunistRate = new(2000, Modifier, White, "Opportunist", OpportunistGreen, 15);
        }
    }
}