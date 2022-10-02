using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UltimateMods.Roles.Patches
{
    [HarmonyPatch(typeof(ShipStatus))]
    public class RolesVisionPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
        public static bool Prefix(ref float __result, ShipStatus __instance, [HarmonyArgument(0)] GameData.PlayerInfo player)
        {
            ISystemType systemType = __instance.Systems.ContainsKey(SystemTypes.Electrical) ? __instance.Systems[SystemTypes.Electrical] : null;
            if (systemType == null) return true;
            SwitchSystem switchSystem = systemType.TryCast<SwitchSystem>();
            if (switchSystem == null) return true;

            float num = (float)switchSystem.Value / 255f;

            if (player == null || player.IsDead) // IsDead
                __result = __instance.MaxLightRadius;
            else if (player.Role.IsImpostor || (PlayerControl.LocalPlayer.isRole(RoleType.Jester) && Jester.HasImpostorVision)) // Jester with Impostor vision
                __result = __instance.MaxLightRadius * PlayerControl.GameOptions.ImpostorLightMod;
            else
                __result = Mathf.Lerp(__instance.MinLightRadius, __instance.MaxLightRadius, num) * PlayerControl.GameOptions.CrewLightMod;

            return false;
        }
    }
}