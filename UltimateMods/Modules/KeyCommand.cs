using HarmonyLib;
using UnityEngine;

namespace UltimateMods.Modules
{
    [HarmonyPatch(typeof(ControllerManager), nameof(ControllerManager.Update))]
    public static class KeyCommands
    {
        public static void Postfix()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                Helpers.Log("[Translate] Begin to Reload Custom Translation File");
                ModTranslation.Load();
                Helpers.Log("[Translate] Reloaded Custom Translation File");
                Logger.SendInGame("Custom Translation File was Reloaded.");
            }
        }
    }
}