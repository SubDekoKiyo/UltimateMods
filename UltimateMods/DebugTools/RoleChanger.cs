//このコードのコメントアウトを消さないでください

/*using System;
using UltimateMods.Roles;
using HarmonyLib;
using UnityEngine;
using Hazel;

namespace UltimateMods.Debug
{
    [HarmonyPatch(typeof(ControllerManager), nameof(ControllerManager.Update))]
    public class Role
    {
        private static RoleId keyrole1 = RoleId.Crewmate;
        private static RoleId keyrole2 = RoleId.Impostor;
        private static RoleId keyrole3 = RoleId.Sheriff;
        private static RoleId keyrole4 = RoleId.Madmate;
        private static RoleId keyrole5 = RoleId.Jester;
        private static RoleId keyrole6 = RoleId.EvilHacker;
        private static RoleId keyrole7 = RoleId.Engineer;
        private static RoleId keyrole8 = RoleId.CustomImpostor;
        private static RoleId keyrole9 = RoleId.Teleporter;


        public static void Postfix()
        {
            if (AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Started && AmongUsClient.Instance.AmHost &&
                UltimateModsPlugin.isBeta && Input.GetKey(KeyCode.RightShift))
            {
                var key = (KeyCode)System.Enum.Parse(typeof(KeyCode), Input.inputString.ToUpper());
                switch(key)
                {
                    case KeyCode.Alpha1:
                        ChangeRole(keyrole1);
                        break;
                    case KeyCode.Alpha2:
                        ChangeRole(keyrole2);
                        break;
                    case KeyCode.Alpha3:
                        ChangeRole(keyrole3);
                        break;
                    case KeyCode.Alpha4:
                        ChangeRole(keyrole4);
                        break;
                    case KeyCode.Alpha5:
                        ChangeRole(keyrole5);
                        break;
                    case KeyCode.Alpha6:
                        ChangeRole(keyrole6);
                        break;
                    case KeyCode.Alpha7:
                        ChangeRole(keyrole7);
                        break;
                    case KeyCode.Alpha8:
                        ChangeRole(keyrole8);
                        break;
                    case KeyCode.Alpha9:
                        ChangeRole(keyrole9);
                        break;
                }
            }
        }

        public static void ChangeRole(RoleId RoleId)
        {
            byte role = (byte)RoleId;

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetRole, Hazel.SendOption.Reliable, -1);
            writer.Write(role);
            writer.Write(PlayerControl.LocalPlayer.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.SetRole(role, PlayerControl.LocalPlayer.PlayerId);
        }
    }
}*/