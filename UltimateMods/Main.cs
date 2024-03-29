﻿global using UnhollowerBaseLib;
global using UnhollowerBaseLib.Attributes;
global using UnhollowerRuntimeLib;

global using BepInEx;
global using BepInEx.Configuration;
global using BepInEx.IL2CPP;
global using BepInEx.IL2CPP.Utils.Collections;
global using UnityEngine;
global using UnityEngine.Events;
global using UnityEngine.UI;
global using UnityEngine.SceneManagement;
global using Object = UnityEngine.Object;
global using static UnityEngine.UI.Button;
global using HarmonyLib;
global using AmongUs.Data;
global using InnerNet;
global using Hazel;
global using TMPro;
global using System;
global using System.Reflection;
global using System.Text;
global using System.IO;
global using System.Linq;
global using System.Collections;
global using System.Collections.Generic;
global using System.Linq.Expressions;
global using System.Text.RegularExpressions;
global using System.Net;
global using System.Net.Http;
global using System.Threading.Tasks;
global using Newtonsoft.Json.Linq;
global using Twitch;
global using AmongUs.Data.Legacy;
global using AmongUs.GameOptions;
global using UltimateMods.Patches;
global using UltimateMods.Roles;
global using UltimateMods.Localization;
global using UltimateMods.Utilities;
global using UltimateMods.Modules;
global using UltimateMods.Objects;
global using UltimateMods.EndGame;
global using static UltimateMods.Options;
global using static UltimateMods.ColorDictionary;
global using static UltimateMods.UltimateMods;
global using static UltimateMods.Modules.Assets;
global using static UltimateMods.GameHistory;
global using static UltimateMods.Modules.CustomOption;
global using static UltimateMods.Modules.CustomOption.CustomOptionType;
global using static UltimateMods.Roles.Patches.ButtonPatches;
global using static UltimateMods.Roles.Patches.OutlinePatch;
global using static UltimateMods.Roles.CrewmateRoles;
global using static UltimateMods.Roles.ImpostorRoles;
global using static UltimateMods.Roles.NeutralRoles;
global using static UltimateMods.Roles.ModifierRoles;
global using static UltimateMods.Roles.RoleManagement;

namespace UltimateMods
{
    [BepInPlugin(Id, "UltimateMods", VersionString)]
    [BepInProcess("Among Us.exe")]
    public class UltimateModsPlugin : BasePlugin
    {
        public const string Id = "jp.DekoKiyo.UltimateMods";
        public const string VersionString = "1.3.0";

        public static Version Version = Version.Parse(VersionString);
        internal static BepInEx.Logging.ManualLogSource Logger;
        public static int OptionsPage = 1;
        public static bool isBeta = false;

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
            LocalizationManager.Load();

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
            RoleInfoList.Load();
            // CustomColors.Load();
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