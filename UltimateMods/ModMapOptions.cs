using System.Collections.Generic;
using UnityEngine;
using UltimateMods.Patches;

namespace UltimateMods
{
    static class ModMapOptions
    {
        // Set values
        public static int MaxNumberOfMeetings = 10;
        public static bool BlockSkippingInEmergencyMeetings = false;
        public static bool NoVoteIsSelfVote = false;
        public static bool HideOutOfSightNametags = false;
        public static bool HidePlayerNames = false;

        public static int RestrictDevices = 0;
        public static float RestrictAdminTime = 600f;
        public static float RestrictAdminTimeMax = 600f;
        public static float RestrictCamerasTime = 600f;
        public static float RestrictCamerasTimeMax = 600f;
        public static float RestrictVitalsTime = 600f;
        public static float RestrictVitalsTimeMax = 600f;

        public static bool GhostsSeeRoles = true;
        public static bool GhostsSeeTasks = true;
        public static bool GhostsSeeVotes = true;
        public static bool ShowRoleSummary = true;
        public static bool HideNameplates = false;
        public static bool AllowParallelMedBayScans = false;
        public static bool EnableCustomSounds = true;
        // public static bool showLighterDarker = false;
        public static bool enableHorseMode = false;

        // Updating values
        public static int MeetingsCount = 0;
        public static List<SurvCamera> CamerasToAdd = new();
        public static List<Vent> VentsToSeal = new();
        public static Dictionary<byte, PoolablePlayer> PlayerIcons = new();

        public static void ClearAndReloadModMapOptions()
        {
            MeetingsCount = 0;
            CamerasToAdd = new();
            VentsToSeal = new();
            PlayerIcons = new();

            MaxNumberOfMeetings = Mathf.RoundToInt(CustomOptionsH.MaxNumberOfMeetings.getSelection());
            BlockSkippingInEmergencyMeetings = CustomOptionsH.BlockSkippingInEmergencyMeetings.getBool();
            NoVoteIsSelfVote = CustomOptionsH.NoVoteIsSelfVote.getBool();

            HideOutOfSightNametags = CustomOptionsH.HideOutOfSightNameTags.getBool();
            HidePlayerNames = CustomOptionsH.HidePlayerNames.getBool();

            RestrictDevices = CustomOptionsH.RestrictDevices.getSelection();
            RestrictAdminTime = RestrictAdminTimeMax = CustomOptionsH.RestrictAdmin.getFloat();
            RestrictCamerasTime = RestrictCamerasTimeMax = CustomOptionsH.RestrictCameras.getFloat();
            RestrictVitalsTime = RestrictVitalsTimeMax = CustomOptionsH.RestrictVitals.getFloat();
        }

        public static void reloadPluginOptions()
        {
            AllowParallelMedBayScans = CustomOptionsH.AllowParallelMedBayScans.getBool();
            GhostsSeeRoles = UltimateModsPlugin.GhostsSeeRoles.Value;
            GhostsSeeTasks = UltimateModsPlugin.GhostsSeeTasks.Value;
            GhostsSeeVotes = UltimateModsPlugin.GhostsSeeVotes.Value;
            HideNameplates = UltimateModsPlugin.HideNameplates.Value;
            ShowRoleSummary = UltimateModsPlugin.ShowRoleSummary.Value;
            EnableCustomSounds = UltimateModsPlugin.EnableCustomSounds.Value;
            // showLighterDarker = UltimateModsPlugin.ShowLighterDarker.Value;
            enableHorseMode = UltimateModsPlugin.EnableHorseMode.Value;
            HorseModePatch.ShouldAlwaysHorseAround.isHorseMode = UltimateModsPlugin.EnableHorseMode.Value;
        }

        public static void ResetDeviceTimes()
        {
            RestrictAdminTime = RestrictAdminTimeMax;
            RestrictCamerasTime = RestrictCamerasTimeMax;
            RestrictVitalsTime = RestrictVitalsTimeMax;
        }

        public static bool canUseAdmin
        {
            get
            {
                return RestrictDevices == 0 || RestrictAdminTime > 0f;
            }
        }

        public static bool couldUseAdmin
        {
            get
            {
                return RestrictDevices == 0 || RestrictAdminTimeMax > 0f;
            }
        }

        public static bool canUseCameras
        {
            get
            {
                return RestrictDevices == 0 || RestrictCamerasTime > 0f;
            }
        }

        public static bool couldUseCameras
        {
            get
            {
                return RestrictDevices == 0 || RestrictCamerasTimeMax > 0f;
            }
        }

        public static bool canUseVitals
        {
            get
            {
                return RestrictDevices == 0 || RestrictVitalsTime > 0f;
            }
        }

        public static bool couldUseVitals
        {
            get
            {
                return RestrictDevices == 0 || RestrictVitalsTimeMax > 0f;
            }
        }
    }
}