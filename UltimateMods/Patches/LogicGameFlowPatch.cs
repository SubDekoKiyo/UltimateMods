namespace UltimateMods.Patches
{
    public class LogicGameFlowPatch
    {
        [HarmonyPatch(typeof(LogicGameFlow), nameof(LogicGameFlow.IsGameOverDueToDeath))]
        [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.IsGameOverDueToDeath))]
        [HarmonyPatch(typeof(LogicGameFlowHnS), nameof(LogicGameFlowHnS.IsGameOverDueToDeath))]
        public static void Postfix(ref bool __result)
        {
            __result = false;
        }
    }
}