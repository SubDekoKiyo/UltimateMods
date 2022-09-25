using UnityEngine;
using HarmonyLib;
using Hazel;
using TMPro;
using System;
using UltimateMods.Utilities;

namespace UltimateMods.Patches
{
    [Harmony]
    public static class CustomLobbyPatch
    {
        public static GameObject GSM;
        public static GameObject ClickText;
        public static GameObject UseButtons;
        public static TMPro.TextMeshPro NewsText;

        public static void ResetLobbyText()
        {
            UnityEngine.Object.Destroy(GSM);
            UnityEngine.Object.Destroy(ClickText);
            UnityEngine.Object.Destroy(NewsText);
            if (UseButtons.transform.localPosition.x == -9.3f)
                UseButtons.transform.localPosition = UseButtons.transform.localPosition - new Vector3(-9.3f, 0.3f, 0f);
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
        public static void Postfix(GameStartManager __instance)
        {
            GSM = __instance.gameObject;
            ClickText = GSM.transform.FindChild("GameRoomButton/GameRoomInfo_TMP").gameObject;
            UseButtons = GameObject.Find("Main Camera/Hud/Buttons/BottomRight");
            Vector3 GSMPos = GSM.transform.localPosition + new Vector3(0f, 0.15f, 0f);
            Vector3 ClickTextPos = ClickText.transform.localPosition + new Vector3(0f, 0.1f, 0f);
            Vector3 UseButtonsPos = UseButtons.transform.localPosition + new Vector3(-9.3f, 0.3f, 0f);

            if (GSM != null && ClickText != null && (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Joined))
            {
                ClickText.transform.localPosition = ClickTextPos;
                GSM.transform.localPosition = GSMPos;
                UseButtons.transform.localPosition = UseButtonsPos;
            }

            LobbyTextBox();
            LobbyNewsText();
            LobbySmartPhone();
        }

        public static void LobbyTextBox()
        {
            SpriteRenderer renderer;
            GameObject LobbyTextBox = new("LobbyTextBox");
            LobbyTextBox.transform.SetParent(GSM.transform);
            LobbyTextBox.transform.localPosition = new Vector3(-1.3f, -1.35f, 0.5f);
            LobbyTextBox.transform.localScale = new Vector3(2f, 1.2f, 1f);
            LobbyTextBox.SetActive(true);

            renderer = LobbyTextBox.AddComponent<SpriteRenderer>();
            renderer.sprite = Helpers.LoadSpriteFromResources("UltimateMods.Resources.LobbyTextBox.png", 200f);
        }

        public static void LobbySmartPhone()
        {
            var SmartPhone = GameObject.Find("Main Camera/Hud/FriendsList");
            if (SmartPhone == null) return;

            var LSmartPhone = UnityEngine.Object.Instantiate(SmartPhone, null);
            LSmartPhone.transform.localPosition = new Vector3(0f, 0f, 0.5f);
            LSmartPhone.transform.SetParent(GSM.transform);
            LSmartPhone.SetActive(true);

            var ClickToClose = GameObject.Find("Main Camera/Hud/GameStartManager/FriendsList(Clone)/ClickToClose");
            var Blur = GameObject.Find("Main Camera/Hud/GameStartManager/FriendsList(Clone)/Blur");
            var CloseButton = GameObject.Find("Main Camera/Hud/GameStartManager/FriendsList(Clone)/Menu/CloseButton");
            var FriendCode = GameObject.Find("Main Camera/Hud/GameStartManager/FriendsList(Clone)/Menu/FriendCode");
            var TabContents = GameObject.Find("Main Camera/Hud/GameStartManager/FriendsList(Clone)/Menu/Tab Contents");
            var Tabs = GameObject.Find("Main Camera/Hud/GameStartManager/FriendsList(Clone)/Menu/Tabs");
            var Divider = GameObject.Find("Main Camera/Hud/GameStartManager/FriendsList(Clone)/Menu/PhoneUI/Divider");
            var WhiteGrade = GameObject.Find("Main Camera/Hud/GameStartManager/FriendsList(Clone)/Menu/PhoneUI/White Grade");
            var HeaderGrade = GameObject.Find("Main Camera/Hud/GameStartManager/FriendsList(Clone)/Menu/PhoneUI/Header Grade");
            UnityEngine.GameObject.Destroy(ClickToClose);
            UnityEngine.GameObject.Destroy(Blur);
            UnityEngine.GameObject.Destroy(CloseButton);
            UnityEngine.GameObject.Destroy(FriendCode);
            UnityEngine.GameObject.Destroy(TabContents);
            UnityEngine.GameObject.Destroy(Tabs);
            UnityEngine.GameObject.Destroy(Divider);
            UnityEngine.GameObject.Destroy(WhiteGrade);
            UnityEngine.GameObject.Destroy(HeaderGrade);

            var LobbySmartPhone = GameObject.Find("Main Camera/Hud/GameStartManager/FriendsList(Clone)/Menu");
            LobbySmartPhone.transform.localPosition = new Vector3(LobbySmartPhone.transform.localPosition.x - 0.76f, LobbySmartPhone.transform.localPosition.y + 0.85f, 0.5f);
            LobbySmartPhone.transform.localScale = new Vector3(0.8f, 0.9f, 1f);
        }

        public static void LobbyNewsText()
        {
            HudManager hudManager = FastDestroyableSingleton<HudManager>.Instance;

            NewsText = UnityEngine.Object.Instantiate(hudManager.TaskText, hudManager.transform);
            NewsText.fontSize = NewsText.fontSizeMin = NewsText.fontSizeMax = 1.15f;
            NewsText.autoSizeTextContainer = false;
            NewsText.enableWordWrapping = false;
            NewsText.alignment = TMPro.TextAlignmentOptions.Right;
            NewsText.transform.localPosition = new Vector3(1.6f, -2.7f, 0.4f);
            NewsText.transform.localScale = Vector3.one * 1.25f;
            NewsText.outlineWidth += 0.02f;
            NewsText.color = Palette.White;
            NewsText.enabled = true;
            NewsText.transform.SetParent(GSM.transform);
            NewsText.text = "Ultimate Modsをプレイしていただき、ありがとうございます。";
        }
    }
}