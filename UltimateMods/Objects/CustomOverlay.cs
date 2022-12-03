// Source Code from Town of Plus & Town of Super

using HarmonyLib;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UltimateMods.Roles;
using UltimateMods.Utilities;

namespace UltimateMods.Objects
{
    [Harmony]
    public class CustomOverlays
    {
        public static Dictionary<int, PlayerVersion> playerVersions = new Dictionary<int, PlayerVersion>();
        private static SpriteRenderer MeetingUnderlay;
        private static SpriteRenderer InfoUnderlay;
        private static TMPro.TextMeshPro InfoOverlayRules;
        private static TMPro.TextMeshPro InfoOverlayPlayer;
        private static TMPro.TextMeshPro InfoOverlayRoles;

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
            UnityEngine.Object.Destroy(MeetingUnderlay);
            UnityEngine.Object.Destroy(InfoUnderlay);
            UnityEngine.Object.Destroy(InfoOverlayRules);
            UnityEngine.Object.Destroy(InfoOverlayPlayer);
            UnityEngine.Object.Destroy(InfoOverlayRoles);
            InfoOverlayRules = InfoOverlayPlayer = InfoOverlayRoles = null;
            MeetingUnderlay = InfoUnderlay = null;
            OverlayShown = false;
        }

        public static bool InitializeOverlays()
        {
            HudManager hudManager = FastDestroyableSingleton<HudManager>.Instance;
            if (hudManager == null) return false;

            if (MeetingUnderlay == null)
            {
                MeetingUnderlay = UnityEngine.Object.Instantiate(hudManager.FullScreen, hudManager.transform);
                MeetingUnderlay.transform.localPosition = new Vector3(0f, 0f, 20f);
                MeetingUnderlay.gameObject.SetActive(true);
                MeetingUnderlay.enabled = false;
            }

            if (InfoUnderlay == null)
            {
                InfoUnderlay = UnityEngine.Object.Instantiate(MeetingUnderlay, hudManager.transform);
                InfoUnderlay.transform.localPosition = new Vector3(0f, 0f, -900f);
                InfoUnderlay.gameObject.SetActive(true);
                InfoUnderlay.enabled = false;
            }

            if (InfoOverlayRules == null)
            {
                InfoOverlayRules = UnityEngine.Object.Instantiate(hudManager.TaskText, hudManager.transform);
                InfoOverlayRules.fontSize = InfoOverlayRules.fontSizeMin = InfoOverlayRules.fontSizeMax = 1.15f;
                InfoOverlayRules.autoSizeTextContainer = false;
                InfoOverlayRules.enableWordWrapping = false;
                InfoOverlayRules.alignment = TMPro.TextAlignmentOptions.TopLeft;
                InfoOverlayRules.transform.position = Vector3.zero;
                InfoOverlayRules.transform.localPosition = new Vector3(-3f, 1f, -910f);
                InfoOverlayRules.transform.localScale = Vector3.one * 1.25f;
                InfoOverlayRules.color = Palette.White;
                InfoOverlayRules.enabled = false;
            }

            if (InfoOverlayPlayer == null)
            {
                InfoOverlayPlayer = UnityEngine.Object.Instantiate(InfoOverlayRules, hudManager.transform);
                InfoOverlayPlayer.maxVisibleLines = 28;
                InfoOverlayPlayer.fontSize = InfoOverlayPlayer.fontSizeMin = InfoOverlayPlayer.fontSizeMax = 1.10f;
                InfoOverlayPlayer.outlineWidth += 0.02f;
                InfoOverlayPlayer.autoSizeTextContainer = false;
                InfoOverlayPlayer.enableWordWrapping = false;
                InfoOverlayPlayer.alignment = TMPro.TextAlignmentOptions.TopLeft;
                InfoOverlayPlayer.transform.position = Vector3.zero;
                InfoOverlayPlayer.transform.localPosition = new Vector3(0f, 1f, -910f);
                InfoOverlayPlayer.transform.localScale = Vector3.one * 1.25f;
                InfoOverlayPlayer.color = Palette.White;
                InfoOverlayPlayer.enabled = false;
            }

            if (InfoOverlayRoles == null)
            {
                InfoOverlayRoles = UnityEngine.Object.Instantiate(InfoOverlayRules, hudManager.transform);
                InfoOverlayRoles.maxVisibleLines = 28;
                InfoOverlayRoles.fontSize = InfoOverlayRoles.fontSizeMin = InfoOverlayRoles.fontSizeMax = 1f;
                InfoOverlayRoles.outlineWidth += 0.02f;
                InfoOverlayRoles.autoSizeTextContainer = false;
                InfoOverlayRoles.enableWordWrapping = false;
                InfoOverlayRoles.alignment = TMPro.TextAlignmentOptions.TopLeft;
                InfoOverlayRoles.transform.position = Vector3.zero;
                InfoOverlayRoles.transform.localPosition = new Vector3(2.7f, 1f, -910f);
                InfoOverlayRoles.transform.localScale = Vector3.one * 1.25f;
                InfoOverlayRoles.color = Palette.White;
                InfoOverlayRoles.enabled = false;
            }

            return true;
        }

        public static void ShowInfoOverlay()
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

            InfoUnderlay.transform.parent = parent;
            InfoOverlayRules.transform.parent = parent;
            InfoOverlayPlayer.transform.parent = parent;
            InfoOverlayRoles.transform.parent = parent;

            InfoUnderlay.color = new Color(0.1f, 0.1f, 0.1f, 0.88f);
            InfoUnderlay.transform.localScale = new Vector3(9f, 5.3f, 1f);
            InfoUnderlay.enabled = true;
            InfoOverlayRules.enabled = true;
            InfoOverlayPlayer.enabled = true;

            string rolesText = "";
            foreach (RoleInfo r in RoleInfo.getRoleInfoForPlayer(PlayerControl.LocalPlayer))
            {
                string roleOptions = r.RoleOptions;
                string roleDesc = r.FullDescription;
                rolesText += $"<size=150%>{r.NameColored}</size>" +
                    (roleDesc != "" ? $"\n{r.FullDescription}" : "") + "\n\n" +
                    (roleOptions != "" ? $"{roleOptions}\n\n" : "");
            }

            InfoOverlayRoles.text = rolesText;
            if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
            {
                InfoOverlayRoles.enabled = true;
            }
            else
            {
                InfoOverlayRoles.enabled = false;
            }

            var underlayTransparent = new Color(0.1f, 0.1f, 0.1f, 0.0f);
            var underlayOpaque = new Color(0.1f, 0.1f, 0.1f, 0.88f);
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(0.2f, new Action<float>(t =>
            {
                InfoUnderlay.color = Color.Lerp(underlayTransparent, underlayOpaque, t);
                InfoOverlayRules.color = Color.Lerp(Palette.ClearWhite, Palette.White, t);
                InfoOverlayPlayer.color = Color.Lerp(Palette.ClearWhite, Palette.White, t);
                InfoOverlayRoles.color = Color.Lerp(Palette.ClearWhite, Palette.White, t);
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
                if (InfoUnderlay != null)
                {
                    InfoUnderlay.color = Color.Lerp(underlayOpaque, underlayTransparent, t);
                    if (t >= 1.0f) InfoUnderlay.enabled = false;
                }

                if (InfoOverlayRules != null)
                {
                    InfoOverlayRules.color = Color.Lerp(Palette.White, Palette.ClearWhite, t);
                    if (t >= 1.0f) InfoOverlayRules.enabled = false;
                }

                if (InfoOverlayPlayer != null)
                {
                    InfoOverlayPlayer.color = Color.Lerp(Palette.White, Palette.ClearWhite, t);
                    if (t >= 1.0f) InfoOverlayPlayer.enabled = false;
                }

                if (InfoOverlayRoles != null)
                {
                    InfoOverlayRoles.color = Color.Lerp(Palette.White, Palette.ClearWhite, t);
                    if (t >= 1.0f) InfoOverlayRoles.enabled = false;
                }
            })));
        }

        public static void ToggleInfoOverlay()
        {
            if (OverlayShown)
                HideInfoOverlay();
            else
                ShowInfoOverlay();
        }

        [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
        public static class CustomOverlayKeybinds
        {
            public static void Postfix(KeyboardJoystick __instance)
            {
                if (Input.GetKeyDown(KeyCode.F3))
                {
                    ToggleInfoOverlay();
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

                GameOptionsData o = PlayerControl.GameOptions;
                List<string> gameOptions = o.ToString().Split("\n", StringSplitOptions.RemoveEmptyEntries).ToList();
                InfoOverlayRules.text = string.Join("\n", gameOptions);
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
                InfoOverlayPlayer.text = PlayerText;
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