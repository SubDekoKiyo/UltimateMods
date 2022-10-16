using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using Hazel;
using System;
using TMPro;
using UltimateMods.Utilities;
// using static UltimateMods.Patches.CustomLobbyPatch;

namespace UltimateMods.Patches
{
    public class GameStartManagerPatch
    {
        public static Dictionary<int, PlayerVersion> playerVersions = new();
        public static float timer = 600f;
        public static float shareTimer;
        private static float kickingTimer = 0f;
        private static bool versionSent = false;
        private static string lobbyCodeText = "";

        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
        public class AmongUsClientOnPlayerJoinedPatch
        {
            public static void Postfix()
            {
                if (PlayerControl.LocalPlayer != null)
                {
                    Helpers.ShareGameVersion();
                }
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
        public class GameStartManagerStartPatch
        {
            public static TMP_Text ErrorText;
            public static void Postfix(GameStartManager __instance)
            {
                // Trigger version refresh
                versionSent = false;
                // Reset lobby countdown timer
                timer = 600f;
                // Reset kicking timer
                kickingTimer = 0f;
                // Copy lobby code
                string code = InnerNet.GameCode.IntToGameName(AmongUsClient.Instance.GameId);
                GUIUtility.systemCopyBuffer = code;
                lobbyCodeText = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.RoomCode, new Il2CppReferenceArray<Il2CppSystem.Object>(0)) + "\r\n" + code;
                ErrorText = GameObject.Instantiate(__instance.GameStartText, __instance.GameStartText.transform);
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        public class GameStartManagerUpdatePatch
        {
            private static bool update = false;
            private static string currentText = "";
            // private static GameObject LobbyText = GameObject.Find("Main Camera/Hud/GameStartManager/TaskText_TMP(Clone)");

            public static void Prefix(GameStartManager __instance)
            {
                if (!GameData.Instance) return; // No instance
                update = GameData.Instance.PlayerCount != __instance.LastPlayerCount;
                // カウントダウンキャンセル
                if (Input.GetKeyDown(KeyCode.LeftShift) && GameStartManager.Instance.startState == GameStartManager.StartingStates.Countdown)
                    GameStartManager.Instance.ResetStartState();
                // 即スタート
                if (Input.GetKeyDown(KeyCode.KeypadEnter) && GameStartManager.Instance.startState == GameStartManager.StartingStates.Countdown)
                    GameStartManager.Instance.countDownTimer = 0;
            }

            public static void Postfix(GameStartManager __instance)
            {
                if (PlayerControl.LocalPlayer != null && !versionSent)
                {
                    versionSent = true;
                    Helpers.ShareGameVersion();
                }

                // Check version handshake infos

                bool versionMismatch = false;
                string message = "";
                foreach (InnerNet.ClientData client in AmongUsClient.Instance.allClients.ToArray())
                {
                    if (client.Character == null) continue;
                    var dummyComponent = client.Character.GetComponent<DummyBehaviour>();
                    if (dummyComponent != null && dummyComponent.enabled)
                        continue;
                    else
                    {
                        PlayerVersion PV = playerVersions[client.Id];
                        int diff = UltimateModsPlugin.Version.CompareTo(PV.version);
                        if (diff > 0)
                        {
                            message += $"<color=#00a2ff>{ModTranslation.getString("ErrorOlderVersion")} {client.Character.Data.PlayerName} (Version{playerVersions[client.Id].version.ToString()})\n</color>";
                            versionMismatch = true;
                        }
                        else if (diff < 0)
                        {
                            message += $"<color=#00a2ff>{ModTranslation.getString("ErrorNewerVersion")} {client.Character.Data.PlayerName} (Version{playerVersions[client.Id].version.ToString()})\n</color>";
                            versionMismatch = true;
                        }
                        else if (!PV.GuidMatches())
                        { // version presumably matches, check if Guid matches
                            message += $"<color=#00a2ff>{ModTranslation.getString("ErrorWrongVersion")} {client.Character.Data.PlayerName} Version{playerVersions[client.Id].version.ToString()} <size=30%>({PV.guid.ToString()})</size>\n</color>";
                            versionMismatch = true;
                        }
                    }
                }

                // Display message to the host
                if (AmongUsClient.Instance.AmHost)
                {
                    if (versionMismatch)
                    {
                        __instance.StartButton.color = __instance.startLabelText.color = Palette.DisabledClear;
                        __instance.GameStartText.text = message;
                        __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition + Vector3.up * 2;
                    }
                    else
                    {
                        __instance.StartButton.color = __instance.startLabelText.color = ((__instance.LastPlayerCount >= __instance.MinPlayers) ? Palette.EnabledColor : Palette.DisabledClear);
                        __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition;
                    }
                }

                // Client update with handshake infos
                else
                {
                    if (!playerVersions.ContainsKey(AmongUsClient.Instance.HostId) || UltimateModsPlugin.Version.CompareTo(playerVersions[AmongUsClient.Instance.HostId].version) != 0)
                    {
                        kickingTimer += Time.deltaTime;
                        if (kickingTimer > 10)
                        {
                            kickingTimer = 0;
                            AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame);
                            SceneChanger.ChangeScene("MainMenu");
                        }

                        __instance.GameStartText.text = string.Format(ModTranslation.getString("ErrorHostNoVersion"), Math.Round(10 - kickingTimer));
                        __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition + Vector3.up * 2;
                    }
                    else if (versionMismatch)
                    {
                        __instance.GameStartText.text = $"<color=#00a2ff>{ModTranslation.getString("ErrorHostNoVersion")}\n</color>" + message;
                        __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition + Vector3.up * 2;
                    }
                    else
                    {
                        __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition;
                        if (__instance.startState != GameStartManager.StartingStates.Countdown)
                        {
                            __instance.GameStartText.text = String.Empty;
                        }
                    }
                }

                // Lobby timer
                if (!GameData.Instance) return; // No instance

                if (update) currentText = __instance.PlayerCounter.text;

                timer = Mathf.Max(0f, timer -= Time.deltaTime);

                int minutes = (int)timer / 60;
                int seconds = (int)timer % 60;
                string suffix = $" ({minutes:00}:{seconds:00})";

                __instance.PlayerCounter.text = currentText + suffix;
                __instance.PlayerCounter.autoSizeTextContainer = true;

                /*if (NewsText.transform.localPosition.x >= -7f)
                {
                    NewsText.transform.localPosition -= new Vector3(0.01f, 0f, 0f);
                }
                else
                {
                    NewsText.transform.localPosition = new Vector3(3.5f, NewsText.transform.localPosition.y, NewsText.transform.localPosition.z);
                }*/
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
        public class GameStartManagerBeginGame
        {
            public static bool Prefix(GameStartManager __instance)
            {
                // Block game start if not everyone has the same mod version
                bool continueStart = true;

                if (AmongUsClient.Instance.AmHost)
                {
                    foreach (InnerNet.ClientData client in AmongUsClient.Instance.allClients.GetFastEnumerator())
                    {
                        if (client.Character == null) continue;
                        var dummyComponent = client.Character.GetComponent<DummyBehaviour>();
                        if (dummyComponent != null && dummyComponent.enabled)
                            continue;

                        if (!playerVersions.ContainsKey(client.Id))
                        {
                            continueStart = false;
                            break;
                        }

                        PlayerVersion PV = playerVersions[client.Id];
                        int diff = UltimateModsPlugin.Version.CompareTo(PV.version);
                        if (diff != 0 || !PV.GuidMatches())
                        {
                            continueStart = false;
                            break;
                        }
                    }

                    if (CustomOptionsH.RandomMap.getBool() && continueStart)
                    {
                        // 0 = Skeld
                        // 1 = Mira HQ
                        // 2 = Polus
                        // 3 = Dleks - deactivated
                        // 4 = Airship
                        List<byte> possibleMaps = new();
                        if (CustomOptionsH.RandomMapEnableSkeld.getBool())
                            possibleMaps.Add(0);
                        if (CustomOptionsH.RandomMapEnableMira.getBool())
                            possibleMaps.Add(1);
                        if (CustomOptionsH.RandomMapEnablePolus.getBool())
                            possibleMaps.Add(2);
                        if (CustomOptionsH.RandomMapEnableAirShip.getBool())
                            possibleMaps.Add(4);
                        if (CustomOptionsH.RandomMapEnableSubmerged.getBool())
                            possibleMaps.Add(5);
                        byte chosenMapId = possibleMaps[UltimateMods.rnd.Next(possibleMaps.Count)];

                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DynamicMapOption, Hazel.SendOption.Reliable, -1);
                        writer.Write(chosenMapId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.DynamicMapOption(chosenMapId);
                    }
                }
                return continueStart;
            }
        }

        public class PlayerVersion
        {
            public readonly Version version;
            public readonly Guid guid;

            public PlayerVersion(Version version, Guid guid)
            {
                this.version = version;
                this.guid = guid;
            }

            public bool GuidMatches()
            {
                return Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.Equals(this.guid);
            }
        }
    }
}