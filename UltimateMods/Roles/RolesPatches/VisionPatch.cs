namespace UltimateMods.Roles.Patches
{
    public class RolesVisionPatch
    {
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
        public static bool Prefix(ref float __result, ShipStatus __instance, [HarmonyArgument(0)] GameData.PlayerInfo player)
        {
            ISystemType systemType = __instance.Systems.ContainsKey(SystemTypes.Electrical) ? __instance.Systems[SystemTypes.Electrical] : null;
            if (systemType == null) return true;
            SwitchSystem switchSystem = systemType.TryCast<SwitchSystem>();
            if (switchSystem == null) return true;

            float num = (float)switchSystem.Value / 255f;

            // Has Impostor Vision
            if (Helpers.HasImpostorVision(player)) __result = GetNeutralLightRadius(__instance, true);
            // // if player is Lighter and Lighter has his ability active
            // else if (PlayerControl.LocalPlayer.IsRole(RoleId.Lighter) && Lighter.LightActive) __result = Mathf.Lerp(__instance.MaxLightRadius * Lighter.LighterModeLightsOffVision, __instance.MaxLightRadius * Lighter.LighterModeLightsOnVision, num);
            // Default light radius
            else __result = GetNeutralLightRadius(__instance, false);

            if (PlayerControl.LocalPlayer.HasModifier(ModifierId.Sunglasses)) __result *= 1f - (Sunglasses.Vision * 0.01f);

            return false;
        }

        public static float GetNeutralLightRadius(ShipStatus shipStatus, bool isImpostor)
        {
            if (isImpostor) return shipStatus.MaxLightRadius * GameManager.Instance.LogicOptions.currentGameOptions.GetFloat(FloatOptionNames.ImpostorLightMod);

            SwitchSystem switchSystem = MapUtilities.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
            float lerpValue = switchSystem.Value / 255f;

            return Mathf.Lerp(shipStatus.MinLightRadius, shipStatus.MaxLightRadius, lerpValue) * GameManager.Instance.LogicOptions.currentGameOptions.GetFloat(FloatOptionNames.CrewLightMod);
        }
    }
}