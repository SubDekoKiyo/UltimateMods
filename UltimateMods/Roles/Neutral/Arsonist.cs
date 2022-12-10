using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UltimateMods.Modules;
using Hazel;
using System.Linq;
using System;
using UltimateMods.Utilities;
using static UltimateMods.Modules.Assets;
using static UltimateMods.ColorDictionary;
using static UltimateMods.Roles.Patches.OutlinePatch;

namespace UltimateMods.Roles
{
    [HarmonyPatch]
    public class Arsonist : RoleBase<Arsonist>
    {
        public static PlayerControl CurrentTarget;
        public static PlayerControl DouseTarget;
        public static List<PlayerControl> DousedPlayers = new();
        private static CustomButton ArsonistButton;
        private static CustomButton IgniteButton;

        public static float Cooldown { get { return CustomRolesH.ArsonistCooldown.getFloat(); } }
        public static float Duration { get { return CustomRolesH.ArsonistDuration.getFloat(); } }
        public static bool TriggerArsonistWin = false;
        public static bool DousedEveryone = false;

        public Arsonist()
        {
            RoleType = roleId = RoleType.Arsonist;
        }

        private static Sprite DouseSprite;
        private static Sprite IgniteSprite;
        public static Sprite GetDouseSprite()
        {
            if (DouseSprite) return DouseSprite;
            DouseSprite = Helpers.LoadSpriteFromTexture2D(ArsonistDouseButton, 115f);
            return DouseSprite;
        }
        public static Sprite GetIgniteSprite()
        {
            if (IgniteSprite) return IgniteSprite;
            IgniteSprite = Helpers.LoadSpriteFromTexture2D(ArsonistIgniteButton, 115f);
            return IgniteSprite;
        }

        public static Sprite GetArsonistButtonSprite()
        {
            if (DousedEveryone)
                return GetIgniteSprite();
            else
                return GetDouseSprite();
        }

        public static bool DousedEveryoneAlive()
        {
            return PlayerControl.AllPlayerControls.ToArray().All(x => { return x == x.isRole(RoleType.Arsonist) || x.Data.IsDead || x.Data.Disconnected || DousedPlayers.Any(y => y.PlayerId == x.PlayerId); });
        }

        public static void UpdateStatus()
        {
            if (PlayerControl.LocalPlayer.isRole(RoleType.Arsonist))
            {
                DousedEveryone = DousedEveryoneAlive();
            }
        }

        public static void UpdateIcons()
        {
            foreach (PoolablePlayer pp in ModMapOptions.PlayerIcons.Values)
            {
                pp.gameObject.SetActive(false);
            }

            if (PlayerControl.LocalPlayer.isRole(RoleType.Arsonist))
            {
                int visibleCounter = 0;
                Vector3 bottomLeft = FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition;
                bottomLeft.x *= -1;
                bottomLeft += new Vector3(-0.25f, -0.25f, 0);

                foreach (PlayerControl p in PlayerControl.AllPlayerControls.GetFastEnumerator())
                {
                    if (p.PlayerId == PlayerControl.LocalPlayer.PlayerId) continue;
                    if (!ModMapOptions.PlayerIcons.ContainsKey(p.PlayerId)) continue;

                    if (p.Data.IsDead || p.Data.Disconnected)
                    {
                        ModMapOptions.PlayerIcons[p.PlayerId].gameObject.SetActive(false);
                    }
                    else
                    {
                        ModMapOptions.PlayerIcons[p.PlayerId].gameObject.SetActive(true);
                        ModMapOptions.PlayerIcons[p.PlayerId].transform.localScale = Vector3.one * 0.25f;
                        ModMapOptions.PlayerIcons[p.PlayerId].transform.localPosition = bottomLeft + Vector3.right * visibleCounter * 0.45f;
                        visibleCounter++;
                    }
                    bool isDoused = DousedPlayers.Any(x => x.PlayerId == p.PlayerId);
                    ModMapOptions.PlayerIcons[p.PlayerId].SetSemiTransparent(!isDoused);
                }
            }
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd()
        {
            UpdateIcons();
        }

        public override void FixedUpdate()
        {
            if (PlayerControl.LocalPlayer.isRole(RoleType.Arsonist))
            {
                List<PlayerControl> UnTargetables;
                if (DouseTarget != null)
                    UnTargetables = PlayerControl.AllPlayerControls.ToArray().Where(x => x.PlayerId != DouseTarget.PlayerId).ToList();
                else
                    UnTargetables = DousedPlayers;
                CurrentTarget = SetTarget(untargetablePlayers: UnTargetables);
                if (CurrentTarget != null) SetPlayerOutline(CurrentTarget, ArsonistOrange);
            }

            if (DousedEveryone)
            {
                ArsonistButton.MaxTimer = 0f;
            }
        }

        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void MakeButtons(HudManager hm)
        {
            ArsonistButton = new CustomButton(
                () =>
                {
                    if (CurrentTarget != null)
                    {
                        DouseTarget = CurrentTarget;
                    }
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleType.Arsonist) && !DousedEveryone && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (DousedEveryone)
                        ArsonistButton.ButtonText = ModTranslation.getString("IgniteText");
                    else
                        ArsonistButton.ButtonText = ModTranslation.getString("DouseText");

                    if (ArsonistButton.IsEffectActive && DouseTarget != CurrentTarget)
                    {
                        DouseTarget = null;
                        ArsonistButton.Timer = 0f;
                        ArsonistButton.IsEffectActive = false;
                    }

                    return PlayerControl.LocalPlayer.CanMove && CurrentTarget != null;
                },
                () =>
                {
                    ArsonistButton.Timer = ArsonistButton.MaxTimer;
                    ArsonistButton.IsEffectActive = false;
                    DouseTarget = null;
                    UpdateStatus();
                },
                GetDouseSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                hm,
                hm.KillButton,
                KeyCode.F,
                true,
                Duration,
                () =>
                {
                    if (DouseTarget != null)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ArsonistDouse, Hazel.SendOption.Reliable, -1);
                        writer.Write(DouseTarget.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.ArsonistDouse(DouseTarget.PlayerId);
                    }

                    DouseTarget = null;
                    UpdateStatus();
                    ArsonistButton.Timer = DousedEveryone ? 0 : ArsonistButton.MaxTimer;

                    foreach (PlayerControl p in DousedPlayers)
                    {
                        if (ModMapOptions.PlayerIcons.ContainsKey(p.PlayerId))
                        {
                            ModMapOptions.PlayerIcons[p.PlayerId].SetSemiTransparent(false);
                        }
                    }
                }
            );
            ArsonistButton.ButtonText = ModTranslation.getString("DouseText");

            IgniteButton = new CustomButton(
                () =>
                {
                    if (DousedEveryone)
                    {
                        MessageWriter winWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ArsonistWin, Hazel.SendOption.Reliable, -1);
                        AmongUsClient.Instance.FinishRpcImmediately(winWriter);
                        RPCProcedure.ArsonistWin();
                    }
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleType.Arsonist) && !PlayerControl.LocalPlayer.Data.IsDead && DousedEveryone && !TriggerArsonistWin; },
                () => { return PlayerControl.LocalPlayer.CanMove && DousedEveryone; },
                () => { },
                GetIgniteSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                hm,
                hm.KillButton,
                KeyCode.F
            );
            IgniteButton.ButtonText = ModTranslation.getString("IgniteText");
        }

        public static void SetButtonCooldowns()
        {
            ArsonistButton.MaxTimer = Cooldown;
            IgniteButton.Timer = IgniteButton.MaxTimer = 0f;
        }

        public static void Clear()
        {
            CurrentTarget = null;
            DouseTarget = null;
            TriggerArsonistWin = false;
            DousedEveryone = false;
            DousedPlayers = new();
            foreach (PoolablePlayer p in ModMapOptions.PlayerIcons.Values)
            {
                if (p != null && p.gameObject != null) p.gameObject.SetActive(false);
            }
            players = new List<Arsonist>();
        }
    }
}