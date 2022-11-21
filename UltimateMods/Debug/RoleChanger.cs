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
        private static RoleType keyrole1 = RoleType.Crewmate;
        private static RoleType keyrole2 = RoleType.Impostor;
        private static RoleType keyrole3 = RoleType.Sheriff;
        private static RoleType keyrole4 = RoleType.Madmate;
        private static RoleType keyrole5 = RoleType.Jester;
        private static RoleType keyrole6 = RoleType.EvilHacker;
        private static RoleType keyrole7 = RoleType.Engineer;
        private static RoleType keyrole8 = RoleType.CustomImpostor;
        private static RoleType keyrole9 = RoleType.Teleporter;


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

        public static void ChangeRole(RoleType roleType)
        {
            byte role = (byte)roleType;

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetRole, Hazel.SendOption.Reliable, -1);
            writer.Write(role);
            writer.Write(PlayerControl.LocalPlayer.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.SetRole(role, PlayerControl.LocalPlayer.PlayerId);
        }
    }
}*/