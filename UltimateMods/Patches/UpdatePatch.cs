namespace UltimateMods.Patches
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    class HudManagerUpdatePatch
    {
        static void Postfix()
        {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started && AlivePlayer.IsForceEnd)
            {
                AlivePlayer.IsForceEnd = false;
            }
            else if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
            {
                CustomButton.HudUpdate();
            }

            var FullScreen = GameObject.Find("FullScreen500(Clone)");
            if (FullScreen) FullScreen.SetActive(false);
            var FullScreenC = GameObject.Find("FullScreen500(Clone)(Clone)");
            if (FullScreenC) FullScreenC.SetActive(false);
        }
    }

    [HarmonyPatch(typeof(ControllerManager), nameof(ControllerManager.Update))]
    public static class KeyboardClass
    {
        public static void Postfix()
        {
            if (AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Started && AmongUsClient.Instance.AmHost && Input.GetKeyDown(KeyCode.F11))
            {
                AlivePlayer.IsForceEnd = true;
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ForceEnd, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.ForceEnd();
            }
        }
    }
}