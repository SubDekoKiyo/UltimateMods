namespace UltimateMods.Patches
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    class PlayerNameColorPatch
    {
        static void ResetNameTagsAndColors() { }

        static void setPlayerNameColor(PlayerControl p, Color color)
        {
            p.cosmetics.nameText.color = color;
            if (MeetingHud.Instance != null)
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                    if (player.NameText != null && p.PlayerId == player.TargetPlayerId)
                        player.NameText.color = color;
        }

        static void setNameColors()
        {
            var p = PlayerControl.LocalPlayer;
            var role = p.isRole;

            if (role(RoleType.Jester))
                setPlayerNameColor(p, JesterPink);
            if (role(RoleType.Sheriff))
                setPlayerNameColor(p, SheriffYellow);
            if (role(RoleType.Engineer))
                setPlayerNameColor(p, EngineerBlue);
            if (role(RoleType.Arsonist))
                setPlayerNameColor(p, ArsonistOrange);
            if (role(RoleType.Seer))
                setPlayerNameColor(p, SeerGreen);
            if (role(RoleType.Snitch))
                setPlayerNameColor(p, SnitchGreen);
            if (role(RoleType.Bakery))
                setPlayerNameColor(p, BakeryYellow);

            if (role(RoleType.Madmate))
            {
                setPlayerNameColor(p, ImpostorRed);

                if (Madmate.KnowsImpostors(p))
                    foreach (var pc in PlayerControl.AllPlayerControls)
                        if (pc.IsImpostor())
                            setPlayerNameColor(p, Palette.ImpostorRed);
            }

            if (p.IsTeamJackal())
            {
                foreach (var jk in Jackal.allPlayers)
                    setPlayerNameColor(jk, JackalBlue);
                foreach (var sk in Sidekick.allPlayers)
                    setPlayerNameColor(sk, JackalBlue);
            }
        }

        // 生存
        static void Postfix()
        {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;

            ResetNameTagsAndColors();
            setNameColors();
        }
    }
}