namespace UltimateMods.Patches
{
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
    [HarmonyPriority(Priority.First)]
    class ExileControllerBeginPatch
    {
        public static GameData.PlayerInfo lastExiled;
        public static void Prefix(ExileController __instance, [HarmonyArgument(0)] ref GameData.PlayerInfo exiled, [HarmonyArgument(1)] bool tie)
        {
            lastExiled = exiled;

            // 1 = Reset per turn
            if (Options.RestrictDevices == 1)
                Options.ResetDeviceTimes();
        }
    }

    [HarmonyPatch]
    class ExileControllerWrapUpPatch
    {
        [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
        class BaseExileControllerPatch
        {
            public static void Postfix(ExileController __instance)
            {
                WrapUpPostfix(__instance.exiled);
            }
        }

        [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
        class AirshipExileControllerPatch
        {
            public static void Postfix(AirshipExileController __instance)
            {
                WrapUpPostfix(__instance.exiled);
            }
        }

        static void WrapUpPostfix(GameData.PlayerInfo exiled)
        {
            if (exiled != null)
            {
                var p = exiled.Object;
                // Jester win condition
                if (p.IsRole(RoleId.Jester))
                {
                    if ((Jester.HasTasks && Jester.TasksComplete(p)) || !Jester.HasTasks)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedEndGame, Hazel.SendOption.Reliable, -1);
                        writer.Write((byte)CustomGameOverReason.JesterExiled);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.UncheckedEndGame((byte)CustomGameOverReason.JesterExiled);
                    }
                }
            }

            // Reset custom button timers where necessary
            CustomButton.MeetingEndedUpdate();

            // Custom role post-meeting functions
            UltimateMods.OnMeetingEnd();

            // Remove all DeadBodies
            DeadBody[] array = Object.FindObjectsOfType<DeadBody>();
            for (int i = 0; i < array.Length; i++)
            {
                Object.Destroy(array[i].gameObject);
            }
        }
    }

    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new Type[] { typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
    class ExileControllerMessagePatch
    {
        static void Postfix(ref string __result, [HarmonyArgument(0)] StringNames id)
        {
            try
            {
                if (ExileController.Instance != null && ExileController.Instance.exiled != null)
                {
                    PlayerControl player = Helpers.PlayerById(ExileController.Instance.exiled.Object.PlayerId);
                    if (player == null) return;
                    List<RoleInfo> infos = RoleInfoList.GetRoleInfoForPlayer(player);
                    RoleInfo roleInfo = infos.Where(info => info.RoleId != RoleId.NoRole).FirstOrDefault();
                    // Exile role text
                    if (id is StringNames.ExileTextPN or StringNames.ExileTextSN or StringNames.ExileTextPP or StringNames.ExileTextSP)
                    {
                        __result = String.Format(LocalizationManager.GetString(TransKey.ExilePlayer), player.Data.PlayerName, roleInfo.Name);
                    }
                    // Hide Number of remaining impostors on Jester win
                    if (id is StringNames.ImpostorsRemainP or StringNames.ImpostorsRemainS)
                    {
                        if (player.IsRole(RoleId.Jester)) __result = "";
                    }
                }
            }
            catch
            {
                // pass - Hopefully prevent leaving while exiling to softlock game
            }
        }
    }
}