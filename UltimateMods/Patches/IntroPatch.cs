namespace UltimateMods.Patches
{
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
    class IntroCutsceneOnDestroyPatch
    {
        public static PoolablePlayer playerPrefab;
        public static void Prefix(IntroCutscene __instance)
        {
            // Generate and initialize player icons
            if (PlayerControl.LocalPlayer != null && FastDestroyableSingleton<HudManager>.Instance != null)
            {
                Vector3 bottomLeft = new Vector3(-FastDestroyableSingleton<HudManager>.Instance.SettingsButton.transform.localPosition.x, -FastDestroyableSingleton<HudManager>.Instance.SettingsButton.transform.localPosition.y, FastDestroyableSingleton<HudManager>.Instance.SettingsButton.transform.localPosition.z);
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    GameData.PlayerInfo data = p.Data;
                    PoolablePlayer player = Object.Instantiate<PoolablePlayer>(__instance.PlayerPrefab, FastDestroyableSingleton<HudManager>.Instance.transform);
                    playerPrefab = __instance.PlayerPrefab;
                    p.SetPlayerMaterialColors(player.cosmetics.currentBodySprite.BodySprite);
                    player.SetSkin(data.DefaultOutfit.SkinId, data.DefaultOutfit.ColorId);
                    player.cosmetics.SetHat(data.DefaultOutfit.HatId, data.DefaultOutfit.ColorId);
                    // PlayerControl.SetPetImage(data.DefaultOutfit.PetId, data.DefaultOutfit.ColorId, player.PetSlot);
                    player.cosmetics.nameText.text = data.PlayerName;
                    player.cosmetics.nameText.transform.localPosition += new Vector3(0f, -0.5f, 0f);
                    player.SetFlipX(true);
                    Options.PlayerIcons[p.PlayerId] = player;

                    if (PlayerControl.LocalPlayer.IsRole(RoleId.BountyHunter))
                    {
                        player.transform.localPosition = bottomLeft + new Vector3(0.2f, 0.25f, 0);
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
                        Vector3 bottomLeft = new Vector3(-FastDestroyableSingleton<HudManager>.Instance.SettingsButton.transform.localPosition.x, -FastDestroyableSingleton<HudManager>.Instance.SettingsButton.transform.localPosition.y, FastDestroyableSingleton<HudManager>.Instance.SettingsButton.transform.localPosition.z) + new Vector3(-0.25f, 1f, 0);
                        BountyHunter.CooldownTimer = Object.Instantiate<TextMeshPro>(FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText, FastDestroyableSingleton<HudManager>.Instance.transform);
                        BountyHunter.CooldownTimer.alignment = TextAlignmentOptions.Center;
                        BountyHunter.CooldownTimer.transform.localScale = Vector3.one * 0.8f;
                        BountyHunter.CooldownTimer.transform.localPosition = bottomLeft + new Vector3(0.45f, -0.25f, -1f);
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
            List<RoleInfo> infos = RoleInfoList.GetRoleInfoForPlayer(PlayerControl.LocalPlayer);
            RoleInfo roleInfo = infos.Where(info => info.RoleId != RoleId.NoRole).FirstOrDefault();
            if (roleInfo == null) return;
            if (PlayerControl.LocalPlayer.IsNeutral())
            {
                __instance.BackgroundBar.material.color = roleInfo.RoleColor;
                __instance.TeamTitle.text = roleInfo.Name;
                __instance.TeamTitle.color = roleInfo.RoleColor;
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
                List<RoleInfo> infos = RoleInfoList.GetRoleInfoForPlayer(PlayerControl.LocalPlayer, new RoleId[] { });
                RoleInfo roleInfo = infos.FirstOrDefault();

                Helpers.Log($"{roleInfo.Name}");
                Helpers.Log($"{roleInfo.IntroDescription}");

                __instance.YouAreText.color = roleInfo.RoleColor;
                __instance.RoleText.text = roleInfo.Name;
                __instance.RoleText.color = roleInfo.RoleColor;
                __instance.RoleBlurbText.text = roleInfo.IntroDescription;
                __instance.RoleBlurbText.color = roleInfo.RoleColor;

                if (PlayerControl.LocalPlayer.IsRole(RoleId.Madmate))
                {
                    __instance.YouAreText.color = ImpostorRed;
                    __instance.RoleText.text = LocalizationManager.GetString(TransKey.Madmate);
                    __instance.RoleText.color = ImpostorRed;
                    __instance.RoleBlurbText.text = LocalizationManager.GetString(TransKey.MadmateIntro);
                    __instance.RoleBlurbText.color = ImpostorRed;
                }

                if (PlayerControl.LocalPlayer.HasModifier(ModifierId.Opportunist))
                {
                    __instance.RoleBlurbText.text += "\n" + Helpers.cs(OpportunistGreen, String.Format(LocalizationManager.GetString(TransKey.OPIntro)));
                }

                if (PlayerControl.LocalPlayer.HasModifier(ModifierId.Watcher))
                {
                    __instance.RoleBlurbText.text += "\n" + Helpers.cs(WatcherPurple, String.Format(LocalizationManager.GetString(TransKey.WTIntro)));
                }

                if (PlayerControl.LocalPlayer.HasModifier(ModifierId.Sunglasses))
                {
                    __instance.RoleBlurbText.text += "\n" + Helpers.cs(SunglassesGray, String.Format(LocalizationManager.GetString(TransKey.SGIntro)));
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