using System.Collections.Generic;
using UnityEngine;
using UltimateMods.Patches;

namespace UltimateMods
{
    static class MapOptions
    {
        // Set values
        public static int maxNumberOfMeetings = 10;
        public static bool blockSkippingInEmergencyMeetings = false;
        public static bool noVoteIsSelfVote = false;
        public static bool hideOutOfSightNametags = false;
        public static bool hidePlayerNames = false;

        public static int restrictDevices = 0;
        public static float restrictAdminTime = 600f;
        public static float restrictAdminTimeMax = 600f;
        public static float restrictCamerasTime = 600f;
        public static float restrictCamerasTimeMax = 600f;
        public static float restrictVitalsTime = 600f;
        public static float restrictVitalsTimeMax = 600f;

        public static bool ghostsSeeRoles = true;
        public static bool ghostsSeeTasks = true;
        public static bool ghostsSeeVotes = true;
        public static bool hideNameplates = false;
        public static bool allowParallelMedBayScans = false;
        // public static bool showLighterDarker = false;
        public static bool enableHorseMode = false;

        // Updating values
        public static int meetingsCount = 0;
        public static List<SurvCamera> camerasToAdd = new();
        public static List<Vent> ventsToSeal = new();
        public static Dictionary<byte, PoolablePlayer> playerIcons = new();

        public static void ClearAndReloadMapOptions()
        {
            meetingsCount = 0;
            camerasToAdd = new List<SurvCamera>();
            ventsToSeal = new List<Vent>();
            playerIcons = new Dictionary<byte, PoolablePlayer>();

            maxNumberOfMeetings = Mathf.RoundToInt(CustomOptionsH.MaxNumberOfMeetings.getSelection());
            blockSkippingInEmergencyMeetings = CustomOptionsH.BlockSkippingInEmergencyMeetings.getBool();
            noVoteIsSelfVote = CustomOptionsH.NoVoteIsSelfVote.getBool();

            hideOutOfSightNametags = CustomOptionsH.HideOutOfSightNameTags.getBool();
            hidePlayerNames = CustomOptionsH.HidePlayerNames.getBool();

            restrictDevices = CustomOptionsH.RestrictDevices.getSelection();
            restrictAdminTime = restrictAdminTimeMax = CustomOptionsH.RestrictAdmin.getFloat();
            restrictCamerasTime = restrictCamerasTimeMax = CustomOptionsH.RestrictCameras.getFloat();
            restrictVitalsTime = restrictVitalsTimeMax = CustomOptionsH.RestrictVitals.getFloat();
        }

        public static void reloadPluginOptions()
        {
            allowParallelMedBayScans = CustomOptionsH.AllowParallelMedBayScans.getBool();
            ghostsSeeRoles = UltimateModsPlugin.GhostsSeeRoles.Value;
            ghostsSeeTasks = UltimateModsPlugin.GhostsSeeTasks.Value;
            ghostsSeeVotes = UltimateModsPlugin.GhostsSeeVotes.Value;
            hideNameplates = UltimateModsPlugin.HideNameplates.Value;
            // showLighterDarker = UltimateModsPlugin.ShowLighterDarker.Value;
            enableHorseMode = UltimateModsPlugin.EnableHorseMode.Value;
            HorseModePatch.ShouldAlwaysHorseAround.isHorseMode = UltimateModsPlugin.EnableHorseMode.Value;
        }

        public static void ResetDeviceTimes()
        {
            restrictAdminTime = restrictAdminTimeMax;
            restrictCamerasTime = restrictCamerasTimeMax;
            restrictVitalsTime = restrictVitalsTimeMax;
        }

        public static bool canUseAdmin
        {
            get
            {
                return restrictDevices == 0 || restrictAdminTime > 0f;
            }
        }

        public static bool couldUseAdmin
        {
            get
            {
                return restrictDevices == 0 || restrictAdminTimeMax > 0f;
            }
        }

        public static bool canUseCameras
        {
            get
            {
                return restrictDevices == 0 || restrictCamerasTime > 0f;
            }
        }

        public static bool couldUseCameras
        {
            get
            {
                return restrictDevices == 0 || restrictCamerasTimeMax > 0f;
            }
        }

        public static bool canUseVitals
        {
            get
            {
                return restrictDevices == 0 || restrictVitalsTime > 0f;
            }
        }

        public static bool couldUseVitals
        {
            get
            {
                return restrictDevices == 0 || restrictVitalsTimeMax > 0f;
            }
        }
    }
}