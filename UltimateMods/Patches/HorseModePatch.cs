// Source Code with TheOtherRoles

using HarmonyLib;
using UnityEngine;
using static UnityEngine.UI.Button;
using Object = UnityEngine.Object;
using UltimateMods.Modules;

namespace UltimateMods.Patches
{
    [HarmonyPatch]
    class HorseModePatch
    {
        [HarmonyPatch(typeof(Constants), nameof(Constants.ShouldHorseAround))]
        public static class ShouldAlwaysHorseAround
        {
            public static bool isHorseMode;
            public static bool Prefix(ref bool __result)
            {
                if (isHorseMode != MapOptions.enableHorseMode && LobbyBehaviour.Instance != null) __result = isHorseMode;
                else
                {
                    __result = MapOptions.enableHorseMode;
                    isHorseMode = MapOptions.enableHorseMode;
                }
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    class ChangeHorseModePatch
    {
        private static bool horseButtonState = MapOptions.enableHorseMode;
        private static Sprite horseModeOffSprite = null;
        private static Sprite horseModeOnSprite = null;
        private static GameObject bottomTemplate;
        private static void Prefix(MainMenuManager __instance)
        {
            // Horse Mode
            var horseModeSelectionBehavior = new ClientOptionsPatch.SelectionBehaviour("Enable Horse Mode", () => MapOptions.enableHorseMode = UltimateModsPlugin.EnableHorseMode.Value = !UltimateModsPlugin.EnableHorseMode.Value, UltimateModsPlugin.EnableHorseMode.Value);

            bottomTemplate = GameObject.Find("InventoryButton");
            if (bottomTemplate == null) return;
            var horseButton = Object.Instantiate(bottomTemplate, bottomTemplate.transform.parent);
            var passiveHorseButton = horseButton.GetComponent<PassiveButton>();
            var spriteHorseButton = horseButton.GetComponent<SpriteRenderer>();

            horseModeOffSprite = Helpers.LoadSpriteFromResources("UltimateMods.Resources.HorseModeButtonOff.png", 75f);
            horseModeOnSprite = Helpers.LoadSpriteFromResources("UltimateMods.Resources.HorseModeButtonOn.png", 75f);

            spriteHorseButton.sprite = horseButtonState ? horseModeOnSprite : horseModeOffSprite;

            passiveHorseButton.OnClick = new ButtonClickedEvent();

            passiveHorseButton.OnClick.AddListener((UnityEngine.Events.UnityAction)delegate
                {
                    horseButtonState = horseModeSelectionBehavior.OnClick();
                    if (horseButtonState)
                    {
                        if (horseModeOnSprite == null) horseModeOnSprite = Helpers.LoadSpriteFromResources("UltimateMods.Resources.HorseModeButtonOn.png", 75f);
                        spriteHorseButton.sprite = horseModeOnSprite;
                    }
                    else
                    {
                        if (horseModeOffSprite == null) horseModeOffSprite = Helpers.LoadSpriteFromResources("UltimateMods.Resources.HorseModeButtonOff.png", 75f);
                        spriteHorseButton.sprite = horseModeOffSprite;
                    }
                    CredentialsPatch.MainMenuButtonPatch.updateSprite();
                    // Avoid wrong Player Particles floating around in the background
                    var particles = GameObject.FindObjectOfType<PlayerParticles>();
                    if (particles != null)
                    {
                        particles.pool.ReclaimAll();
                        particles.Start();
                    }
                });
        }
    }
}