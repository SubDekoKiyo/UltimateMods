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
        public static Sprite LoadSpriteFromTexture2D(Texture2D texture, float pixelsPerUnit)
        {
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }

        public static PlayerControl PlayerById(byte id)
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
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
            return (player.IsNeutral() && !player.NeutralHasTasks()) ||
            (player.isRole(RoleType.Madmate) && !Madmate.HasTasks);
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
                    var info = infos.FirstOrDefault(x => textTask.Text.StartsWith(x.Name));
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
                task.Text = cs(roleInfo.color, $"{roleInfo.Name}: {roleInfo.ShortDescription}");
                // }

                player.myTasks.Insert(0, task);
            }
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
            else if (source.Data.Role.IsImpostor && target.Data.Role.IsImpostor)/* || target == Spy.spy || target == Sidekick.sidekick && Sidekick.wasTeamRed || target == Jackal.jackal && Jackal.wasTeamRed))*/ return false;/* // Members of team Impostors see the names of Impostors/Spies
            // if (Camouflager.camouflageTimer > 0f) return true; // No names are visible
            // if (!source.isImpostor() && Ninja.isStealthed(target)) return true; // Hide ninja nametags from non-impostors
            // if (!source.isRole(RoleType.Fox) && !source.Data.IsDead && Fox.isStealthed(target)) return true;
            */
            if (MapOptions.HideOutOfSightNametags && COHelpers.GameStarted && ShipStatus.Instance != null && source.transform != null && target.transform != null)
            {
                float distMod = 1.025f;
                float distance = Vector3.Distance(source.transform.position, target.transform.position);
                bool anythingBetween = PhysicsHelpers.AnythingBetween(source.GetTruePosition(), target.GetTruePosition(), Constants.ShadowMask, false);

                if (distance > ShipStatus.Instance.CalculateLightRadius(source.Data) * distMod || anythingBetween) return true;
            }
            if (!MapOptions.HidePlayerNames) return false; // All names are visible
            // if (source.isImpostor() && (target.isImpostor() || target.isRole(RoleType.Spy))) return false; // Members of team Impostors see the names of Impostors/Spies
            // if (source.getPartner() == target) return false; // Members of team Lovers see the names of each other
            // if ((source.isRole(RoleType.Jackal) || source.isRole(RoleType.Sidekick)) && (target.isRole(RoleType.Jackal) || target.isRole(RoleType.Sidekick) || target == Jackal.fakeSidekick)) return false; // Members of team Jackal see the names of each other
            return true;
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
            else if (Jester.CanUseVents && player.isRole(RoleType.Jester))
                roleCouldUse = true;
            else if (Madmate.CanUseVents && player.isRole(RoleType.Madmate))
                roleCouldUse = true;
            else if (player.Data?.Role != null && player.Data.Role.CanVent)
            {
                if (!CustomImpostor.CanUseVents && player.isRole(RoleType.CustomImpostor))
                    roleCouldUse = false;
                else
                    roleCouldUse = true;
            }
            return roleCouldUse;
        }

        public static bool RoleCanSabotage(this PlayerControl player)
        {
            bool roleCouldUse = false;
            if (Jester.CanSabotage && player.isRole(RoleType.Jester))
                roleCouldUse = true;
            else if (Madmate.CanSabotage && player.isRole(RoleType.Madmate))
                roleCouldUse = true;
            else if (!CustomImpostor.CanSabotage && player.isRole(RoleType.CustomImpostor))
                roleCouldUse = false;
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

        public static bool HasImpostorVision(GameData.PlayerInfo player)
        {
            return player.Role.IsImpostor
                || (PlayerControl.LocalPlayer.isRole(RoleType.Jester) && Jester.HasImpostorVision)
                || (PlayerControl.LocalPlayer.isRole(RoleType.Madmate) && Madmate.HasImpostorVision);
        }

        public static T GetRandom<T>(this List<T> list)
        {
            var random = rnd.Next(0, list.Count);
            return list[random];
        }

        public static void ShowFlash(Color color, float duration = 1f)
        {
            if (FastDestroyableSingleton<HudManager>.Instance == null || FastDestroyableSingleton<HudManager>.Instance.FullScreen == null) return;
            FastDestroyableSingleton<HudManager>.Instance.FullScreen.gameObject.SetActive(true);
            FastDestroyableSingleton<HudManager>.Instance.FullScreen.enabled = true;
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(duration, new Action<float>((p) =>
            {
                var renderer = FastDestroyableSingleton<HudManager>.Instance.FullScreen;

                if (p < 0.5)
                {
                    if (renderer != null)
                        renderer.color = new(color.r, color.g, color.b, Mathf.Clamp01(p * 2 * 0.75f));
                }
                else
                {
                    if (renderer != null)
                        renderer.color = new(color.r, color.g, color.b, Mathf.Clamp01((1 - p) * 2 * 0.75f));
                }
                if (p == 1f && renderer != null) renderer.enabled = false;
            })));
        }
    }
}