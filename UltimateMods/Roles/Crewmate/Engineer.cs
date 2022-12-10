namespace UltimateMods.Roles
{
    [HarmonyPatch]
    public class Engineer : RoleBase<Engineer>
    {
        private static Sprite EngineerRepairButtonSprite;
        private static CustomButton EngineerRepairButton;
        public static TMP_Text EngineerRepairButtonText;
        public int RemainingFixes = 2;

        public static bool CanFixSabo { get { return CustomRolesH.EngineerCanFixSabo.getBool(); } }
        public static int FixCount { get { return Mathf.RoundToInt(CustomRolesH.EngineerMaxFixCount.getFloat()); } }
        public static bool CanUseVents { get { return CustomRolesH.EngineerCanUseVents.getBool(); } }
        // public static float VentCooldown { get { return CustomRolesH.EngineerVentCooldown.getFloat(); } }

        public Engineer()
        {
            RoleType = roleId = RoleType.Engineer;
            RemainingFixes = FixCount;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void MakeButtons(HudManager hm)
        {
            EngineerRepairButton = new CustomButton(
                () =>
                {
                    EngineerRepairButton.Timer = 0f;

                    MessageWriter usedRepairWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EngineerUsedRepair, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(usedRepairWriter);
                    RPCProcedure.EngineerUsedRepair(PlayerControl.LocalPlayer.Data.PlayerId);

                    foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks.GetFastEnumerator())
                    {
                        if (task.TaskType == TaskTypes.FixLights)
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EngineerFixLights, Hazel.SendOption.Reliable, -1);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.EngineerFixLights();
                        }
                        else if (task.TaskType == TaskTypes.RestoreOxy)
                        {
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.LifeSupp, 0 | 64);
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.LifeSupp, 1 | 64);
                        }
                        else if (task.TaskType == TaskTypes.ResetReactor)
                        {
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 16);
                        }
                        else if (task.TaskType == TaskTypes.ResetSeismic)
                        {
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Laboratory, 16);
                        }
                        else if (task.TaskType == TaskTypes.FixComms)
                        {
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Comms, 16 | 0);
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Comms, 16 | 1);
                        }
                        else if (task.TaskType == TaskTypes.StopCharles)
                        {
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 0 | 16);
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 1 | 16);
                        }
                    }
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleType.Engineer) && !PlayerControl.LocalPlayer.Data.IsDead && local.RemainingFixes > 0 && CanFixSabo; },
                () =>
                {
                    bool sabotageActive = false;
                    foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks.GetFastEnumerator())
                        if (task.TaskType == TaskTypes.FixLights || task.TaskType == TaskTypes.RestoreOxy || task.TaskType == TaskTypes.ResetReactor || task.TaskType == TaskTypes.ResetSeismic || task.TaskType == TaskTypes.FixComms || task.TaskType == TaskTypes.StopCharles)
                            sabotageActive = true;

                    if (EngineerRepairButtonText != null)
                    {
                        if (local.RemainingFixes > 0)
                            EngineerRepairButtonText.text = String.Format(ModTranslation.getString("ReamingCount"), local.RemainingFixes);
                        else
                            EngineerRepairButtonText.text = "";
                    }

                    return sabotageActive && local.RemainingFixes > 0 && PlayerControl.LocalPlayer.CanMove;
                },
                () => { },
                Engineer.GetFixButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                hm,
                hm.UseButton,
                KeyCode.F
            );

            EngineerRepairButton.ButtonText = ModTranslation.getString("EngineerRepairButtonText");
            EngineerRepairButtonText = GameObject.Instantiate(EngineerRepairButton.actionButton.cooldownTimerText, EngineerRepairButton.actionButton.cooldownTimerText.transform.parent); // TMP初期化
            EngineerRepairButtonText.text = "";
            EngineerRepairButtonText.enableWordWrapping = false;
            EngineerRepairButtonText.transform.localScale = Vector3.one * 0.5f;
            EngineerRepairButtonText.transform.localPosition += new Vector3(-0.05f, 0.5f, 0);
        }

        public static Sprite GetFixButtonSprite()
        {
            if (EngineerRepairButtonSprite) return EngineerRepairButtonSprite;
            EngineerRepairButtonSprite = Helpers.LoadSpriteFromTexture2D(Modules.Assets.EngineerRepairButton, 115f);
            return EngineerRepairButtonSprite;
        }

        public static void SetButtonCooldowns()
        {
            EngineerRepairButton.MaxTimer = 0f;
        }

        public static void Clear()
        {
            players = new List<Engineer>();
        }
    }
}