// Source Code from Town of Plus & Town of Super

using HarmonyLib;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UltimateMods.Roles;
using UltimateMods.Utilities;
using AmongUs.GameOptions;

namespace UltimateMods.Objects
{
    [Harmony]
    public class CustomOverlays
    {
        public static Dictionary<int, PlayerVersion> playerVersions = new Dictionary<int, PlayerVersion>();
        private static SpriteRenderer meetingUnderlay;
        private static SpriteRenderer infoUnderlay;
        private static TMPro.TextMeshPro infoOverlayRules;
        private static TMPro.TextMeshPro infoOverlayPlayer;
        private static TMPro.TextMeshPro infoOverlayRoles;

        public static bool OverlayShown = false;

        public static bool SendVersion = false;

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
        public class GameStartManagerStartPatch
        {
            public static void Postfix(GameStartManager __instance)
            {
                SendVersion = false;
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        public class GameStartManagerPatch
        {
            public static void Postfix(GameStartManager __instance)
            {
                if (PlayerControl.LocalPlayer != null && SendVersion == false)
                {
                    SendVersion = true;
                }
            }
        }

        public static void ResetOverlays()
        {
            HideInfoOverlay();
            UnityEngine.Object.Destroy(meetingUnderlay);
            UnityEngine.Object.Destroy(infoUnderlay);
            UnityEngine.Object.Destroy(infoOverlayRules);
            UnityEngine.Object.Destroy(infoOverlayPlayer);
            UnityEngine.Object.Destroy(infoOverlayRoles);
            infoOverlayRules = infoOverlayPlayer = infoOverlayRoles = null;
            meetingUnderlay = infoUnderlay = null;
            OverlayShown = false;
        }

        public static bool InitializeOverlays()
        {
            HudManager hudManager = FastDestroyableSingleton<HudManager>.Instance;
            if (hudManager == null) return false;

            if (meetingUnderlay == null)
            {
                meetingUnderlay = UnityEngine.Object.Instantiate(hudManager.FullScreen, hudManager.transform);
                meetingUnderlay.transform.localPosition = new Vector3(0f, 0f, 20f);
                meetingUnderlay.gameObject.SetActive(true);
                meetingUnderlay.enabled = false;
            }

            if (infoUnderlay == null)
            {
                infoUnderlay = UnityEngine.Object.Instantiate(meetingUnderlay, hudManager.transform);
                infoUnderlay.transform.localPosition = new Vector3(0f, 0f, -900f);
                infoUnderlay.gameObject.SetActive(true);
                infoUnderlay.enabled = false;
            }

            if (infoOverlayRules == null)
            {
                infoOverlayRules = UnityEngine.Object.Instantiate(hudManager.TaskPanel.taskText, hudManager.transform);
                infoOverlayRules.fontSize = infoOverlayRules.fontSizeMin = infoOverlayRules.fontSizeMax = 1.15f;
                infoOverlayRules.autoSizeTextContainer = false;
                infoOverlayRules.enableWordWrapping = false;
                infoOverlayRules.alignment = TMPro.TextAlignmentOptions.TopLeft;
                infoOverlayRules.transform.position = Vector3.zero;
                infoOverlayRules.transform.localPosition = new Vector3(-3f, 1f, -910f);
                infoOverlayRules.transform.localScale = Vector3.one * 1.25f;
                infoOverlayRules.color = Palette.White;
                infoOverlayRules.enabled = false;
            }

            if (infoOverlayPlayer == null)
            {
                infoOverlayPlayer = UnityEngine.Object.Instantiate(infoOverlayRules, hudManager.transform);
                infoOverlayPlayer.maxVisibleLines = 28;
                infoOverlayPlayer.fontSize = infoOverlayPlayer.fontSizeMin = infoOverlayPlayer.fontSizeMax = 1.10f;
                infoOverlayPlayer.outlineWidth += 0.02f;
                infoOverlayPlayer.autoSizeTextContainer = false;
                infoOverlayPlayer.enableWordWrapping = false;
                infoOverlayPlayer.alignment = TMPro.TextAlignmentOptions.TopLeft;
                infoOverlayPlayer.transform.position = Vector3.zero;
                infoOverlayPlayer.transform.localPosition = new Vector3(0f, 1f, -910f);
                infoOverlayPlayer.transform.localScale = Vector3.one * 1.25f;
                infoOverlayPlayer.color = Palette.White;
                infoOverlayPlayer.enabled = false;
            }

            if (infoOverlayRoles == null)
            {
                infoOverlayRoles = UnityEngine.Object.Instantiate(infoOverlayRules, hudManager.transform);
                infoOverlayRoles.maxVisibleLines = 28;
                infoOverlayRoles.fontSize = infoOverlayRoles.fontSizeMin = infoOverlayRoles.fontSizeMax = 1f;
                infoOverlayRoles.outlineWidth += 0.02f;
                infoOverlayRoles.autoSizeTextContainer = false;
                infoOverlayRoles.enableWordWrapping = false;
                infoOverlayRoles.alignment = TMPro.TextAlignmentOptions.TopLeft;
                infoOverlayRoles.transform.position = Vector3.zero;
                infoOverlayRoles.transform.localPosition = new Vector3(2.7f, 1f, -910f);
                infoOverlayRoles.transform.localScale = Vector3.one * 1.25f;
                infoOverlayRoles.color = Palette.White;
                infoOverlayRoles.enabled = false;
            }

            return true;
        }

        public static void ShowinfoOverlay()
        {
            if (OverlayShown) return;

            HudManager hudManager = FastDestroyableSingleton<HudManager>.Instance;
            if (PlayerControl.LocalPlayer == null || hudManager == null)
                return;

            if (!InitializeOverlays()) return;

            if (MapBehaviour.Instance != null)
                MapBehaviour.Instance.Close();

            if (MeetingHud.Instance != null) hudManager.SetHudActive(false);

            OverlayShown = true;

            Transform parent;
            parent = hudManager.transform;

            infoUnderlay.transform.parent = parent;
            infoOverlayRules.transform.parent = parent;
            infoOverlayPlayer.transform.parent = parent;
            infoOverlayRoles.transform.parent = parent;

            infoUnderlay.color = new Color(0.1f, 0.1f, 0.1f, 0.88f);
            infoUnderlay.transform.localScale = new Vector3(9f, 5.3f, 1f);
            infoUnderlay.enabled = true;
            infoOverlayRules.enabled = true;
            infoOverlayPlayer.enabled = true;

            string rolesText = "";
            foreach (RoleInfo r in RoleInfo.getRoleInfoForPlayer(PlayerControl.LocalPlayer))
            {
                string roleOptions = r.RoleOptions;
                string roleDesc = r.FullDescription;
                rolesText += $"<size=150%>{r.NameColored}</size>" +
                    (roleDesc != "" ? $"\n{r.FullDescription}" : "") + "\n\n" +
                    (roleOptions != "" ? $"{roleOptions}\n\n" : "");
            }

            infoOverlayRoles.text = rolesText;
            if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
            {
                infoOverlayRoles.enabled = true;
            }
            else
            {
                infoOverlayRoles.enabled = false;
            }

            var underlayTransparent = new Color(0.1f, 0.1f, 0.1f, 0.0f);
            var underlayOpaque = new Color(0.1f, 0.1f, 0.1f, 0.88f);
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(0.2f, new Action<float>(t =>
            {
                infoUnderlay.color = Color.Lerp(underlayTransparent, underlayOpaque, t);
                infoOverlayRules.color = Color.Lerp(Palette.ClearWhite, Palette.White, t);
                infoOverlayPlayer.color = Color.Lerp(Palette.ClearWhite, Palette.White, t);
                infoOverlayRoles.color = Color.Lerp(Palette.ClearWhite, Palette.White, t);
            })));
        }

        public static void HideInfoOverlay()
        {
            if (!OverlayShown) return;

            if (MeetingHud.Instance == null && ShipStatus.Instance != null) FastDestroyableSingleton<HudManager>.Instance.SetHudActive(true);

            OverlayShown = false;
            var underlayTransparent = new Color(0.1f, 0.1f, 0.1f, 0.0f);
            var underlayOpaque = new Color(0.1f, 0.1f, 0.1f, 0.88f);

            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(0.2f, new Action<float>(t =>
            {
                if (infoUnderlay != null)
                {
                    infoUnderlay.color = Color.Lerp(underlayOpaque, underlayTransparent, t);
                    if (t >= 1.0f) infoUnderlay.enabled = false;
                }

                if (infoOverlayRules != null)
                {
                    infoOverlayRules.color = Color.Lerp(Palette.White, Palette.ClearWhite, t);
                    if (t >= 1.0f) infoOverlayRules.enabled = false;
                }

                if (infoOverlayPlayer != null)
                {
                    infoOverlayPlayer.color = Color.Lerp(Palette.White, Palette.ClearWhite, t);
                    if (t >= 1.0f) infoOverlayPlayer.enabled = false;
                }

                if (infoOverlayRoles != null)
                {
                    infoOverlayRoles.color = Color.Lerp(Palette.White, Palette.ClearWhite, t);
                    if (t >= 1.0f) infoOverlayRoles.enabled = false;
                }
            })));
        }

        public static void ToggleinfoOverlay()
        {
            if (OverlayShown)
                HideInfoOverlay();
            else
                ShowinfoOverlay();
        }

        [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
        public static class CustomOverlayKeybinds
        {
            public static void Postfix(KeyboardJoystick __instance)
            {
                if (Input.GetKeyDown(KeyCode.F3))
                {
                    ToggleinfoOverlay();
                }
            }
        }
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        public static class CustomOverlayUpdate
        {
            public static void Postfix(HudManager __instance)
            {
                if (!InitializeOverlays()) return;
                if (!OverlayShown) return;
                HudManager hudManager = FastDestroyableSingleton<HudManager>.Instance;
                if (PlayerControl.LocalPlayer == null || hudManager == null)
                    return;

                List<string> gameOptions = GameOptionsManager.Instance.CurrentGameOptions.Cast<NormalGameOptionsV07>().ToString().Split("\n", StringSplitOptions.RemoveEmptyEntries).ToList();
                infoOverlayRules.text = string.Join("\n", gameOptions);
                string PlayerText = ModTranslation.getString("PlatformTitle");
                foreach (InnerNet.ClientData Client in AmongUsClient.Instance.allClients.ToArray())
                {
                    if (Client == null) continue;
                    if (Client.Character == null) continue;
                    var player = Helpers.PlayerById(Client.Character.PlayerId);
                    var Platform = $"{Client.PlatformData.Platform}";

                    var PlayerName = Client.PlayerName.DeleteHTML();
                    var HEXcolor = Helpers.GetColorHEX(Client);
                    if (HEXcolor == "") HEXcolor = "FF000000";
                    PlayerText += $"\n<color=#{HEXcolor}>■</color>{PlayerName} : {Platform.Replace("Standalone", "")}";
                }
                infoOverlayPlayer.text = PlayerText;
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