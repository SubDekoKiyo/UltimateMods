using HarmonyLib;

namespace UltimateMods.Roles.Patches
{
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
}