namespace UltimateMods.Patches
{
    public class LogicGameFlowPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(LogicGameFlow), nameof(LogicGameFlow.IsGameOverDueToDeath))]
        [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.IsGameOverDueToDeath))]
        [HarmonyPatch(typeof(LogicGameFlowHnS), nameof(LogicGameFlowHnS.IsGameOverDueToDeath))]
        public static void Postfix2(ref bool __result)
        {
            __result = false;
        }
    }
}