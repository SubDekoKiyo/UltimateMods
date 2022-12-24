namespace UltimateMods.Patches
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    class HudManagerUpdatePatch
    {
        static void Postfix()
        {
            if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
            {
                CustomButton.HudUpdate();

                if (AmongUsClient.Instance.AmHost && Input.GetKeyDown(KeyCode.F11))
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedEndGame, Hazel.SendOption.Reliable, -1);
                    writer.Write((byte)CustomGameOverReason.ForceEnd);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.UncheckedEndGame((byte)CustomGameOverReason.ForceEnd);
                }
            }

            var FullScreen = GameObject.Find("FullScreen500(Clone)");
            if (FullScreen)
            {
                FullScreen.SetActive(false);
                UltimateModsPlugin.Logger.LogWarning("[WARNING] Crew Loading Animation was disabled with mod!");
            }
            var FullScreenC = GameObject.Find("FullScreen500(Clone)(Clone)");
            if (FullScreenC)
            {
                FullScreenC.SetActive(false);
                UltimateModsPlugin.Logger.LogWarning("[WARNING] Crew Loading Animation was disabled with mod!");
            }
        }
    }
}