using HarmonyLib;
using UnityEngine;
using Hazel;
using UltimateMods.Modules;
using UltimateMods.Roles;

namespace UltimateMods.Patches
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    class HudManagerUpdatePatch
    {
        static void Postfix()
        {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started)
            {
                AlivePlayer.IsForceEnd = false;
            }
            else
            {
                CustomButton.HudUpdate();
            }
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