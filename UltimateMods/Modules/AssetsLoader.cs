using System.IO;
using System.Reflection;
using UnityEngine;

namespace UltimateMods.Modules
{
    public static class Assets
    {
        private static readonly Assembly AudioAssets = Assembly.GetExecutingAssembly();
        private static readonly Assembly SpriteAssets = Assembly.GetExecutingAssembly();
        private static readonly Assembly ButtonAssets = Assembly.GetExecutingAssembly();
        private static readonly Assembly GodMiraHQAssets = Assembly.GetExecutingAssembly();

        public static AudioClip JesterWinSound;
        public static AudioClip EveryoneLoseSound;
        public static AudioClip Bomb;
        public static AudioClip Teleport;

        public static Texture2D Arrow;
        public static Texture2D NormalBanner;
        public static Texture2D HorseBanner;
        public static Texture2D DeadBodySprite;
        public static Texture2D TabSet;
        public static Texture2D TabCrew;
        public static Texture2D TabImp;
        public static Texture2D TabNeu;
        public static Texture2D TabMod;
        public static Texture2D TabOth;
        public static Texture2D Soul;

        public static Texture2D CreditsButton;
        public static Texture2D HorseModeOnButton;
        public static Texture2D HorseModeOffButton;
        public static Texture2D EngineerRepairButton;
        public static Texture2D UnderTakerMoveButton;
        public static Texture2D ZoomInButton;
        public static Texture2D ZoomOutButton;
        public static Texture2D TeleporterTeleportButton;
        public static Texture2D AltruistReviveButton;
        public static Texture2D JackalSidekickButton;
        public static Texture2D ArsonistDouseButton;
        public static Texture2D ArsonistIgniteButton;

        public static GameObject GodMiraHQ;
        public static GameObject NewDropShip;

        public static void LoadAssets()
        {
            var AudioAssetsResource = AudioAssets.GetManifestResourceStream("UltimateMods.Resources.AssetsBundlesUM.Assets.ultimateaudio");
            var AudioAssetsBundle = AssetBundle.LoadFromMemory(AudioAssetsResource.ReadFully());

            JesterWinSound = AudioAssetsBundle.LoadAsset<AudioClip>("JesterWin.wav").DontUnload();
            EveryoneLoseSound = AudioAssetsBundle.LoadAsset<AudioClip>("EveryoneLose.wav").DontUnload();
            Bomb = AudioAssetsBundle.LoadAsset<AudioClip>("Bomb.mp3").DontUnload();
            Teleport = AudioAssetsBundle.LoadAsset<AudioClip>("Teleport.mp3").DontUnload();

            var SpriteAssetsResource = SpriteAssets.GetManifestResourceStream("UltimateMods.Resources.AssetsBundlesUM.Assets.ultimatesprite");
            var SpriteAssetsBundle = AssetBundle.LoadFromMemory(SpriteAssetsResource.ReadFully());

            Arrow = SpriteAssetsBundle.LoadAsset<Texture2D>("Arrow.png").DontUnload();
            NormalBanner = SpriteAssetsBundle.LoadAsset<Texture2D>("NormalBanner.png").DontUnload();
            HorseBanner = SpriteAssetsBundle.LoadAsset<Texture2D>("HorseBanner.png").DontUnload();
            DeadBodySprite = SpriteAssetsBundle.LoadAsset<Texture2D>("DeadBody.png").DontUnload();
            TabSet = SpriteAssetsBundle.LoadAsset<Texture2D>("MainSettings.png").DontUnload();
            TabCrew = SpriteAssetsBundle.LoadAsset<Texture2D>("CrewmateSettings.png").DontUnload();
            TabImp = SpriteAssetsBundle.LoadAsset<Texture2D>("ImpostorSettings.png").DontUnload();
            TabNeu = SpriteAssetsBundle.LoadAsset<Texture2D>("NeutralSettings.png").DontUnload();
            TabMod = SpriteAssetsBundle.LoadAsset<Texture2D>("ModifierSettings.png").DontUnload();
            TabOth = SpriteAssetsBundle.LoadAsset<Texture2D>("OtherSettings.png").DontUnload();
            Soul = SpriteAssetsBundle.LoadAsset<Texture2D>("Soul.png").DontUnload();

            var ButtonAssetsResource = ButtonAssets.GetManifestResourceStream("UltimateMods.Resources.AssetsBundlesUM.Assets.ultimatebutton");
            var ButtonAssetsBundle = AssetBundle.LoadFromMemory(ButtonAssetsResource.ReadFully());

            CreditsButton = ButtonAssetsBundle.LoadAsset<Texture2D>("CreditsButton.png").DontUnload();
            HorseModeOnButton = ButtonAssetsBundle.LoadAsset<Texture2D>("HorseModeButtonOn.png").DontUnload();
            HorseModeOffButton = ButtonAssetsBundle.LoadAsset<Texture2D>("HorseModeButtonOff.png").DontUnload();
            EngineerRepairButton = ButtonAssetsBundle.LoadAsset<Texture2D>("EngineerRepairButton.png").DontUnload();
            UnderTakerMoveButton = ButtonAssetsBundle.LoadAsset<Texture2D>("UnderTakerMoveButton.png").DontUnload();
            ZoomInButton = ButtonAssetsBundle.LoadAsset<Texture2D>("ZoomIn.png").DontUnload();
            ZoomOutButton = ButtonAssetsBundle.LoadAsset<Texture2D>("ZoomOut.png").DontUnload();
            TeleporterTeleportButton = ButtonAssetsBundle.LoadAsset<Texture2D>("TeleporterTeleportButton.png").DontUnload();
            AltruistReviveButton = ButtonAssetsBundle.LoadAsset<Texture2D>("AltruistReviveButton.png").DontUnload();
            JackalSidekickButton = ButtonAssetsBundle.LoadAsset<Texture2D>("JackalSidekickButton.png").DontUnload();
            ArsonistDouseButton = ButtonAssetsBundle.LoadAsset<Texture2D>("ArsonistDouse.png").DontUnload();
            ArsonistIgniteButton = ButtonAssetsBundle.LoadAsset<Texture2D>("ArsonistIgnite.png").DontUnload();

            var GodMiraHQAssetsResource = GodMiraHQAssets.GetManifestResourceStream("UltimateMods.GodMiraHQ.Resources.godmirahq");
            var GodMiraHQAssetsBundle = AssetBundle.LoadFromMemory(GodMiraHQAssetsResource.ReadFully());

            GodMiraHQ = GodMiraHQAssetsBundle.LoadAsset<GameObject>("GodMiraHQ.prefab").DontUnload();
            NewDropShip = GodMiraHQAssetsBundle.LoadAsset<GameObject>("DropShip.prefab").DontUnload();

            AudioAssetsBundle.Unload(false);
            SpriteAssetsBundle.Unload(false);
            ButtonAssetsBundle.Unload(false);
            GodMiraHQAssetsBundle.Unload(false);
        }

        public static byte[] ReadFully(this Stream input)
        {
            using var ms = new MemoryStream();
            input.CopyTo(ms);
            return ms.ToArray();
        }

#nullable enable
        public static T? LoadAsset<T>(this AssetBundle assetBundle, string name) where T : UnityEngine.Object
        {
            return assetBundle.LoadAsset(name, Il2CppType.Of<T>())?.Cast<T>();
        }

#nullable disable
        public static T DontUnload<T>(this T obj) where T : Object
        {
            obj.hideFlags |= HideFlags.DontUnloadUnusedAsset;

            return obj;
        }
    }
}