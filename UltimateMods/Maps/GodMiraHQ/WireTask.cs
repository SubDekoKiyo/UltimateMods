using UnityEngine;
using HarmonyLib;

namespace UltimateMods.Maps
{
    public static class GodMiraWireTask
    {
        public static void MiraWireTaskPositionChange()
        {
            var LabHallWireTask = GameObject.Find("MiraShip(Clone)/LabHall/FixWiringConsole");
            Vector3 LabHallWireTaskPos = LabHallWireTask.transform.position + new Vector3(-0.65f, 0.15f, 0f);
            if (LabHallWireTask != null && LabHallWireTask != null && PlayerControl.LocalPlayer.IsMiraHQ() && CustomOptionsH.EnableGodMiraHQ.getBool())
            {
                LabHallWireTask.transform.position = LabHallWireTaskPos;
            }
        }
    }
}