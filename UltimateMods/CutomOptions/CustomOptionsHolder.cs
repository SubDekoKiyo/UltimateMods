using System.Collections.Generic;
using UnityEngine;
using UltimateMods.Modules;
using static UltimateMods.ColorDictionary;
using static UltimateMods.Modules.CustomOption.CustomOptionType;

namespace UltimateMods
{
    public class CustomOptionsH
    {
        public static string[] TenRates = new string[] { "0%", "10%", "20%", "30%", "40%", "50%", "60%", "70%", "80%", "90%", "100%" };
        public static string[] Presets = new string[] { "Preset1", "Preset2", "Preset3", "Preset4", "Preset5" };

        public static CustomOption PresetSelection;
        public static CustomOption ActivateModRoles;
        // public static CustomOption activateModSettings;
        public static CustomOption RandomGen;
        public static CustomOption CrewmateRolesCountMin;
        public static CustomOption CrewmateRolesCountMax;
        public static CustomOption NeutralRolesCountMin;
        public static CustomOption NeutralRolesCountMax;
        public static CustomOption ImpostorRolesCountMin;
        public static CustomOption ImpostorRolesCountMax;
        public static CustomOption ModifierCountMin;
        public static CustomOption ModifierCountMax;
        // public static CustomOption RememberClassic;

        public static CustomOption SpecialOptions;
        public static CustomOption MaxNumberOfMeetings;
        public static CustomOption BlockSkippingInEmergencyMeetings;
        public static CustomOption NoVoteIsSelfVote;
        public static CustomOption AllowParallelMedBayScans;
        // public static CustomOption onePlayerStart;
        // public static CustomOption betterStartButtons;
        public static CustomOption HideOutOfSightNameTags;
        public static CustomOption RefundVotesOnDeath;
        public static CustomOption EnableMirrorMap;

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
            // Role Options
            ActivateModRoles = CustomOption.Create(1, TypeGeneral, ClearWhite, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "ActivateRoles"), true, null, true);
            // activateModSettings = CustomOption.Create(2, TypeGeneral, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "activateSettings"), true, null, true);

            PresetSelection = CustomOption.Create(3, TypeGeneral, ClearWhite, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "PresetSelection"), Presets, null, true);

            RandomGen = CustomOption.Create(4, TypeGeneral, ClearWhite, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "RandomGen"), true, null, true);

            // RememberClassic = CustomOption.Create(13, TypeGeneral, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "RememberClassic"), false, null, true);
            EnableMirrorMap = CustomOption.Create(33, TypeGeneral, ClearWhite, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "MirrorMap"), false, null, true);

            // Using new id's for the options to not break compatibility with older versions
            CrewmateRolesCountMin = CustomOption.Create(5, TypeGeneral, ClearWhite, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "CrewmateRolesCountMin"), 0f, 0f, 15f, 1f, null, true, format: "FormatPlayer");
            CrewmateRolesCountMax = CustomOption.Create(6, TypeGeneral, ClearWhite, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "CrewmateRolesCountMax"), 0f, 0f, 15f, 1f, format: "FormatPlayer");
            NeutralRolesCountMin = CustomOption.Create(7, TypeGeneral, ClearWhite, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "NeutralRolesCountMin"), 0f, 0f, 15f, 1f, format: "FormatPlayer");
            NeutralRolesCountMax = CustomOption.Create(8, TypeGeneral, ClearWhite, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "NeutralRolesCountMax"), 0f, 0f, 15f, 1f, format: "FormatPlayer");
            ImpostorRolesCountMin = CustomOption.Create(9, TypeGeneral, ClearWhite, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "ImpostorRolesCountMin"), 0f, 0f, 15f, 1f, format: "FormatPlayer");
            ImpostorRolesCountMax = CustomOption.Create(10, TypeGeneral, ClearWhite, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "ImpostorRolesCountMax"), 0f, 0f, 15f, 1f, format: "FormatPlayer");
            ModifierCountMin = CustomOption.Create(11, TypeGeneral, ClearWhite, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "ModifierCountMin"), 0f, 0f, 15f, 1f, format: "FormatPlayer");
            ModifierCountMax = CustomOption.Create(12, TypeGeneral, ClearWhite, cs(new Color(204f / 255f, 204f / 255f, 0, 1f), "ModifierCountMax"), 0f, 0f, 15f, 1f, format: "FormatPlayer");

            SpecialOptions = new CustomOptionBlank(null);
            MaxNumberOfMeetings = CustomOption.Create(20, TypeGeneral, ClearWhite, "MaxNumberOfMeetings", 10f, 0f, 15f, 1f, SpecialOptions, format: "FormatTimes");
            BlockSkippingInEmergencyMeetings = CustomOption.Create(21, TypeGeneral, ClearWhite, "BlockSkip", false, SpecialOptions);
            NoVoteIsSelfVote = CustomOption.Create(22, TypeGeneral, ClearWhite, "NoVoteIsSelfVote", false, SpecialOptions);
            AllowParallelMedBayScans = CustomOption.Create(24, TypeGeneral, ClearWhite, "NoMedBayLimit", true, SpecialOptions);
            // onePlayerStart = CustomOption.Create(27, TypeGeneral, "oneStart", false, specialOptions);
            // betterStartButtons = CustomOption.Create(30, TypeGeneral, "betterStart", true, specialOptions);
            HideOutOfSightNameTags = CustomOption.Create(31, TypeGeneral, ClearWhite, "HideOutName", true, SpecialOptions);
            RefundVotesOnDeath = CustomOption.Create(32, TypeGeneral, ClearWhite, "RefundVoteDeath", true, SpecialOptions);
            // EnableGodMiraHQ = CustomOption.Create(35, TypeGeneral, "EnableGodMira", false, SpecialOptions);
            RandomMap = CustomOption.Create(34, TypeGeneral, ClearWhite, "PlayRandomMaps", false, SpecialOptions);
            RandomMapEnableSkeld = CustomOption.Create(50, TypeGeneral, ClearWhite, "RandomMapsEnableSkeld", true, RandomMap, false);
            RandomMapEnableMira = CustomOption.Create(51, TypeGeneral, ClearWhite, "RandomMapsEnableMira", true, RandomMap, false);
            RandomMapEnablePolus = CustomOption.Create(52, TypeGeneral, ClearWhite, "RandomMapsEnablePolus", true, RandomMap, false);
            RandomMapEnableAirShip = CustomOption.Create(53, TypeGeneral, ClearWhite, "RandomMapsEnableAirShip", true, RandomMap, false);
            RandomMapEnableSubmerged = CustomOption.Create(54, TypeGeneral, ClearWhite, "RandomMapsEnableSubmerged", true, RandomMap, false);
            RestrictDevices = CustomOption.Create(60, TypeGeneral, ClearWhite, "RestrictDevices", new string[] { "OptionOff", "RestrictPerTurn", "RestrictPerGame" }, SpecialOptions);
            RestrictAdmin = CustomOption.Create(61, TypeGeneral, ClearWhite, "DisableAdmin", 30f, 0f, 600f, 5f, RestrictDevices, format: "FormatSeconds");
            RestrictCameras = CustomOption.Create(62, TypeGeneral, ClearWhite, "DisableCameras", 30f, 0f, 600f, 5f, RestrictDevices, format: "FormatSeconds");
            RestrictVitals = CustomOption.Create(63, TypeGeneral, ClearWhite, "DisableVitals", 30f, 0f, 600f, 5f, RestrictDevices, format: "FormatSeconds");

            AirShipSettings = CustomOption.Create(80, TypeGeneral, ClearWhite, "AirShipSettings", false, SpecialOptions);
            OldAirShipAdmin = CustomOption.Create(81, TypeGeneral, ClearWhite, "OldAirShipAdmin", true, AirShipSettings);
            EnableRecordsAdmin = CustomOption.Create(82, TypeGeneral, ClearWhite, "EnableRecordsAdmin", false, AirShipSettings);
            EnableCockpitAdmin = CustomOption.Create(83, TypeGeneral, ClearWhite, "EnableCockpitAdmin", false, AirShipSettings);
            AirshipReactorDuration = CustomOption.Create(84, TypeGeneral, ClearWhite, "AirShipReactorDuration", 90f, 10f, 600f, 5f, AirShipSettings, format: "FormatSeconds");
        }
    }
}