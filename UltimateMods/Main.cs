global using UnhollowerBaseLib;
global using UnhollowerBaseLib.Attributes;
global using UnhollowerRuntimeLib;

using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using System;
using UnityEngine;
using UltimateMods.Modules;

namespace UltimateMods
{
    [BepInPlugin(Id, "UltimateMods", VersionString)]
    [BepInProcess("Among Us.exe")]
    public class UltimateModsPlugin : BasePlugin
    {
        public const string Id = "jp.DekoKiyo.UltimateMods";
        public const string VersionString = "1.2.0.1";

        public static Version Version = Version.Parse(VersionString);
        internal static BepInEx.Logging.ManualLogSource Logger;
        public static int OptionsPage = 1;
        public static bool isBeta = true;

        public static ConfigEntry<bool> DebugMode { get; private set; }
        public static ConfigEntry<bool> GhostsSeeTasks { get; set; }
        public static ConfigEntry<bool> GhostsSeeRoles { get; set; }
        public static ConfigEntry<bool> GhostsSeeVotes { get; set; }
        public static ConfigEntry<bool> HideNameplates { get; set; }
        public static ConfigEntry<bool> ShowRoleSummary { get; set; }
        public static ConfigEntry<bool> EnableCustomSounds { get; set; }
        // public static ConfigEntry<bool> ShowLighterDarker { get; set; }
        public static ConfigEntry<bool> EnableHorseMode { get; set; }
        public static ConfigEntry<string> RoomCodeText { get; set; }
        public static ConfigEntry<string> ShowPopUpVersion { get; set; }
        public static ConfigEntry<int> LanguageNum { get; set; }

        public Harmony Harmony { get; } = new Harmony(Id);
        public static UltimateModsPlugin Instance;

        public override void Load()
        {
            Logger = Log;
            Instance = this;
            ModTranslation.Load();

            DebugMode = Config.Bind("Custom", "Enable Debug Mode", false);
            GhostsSeeTasks = Config.Bind("Custom", "Ghosts See Remaining Tasks", true);
            GhostsSeeRoles = Config.Bind("Custom", "Ghosts See Roles", true);
            GhostsSeeVotes = Config.Bind("Custom", "Ghosts See Votes", true);
            ShowRoleSummary = Config.Bind("Custom", "ShowRoleSummary", false);
            HideNameplates = Config.Bind("Custom", "Hide Nameplates", false);
            EnableCustomSounds = Config.Bind("Custom", "Enable Custom Sounds", true);
            LanguageNum = Config.Bind("Custom", "Language Number", 0);
            // ShowLighterDarker = Config.Bind("Custom", "Show Lighter / Darker", false);
            EnableHorseMode = Config.Bind("Custom", "Enable Horse Mode", false);
            RoomCodeText = Config.Bind("Custom", "Streamer Mode Room Code Text", "Ultimate Mods");
            ShowPopUpVersion = Config.Bind("Custom", "Show PopUp", "0");
            // DebugRepo = Config.Bind("Custom", "Debug Hat Repo", "");

            CustomRolesH.Load();
            CustomOptionsH.Load();
            CustomColors.Load();
            // DiscordPatch.StartDiscord();
            Patches.FreeNamePatch.Initialize();
            ModLanguageSelector.Initialize();
            Harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(StatsManager), nameof(StatsManager.AmBanned), MethodType.Getter)]
    public static class AmBannedPatch
    {
        public static void Postfix(out bool __result)
        {
            __result = false;
        }
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
    public static class ChatControllerUpdatePatch
    {
        public static void Postfix(ChatController __instance)
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                if (!__instance.isActiveAndEnabled) return;
                __instance.SetVisible(false);
                new LateTask(() =>
                {
                    __instance.SetVisible(true);
                }, 0f, "AntiChatBug");
            }
        }
    }
}