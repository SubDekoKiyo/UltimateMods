using System.Collections.Generic;
using UnityEngine;
using UltimateMods.Modules;
using static UltimateMods.ColorDictionary;
using static UltimateMods.Modules.CustomOption;
using static UltimateMods.Modules.CustomOption.CustomOptionType;

namespace UltimateMods
{
    public class CustomOptionsH
    {
        public static string[] TenRates = new[] { "0%", "10%", "20%", "30%", "40%", "50%", "60%", "70%", "80%", "90%", "100%" };
        public static string[] HundredRates = new[] { "0%", "100%" };
        public static string[] Presets = new[] { "Preset1", "Preset2", "Preset3", "Preset4", "Preset5" };

        public static CustomOption PresetSelection;
        public static CustomOption ActivateModRoles;
        public static CustomOption CrewmateRolesCount;
        public static CustomOption ImpostorRolesCount;
        public static CustomOption NeutralRolesCount;
        public static CustomOption ModifierCount;
        // public static CustomOption RememberClassic;

        public static CustomOption SpecialOptions;
        public static CustomOption MaxNumberOfMeetings;
        public static CustomOption BlockSkippingInEmergencyMeetings;
        public static CustomOption NoVoteIsSelfVote;
        public static CustomOption AllowParallelMedBayScans;
        public static CustomOption HideOutOfSightNameTags;
        public static CustomOption HidePlayerNames;
        public static CustomOption RefundVotesOnDeath;
        public static CustomOption EnableMirrorMap;
        public static CustomOption CanZoomInOutWhenPlayerIsDead;

        public static CustomOption RandomMap;
        public static CustomOption RandomMapEnableSkeld;
        public static CustomOption RandomMapEnableMira;
        public static CustomOption RandomMapEnablePolus;
        public static CustomOption RandomMapEnableAirShip;
        public static CustomOption RandomMapEnableSubmerged;

        public static CustomOption RestrictDevices;
        public static CustomOption RestrictAdmin;
        public static CustomOption RestrictCameras;
        public static CustomOption RestrictVitals;

        // public static CustomOption EnableGodMiraHQ;

        public static CustomOption AirShipSettings;
        public static CustomOption OldAirShipAdmin;
        public static CustomOption AirshipReactorDuration;
        public static CustomOption EnableRecordsAdmin;
        public static CustomOption EnableCockpitAdmin;

        internal static Dictionary<byte, byte[]> BlockedRolePairings = new();

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
            // Role Options
            ActivateModRoles = Create(1, General, Yellow, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "ActivateRoles"), true, null, true);

            PresetSelection = Create(3, General, Yellow, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "PresetSelection"), Presets, null, true);

            // RememberClassic = Create(4, General, Yellow, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "RememberClassic"), false, null, true);
            EnableMirrorMap = Create(5, General, Yellow, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "MirrorMap"), false, null, true);
            CanZoomInOutWhenPlayerIsDead = Create(6, General, Yellow, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "CanZoomInOutDead"), true, null, true);
            // EnableGodMiraHQ = Create(7, General, Yellow, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "EnableGodMiraHQ"), false, null, true);

            // Using new id's for the options to not break compatibility with older versions
            CrewmateRolesCount = Create(10, General, Yellow, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "CrewmateRolesCount"), 0f, 0f, 15f, 1f, null, true, format: "FormatPlayer");
            ImpostorRolesCount = Create(11, General, Yellow, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "ImpostorRolesCount"), 0f, 0f, 15f, 1f, format: "FormatPlayer");
            NeutralRolesCount = Create(12, General, Yellow, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "NeutralRolesCount"), 0f, 0f, 15f, 1f, format: "FormatPlayer");
            ModifierCount = Create(13, General, Yellow, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "ModifierCount"), 0f, 0f, 15f, 1f, format: "FormatPlayer");

            SpecialOptions = new CustomOptionBlank(null);
            MaxNumberOfMeetings = Create(20, General, White, "MaxNumberOfMeetings", 10f, 0f, 15f, 1f, SpecialOptions, format: "FormatTimes");
            BlockSkippingInEmergencyMeetings = Create(21, General, White, "BlockSkip", false, SpecialOptions);
            NoVoteIsSelfVote = Create(22, General, White, "NoVoteIsSelfVote", false, SpecialOptions);
            AllowParallelMedBayScans = Create(23, General, White, "NoMedBayLimit", true, SpecialOptions);
            HideOutOfSightNameTags = Create(24, General, White, "HideOutName", true, SpecialOptions);
            HidePlayerNames = Create(25, General, White, "HidePlayerName", false, SpecialOptions);
            RefundVotesOnDeath = Create(26, General, White, "RefundVoteDeath", true, SpecialOptions);
            // EnableGodMiraHQ = Create(27, General, "EnableGodMira", false, SpecialOptions);
            RandomMap = Create(34, General, White, "PlayRandomMaps", false, SpecialOptions);
            RandomMapEnableSkeld = Create(50, General, White, "RandomMapsEnableSkeld", true, RandomMap, false);
            RandomMapEnableMira = Create(51, General, White, "RandomMapsEnableMira", true, RandomMap, false);
            RandomMapEnablePolus = Create(52, General, White, "RandomMapsEnablePolus", true, RandomMap, false);
            RandomMapEnableAirShip = Create(53, General, White, "RandomMapsEnableAirShip", true, RandomMap, false);
            RandomMapEnableSubmerged = Create(54, General, White, "RandomMapsEnableSubmerged", true, RandomMap, false);
            RestrictDevices = Create(60, General, White, "RestrictDevices", new string[] { "OptionOff", "RestrictPerTurn", "RestrictPerGame" }, SpecialOptions);
            RestrictAdmin = Create(61, General, White, "DisableAdmin", 30f, 0f, 600f, 5f, RestrictDevices, format: "FormatSeconds");
            RestrictCameras = Create(62, General, White, "DisableCameras", 30f, 0f, 600f, 5f, RestrictDevices, format: "FormatSeconds");
            RestrictVitals = Create(63, General, White, "DisableVitals", 30f, 0f, 600f, 5f, RestrictDevices, format: "FormatSeconds");

            AirShipSettings = Create(80, General, White, "AirShipSettings", false, SpecialOptions);
            OldAirShipAdmin = Create(81, General, White, "OldAirShipAdmin", true, AirShipSettings);
            EnableRecordsAdmin = Create(82, General, White, "EnableRecordsAdmin", false, AirShipSettings);
            EnableCockpitAdmin = Create(83, General, White, "EnableCockpitAdmin", false, AirShipSettings);
            AirshipReactorDuration = Create(84, General, White, "AirShipReactorDuration", 90f, 10f, 600f, 5f, AirShipSettings, format: "FormatSeconds");
        }
    }
}