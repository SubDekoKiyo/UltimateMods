using System.IO;
using System.Reflection;
using UnityEngine;
using Il2CppType = UnhollowerRuntimeLib.Il2CppType;

namespace UltimateMods.Modules
{
    public static class AssetLoader
    {
        private static readonly Assembly sounds = Assembly.GetExecutingAssembly();
        public static AudioClip JesterWinSound;
        public static AudioClip EveryoneLoseSound;

        public static void LoadAssets()
        {
            var ResourceAudioFileStream = sounds.GetManifestResourceStream("UltimateMods.Resources.Sounds.Assets.ultimatesounds");
            var AssetBundleBundle = AssetBundle.LoadFromMemory(ResourceAudioFileStream.ReadFully());

            JesterWinSound = AssetBundleBundle.LoadAsset<AudioClip>("JesterWin.wav").DontUnload();
            EveryoneLoseSound = AssetBundleBundle.LoadAsset<AudioClip>("EveryoneLose.wav").DontUnload();
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