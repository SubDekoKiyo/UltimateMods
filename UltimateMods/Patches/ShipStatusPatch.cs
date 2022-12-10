using HarmonyLib;
using UnityEngine;
using UltimateMods.Utilities;
using AmongUs.GameOptions;

namespace UltimateMods.Patches
{
    [HarmonyPatch(typeof(ShipStatus))]
    public class ShipStatusPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.IsGameOverDueToDeath))]
        public static void Postfix2(LogicGameFlowNormal __instance, ref bool __result)
        {
            __result = false;
        }
    }
}