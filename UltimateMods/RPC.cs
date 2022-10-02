using HarmonyLib;
using Hazel;
using UltimateMods.Patches;
using UltimateMods.Modules;
using System.Linq;
using System;
using static UltimateMods.GameHistory;
using UltimateMods.Utilities;
using UltimateMods.Roles;
using UltimateMods.Objects;
using UltimateMods.EndGame;

namespace UltimateMods
{
    enum CustomRPC
    {
        ResetVariables = 60,
        ShareOptions,
        DynamicMapOption,
        VersionHandshake,
        SetRole,
        AddModifier,
        UseAdminTime,
        UseCameraTime,
        UseVitalsTime,
        UncheckedMurderPlayer,
        SheriffKill = 70,
        EngineerFixLights,
        EngineerUsedRepair,
        EngineerFixSubmergedOxygen,
        UncheckedSetTasks,
    }

    public static class RPCProcedure
    {
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
        class RPCHandlerPatch
        {
            static void Postfix([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
            {
                byte packetId = callId;
                switch (packetId)
                {
                    // 60
                    case (byte)CustomRPC.ResetVariables:
                        RPCProcedure.ResetVariables();
                        break;
                    // 61
                    case (byte)CustomRPC.ShareOptions:
                        RPCProcedure.ShareOptions((int)reader.ReadPackedUInt32(), reader);
                        break;
                    // 62
                    case (byte)CustomRPC.DynamicMapOption:
                        byte mapId = reader.ReadByte();
                        RPCProcedure.DynamicMapOption(mapId);
                        break;
                    // 63
                    case (byte)CustomRPC.VersionHandshake:
                        int major = reader.ReadPackedInt32();
                        int minor = reader.ReadPackedInt32();
                        int patch = reader.ReadPackedInt32();
                        int versionOwnerId = reader.ReadPackedInt32();
                        byte revision = 0xFF;
                        Guid guid;
                        if (reader.Length - reader.Position >= 17)
                        { // enough bytes left to read
                            revision = reader.ReadByte();
                            // GUID
                            byte[] GBytes = reader.ReadBytes(16);
                            guid = new Guid(GBytes);
                        }
                        else
                        {
                            guid = new Guid(new byte[16]);
                        }
                        RPCProcedure.VersionHandshake(major, minor, patch, revision == 0xFF ? -1 : revision, guid, versionOwnerId);
                        break;
                    // 64
                    case (byte)CustomRPC.SetRole:
                        byte roleId = reader.ReadByte();
                        byte playerId = reader.ReadByte();
                        byte flag = reader.ReadByte();
                        RPCProcedure.setRole(roleId, playerId, flag);
                        break;
                    // 65
                    case (byte)CustomRPC.AddModifier:
                        RPCProcedure.AddModifier(reader.ReadByte(), reader.ReadByte());
                        break;
                    // 66
                    case (byte)CustomRPC.UseAdminTime:
                        RPCProcedure.UseAdminTime(reader.ReadSingle());
                        break;
                    // 67
                    case (byte)CustomRPC.UseCameraTime:
                        RPCProcedure.UseCameraTime(reader.ReadSingle());
                        break;
                    // 68
                    case (byte)CustomRPC.UseVitalsTime:
                        RPCProcedure.UseVitalsTime(reader.ReadSingle());
                        break;
                    // 69
                    case (byte)CustomRPC.UncheckedMurderPlayer:
                        byte source = reader.ReadByte();
                        byte target = reader.ReadByte();
                        byte showAnimation = reader.ReadByte();
                        RPCProcedure.UncheckedMurderPlayer(source, target, showAnimation);
                        break;
                    // 70
                    case (byte)CustomRPC.SheriffKill:
                        RPCProcedure.SheriffKill(reader.ReadByte(), reader.ReadByte(), reader.ReadBoolean());
                        break;
                    // 71
                    case (byte)CustomRPC.EngineerFixLights:
                        RPCProcedure.EngineerFixLights();
                        break;
                    // 72
                    case (byte)CustomRPC.EngineerUsedRepair:
                        RPCProcedure.EngineerUsedRepair(reader.ReadByte());
                        break;
                    // 73
                    case (byte)CustomRPC.EngineerFixSubmergedOxygen:
                        RPCProcedure.EngineerFixSubmergedOxygen();
                        break;
                    // 74
                    case (byte)CustomRPC.UncheckedSetTasks:
                        RPCProcedure.UncheckedSetTasks(reader.ReadByte(), reader.ReadBytesAndSize());
                        break;
                }
            }
        }

        public static void ResetVariables()
        {
            MapOptions.ClearAndReloadMapOptions();
            UltimateMods.ClearAndReloadRoles();
            GameHistory.clearGameHistory();
            AdminPatch.ResetData();
            CameraPatch.ResetData();
            VitalsPatch.ResetData();
            Buttons.SetCustomButtonCooldowns();
            CustomOverlays.ResetOverlays();
            MapBehaviorPatch.ResetIcons();
            // CustomLobbyPatch.ResetLobbyText();
        }

        public static void ShareOptions(int numberOfOptions, MessageReader reader)
        {
            try
            {
                for (int i = 0; i < numberOfOptions; i++)
                {
                    uint optionId = reader.ReadPackedUInt32();
                    uint selection = reader.ReadPackedUInt32();
                    CustomOption option = CustomOption.options.FirstOrDefault(option => option.id == (int)optionId);
                    option.updateSelection((int)selection);
                }
            }
            catch (Exception e)
            {
                UltimateModsPlugin.Logger.LogError("Error while deserializing options: " + e.Message);
            }
        }

        public static void DynamicMapOption(byte mapId)
        {
            PlayerControl.GameOptions.MapId = mapId;
        }

        public static void VersionHandshake(int major, int minor, int build, int revision, Guid guid, int clientId)
        {
            System.Version ver;
            if (revision < 0)
                ver = new System.Version(major, minor, build);
            else
                ver = new System.Version(major, minor, build, revision);
            GameStartManagerPatch.playerVersions[clientId] = new GameStartManagerPatch.PlayerVersion(ver, guid);
        }

        public static void setRole(byte roleId, byte playerId, byte flag)
        {
            PlayerControl.AllPlayerControls.ToArray().DoIf(
                x => x.PlayerId == playerId,
                x => x.setRole((RoleType)roleId)
            );
        }

        public static void AddModifier(byte modId, byte playerId)
        {
            PlayerControl.AllPlayerControls.ToArray().DoIf(
                x => x.PlayerId == playerId,
                x => x.AddModifier((ModifierType)modId)
            );
        }

        public static void UseAdminTime(float time)
        {
            MapOptions.restrictAdminTime -= time;
        }

        public static void UseCameraTime(float time)
        {
            MapOptions.restrictCamerasTime -= time;
        }

        public static void UseVitalsTime(float time)
        {
            MapOptions.restrictVitalsTime -= time;
        }

        public static void UncheckedMurderPlayer(byte sourceId, byte targetId, byte showAnimation)
        {
            PlayerControl source = Helpers.PlayerById(sourceId);
            PlayerControl target = Helpers.PlayerById(targetId);
            if (source != null && target != null)
            {
                if (showAnimation == 0) KillAnimationCoPerformKillPatch.hideNextAnimation = true;
                source.MurderPlayer(target);
            }
        }

        public static void SheriffKill(byte sheriffId, byte targetId, bool misfire)
        {
            PlayerControl sheriff = Helpers.PlayerById(sheriffId);
            PlayerControl target = Helpers.PlayerById(targetId);
            if (sheriff == null || target == null) return;

            Sheriff role = Sheriff.getRole(sheriff);
            if (role != null)
                role.NumShots--;

            if (misfire)
            {
                sheriff.MurderPlayer(sheriff);
                finalStatuses[sheriffId] = FinalStatus.Misfire;

                if (!Sheriff.MisfireKillsTarget) return;
                finalStatuses[targetId] = FinalStatus.Misfire;
            }

            sheriff.MurderPlayer(target);
        }

        public static void UpdateMeeting(byte targetId, bool dead = true)
        {
            if (MeetingHud.Instance)
            {
                foreach (PlayerVoteArea pva in MeetingHud.Instance.playerStates)
                {
                    if (pva.TargetPlayerId == targetId)
                    {
                        pva.SetDead(pva.DidReport, dead);
                        pva.Overlay.gameObject.SetActive(dead);
                    }

                    // Give players back their vote if target is shot dead
                    if (COHelpers.RefundVotes && dead)
                    {
                        if (pva.VotedFor != targetId) continue;
                        pva.UnsetVote();
                        var voteAreaPlayer = Helpers.PlayerById(pva.TargetPlayerId);
                        if (!voteAreaPlayer.AmOwner) continue;
                        MeetingHud.Instance.ClearVote();
                    }
                }

                if (AmongUsClient.Instance.AmHost)
                    MeetingHud.Instance.CheckForEndVoting();
            }
        }

        public static void EngineerFixLights()
        {
            SwitchSystem switchSystem = MapUtilities.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
            switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
        }

        public static void EngineerUsedRepair(byte engineerId)
        {
            PlayerControl engineer = Helpers.PlayerById(engineerId);
            Engineer role = Engineer.getRole(engineer);
            if (role != null)
                role.RemainingFixes--;
        }

        public static void EngineerFixSubmergedOxygen()
        {
            SubmergedCompatibility.RepairOxygen();
        }

        public static void UncheckedSetTasks(byte playerId, byte[] taskTypeIds)
        {
            var player = Helpers.PlayerById(playerId);
            player.ClearAllTasks();

            GameData.Instance.SetTasks(playerId, taskTypeIds);
        }
    }
}