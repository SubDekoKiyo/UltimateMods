using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using UnhollowerBaseLib;
using UnityEngine;
using UltimateMods.Roles;
using UltimateMods.Modules;
using static UltimateMods.Roles.NeutralRoles;
using static UltimateMods.UltimateMods;

namespace UltimateMods.Patches
{
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
    [HarmonyPriority(Priority.First)]
    class ExileControllerBeginPatch
    {
        public static GameData.PlayerInfo lastExiled;
        public static void Prefix(ExileController __instance, [HarmonyArgument(0)] ref GameData.PlayerInfo exiled, [HarmonyArgument(1)] bool tie)
        {
            lastExiled = exiled;

            // 1 = Reset per turn
            if (MapOptions.restrictDevices == 1)
                MapOptions.ResetDeviceTimes();
        }
    }

    [HarmonyPatch]
    class ExileControllerWrapUpPatch
    {
        [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
        class BaseExileControllerPatch
        {
            public static void Postfix(ExileController __instance)
            {
                WrapUpPostfix(__instance.exiled);
            }
        }

        [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
        class AirshipExileControllerPatch
        {
            public static void Postfix(AirshipExileController __instance)
            {
                WrapUpPostfix(__instance.exiled);
            }
        }

        // Workaround to add a "postfix" to the destroying of the exile controller (i.e. cutscene) of submerged
        [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Destroy), new Type[] { typeof(GameObject) })]
        public static void Prefix(GameObject obj)
        {
            if (!SubmergedCompatibility.IsSubmerged) return;
            if (obj != null && obj.name.Contains("ExileCutscene"))
            {
                WrapUpPostfix(ExileControllerBeginPatch.lastExiled);
            }
        }

        static void WrapUpPostfix(GameData.PlayerInfo exiled)
        {
            if (exiled != null)
            {
                var p = exiled.Object;
                // Jester win condition
                if (p.isRole(RoleType.Jester))
                {
                    Jester.TriggerJesterWin = true;
                }

                if (SubmergedCompatibility.IsSubmerged)
                {
                    var fullscreen = UnityEngine.GameObject.Find("FullScreen500(Clone)");
                    if (fullscreen) fullscreen.SetActive(false);
                }
            }
        }
    }

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.ReEnableGameplay))]
    class ExileControllerReEnableGameplayPatch
    {
        public static void Postfix(ExileController __instance)
        {
            ReEnableGameplay();
        }
        public static void ReEnableGameplay()
        {
            // Reset custom button timers where necessary
            CustomButton.MeetingEndedUpdate();

            // Custom role post-meeting functions
            UltimateMods.OnMeetingEnd();

            ClassicAmongUs.MeetingDiscuss.OnMeetingEnd();

            // Remove DeadBodys
            DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            for (int i = 0; i < array.Length; i++)
            {
                UnityEngine.Object.Destroy(array[i].gameObject);
            }
        }
    }

    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new Type[] { typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
    class ExileControllerMessagePatch
    {
        static void Postfix(ref string __result, [HarmonyArgument(0)] StringNames id)
        {
            try
            {
                if (ExileController.Instance != null && ExileController.Instance.exiled != null)
                {
                    PlayerControl player = Helpers.PlayerById(ExileController.Instance.exiled.Object.PlayerId);
                    if (player == null) return;
                    // Exile role text
                    if (id is StringNames.ExileTextPN or StringNames.ExileTextSN or StringNames.ExileTextPP or StringNames.ExileTextSP)
                    {
                        switch ((int)SaveManager.LastLanguage)
                        {
                            case 0:
                                __result = player.Data.PlayerName + " " + ModTranslation.getString("wasThe") + " " + String.Join(" ", RoleInfo.getRoleInfoForPlayer(player).Select(x => x.name).ToArray());
                                break;
                            case 11:
                                __result = player.Data.PlayerName + ModTranslation.getString("HA") + String.Join(" ", RoleInfo.getRoleInfoForPlayer(player).Select(x => x.name).ToArray()) + ModTranslation.getString("wasThe");
                                break;
                        }
                    }
                    // Hide number of remaining impostors on Jester win
                    if (id is StringNames.ImpostorsRemainP or StringNames.ImpostorsRemainS)
                    {
                        if (Jester.jester != null && player.PlayerId == Jester.jester.PlayerId) __result = "";
                    }
                }
            }
            catch
            {
                // pass - Hopefully prevent leaving while exiling to softlock game
            }
        }
    }
}