namespace UltimateMods.Roles;

public abstract class Modifier
{
    public static List<Modifier> allModifiers = new();
    public PlayerControl player;
    public ModifierId modId;

    public abstract string ModifierPostfix();
    public abstract void OnMeetingStart();
    public abstract void OnMeetingEnd();
    public abstract void FixedUpdate();
    public abstract void OnKill(PlayerControl target);
    public abstract void OnDeath(PlayerControl killer = null);
    public abstract void HandleDisconnect(PlayerControl player, DisconnectReasons reason);
    public abstract void Clear();

    public static void ClearAll()
    {
        allModifiers = new();
    }
}

[HarmonyPatch]
public abstract class ModifierBase<T> : Modifier where T : ModifierBase<T>, new()
{
    public static List<T> players = new();
    public static ModifierId ModId;

    public void Init(PlayerControl player)
    {
        this.player = player;
        players.Add((T)this);
        allModifiers.Add(this);
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

    public static T GetModifier(PlayerControl player = null)
    {
        player = player ?? PlayerControl.LocalPlayer;
        return players.FirstOrDefault(x => x.player == player);
    }

    public static bool HasModifier(PlayerControl player)
    {
        return players.Any(x => x.player == player);
    }

    public static void AddModifier(PlayerControl player)
    {
        if (!HasModifier(player))
        {
            T mod = new T();
            mod.Init(player);
        }
    }

    public static void EraseModifier(PlayerControl player)
    {
        players.RemoveAll(x => x.player == player && x.modId == ModId);
        allModifiers.RemoveAll(x => x.player == player && x.modId == ModId);
    }

    public static void SwapModifier(PlayerControl p1, PlayerControl p2)
    {
        var index = players.FindIndex(x => x.player == p1);
        if (index >= 0)
        {
            players[index].player = p2;
        }
    }
}

public enum ModifierId
{
    None = 0,

    // 新しく書く場合は一番下へ
    Opportunist,
    Sunglasses,
    Watcher,
}

[HarmonyPatch]
public static class ModifierData
{
    public static Dictionary<ModifierId, Type> allModTypes = new Dictionary<ModifierId, Type>
    {
        { ModifierId.Opportunist, typeof(ModifierBase<Opportunist>) },
        { ModifierId.Watcher, typeof(ModifierBase<Watcher>) },
        { ModifierId.Sunglasses, typeof(ModifierBase<Sunglasses>) },
    };
}