using HarmonyLib;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace UltimateMods.Debug
{
    [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
    public static class DebugBots
    {
        private static readonly System.Random random = new System.Random((int)DateTime.Now.Ticks);
        private static List<PlayerControl> bots = new();
        public static int BotCount = 0;
        public static void Postfix(KeyboardJoystick __instance)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                BotCount++;
                var playerControl = UnityEngine.Object.Instantiate(AmongUsClient.Instance.PlayerPrefab);
                var i = playerControl.PlayerId = (byte)GameData.Instance.GetAvailableId();

                bots.Add(playerControl);
                GameData.Instance.AddPlayer(playerControl);
                AmongUsClient.Instance.Spawn(playerControl, -2, InnerNet.SpawnFlags.None);

                int hat = random.Next(HatManager.Instance.allHats.Count);
                int pet = random.Next(HatManager.Instance.allPets.Count);
                int skin = random.Next(HatManager.Instance.allSkins.Count);
                int visor = random.Next(HatManager.Instance.allVisors.Count);
                int color = random.Next(Palette.PlayerColors.Length);
                int nameplate = random.Next(HatManager.Instance.allNamePlates.Count);

                playerControl.transform.position = PlayerControl.LocalPlayer.transform.position;
                playerControl.GetComponent<DummyBehaviour>().enabled = true;
                playerControl.NetTransform.enabled = false;
                playerControl.SetName("Bot" + BotCount);
                playerControl.SetColor(color);
                playerControl.SetHat(HatManager.Instance.allHats[hat].ProductId, color);
                playerControl.SetPet(HatManager.Instance.allPets[pet].ProductId, color);
                playerControl.SetVisor(HatManager.Instance.allVisors[visor].ProductId, color);
                playerControl.SetSkin(HatManager.Instance.allSkins[skin].ProductId, color);
                playerControl.SetNamePlate(HatManager.Instance.allNamePlates[nameplate].ProductId);
                GameData.Instance.RpcSetTasks(playerControl.PlayerId, new byte[0]);
            }
        }
    }
}