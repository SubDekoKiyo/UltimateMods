using System;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UltimateMods.Modules;
using UltimateMods.Utilities;

namespace UltimateMods.Roles
{
    [HarmonyPatch]
    public class EvilHacker : RoleBase<EvilHacker>
    {
        private static CustomButton AdminButton;
        public static Sprite AdminButtonSprite;

        public static bool canHasBetterAdmin { get { return CustomRolesH.EvilHackerCanHasBetterAdmin.getBool(); } }

        public EvilHacker()
        {
            RoleType = roleId = RoleType.EvilHacker;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static Sprite GetButtonSprite()
        {
            byte mapId = PlayerControl.GameOptions.MapId;
            UseButtonSettings button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.PolusAdminButton]; // Polus
            if (mapId == 0 || mapId == 3) button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.AdminMapButton]; // Skeld
            else if (mapId == 1) button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.MIRAAdminButton]; // Mira HQ
            else if (mapId == 4) button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.AirshipAdminButton]; // Airship
            AdminButtonSprite = button.Image;
            return AdminButtonSprite;
        }

        public static void MakeButtons(HudManager hm)
        {
            AdminButton = new CustomButton(
                () =>
                {
                    PlayerControl.LocalPlayer.NetTransform.Halt();
                    Action<MapBehaviour> tmpAction = (MapBehaviour m) => { m.ShowCountOverlay(); };
                    DestroyableSingleton<HudManager>.Instance.ShowMap(tmpAction);
                },
                () =>
                {
                    return PlayerControl.LocalPlayer.isRole(RoleType.EvilHacker) &&
                    PlayerControl.LocalPlayer.IsAlive();
                },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () => { },
                EvilHacker.GetButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                hm,
                hm.KillButton,
                KeyCode.F,
                false,
                0f,
                () => { },
                PlayerControl.GameOptions.MapId == 3,
                DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Admin)
            );
        }

        public static void SetButtonCooldowns() { }

        public static void Clear()
        {
            players = new List<EvilHacker>();
        }
    }
}