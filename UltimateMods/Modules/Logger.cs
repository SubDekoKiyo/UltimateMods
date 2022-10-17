// Source Code by TownOfHost

using UltimateMods.Utilities;

namespace UltimateMods.Modules
{
    class Logger
    {
        public static void SendInGame(string text)
        {
            if (DestroyableSingleton<HudManager>._instance) FastDestroyableSingleton<HudManager>.Instance.Notifier.AddItem(text);
        }
    }
}