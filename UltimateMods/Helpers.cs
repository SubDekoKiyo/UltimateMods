using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using System.Linq;
using HarmonyLib;
using Hazel;
using UltimateMods.Utilities;
using UltimateMods.Roles;
using UltimateMods.EndGame;
using static UltimateMods.UltimateMods;
using static UltimateMods.GameHistory;

namespace UltimateMods
{
    public enum MurderAttemptResult
    {
        PerformKill,
        SuppressKill,
        BlankKill
    }

    public static class Helpers
    {
        public static Dictionary<string, Sprite> CachedSprites = new();
        public static Sprite LoadSpriteFromResources(Texture2D texture, float pixelsPerUnit, Rect textureRect)
        {
            return Sprite.Create(texture, textureRect, new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }

        public static Sprite LoadSpriteFromResources(Texture2D texture, float pixelsPerUnit, Rect textureRect, Vector2 pivot)
        {
            return Sprite.Create(texture, textureRect, pivot, pixelsPerUnit);
        }

        public static Sprite LoadSpriteFromResources(string path, float pixelsPerUnit)
        {
            try
            {
                Texture2D texture = LoadTextureFromResources(path);
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            }
            catch
            {
                System.Console.WriteLine("Error loading sprite from path: " + path);
            }
            return null;
        }

        public static Texture2D LoadTextureFromResources(string path)
        {
            try
            {
                Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream stream = assembly.GetManifestResourceStream(path);
                var byteTexture = new byte[stream.Length];
                var read = stream.Read(byteTexture, 0, (int)stream.Length);
                LoadImage(texture, byteTexture, false);
                return texture;
            }
            catch
            {
                System.Console.WriteLine("Error loading texture from resources: " + path);
            }
            return null;
        }

        public static Texture2D LoadTextureFromDisk(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                    byte[] byteTexture = File.ReadAllBytes(path);
                    LoadImage(texture, byteTexture, false);
                    return texture;
                }
            }
            catch
            {
                System.Console.WriteLine("Error loading texture from disk: " + path);
            }
            return null;
        }

        internal delegate bool d_LoadImage(IntPtr tex, IntPtr data, bool markNonReadable);
        internal static d_LoadImage iCall_LoadImage;
        private static bool LoadImage(Texture2D tex, byte[] data, bool markNonReadable)
        {
            if (iCall_LoadImage == null)
                iCall_LoadImage = IL2CPP.ResolveICall<d_LoadImage>("UnityEngine.ImageConversion::LoadImage");
            var il2cppArray = (Il2CppStructArray<byte>)data;
            return iCall_LoadImage.Invoke(tex.Pointer, il2cppArray.Pointer, markNonReadable);
        }

        public static PlayerControl PlayerById(byte id)
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls.GetFastEnumerator())
                if (player.PlayerId == id)
                    return player;
            return null;
        }

        public static bool IsDead(this PlayerControl player)
        {
            return player == null || player?.Data?.IsDead == true || player?.Data?.Disconnected == true ||
                (finalStatuses != null && finalStatuses.ContainsKey(player.PlayerId) && finalStatuses[player.PlayerId] != FinalStatus.Alive);
        }

        public static bool IsAlive(this PlayerControl player)
        {
            return !IsDead(player);
        }

        public static string cs(Color c, string s)
        {
            return string.Format("<color=#{0:X2}{1:X2}{2:X2}{3:X2}>{4}</color>", ToByte(c.r), ToByte(c.g), ToByte(c.b), ToByte(c.a), s);
        }

        private static byte ToByte(float f)
        {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }

        public static List<byte> GenerateTasks(int numCommon, int numShort, int numLong)
        {
            if (numCommon + numShort + numLong <= 0)
            {
                numShort = 1;
            }

            var tasks = new Il2CppSystem.Collections.Generic.List<byte>();
            var hashSet = new Il2CppSystem.Collections.Generic.HashSet<TaskTypes>();

            var commonTasks = new Il2CppSystem.Collections.Generic.List<NormalPlayerTask>();
            foreach (var task in ShipStatus.Instance.CommonTasks.OrderBy(x => rnd.Next())) commonTasks.Add(task);

            var shortTasks = new Il2CppSystem.Collections.Generic.List<NormalPlayerTask>();
            foreach (var task in ShipStatus.Instance.NormalTasks.OrderBy(x => rnd.Next())) shortTasks.Add(task);

            var longTasks = new Il2CppSystem.Collections.Generic.List<NormalPlayerTask>();
            foreach (var task in ShipStatus.Instance.LongTasks.OrderBy(x => rnd.Next())) longTasks.Add(task);

            int start = 0;
            ShipStatus.Instance.AddTasksFromList(ref start, numCommon, tasks, hashSet, commonTasks);

            start = 0;
            ShipStatus.Instance.AddTasksFromList(ref start, numShort, tasks, hashSet, shortTasks);

            start = 0;
            ShipStatus.Instance.AddTasksFromList(ref start, numLong, tasks, hashSet, longTasks);

            return tasks.ToArray().ToList();
        }

        public static void Log(string msg)
        {
            UltimateModsPlugin.Instance.Log.LogInfo(msg);
        }

        public static bool IsCrew(this PlayerControl player)
        {
            return player != null && !player.IsImpostor() && !player.IsNeutral();
        }

        public static bool IsImpostor(this PlayerControl player)
        {
            return player != null && player.Data.Role.IsImpostor;
        }

        public static bool HasFakeTasks(this PlayerControl player)
        {
            return (player.IsNeutral() && !player.NeutralHasTasks());
        }

        public static bool NeutralHasTasks(this PlayerControl player)
        {
            return player.IsNeutral() &&
                (player.isRole(RoleType.Jester) && Jester.HasTasks);
        }

        public static bool IsNeutral(this PlayerControl player)
        {
            return (player != null &&
                    player.isRole(RoleType.Jester));
        }

        public static void ShareGameVersion()
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.VersionHandshake, Hazel.SendOption.Reliable, -1);
            writer.WritePacked(UltimateModsPlugin.Version.Major);
            writer.WritePacked(UltimateModsPlugin.Version.Minor);
            writer.WritePacked(UltimateModsPlugin.Version.Build);
            writer.WritePacked(AmongUsClient.Instance.ClientId);
            writer.Write((byte)(UltimateModsPlugin.Version.Revision < 0 ? 0xFF : UltimateModsPlugin.Version.Revision));
            writer.Write(Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.ToByteArray());
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.VersionHandshake(UltimateModsPlugin.Version.Major, UltimateModsPlugin.Version.Minor, UltimateModsPlugin.Version.Build, UltimateModsPlugin.Version.Revision, Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId, AmongUsClient.Instance.ClientId);
        }

        public static void RefreshRoleDescription(PlayerControl player)
        {
            if (player == null) return;

            List<RoleInfo> infos = RoleInfo.getRoleInfoForPlayer(player);

            var toRemove = new List<PlayerTask>();
            foreach (PlayerTask t in player.myTasks)
            {
                var textTask = t.gameObject.GetComponent<ImportantTextTask>();
                if (textTask != null)
                {
                    var info = infos.FirstOrDefault(x => textTask.Text.StartsWith(x.name));
                    if (info != null)
                        infos.Remove(info); // TextTask for this RoleInfo does not have to be added, as it already exists
                    else
                        toRemove.Add(t); // TextTask does not have a corresponding RoleInfo and will hence be deleted
                }
            }

            foreach (PlayerTask t in toRemove)
            {
                t.OnRemove();
                player.myTasks.Remove(t);
                UnityEngine.Object.Destroy(t.gameObject);
            }

            // Add TextTask for remaining RoleInfos
            foreach (RoleInfo roleInfo in infos)
            {
                var task = new GameObject("RoleTask").AddComponent<ImportantTextTask>();
                task.transform.SetParent(player.transform, false);

                /*if (roleInfo.roleType == RoleType.Jackal)
                {
                    if (Jackal.canCreateSidekick)
                    {
                        task.Text = cs(roleInfo.color, $"{roleInfo.name}: " + ModTranslation.getString("jackalWithSidekick"));
                    }
                    else
                    {
                        task.Text = cs(roleInfo.color, $"{roleInfo.name}: " + ModTranslation.getString("jackalShortDesc"));
                    }
                }
                else
                {*/
                task.Text = cs(roleInfo.color, $"{roleInfo.name}: {roleInfo.shortDescription}");
                // }

                player.myTasks.Insert(0, task);
            }

            /*if (player.hasModifier(ModifierType.Madmate) || player.hasModifier(ModifierType.CreatedMadmate))
            {
                var task = new GameObject("RoleTask").AddComponent<ImportantTextTask>();
                task.transform.SetParent(player.transform, false);
                task.Text = cs(Madmate.color, $"{Madmate.fullName}: " + ModTranslation.getString("madmateShortDesc"));
                player.myTasks.Insert(0, task);
            }*/
        }

        public static bool HidePlayerName(PlayerControl target)
        {
            return HidePlayerName(PlayerControl.LocalPlayer, target);
        }

        public static bool HidePlayerName(PlayerControl source, PlayerControl target)
        {
            if (source == target) return false;
            if (source == null || target == null) return true;
            if (source.IsDead()) return false;
            if (target.IsDead()) return true;
            else if (source.Data.Role.IsImpostor && (target.Data.Role.IsImpostor /*|| target == Spy.spy || target == Sidekick.sidekick && Sidekick.wasTeamRed || target == Jackal.jackal && Jackal.wasTeamRed*/)) return false; // Members of team Impostors see the names of Impostors/Spies
            // if (Camouflager.camouflageTimer > 0f) return true; // No names are visible
            // if (!source.isImpostor() && Ninja.isStealthed(target)) return true; // Hide ninja nametags from non-impostors
            // if (!source.isRole(RoleType.Fox) && !source.Data.IsDead && Fox.isStealthed(target)) return true;
            if (MapOptions.hideOutOfSightNametags && COHelpers.GameStarted && ShipStatus.Instance != null && source.transform != null && target.transform != null)
            {
                float distMod = 1.025f;
                float distance = Vector3.Distance(source.transform.position, target.transform.position);
                bool anythingBetween = PhysicsHelpers.AnythingBetween(source.GetTruePosition(), target.GetTruePosition(), Constants.ShadowMask, false);

                if (distance > ShipStatus.Instance.CalculateLightRadius(source.Data) * distMod || anythingBetween) return true;
            }
            // if (source.isImpostor() && (target.isImpostor() || target.isRole(RoleType.Spy))) return false; // Members of team Impostors see the names of Impostors/Spies
            // if (source.getPartner() == target) return false; // Members of team Lovers see the names of each other
            // if ((source.isRole(RoleType.Jackal) || source.isRole(RoleType.Sidekick)) && (target.isRole(RoleType.Jackal) || target.isRole(RoleType.Sidekick) || target == Jackal.fakeSidekick)) return false; // Members of team Jackal see the names of each other
            return true;
        }

        public static bool IsSkeld(this PlayerControl player)
        {
            if (PlayerControl.GameOptions.MapId == 0)
                return true;
            return false;
        }

        public static bool IsMiraHQ(this PlayerControl player)
        {
            if (PlayerControl.GameOptions.MapId == 1)
                return true;
            return false;
        }

        public static bool IsAirship(this PlayerControl player)
        {
            if (PlayerControl.GameOptions.MapId == 4)
                return true;
            return false;
        }

        public static MurderAttemptResult CheckMurderAttempt(PlayerControl killer, PlayerControl target, bool blockRewind = false)
        {
            // Modified vanilla checks
            if (AmongUsClient.Instance.IsGameOver) return MurderAttemptResult.SuppressKill;
            if (killer == null || killer.Data == null || killer.Data.IsDead || killer.Data.Disconnected) return MurderAttemptResult.SuppressKill; // Allow non Impostor kills compared to vanilla code
            if (target == null || target.Data == null || target.Data.IsDead || target.Data.Disconnected) return MurderAttemptResult.SuppressKill; // Allow killing players in vents compared to vanilla code

            return MurderAttemptResult.PerformKill;
        }

        public static MurderAttemptResult CheckMurderAttemptAndKill(PlayerControl killer, PlayerControl target, bool isMeetingStart = false, bool showAnimation = true)
        {
            // The local player checks for the validity of the kill and performs it afterwards (different to vanilla, where the host performs all the checks)
            // The kill attempt will be shared using a custom RPC, hence combining modded and unModded versions is impossible

            MurderAttemptResult murder = CheckMurderAttempt(killer, target, isMeetingStart);
            if (murder == MurderAttemptResult.PerformKill)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedMurderPlayer, Hazel.SendOption.Reliable, -1);
                writer.Write(killer.PlayerId);
                writer.Write(target.PlayerId);
                writer.Write(showAnimation ? Byte.MaxValue : 0);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.UncheckedMurderPlayer(killer.PlayerId, target.PlayerId, showAnimation ? Byte.MaxValue : (byte)0);
            }
            return murder;
        }

        public static bool RoleCanUseVents(this PlayerControl player)
        {
            bool roleCouldUse = false;
            if (Engineer.CanUseVents && player.isRole(RoleType.Engineer))
                roleCouldUse = true;
            if (Jester.CanUseVents && player.isRole(RoleType.Jester))
                roleCouldUse = true;
            else if (player.Data?.Role != null && player.Data.Role.CanVent)
            {
                roleCouldUse = true;
            }
            return roleCouldUse;
        }

        public static bool RoleCanSabotage(this PlayerControl player)
        {
            bool roleCouldUse = false;
            if (Jester.CanSabotage && player.isRole(RoleType.Jester))
                roleCouldUse = true;
            else if (player.Data?.Role != null && player.Data.Role.IsImpostor)
                roleCouldUse = true;

            return roleCouldUse;
        }

        public static object TryCast(this Il2CppObjectBase self, Type type)
        {
            return AccessTools.Method(self.GetType(), nameof(Il2CppObjectBase.TryCast)).MakeGenericMethod(type).Invoke(self, Array.Empty<object>());
        }

        public static string DeleteHTML(this string name)
        {
            var PlayerName = name.Replace("\n", "").Replace("\r", "");
            while (PlayerName.Contains("<") || PlayerName.Contains(">"))
            {
                PlayerName = PlayerName.Remove(PlayerName.IndexOf("<"), PlayerName.IndexOf(">") - PlayerName.IndexOf("<") + 1);
            }
            return PlayerName;
        }

        public static string ColorToHex(Color32 color)
        {
            string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2") + color.a.ToString("X2");
            return hex;
        }

        public static string GetColorHEX(InnerNet.ClientData Client)
        {
            try
            {
                return ColorToHex(Palette.PlayerColors[Client.ColorId]);
            }
            catch
            {
                return "";
            }
        }

        public static bool IsLobby()
        {
            if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Joined)
                return true;
            return false;
        }

        public static void ClearAllTasks(this PlayerControl player)
        {
            if (player == null) return;
            for (int i = 0; i < player.myTasks.Count; i++)
            {
                PlayerTask playerTask = player.myTasks[i];
                playerTask.OnRemove();
                UnityEngine.Object.Destroy(playerTask.gameObject);
            }
            player.myTasks.Clear();

            if (player.Data != null && player.Data.Tasks != null)
                player.Data.Tasks.Clear();
        }

        public static PlayerControl GetPlayerControl(this byte id)
        {
            return PlayerById(id);
        }

        public static int GetRandomInt(int max, int min = 0)
        {
            return UnityEngine.Random.Range(min, max + 1);
        }

        public static void GenerateAndAssignTasks(this PlayerControl player, int numCommon, int numShort, int numLong)
        {
            if (player == null) return;

            List<byte> taskTypeIds = GenerateTasks(numCommon, numShort, numLong);

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedSetTasks, Hazel.SendOption.Reliable, -1);
            writer.Write(player.PlayerId);
            writer.WriteBytesAndSize(taskTypeIds.ToArray());
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.UncheckedSetTasks(player.PlayerId, taskTypeIds.ToArray());
        }
    }
}