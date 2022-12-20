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
                    Options.PlayerIcons[p.PlayerId] = player;

                    if (PlayerControl.LocalPlayer.IsRole(RoleId.BountyHunter))
                    {
                        player.transform.localPosition = bottomLeft + new Vector3(-0.25f, 0f, 0);
                        player.transform.localScale = Vector3.one * 0.4f;
                        player.gameObject.SetActive(false);
                    }
                    else player.gameObject.SetActive(false);
                }
            }

            if (BountyHunter.exists)
            {
                if (BountyHunter.Bounty != null && PlayerControl.LocalPlayer.IsRole(RoleId.BountyHunter))
                {
                    BountyHunter.BountyUpdateTimer = 0f;
                    if (FastDestroyableSingleton<HudManager>.Instance != null)
                    {
                        Vector3 bottomLeft = new Vector3(-FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.x, FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.y, FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.z) + new Vector3(-0.25f, 1f, 0);
                        BountyHunter.CooldownTimer = UnityEngine.Object.Instantiate<TextMeshPro>(FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText, FastDestroyableSingleton<HudManager>.Instance.transform);
                        BountyHunter.CooldownTimer.alignment = TextAlignmentOptions.Center;
                        BountyHunter.CooldownTimer.transform.localPosition = bottomLeft + new Vector3(0f, -1f, -1f);
                        BountyHunter.CooldownTimer.gameObject.SetActive(true);
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
        public static RoleId roleId = PlayerControl.LocalPlayer.GetRoleId();
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
            if (PlayerControl.LocalPlayer.IsNeutral())
            {
                __instance.BackgroundBar.material.color = PlayerControl.LocalPlayer.GetRoleColor(roleId);
                __instance.TeamTitle.text = PlayerControl.LocalPlayer.GetTranslatedRoleString(roleId);
                __instance.TeamTitle.color = PlayerControl.LocalPlayer.GetRoleColor(roleId);
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
                __instance.YouAreText.color = PlayerControl.LocalPlayer.GetRoleColor(roleId);
                __instance.RoleText.text = PlayerControl.LocalPlayer.GetTranslatedRoleString(roleId);
                __instance.RoleText.color = PlayerControl.LocalPlayer.GetRoleColor(roleId);
                __instance.RoleBlurbText.text = RoleManagement.GetRoleIntroDesc(roleId);
                __instance.RoleBlurbText.color = PlayerControl.LocalPlayer.GetRoleColor(roleId);

                if (PlayerControl.LocalPlayer.IsRole(RoleId.Madmate))
                {
                    __instance.YouAreText.color = ImpostorRed;
                    __instance.RoleText.text = ModTranslation.getString("Madmate");
                    __instance.RoleText.color = ImpostorRed;
                    __instance.RoleBlurbText.text = ModTranslation.getString("MadmateIntro");
                    __instance.RoleBlurbText.color = ImpostorRed;
                }

                if (PlayerControl.LocalPlayer.HasModifier(ModifierId.Opportunist))
                {
                    __instance.RoleBlurbText.text += "\n" + Helpers.cs(OpportunistGreen, String.Format(ModTranslation.getString("OPIntro")));
                }

                if (PlayerControl.LocalPlayer.HasModifier(ModifierId.Watcher))
                {
                    __instance.RoleBlurbText.text += "\n" + Helpers.cs(WatcherPurple, String.Format(ModTranslation.getString("WTIntro")));
                }

                if (PlayerControl.LocalPlayer.HasModifier(ModifierId.Sunglasses))
                {
                    __instance.RoleBlurbText.text += "\n" + Helpers.cs(SunglassesGray, String.Format(ModTranslation.getString("SGIntro")));
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
    }
}