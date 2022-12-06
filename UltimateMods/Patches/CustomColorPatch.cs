using UltimateMods.Modules;
using HarmonyLib;
using UnityEngine;
using AmongUs.Data.Legacy;
using UltimateMods.Utilities;

namespace UltimateMods.Patches
{
    [HarmonyPatch]
    public static class CustomColorPatches
    {
        [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new[] {
            typeof(StringNames),
            typeof(Il2CppReferenceArray<Il2CppSystem.Object>)
        })]
        public class ColorStringPatch
        {
            public static bool Prefix(ref string __result, [HarmonyArgument(0)] StringNames name)
            {
                if ((int)name >= 50000)
                {
                    string text = CustomColors.ColorStrings[(int)name];
                    if (text is not null)
                    {
                        __result = ModTranslation.getString(text) + " \n(UMColor)";
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.OnEnable))]
        public static class PlayerTabEnablePatch
        {
            public static void Postfix(PlayerTab __instance)
            {
                Il2CppArrayBase<ColorChip> chips = __instance.ColorChips.ToArray();

                int cols = 12;
                for (int i = 0; i < CustomColors.ORDER.Count; i++)
                {
                    int pos = CustomColors.ORDER[i];
                    if (pos < 0 || pos > chips.Length)
                        continue;
                    ColorChip chip = chips[pos];
                    int row = i / cols, col = i % cols;
                    chip.transform.localPosition = new Vector3(-0.975f + (col * 0.485f), 1.475f - (row * 0.49f), chip.transform.localPosition.z);
                    chip.transform.localScale *= 0.78f;
                }
                for (int j = CustomColors.ORDER.Count; j < chips.Length; j++)
                { // If number isn't in order, hide it
                    ColorChip chip = chips[j];
                    chip.transform.localScale *= 0f;
                    chip.enabled = false;
                    chip.Button.enabled = false;
                    chip.Button.OnClick.RemoveAllListeners();
                }
            }
        }

        [HarmonyPatch(typeof(LegacySaveManager), nameof(LegacySaveManager.LoadPlayerPrefs))]
        public static class LoadPlayerPrefsPatch
        {
            private static bool needsPatch = false;
            public static void Prefix([HarmonyArgument(0)] bool overrideLoad)
            {
                if (!LegacySaveManager.loaded || overrideLoad)
                    needsPatch = true;
            }

            public static void Postfix()
            {
                if (!needsPatch) return;
                LegacySaveManager.colorConfig %= CustomColors.pickableColors;
                needsPatch = false;
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckColor))]
        public static class PlayerCntrolCheckColorPatch
        {
            private static bool isTaken(PlayerControl player, uint color) 
            {
                foreach (GameData.PlayerInfo p in GameData.Instance.AllPlayers.GetFastEnumerator())
                    if (!p.Disconnected && p.PlayerId != player.PlayerId && p.DefaultOutfit.ColorId == color)
                        return true;
                return false;
            }
            public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte bodyColor) 
            { 
                uint color = (uint)bodyColor;
                if (isTaken(__instance, color) || color >= Palette.PlayerColors.Length) 
                {
                    int num = 0;
                    while (num++ < 50 && (color >= CustomColors.pickableColors || isTaken(__instance, color))) 
                    {
                        color = (color + 1) % CustomColors.pickableColors;
                    }
                }

                __instance.RpcSetColor((byte)color);
                return false;
            }
        }
    }
}