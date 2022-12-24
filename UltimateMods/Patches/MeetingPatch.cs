namespace UltimateMods.Patches
{
    [HarmonyPatch]
    class MeetingHudPatch
    {
        private const float scale = 0.65f;
        private static Sprite blankNameplate = null;
        public static bool nameplatesChanged = true;

        public static void updateNameplate(PlayerVoteArea pva, byte playerId = Byte.MaxValue)
        {
            blankNameplate = blankNameplate ?? HatManager.Instance.GetNamePlateById("nameplate_NoPlate")?.viewData?.viewData?.Image;

            var nameplate = blankNameplate;
            if (!HideNameplates)
            {
                var p = Helpers.PlayerById(playerId != Byte.MaxValue ? playerId : pva.TargetPlayerId);
                var nameplateId = p?.CurrentOutfit?.NamePlateId;
                nameplate = HatManager.Instance.GetNamePlateById(nameplateId)?.viewData?.viewData?.Image;
            }
            pva.Background.sprite = nameplate;
        }

        [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetCosmetics))]
        class PlayerVoteAreaCosmetics
        {
            static void Postfix(PlayerVoteArea __instance, GameData.PlayerInfo playerInfo)
            {
                updateNameplate(__instance, playerInfo.PlayerId);
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.BloopAVoteIcon))]
        class MeetingHudBloopAVoteIconPatch
        {
            public static bool Prefix(MeetingHud __instance, [HarmonyArgument(0)] GameData.PlayerInfo voterPlayer, [HarmonyArgument(1)] int index, [HarmonyArgument(2)] Transform parent)
            {
                SpriteRenderer spriteRenderer = UnityEngine.Object.Instantiate<SpriteRenderer>(__instance.PlayerVotePrefab);
                int cId = voterPlayer.DefaultOutfit.ColorId;
                if (!(!GameManager.Instance.LogicOptions.currentGameOptions.GetBool(BoolOptionNames.AnonymousVotes) || (PlayerControl.LocalPlayer.Data.IsDead && Options.GhostsSeeVotes) || PlayerControl.LocalPlayer.HasModifier(ModifierId.Watcher) || (PlayerControl.LocalPlayer.IsRole(RoleId.Adversity) && CustomRolesH.AdversityAdversityStateCanSeeVotes.getBool())))
                    voterPlayer.Object.SetColor(6);
                voterPlayer.Object.SetPlayerMaterialColors(spriteRenderer);
                spriteRenderer.transform.SetParent(parent);
                spriteRenderer.transform.localScale = Vector3.zero;
                __instance.StartCoroutine(Effects.Bloop((float)index * 0.3f, spriteRenderer.transform, 1f, 0.5f));
                parent.GetComponent<VoteSpreader>().AddVote(spriteRenderer);
                voterPlayer.Object.SetColor(cId);
                return false;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
        class MeetingHudUpdatePatch
        {
            static void Postfix(MeetingHud __instance)
            {
                if (nameplatesChanged)
                {
                    foreach (var pva in __instance.playerStates)
                    {
                        updateNameplate(pva);
                    }
                    nameplatesChanged = false;
                }

                if (__instance.state == MeetingHud.VoteStates.Animating)
                    return;

                // Deactivate skip Button if skipping on emergency meetings is disabled
                if (BlockSkippingInEmergencyMeetings)
                    __instance.SkipVoteButton?.gameObject?.SetActive(false);

                // This fixes a bug with the original game where pressing the button and a kill happens simultaneously
                // results in bodies sometimes being created *after* the meeting starts, marking them as dead and
                // removing the corpses so there's no random corpses leftover afterwards
                foreach (DeadBody b in UnityEngine.Object.FindObjectsOfType<DeadBody>())
                {
                    if (b == null) continue;

                    foreach (PlayerVoteArea pva in __instance.playerStates)
                    {
                        if (pva == null) continue;

                        if (pva.TargetPlayerId == b?.ParentId && !pva.AmDead)
                        {
                            pva?.SetDead(pva.DidReport, true);
                            pva?.Overlay?.gameObject?.SetActive(true);
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
        class MeetingCalculateVotesPatch
        {
            private static Dictionary<byte, int> CalculateVotes(MeetingHud __instance)
            {
                Dictionary<byte, int> dictionary = new();
                for (int i = 0; i < __instance.playerStates.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    byte votedFor = playerVoteArea.VotedFor;
                    if (votedFor != 252 && votedFor != 255 && votedFor != 254)
                    {
                        int additionalVotes = 1;
                        PlayerControl player = Helpers.PlayerById((byte)playerVoteArea.TargetPlayerId);
                        if (player == null || player.Data == null || player.Data.IsDead || player.Data.Disconnected) continue;
                        foreach (var mayor in Mayor.allPlayers) additionalVotes = (Mayor.exists && mayor.PlayerId == playerVoteArea.TargetPlayerId) ? Mayor.NumVotes : 1; // May
                        if (dictionary.TryGetValue(votedFor, out int currentVotes)) dictionary[votedFor] = currentVotes + additionalVotes;
                        else dictionary[votedFor] = additionalVotes;
                    }
                }
                return dictionary;
            }

            static bool Prefix(MeetingHud __instance)
            {
                if (__instance.playerStates.All((PlayerVoteArea ps) => ps.AmDead || ps.DidVote))
                {

                    Dictionary<byte, int> self = CalculateVotes(__instance);
                    bool tie;
                    KeyValuePair<byte, int> max = self.MaxPair(out tie);
                    GameData.PlayerInfo exiled = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(v => !tie && v.PlayerId == max.Key && !v.IsDead);

                    MeetingHud.VoterState[] array = new MeetingHud.VoterState[__instance.playerStates.Length];
                    for (int i = 0; i < __instance.playerStates.Length; i++)
                    {
                        PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                        array[i] = new MeetingHud.VoterState
                        {
                            VoterId = playerVoteArea.TargetPlayerId,
                            VotedForId = playerVoteArea.VotedFor
                        };
                    }

                    // RPCVotingComplete
                    __instance.RpcVotingComplete(array, exiled, tie);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Select))]
        class MeetingHudSelectPatch
        {
            public static bool Prefix(ref bool __result, MeetingHud __instance, [HarmonyArgument(0)] int suspectStateIdx)
            {
                __result = false;
                if (NoVoteIsSelfVote && PlayerControl.LocalPlayer.PlayerId == suspectStateIdx) return false;
                if (BlockSkippingInEmergencyMeetings && suspectStateIdx == -1) return false;

                return true;
            }
        }

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.OpenMeetingRoom))]
        class OpenMeetingPatch
        {
            public static void Prefix(HudManager __instance)
            {
                UltimateMods.OnMeetingStart();
            }
        }

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive))]
        class HudManagerSetHudActive
        {
            public static void Postfix(HudManager __instance)
            {
                FastDestroyableSingleton<HudManager>.Instance.transform.FindChild("TaskDisplay").FindChild("TaskPanel").gameObject.SetActive(true);
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.PopulateResults))]
        class MeetingHudPopulateVotesPatch
        {

            static bool Prefix(MeetingHud __instance, Il2CppStructArray<MeetingHud.VoterState> states)
            {
                __instance.TitleText.text = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MeetingVotingResults, new Il2CppReferenceArray<Il2CppSystem.Object>(0));
                int num = 0;
                for (int i = 0; i < __instance.playerStates.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    byte targetPlayerId = playerVoteArea.TargetPlayerId;

                    playerVoteArea.ClearForResults();
                    int num2 = 0;
                    // bool mayorFirstVoteDisplayed = false;
                    Dictionary<int, int> votesApplied = new Dictionary<int, int>();
                    for (int j = 0; j < states.Length; j++)
                    {
                        MeetingHud.VoterState voterState = states[j];
                        PlayerControl voter = Helpers.PlayerById(voterState.VoterId);
                        if (voter == null) continue;

                        GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(voterState.VoterId);
                        if (playerById == null)
                        {
                            UnityEngine.Debug.LogError(string.Format("Couldn't find player info for voter: {0}", voterState.VoterId));
                        }
                        else if (i == 0 && voterState.SkippedVote && !playerById.IsDead)
                        {
                            __instance.BloopAVoteIcon(playerById, num, __instance.SkippedVoting.transform);
                            num++;
                        }
                        else if (voterState.VotedForId == targetPlayerId && !playerById.IsDead)
                        {
                            __instance.BloopAVoteIcon(playerById, num2, playerVoteArea.transform);
                            num2++;
                        }

                        if (!votesApplied.ContainsKey(voter.PlayerId))
                            votesApplied[voter.PlayerId] = 0;

                        votesApplied[voter.PlayerId]++;
                        // Major vote, redo this iteration to place a second vote
                        foreach (var mayor in Mayor.allPlayers) if (Mayor.exists && voter.PlayerId == mayor.PlayerId && votesApplied[voter.PlayerId] < Mayor.NumVotes) j--;
                    }
                }
                return false;
            }
        }
    }
}