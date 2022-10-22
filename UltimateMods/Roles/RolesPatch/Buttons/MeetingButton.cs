using HarmonyLib;
using System;
using UnityEngine;
using static UltimateMods.MapOptions;

namespace UltimateMods.Roles.Patches
{
    public static class MeetingButtonPatch
    {
        [HarmonyPatch(typeof(EmergencyMinigame), nameof(EmergencyMinigame.Update))]
        class EmergencyMinigameUpdatePatch
        {
            static void Postfix(EmergencyMinigame __instance)
            {
                var roleCanCallEmergency = true;
                var statusText = "";

                // Jester
                if (PlayerControl.LocalPlayer.isRole(RoleType.Jester) && !Jester.CanCallEmergency)
                {
                    roleCanCallEmergency = false;
                    statusText = ModTranslation.getString("JesterMeetingButton");
                }

                if (!roleCanCallEmergency)
                {
                    __instance.StatusText.text = statusText;
                    __instance.NumberText.text = string.Empty;
                    __instance.ClosedLid.gameObject.SetActive(true);
                    __instance.OpenLid.gameObject.SetActive(false);
                    __instance.ButtonActive = false;
                    return;
                }

                // Handle max number of meetings
                if (__instance.state == 1)
                {
                    int localRemaining = PlayerControl.LocalPlayer.RemainingEmergencies;
                    int teamRemaining = Mathf.Max(0, maxNumberOfMeetings - meetingsCount);
                    int remaining = Mathf.Min(localRemaining, teamRemaining);

                    __instance.StatusText.text = String.Format(ModTranslation.getString("MeetingStatus"), PlayerControl.LocalPlayer.name, localRemaining.ToString(), teamRemaining.ToString());
                    __instance.NumberText.text = "";
                    __instance.ButtonActive = remaining > 0;
                    __instance.ClosedLid.gameObject.SetActive(!__instance.ButtonActive);
                    __instance.OpenLid.gameObject.SetActive(__instance.ButtonActive);
                    return;
                }
            }
        }
    }
}