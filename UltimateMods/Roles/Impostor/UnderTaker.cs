using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UltimateMods.Modules;
using Hazel;
using TMPro;
using UltimateMods.Patches;
using static UltimateMods.Modules.Assets;

namespace UltimateMods.Roles
{
    [HarmonyPatch]
    public class UnderTaker : RoleBase<UnderTaker>
    {
        private static CustomButton UnderTakerButton;
        public static TMP_Text UnderTakerButtonText;
        public static Sprite UnderTakerButtonSprite;

        public static float KillCooldown { get { return CustomRolesH.UnderTakerKillCooldown.getFloat(); } }
        public static float MoveCooldown { get { return CustomRolesH.UnderTakerDuration.getFloat(); } }
        public static bool HasDuration { get { return CustomRolesH.UnderTakerHasDuration.getBool(); } }
        public static float Duration { get { return CustomRolesH.UnderTakerDuration.getFloat(); } }
        public static float SpeedDown { get { return CustomRolesH.UnderTakerDraggingSpeed.getFloat(); } }
        public static bool CanDumpVent { get { return CustomRolesH.UnderTakerCanDumpBodyVents.getBool(); } }

        public static bool DraggingBody = false;
        public static byte BodyId = 0;

        public UnderTaker()
        {
            RoleType = roleId = RoleType.UnderTaker;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd()
        {
            if (PlayerControl.LocalPlayer == player)
                player.SetKillTimerUnchecked(KillCooldown);
        }
        public override void FixedUpdate()
        {
            if (UnderTaker.DraggingBody)
            {
                DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
                for (int i = 0; i < array.Length; i++)
                {
                    if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == UnderTaker.BodyId)
                    {
                        foreach (var underTaker in UnderTaker.allPlayers)
                        {
                            var currentPosition = underTaker.GetTruePosition();
                            var velocity = underTaker.gameObject.GetComponent<Rigidbody2D>().velocity.normalized;
                            var newPos = ((Vector2)underTaker.GetTruePosition()) - (velocity / 3) + new Vector2(0.15f, 0.25f) + array[i].myCollider.offset;
                            if (!PhysicsHelpers.AnythingBetween(
                                currentPosition,
                                newPos,
                                Constants.ShipAndObjectsMask,
                                false
                            ))
                            {
                                if (PlayerControl.GameOptions.MapId == 5)
                                {
                                    array[i].transform.position = newPos;
                                    array[i].transform.position += new Vector3(0, 0, -0.5f);
                                }
                                else
                                {
                                    array[i].transform.position = newPos;
                                }
                            }
                        }
                    }
                }
            }
        }
        public override void OnKill(PlayerControl target)
        {

            if (PlayerControl.LocalPlayer == player)
                player.SetKillTimerUnchecked(KillCooldown);
        }
        public override void OnDeath(PlayerControl killer = null)
        {
            if (DraggingBody)
                UnderTaker.UnderTakerResetValuesAtDead();
        }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static Sprite GetButtonSprite()
        {
            if (UnderTakerButtonSprite) return UnderTakerButtonSprite;
            UnderTakerButtonSprite = Helpers.LoadSpriteFromTexture2D(UnderTakerMoveButton, 115f);
            return UnderTakerButtonSprite;
        }

        public static void MakeButtons(HudManager hm)
        {
            UnderTakerButton = new CustomButton(
                () =>
                {
                    if (UnderTaker.DraggingBody)
                    {
                        if (HasDuration && UnderTakerButton.IsEffectActive)
                        {
                            UnderTakerButton.Timer = 0f;
                            return;
                        }
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DragPlaceBody, Hazel.SendOption.Reliable, -1);
                        writer.Write(UnderTaker.BodyId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.DragPlaceBody(UnderTaker.BodyId);

                        foreach (var underTaker in UnderTaker.allPlayers)
                            underTaker.killTimer = KillCooldown;
                        UnderTakerButton.Timer = UnderTakerButton.MaxTimer = MoveCooldown;
                    }
                    else
                    {
                        foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(), 1f, Constants.PlayersOnlyMask))
                        {
                            if (collider2D.tag == "DeadBody")
                            {
                                DeadBody component = collider2D.GetComponent<DeadBody>();
                                if (component && !component.Reported)
                                {
                                    Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                                    Vector2 truePosition2 = component.TruePosition;
                                    if (Vector2.Distance(truePosition2, truePosition) <= PlayerControl.LocalPlayer.MaxReportDistance && PlayerControl.LocalPlayer.CanMove && !PhysicsHelpers.AnythingBetween(truePosition, truePosition2, Constants.ShipAndObjectsMask, false))
                                    {
                                        GameData.PlayerInfo playerInfo = GameData.Instance.GetPlayerById(component.ParentId);
                                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DragPlaceBody, Hazel.SendOption.Reliable, -1);
                                        writer.Write(playerInfo.PlayerId);
                                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                                        RPCProcedure.DragPlaceBody(playerInfo.PlayerId);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                },
                () => { return PlayerControl.LocalPlayer.isRole(RoleType.UnderTaker) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (UnderTaker.DraggingBody)
                        UnderTakerButton.ButtonText = ModTranslation.getString("UnderTakerDropText");
                    else
                        UnderTakerButton.ButtonText = ModTranslation.getString("UnderTakerDragText");
                    bool canDrag = false;
                    foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(), 1f, Constants.PlayersOnlyMask))
                        if (collider2D.tag == "DeadBody")
                            canDrag = true;
                    return canDrag && PlayerControl.LocalPlayer.CanMove;
                },
                () =>
                {
                    UnderTakerButton.Timer = UnderTakerButton.MaxTimer = MoveCooldown;
                    UnderTaker.UnderTakerResetValuesAtDead();
                },
                UnderTaker.GetButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0f),
                hm,
                hm.KillButton,
                KeyCode.F,
                HasDuration,
                Duration,
                () =>
                {
                    if (DraggingBody)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DragPlaceBody, Hazel.SendOption.Reliable, -1);
                        writer.Write(UnderTaker.BodyId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.DragPlaceBody(UnderTaker.BodyId);
                    }
                    else
                    {
                        UnderTaker.DraggingBody = false;
                        UnderTaker.BodyId = 0;
                        foreach (var underTaker in UnderTaker.allPlayers)
                            underTaker.killTimer = KillCooldown;
                    }
                    UnderTakerButton.Timer = UnderTakerButton.MaxTimer = MoveCooldown;
                }
            );
            UnderTakerButton.ButtonText = ModTranslation.getString("UnderTakerDragText");
            UnderTakerButton.EffectCancellable = true;
        }

        public static void SetButtonCooldowns()
        {
            UnderTakerButton.MaxTimer = MoveCooldown;
            UnderTakerButton.EffectDuration = Duration;
        }
        public static void UnderTakerResetValuesAtDead()
        {
            DraggingBody = false;
            BodyId = 0;
            if (PlayerControl.GameOptions.MapId == 5)
            {
                GameObject vent = GameObject.Find("LowerCentralVent");
                vent.GetComponent<BoxCollider2D>().enabled = true;
            }
        }

        public static void Clear()
        {
            players = new List<UnderTaker>();
        }

        [HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent))]
        class DumpDeadBody
        {
            public static void Postfix()
            {
                if (CanDumpVent && DraggingBody)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CleanBody, Hazel.SendOption.Reliable, -1);
                    writer.Write(BodyId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.CleanBody(BodyId);
                    DraggingBody = false;
                    if (HasDuration && UnderTakerButton.IsEffectActive)
                    {
                        UnderTakerButton.Timer = 0f;
                        return;
                    }
                }
            }
        }
    }
}