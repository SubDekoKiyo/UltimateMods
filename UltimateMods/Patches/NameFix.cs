namespace UltimateMods.Patches
{
    [Harmony]
    public class AccountManagerPatch
    {
        [HarmonyPatch(typeof(AccountManager), nameof(AccountManager.RandomizeName))]
        public static class RandomizeNamePatch
        {
            static bool Prefix(AccountManager __instance)
            {
                if (LegacySaveManager.lastPlayerName == null)
                    return true;
                AmongUs.Data.DataManager.Player.Customization.name = LegacySaveManager.lastPlayerName;
                __instance.accountTab.UpdateNameDisplay();
                return false; // Don't execute original
            }
        }
    }
}