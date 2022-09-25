using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using static UltimateMods.UltimateMods;
using static UltimateMods.MapOptions;
using static UltimateMods.GameHistory;
using System;
using UnityEngine;
using UltimateMods.Utilities;
using UltimateMods.Modules;
using TMPro;

namespace UltimateMods.Patches
{
    [HarmonyPatch]
    class MeetingHudPatch
    {
        private const float scale = 0.65f;
        private static Sprite blankNameplate = null;
        public static bool nameplatesChanged = true;

        public static void updateNameplate(PlayerVoteArea pva, byte playerId = Byte.MaxValue)
        {
            blankNameplate = blankNameplate ?? HatManager.Instance.GetNamePlateById("nameplate_NoPlate")?.viewData?.viewData?.Image;

            var nameplate = blankNameplate;
            if (!hideNameplates)
            {
                var p = Helpers.PlayerById(playerId != Byte.MaxValue ? playerId : pva.TargetPlayerId);
                var nameplateId = p?.CurrentOutfit?.NamePlateId;
                nameplate = HatManager.Instance.GetNamePlateById(nameplateId)?.viewData?.viewData?.Image;
            }
            pva.Background.sprite = nameplate;
        }

        [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetCosmetics))]
        class PlayerVoteAreaCosmetics
        {
            static void Postfix(PlayerVoteArea __instance, GameData.PlayerInfo playerInfo)
            {
                updateNameplate(__instance, playerInfo.PlayerId);
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
        class MeetingHudUpdatePatch
        {
            static void Postfix(MeetingHud __instance)
            {
                if (nameplatesChanged)
                {
                    foreach (var pva in __instance.playerStates)
                    {
                        updateNameplate(pva);
                    }
                    nameplatesChanged = false;
                }

                if (__instance.state == MeetingHud.VoteStates.Animating)
                    return;

                // Deactivate skip Button if skipping on emergency meetings is disabled
                if (blockSkippingInEmergencyMeetings)
                    __instance.SkipVoteButton?.gameObject?.SetActive(false);

                // This fixes a bug with the original game where pressing the button and a kill happens simultaneously
                // results in bodies sometimes being created *after* the meeting starts, marking them as dead and
                // removing the corpses so there's no random corpses leftover afterwards
                foreach (DeadBody b in UnityEngine.Object.FindObjectsOfType<DeadBody>())
                {
                    if (b == null) continue;

                    foreach (PlayerVoteArea pva in __instance.playerStates)
                    {
                        if (pva == null) continue;

                        if (pva.TargetPlayerId == b?.ParentId && !pva.AmDead)
                        {
                            pva?.SetDead(pva.DidReport, true);
                            pva?.Overlay?.gameObject?.SetActive(true);
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Select))]
        class MeetingHudSelectPatch
        {
            public static bool Prefix(ref bool __result, MeetingHud __instance, [HarmonyArgument(0)] int suspectStateIdx)
            {
                __result = false;
                if (noVoteIsSelfVote && PlayerControl.LocalPlayer.PlayerId == suspectStateIdx) return false;
                if (blockSkippingInEmergencyMeetings && suspectStateIdx == -1) return false;

                return true;
            }
        }

        public static void startMeeting()
        {
            UltimateMods.OnMeetingStart();
        }

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.OpenMeetingRoom))]
        class OpenMeetingPatch
        {
            public static void Prefix(HudManager __instance)
            {
                startMeeting();
            }
        }

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive))]
        class HudManagerSetHudActive
        {
            public static void Postfix(HudManager __instance)
            {
                FastDestroyableSingleton<HudManager>.Instance.transform.FindChild("TaskDisplay").FindChild("TaskPanel").gameObject.SetActive(true);
            }
        }
    }
}