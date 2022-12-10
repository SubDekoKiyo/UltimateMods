namespace UltimateMods.Patches
{
    [HarmonyPatch(typeof(HeliSabotageSystem), nameof(HeliSabotageSystem.RepairDamage))]
    class HeliSabotageSystemRepairDamagePatch
    {
        static void Postfix(HeliSabotageSystem __instance, byte amount)
        {
            HeliSabotageSystem.Tags tags = (HeliSabotageSystem.Tags)(amount & 240);
            if (tags != HeliSabotageSystem.Tags.ActiveBit)
            {
                if (tags == HeliSabotageSystem.Tags.DamageBit)
                {
                    __instance.Countdown = CustomOptionsH.AirshipReactorDuration.getFloat();
                }
            }
        }
    }
}