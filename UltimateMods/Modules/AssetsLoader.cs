using System.IO;
using System.Reflection;
using UnityEngine;

namespace UltimateMods.Modules
{
    public static class AssetLoader
    {
        private static readonly Assembly assets = Assembly.GetExecutingAssembly();

        public static AudioClip JesterWinSound;
        public static AudioClip EveryoneLoseSound;

        public static Texture2D ClassicMeetingStart0;
        public static Texture2D ClassicMeetingStart1;
        public static Texture2D ClassicMeetingStart2;
        public static Texture2D ClassicMeetingStart3;
        public static Texture2D ClassicMeetingDeadBodyReport;

        public static void LoadAssets()
        {
            var AssetsResource = assets.GetManifestResourceStream("UltimateMods.Resources.Sounds.Assets.ultimatebundle");
            var AssetsBundle = AssetBundle.LoadFromMemory(AssetsResource.ReadFully());

            JesterWinSound = AssetsBundle.LoadAsset<AudioClip>("JesterWin.wav").DontUnload();
            EveryoneLoseSound = AssetsBundle.LoadAsset<AudioClip>("EveryoneLose.wav").DontUnload();
            ClassicMeetingStart0 = AssetsBundle.LoadAsset<Texture2D>("EmergencyScreen0.png").DontUnload();
            ClassicMeetingStart1 = AssetsBundle.LoadAsset<Texture2D>("EmergencyScreen1.png").DontUnload();
            ClassicMeetingStart2 = AssetsBundle.LoadAsset<Texture2D>("EmergencyScreen2.png").DontUnload();
            ClassicMeetingStart3 = AssetsBundle.LoadAsset<Texture2D>("EmergencyScreen3.png").DontUnload();
            ClassicMeetingDeadBodyReport = AssetsBundle.LoadAsset<Texture2D>("DeadBodyReport.png").DontUnload();
        }

        public static byte[] ReadFully(this Stream input)
        {
            using (var ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public static T LoadAsset<T>(this AssetBundle assetBundle, string name) where T : UnityEngine.Object
        {
            return assetBundle.LoadAsset(name, Il2CppType.Of<T>())?.Cast<T>();
        }

        public static T DontUnload<T>(this T obj) where T : Object
        {
            obj.hideFlags |= HideFlags.DontUnloadUnusedAsset;

            return obj;
        }
    }
}