namespace UltimateMods.Patches
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    class PlayerNameColorPatch
    {
        static void ResetNameTagsAndColors() { }

        static void SetPlayerNameColor(PlayerControl p, Color color)
        {
            p.cosmetics.nameText.color = color;
            if (MeetingHud.Instance != null) foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                    if (player.NameText != null && p.PlayerId == player.TargetPlayerId) player.NameText.color = color;
        }

        static void SetNameColors()
        {
            var p = PlayerControl.LocalPlayer;
            var role = p.IsRole;

            switch (p.GetRoleId())
            {
                case RoleId.Impostor:
                case RoleId.Adversity:
                case RoleId.CustomImpostor:
                case RoleId.UnderTaker:
                case RoleId.BountyHunter:
                case RoleId.Teleporter:
                case RoleId.EvilHacker:
                    SetPlayerNameColor(p, ImpostorRed);
                    break;

                case RoleId.Sheriff: SetPlayerNameColor(p, SheriffYellow); break;
                case RoleId.ProEngineer: SetPlayerNameColor(p, EngineerBlue); break;
                case RoleId.Bakery: SetPlayerNameColor(p, BakeryYellow); break;
                case RoleId.Snitch: SetPlayerNameColor(p, SnitchGreen); break;
                case RoleId.Seer: SetPlayerNameColor(p, SeerGreen); break;
                // case RoleId.Lighter: SetPlayerNameColor(p, LighterYellow); break;
                case RoleId.Altruist: SetPlayerNameColor(p, AltruistRed); break;
                case RoleId.Mayor: SetPlayerNameColor(p, MayorGreen); break;
                case RoleId.Crewmate: SetPlayerNameColor(p, CrewmateBlue); break;
                case RoleId.Engineer: SetPlayerNameColor(p, EngineerOrange); break;
                case RoleId.Scientist: SetPlayerNameColor(p, ScientistBlue); break;

                case RoleId.Jester: SetPlayerNameColor(p, JesterPink); break;
                case RoleId.Arsonist: SetPlayerNameColor(p, ArsonistOrange); break;

                case RoleId.Madmate:
                    SetPlayerNameColor(p, ImpostorRed);
                    if (Madmate.KnowsImpostors(p))
                        foreach (var pc in PlayerControl.AllPlayerControls)
                            if (pc.IsImpostor()) SetPlayerNameColor(p, Palette.ImpostorRed);
                    break;

                case RoleId.Jackal:
                case RoleId.Sidekick:
                    SetPlayerNameColor(p, JackalBlue);
                    foreach (var jk in Jackal.allPlayers) SetPlayerNameColor(jk, JackalBlue);
                    foreach (var sk in Sidekick.allPlayers) SetPlayerNameColor(sk, JackalBlue);
                    break;
            }
        }

        static void Postfix()
        {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;

            ResetNameTagsAndColors();
            SetNameColors();
        }
    }
}