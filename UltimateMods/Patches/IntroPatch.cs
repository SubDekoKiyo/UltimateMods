using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.IL2CPP.Utils.Collections;
using HarmonyLib;
using UnityEngine;
using UltimateMods.Utilities;
using UltimateMods.Roles;
using static UltimateMods.ColorDictionary;

namespace UltimateMods.Patches
{
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
    class IntroCutsceneOnDestroyPatch
    {
        public static PoolablePlayer playerPrefab;
        public static void Prefix(IntroCutscene __instance)
        {
            // Generate and initialize player icons
            if (PlayerControl.LocalPlayer != null && HudManager.Instance != null)
            {
                Vector3 bottomLeft = new Vector3(-HudManager.Instance.UseButton.transform.localPosition.x, HudManager.Instance.UseButton.transform.localPosition.y, HudManager.Instance.UseButton.transform.localPosition.z);
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    GameData.PlayerInfo data = p.Data;
                    PoolablePlayer player = UnityEngine.Object.Instantiate<PoolablePlayer>(__instance.PlayerPrefab, FastDestroyableSingleton<HudManager>.Instance.transform);
                    playerPrefab = __instance.PlayerPrefab;
                    p.SetPlayerMaterialColors(player.cosmetics.currentBodySprite.BodySprite);
                    player.SetSkin(data.DefaultOutfit.SkinId, data.DefaultOutfit.ColorId);
                    player.cosmetics.SetHat(data.DefaultOutfit.HatId, data.DefaultOutfit.ColorId);
                    // PlayerControl.SetPetImage(data.DefaultOutfit.PetId, data.DefaultOutfit.ColorId, player.PetSlot);
                    player.cosmetics.nameText.text = data.PlayerName;
                    player.SetFlipX(true);
                    MapOptions.PlayerIcons[p.PlayerId] = player;

                    if (PlayerControl.LocalPlayer.isRole(RoleType.BountyHunter))
                    {
                        player.transform.localPosition = bottomLeft + new Vector3(-0.25f, 0f, 0);
                        player.transform.localScale = Vector3.one * 0.4f;
                        player.gameObject.SetActive(false);
                    }
                    else
                        player.gameObject.SetActive(false);
                }
            }

            if (BountyHunter.exists)
            {
                foreach (var bountyHunter in BountyHunter.allPlayers)
                {
                    if (BountyHunter.Bounty != null && PlayerControl.LocalPlayer == bountyHunter)
                    {
                        BountyHunter.BountyUpdateTimer = 0f;
                        if (FastDestroyableSingleton<HudManager>.Instance != null)
                        {
                            Vector3 bottomLeft = new Vector3(-FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.x, FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.y, FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.z) + new Vector3(-0.25f, 1f, 0);
                            BountyHunter.CooldownTimer = UnityEngine.Object.Instantiate<TMPro.TextMeshPro>(FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText, FastDestroyableSingleton<HudManager>.Instance.transform);
                            BountyHunter.CooldownTimer.alignment = TMPro.TextAlignmentOptions.Center;
                            BountyHunter.CooldownTimer.transform.localPosition = bottomLeft + new Vector3(0f, -1f, -1f);
                            BountyHunter.CooldownTimer.gameObject.SetActive(true);
                        }
                    }
                }
            }

            if (AmongUsClient.Instance.AmHost)
            {
                Adversity.CheckAndAdversityState();
            }

            Arsonist.UpdateIcons();
        }
    }

    [HarmonyPatch]
    class IntroPatch
    {
        public static void setupIntroTeamIcons(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {
            // Intro solo teams
            if (PlayerControl.LocalPlayer.IsNeutral())
            {
                var soloTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                soloTeam.Add(PlayerControl.LocalPlayer);
                yourTeam = soloTeam;
            }
        }

        public static void setupIntroTeam(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {
            List<RoleInfo> infos = RoleInfo.getRoleInfoForPlayer(PlayerControl.LocalPlayer);
            RoleInfo roleInfo = infos.Where(info => info.roleType != RoleType.NoRole).FirstOrDefault();
            if (roleInfo == null) return;
            if (PlayerControl.LocalPlayer.IsNeutral())
            {
                __instance.BackgroundBar.material.color = roleInfo.color;
                __instance.TeamTitle.text = roleInfo.Name;
                __instance.TeamTitle.color = roleInfo.color;
                __instance.ImpostorText.text = "";
            }
            if (PlayerControl.LocalPlayer.IsYakuza())
            {
                __instance.BackgroundBar.material.color = YakuzaBlue;
                __instance.TeamTitle.text = ModTranslation.getString("Yakuza");
                __instance.TeamTitle.color = YakuzaBlue;
                __instance.ImpostorText.text = "";
            }
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.ShowRole))]
        class SetUpRoleTextPatch
        {
            public static bool Prefix(IntroCutscene __instance, ref Il2CppSystem.Collections.IEnumerator __result)
            {
                if (!CustomOptionsH.ActivateModRoles.getBool()) return true; // Don't override the intro of the vanilla roles
                __result = SetupRole(__instance).WrapToIl2Cpp();
                return false;
            }

            private static IEnumerator SetupRole(IntroCutscene __instance)
            {
                List<RoleInfo> infos = RoleInfo.getRoleInfoForPlayer(PlayerControl.LocalPlayer, new RoleType[] { });
                RoleInfo roleInfo = infos.FirstOrDefault();

                Helpers.Log($"{roleInfo.Name}");
                Helpers.Log($"{roleInfo.IntroDescription}");

                __instance.YouAreText.color = roleInfo.color;
                __instance.RoleText.text = roleInfo.Name;
                __instance.RoleText.color = roleInfo.color;
                __instance.RoleBlurbText.text = roleInfo.IntroDescription;
                __instance.RoleBlurbText.color = roleInfo.color;

                if (PlayerControl.LocalPlayer.isRole(RoleType.Madmate))
                {
                    __instance.YouAreText.color = ImpostorRed;
                    __instance.RoleText.text = ModTranslation.getString("Madmate");
                    __instance.RoleText.color = ImpostorRed;
                    __instance.RoleBlurbText.text = ModTranslation.getString("MadmateIntro");
                    __instance.RoleBlurbText.color = ImpostorRed;
                }

                // 従来処理
                SoundManager.Instance.PlaySound(PlayerControl.LocalPlayer.Data.Role.IntroSound, false, 1f);
                __instance.YouAreText.gameObject.SetActive(true);
                __instance.RoleText.gameObject.SetActive(true);
                __instance.RoleBlurbText.gameObject.SetActive(true);

                if (__instance.ourCrewmate == null)
                {
                    __instance.ourCrewmate = __instance.CreatePlayer(0, 1, PlayerControl.LocalPlayer.Data, false);
                    __instance.ourCrewmate.gameObject.SetActive(false);
                }
                __instance.ourCrewmate.gameObject.SetActive(true);
                __instance.ourCrewmate.transform.localPosition = new Vector3(0f, -1.05f, -18f);
                __instance.ourCrewmate.transform.localScale = new Vector3(1f, 1f, 1f);
                yield return new WaitForSeconds(2.5f);
                __instance.YouAreText.gameObject.SetActive(false);
                __instance.RoleText.gameObject.SetActive(false);
                __instance.RoleBlurbText.gameObject.SetActive(false);
                __instance.ourCrewmate.gameObject.SetActive(false);

                yield break;
            }
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
        class BeginCrewmatePatch
        {
            public static void Prefix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay)
            {
                setupIntroTeamIcons(__instance, ref teamToDisplay);
            }

            public static void Postfix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay)
            {
                setupIntroTeam(__instance, ref teamToDisplay);
            }
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
        class BeginImpostorPatch
        {
            public static void Prefix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
            {
                setupIntroTeamIcons(__instance, ref yourTeam);
            }

            public static void Postfix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
            {
                setupIntroTeam(__instance, ref yourTeam);
            }
        }

        [HarmonyPatch(typeof(Constants), nameof(Constants.ShouldHorseAround))]
        public static class ShouldAlwaysHorseAround
        {
            public static bool isHorseMode;
            public static bool Prefix(ref bool __result)
            {
                if (isHorseMode != MapOptions.enableHorseMode && LobbyBehaviour.Instance != null) __result = isHorseMode;
                else
                {
                    __result = MapOptions.enableHorseMode;
                    isHorseMode = MapOptions.enableHorseMode;
                }
                return false;
            }
        }
    }
}