global using UnhollowerBaseLib;
global using UnhollowerBaseLib.Attributes;
global using UnhollowerRuntimeLib;

using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Hazel;
using UltimateMods.Roles;

namespace UltimateMods
{
    [BepInPlugin(Id, "UltimateMods", VersionString)]
    [BepInDependency(SubmergedCompatibility.SUBMERGED_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInProcess("Among Us.exe")]
    public class UltimateModsPlugin : BasePlugin
    {
        public const string Id = "jp.DekoKiyo.UltimateMods";
        public const string VersionString = "0.0.1";

        public static System.Version Version = System.Version.Parse(VersionString);
        internal static BepInEx.Logging.ManualLogSource Logger;
        public static int OptionsPage = 1;
        public static bool isBeta = true;

        public static ConfigEntry<bool> DebugMode { get; private set; }
        public static ConfigEntry<bool> GhostsSeeTasks { get; set; }
        public static ConfigEntry<bool> GhostsSeeRoles { get; set; }
        public static ConfigEntry<bool> GhostsSeeVotes { get; set; }
        public static ConfigEntry<bool> HideNameplates { get; set; }
        // public static ConfigEntry<bool> ShowLighterDarker { get; set; }
        public static ConfigEntry<bool> EnableHorseMode { get; set; }
        public static ConfigEntry<string> RoomCodeText { get; set; }
        public static ConfigEntry<string> ShowPopUpVersion { get; set; }

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
            HideNameplates = Config.Bind("Custom", "Hide Nameplates", false);
            // ShowLighterDarker = Config.Bind("Custom", "Show Lighter / Darker", false);
            EnableHorseMode = Config.Bind("Custom", "Enable Horse Mode", false);
            RoomCodeText = Config.Bind("Custom", "Streamer Mode Room Code Text", "Ultimate Mods");
            ShowPopUpVersion = Config.Bind("Custom", "Show PopUp", "0");
            // DebugRepo = Config.Bind("Custom", "Debug Hat Repo", "");

            CustomRolesH.Load();
            CustomOptionsH.Load();
            // DiscordPatch.StartDiscord();
            Patches.FreeNamePatch.Initialize();
            SubmergedCompatibility.Initialize();
            RandomGenerator.Initialize();
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