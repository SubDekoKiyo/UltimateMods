namespace UltimateMods.Roles.Patches;

public static class ButtonPatches
{
    public static class BlockButtonPatch
    {
        public static bool IsBlocked(PlayerTask task, PlayerControl pc)
        {
            if (task == null || pc == null || pc != PlayerControl.LocalPlayer) return false;

            bool isLights = task.TaskType == TaskTypes.FixLights;
            bool isComms = task.TaskType == TaskTypes.FixComms;
            bool isReactor = task.TaskType == TaskTypes.StopCharles || task.TaskType == TaskTypes.ResetSeismic || task.TaskType == TaskTypes.ResetReactor;
            bool isO2 = task.TaskType == TaskTypes.RestoreOxy;

            if (pc.IsRole(RoleId.Madmate) && (isLights && !Madmate.CanFixBlackout) || (isReactor && !Madmate.CanFixReactor) || (isO2 && !Madmate.CanFixO2) || (isComms && !Madmate.CanFixComms))
            {
                return true;
            }

            return false;
        }

        public static bool IsBlocked(Console console, PlayerControl pc)
        {
            if (console == null || pc == null || pc != PlayerControl.LocalPlayer)
            {
                return false;
            }

            PlayerTask task = console.FindTask(pc);
            return IsBlocked(task, pc);
        }

        public static bool IsBlocked(SystemConsole console, PlayerControl pc)
        {
            if (console == null || pc == null || pc != PlayerControl.LocalPlayer)
            {
                return false;
            }

            string name = console.name;
            bool isSecurity = name == "task_cams" || name == "Surv_Panel" || name == "SurvLogConsole" || name == "SurvConsole";
            bool isVitals = name == "panel_vitals";
            bool isButton = name == "EmergencyButton" || name == "EmergencyConsole" || name == "task_emergency";

            if ((isSecurity && !Options.canUseCameras) || (isVitals && !Options.canUseVitals)) return true;
            return false;
        }

        public static bool IsBlocked(IUsable target, PlayerControl pc)
        {
            if (target == null) return false;

            Console targetConsole = target.TryCast<Console>();
            SystemConsole targetSysConsole = target.TryCast<SystemConsole>();
            MapConsole targetMapConsole = target.TryCast<MapConsole>();
            if ((targetConsole != null && IsBlocked(targetConsole, pc)) ||
                (targetSysConsole != null && IsBlocked(targetSysConsole, pc)) ||
                (targetMapConsole != null && !Options.canUseAdmin))
            {
                return true;
            }
            return false;
        }
    }

    public static class KillButtonPatch
    {
        [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
        class KillButtonDoClickPatch
        {
            public static bool Prefix(KillButton __instance)
            {
                if (__instance.isActiveAndEnabled && __instance.currentTarget && !__instance.isCoolingDown && PlayerControl.LocalPlayer.IsAlive() && PlayerControl.LocalPlayer.CanMove)
                {
                    bool showAnimation = true;

                    // Use an unchecked kill command, to allow shorter kill cooldowns etc. without getting kicked
                    MurderAttemptResult res = Helpers.CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, __instance.currentTarget, showAnimation: showAnimation);
                    // Handle blank kill
                    if (res == MurderAttemptResult.BlankKill)
                    {
                        PlayerControl.LocalPlayer.killTimer = GameManager.Instance.LogicOptions.currentGameOptions.GetFloat(FloatOptionNames.KillCooldown);
                    }

                    __instance.SetTarget(null);
                }
                return false;
            }
        }
    }

    public static class MeetingButtonPatch
    {
        [HarmonyPatch(typeof(EmergencyMinigame), nameof(EmergencyMinigame.Update))]
        class EmergencyMinigameUpdatePatch
        {
            static void Postfix(EmergencyMinigame __instance)
            {
                var roleCanCallEmergency = true;
                var statusText = "";

                // Jester
                if (PlayerControl.LocalPlayer.IsRole(RoleId.Jester) && !Jester.CanCallEmergency)
                {
                    roleCanCallEmergency = false;
                    statusText = ModTranslation.getString("JesterMeetingButton");
                }

                if (!roleCanCallEmergency)
                {
                    __instance.StatusText.text = statusText;
                    __instance.NumberText.text = string.Empty;
                    __instance.ClosedLid.gameObject.SetActive(true);
                    __instance.OpenLid.gameObject.SetActive(false);
                    __instance.ButtonActive = false;
                    return;
                }

                // Handle max Number of meetings
                if (__instance.state == 1)
                {
                    int localRemaining = PlayerControl.LocalPlayer.RemainingEmergencies;
                    int teamRemaining = Mathf.Max(0, MaxNumberOfMeetings - MeetingsCount);
                    int remaining = Mathf.Min(localRemaining, (PlayerControl.LocalPlayer.IsRole(RoleId.Mayor)) ? 1 : teamRemaining);

                    __instance.StatusText.text = String.Format(ModTranslation.getString("MeetingStatus"), PlayerControl.LocalPlayer.name, localRemaining.ToString(), teamRemaining.ToString());
                    __instance.NumberText.text = "";
                    __instance.ButtonActive = remaining > 0;
                    __instance.ClosedLid.gameObject.SetActive(!__instance.ButtonActive);
                    __instance.OpenLid.gameObject.SetActive(__instance.ButtonActive);
                    return;
                }
            }
        }
    }

    public static class SabotageButtonPatch
    {
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
        class VentButtonVisibilityPatch
        {
            static void Postfix(PlayerControl __instance)
            {
                if (__instance.AmOwner && Helpers.ShowButtons)
                {
                    FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.Hide();
                    FastDestroyableSingleton<HudManager>.Instance.SabotageButton.Hide();

                    if (Helpers.ShowButtons)
                    {
                        if (__instance.RoleCanUseVents()) FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.Show();

                        if (__instance.RoleCanSabotage())
                        {
                            FastDestroyableSingleton<HudManager>.Instance.SabotageButton.Show();
                            FastDestroyableSingleton<HudManager>.Instance.SabotageButton.gameObject.SetActive(true);
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.DoClick))]
        public static class SabotageButtonDoClickPatch
        {
            public static bool Prefix(SabotageButton __instance)
            {
                if (!PlayerControl.LocalPlayer.IsNeutral()) return true;

                HudManager.Instance.ToggleMapVisible(new MapOptions()
                {
                    Mode = MapOptions.Modes.Sabotage,
                    AllowMovementWhileMapOpen = true
                });

                return false;
            }
        }
    }

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowSabotageMap))]
    class ShowImpostorMapPatch
    {
        public static void Prefix(ref RoleTeamTypes __state)
        {
            var player = PlayerControl.LocalPlayer;
            if (player.IsRole(RoleId.Jester) && CustomRolesH.JesterCanSabotage.getBool())
            {
                __state = player.Data.Role.TeamType;
                player.Data.Role.TeamType = RoleTeamTypes.Impostor;
            }
            if (player.IsRole(RoleId.CustomImpostor) && CustomRolesH.CustomImpostorCanSabotage.getBool())
            {
                __state = player.Data.Role.TeamType;
                player.Data.Role.TeamType = RoleTeamTypes.Impostor;
            }
        }

        public static void Postfix(ref RoleTeamTypes __state)
        {
            var player = PlayerControl.LocalPlayer;
            if (player.IsRole(RoleId.Jester) && CustomRolesH.JesterCanSabotage.getBool())
            {
                player.Data.Role.TeamType = __state;
            }
            if (player.IsRole(RoleId.CustomImpostor) && CustomRolesH.CustomImpostorCanSabotage.getBool())
            {
                player.Data.Role.TeamType = __state;
            }
        }
    }

    public static class UseButtonPatch
    {
        [HarmonyPatch(typeof(UseButton), nameof(UseButton.SetTarget))]
        class UseButtonSetTargetPatch
        {
            static bool Prefix(UseButton __instance, [HarmonyArgument(0)] IUsable target)
            {
                PlayerControl pc = PlayerControl.LocalPlayer;
                __instance.enabled = true;

                if (BlockButtonPatch.IsBlocked(target, pc))
                {
                    __instance.currentTarget = null;
                    __instance.buttonLabelText.text = ModTranslation.getString("ButtonBlocked");
                    __instance.enabled = false;
                    __instance.graphic.color = Palette.DisabledClear;
                    __instance.graphic.material.SetFloat("_Desat", 0f);
                    return false;
                }

                __instance.currentTarget = target;
                return true;
            }
        }
    }

    public static class VentButtonPatch
    {
        [HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
        public static class VentCanUsePatch
        {
            public static bool Prefix(Vent __instance, ref float __result, [HarmonyArgument(0)] GameData.PlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
            {
                float Num = float.MaxValue;
                PlayerControl @object = pc.Object;
                bool roleCouldUse = @object.RoleCanUseVents();

                var usableDistance = __instance.UsableDistance;
                if (__instance.name.StartsWith("SealedVent_"))
                {
                    canUse = couldUse = false;
                    __result = Num;
                    return false;
                }

                couldUse = (@object.inVent || roleCouldUse) && !pc.IsDead && (@object.CanMove || @object.inVent);
                canUse = couldUse;
                if (canUse)
                {
                    Vector2 truePosition = @object.GetTruePosition();
                    Vector3 position = __instance.transform.position;
                    Num = Vector2.Distance(truePosition, position);

                    canUse &= (Num <= usableDistance && !PhysicsHelpers.AnythingBetween(truePosition, position, Constants.ShipOnlyMask, false));
                }
                __result = Num;
                return false;
            }
        }

        [HarmonyPatch(typeof(VentButton), nameof(VentButton.DoClick))]
        class VentButtonDoClickPatch
        {
            static bool Prefix(VentButton __instance)
            {
                // Manually modifying the VentButton to use Vent.Use again in order to trigger the Vent.Use prefix patch
                if (__instance.currentTarget != null) __instance.currentTarget.Use();
                return false;
            }
        }

        [HarmonyPatch(typeof(Vent), nameof(Vent.Use))]
        public static class VentUsePatch
        {
            public static bool Prefix(Vent __instance)
            {
                bool canUse;
                bool couldUse;
                __instance.CanUse(PlayerControl.LocalPlayer.Data, out canUse, out couldUse);
                bool CannotMoveInVents = (PlayerControl.LocalPlayer.IsRole(RoleId.Madmate) && !Madmate.CanMoveInVents) ||
                                        (PlayerControl.LocalPlayer.IsRole(RoleId.Jester) && !Jester.CanMoveInVents);
                if (!canUse) return false; // No need to execute the native method as using is disallowed anyways
                bool isEnter = !PlayerControl.LocalPlayer.inVent;

                if (isEnter) PlayerControl.LocalPlayer.MyPhysics.RpcEnterVent(__instance.Id);

                else PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(__instance.Id);

                __instance.SetButtons(isEnter && !CannotMoveInVents);
                return false;
            }
        }

        [HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent))]
        public static class DumpDeadBody
        {
            public static void Postfix()
            {
                UnderTaker.OnEnterVent();
            }
        }
    }
}