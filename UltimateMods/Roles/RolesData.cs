namespace UltimateMods.Roles;

public abstract class Role
{
    public static List<Role> allRoles = new();
    public PlayerControl player;
    public RoleId roleId;

    public abstract void OnMeetingStart();
    public abstract void OnMeetingEnd();
    public abstract void FixedUpdate();
    public abstract void OnKill(PlayerControl target);
    public abstract void OnDeath(PlayerControl killer = null);
    public abstract void HandleDisconnect(PlayerControl player, DisconnectReasons reason);
    public abstract void Clear();

    public static void ClearAll()
    {
        allRoles = new();
    }
}

[HarmonyPatch]
public abstract class RoleBase<T> : Role where T : RoleBase<T>, new()
{
    public static List<T> players = new();
    public static RoleId RoleId;

    public void Init(PlayerControl player)
    {
        this.player = player;
        players.Add((T)this);
        allRoles.Add(this);
    }

    public static T local
    {
        get
        {
            return players.FirstOrDefault(x => x.player == PlayerControl.LocalPlayer);
        }
    }

    public static List<PlayerControl> allPlayers
    {
        get
        {
            return players.Select(x => x.player).ToList();
        }
    }

    public static List<PlayerControl> livingPlayers
    {
        get
        {
            return players.Select(x => x.player).Where(x => x.IsAlive()).ToList();
        }
    }

    public static List<PlayerControl> deadPlayers
    {
        get
        {
            return players.Select(x => x.player).Where(x => !x.IsAlive()).ToList();
        }
    }

    public static bool exists
    {
        get { return Helpers.RolesEnabled && players.Count > 0; }
    }

    public static T GetRole(PlayerControl player = null)
    {
        player = player ?? PlayerControl.LocalPlayer;
        return players.FirstOrDefault(x => x.player == player);
    }

    public static bool IsRole(PlayerControl player)
    {
        return players.Any(x => x.player == player);
    }

    public static void SetRole(PlayerControl player)
    {
        if (!IsRole(player))
        {
            T role = new T();
            role.Init(player);
        }
    }

    public static void EraseRole(PlayerControl player)
    {
        players.RemoveAll(x => x.player == player && x.roleId == RoleId);
        allRoles.RemoveAll(x => x.player == player && x.roleId == RoleId);
    }

    public static void SwapRole(PlayerControl p1, PlayerControl p2)
    {
        var index = players.FindIndex(x => x.player == p1);
        if (index >= 0)
            players[index].player = p2;
    }
}

public enum RoleId
{
    NoRole = 0,
    Crewmate,
    Impostor,
    Engineer,
    ShapeShifter,
    Scientist,

    // 新しく書く場合は陣営関係なく一番下へ
    Sheriff = 10,
    ProEngineer,
    Madmate,
    Bakery,
    Snitch,
    Seer,
    Lighter,
    Altruist,
    Mayor,
    CustomImpostor,
    UnderTaker,
    BountyHunter,
    Teleporter,
    EvilHacker,
    Adversity,
    Jester,
    Jackal,
    Sidekick,
    Arsonist,
}

public enum GhostRoleId
{
    GuardianEngel = 0,

    // 新しく書く場合は陣営関係なく一番下へ
}

[HarmonyPatch]
public static class RoleData
{
    public static Dictionary<RoleId, Type> allRoleIds = new()
    {
        // Crewmate
        { RoleId.Sheriff, typeof(RoleBase<Sheriff>) },
        { RoleId.ProEngineer, typeof(RoleBase<ProEngineer>) },
        { RoleId.Madmate, typeof(RoleBase<Madmate>) },
        { RoleId.Bakery, typeof(RoleBase<Bakery>) },
        { RoleId.Snitch, typeof(RoleBase<Snitch>) },
        { RoleId.Seer, typeof(RoleBase<Seer>) },
        { RoleId.Lighter, typeof(RoleBase<Lighter>) },
        { RoleId.Altruist, typeof(RoleBase<Altruist>) },
        { RoleId.Mayor, typeof(RoleBase<Mayor>) },

        // Neutral
        { RoleId.Jester, typeof(RoleBase<Jester>) },
        { RoleId.Jackal, typeof(RoleBase<Jackal>) },
        { RoleId.Sidekick, typeof(RoleBase<Sidekick>) },
        { RoleId.Arsonist, typeof(RoleBase<Arsonist>) },

        // Impostor
        { RoleId.CustomImpostor, typeof(RoleBase<CustomImpostor>) },
        { RoleId.UnderTaker, typeof(RoleBase<UnderTaker>) },
        { RoleId.BountyHunter, typeof(RoleBase<BountyHunter>) },
        { RoleId.Teleporter, typeof(RoleBase<Teleporter>) },
        { RoleId.EvilHacker, typeof(RoleBase<EvilHacker>) },
        { RoleId.Adversity, typeof(RoleBase<Adversity>) },
    };
}