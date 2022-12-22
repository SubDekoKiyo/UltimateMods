namespace UltimateMods.Roles;

public static class ModifierManagement
{
    public static string GetModifierPostfixString(this PlayerControl player, ModifierId modId)
    {
        foreach (var mod in Modifier.allModifiers)
        {
            foreach (var t in ModifierData.allModTypes)
                if (modId == t.Key) return mod.ModifierPostfix();
        }
        return "NoData";
    }

    public static bool HasModifier(this PlayerControl player, ModifierId modId)
    {
        foreach (var t in ModifierData.allModTypes)
        {
            if (modId == t.Key)
            {
                return (bool)t.Value.GetMethod("HasModifier", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });
            }
        }
        return false;
    }

    public static void AddModifier(this PlayerControl player, ModifierId modId)
    {
        foreach (var t in ModifierData.allModTypes)
        {
            if (modId == t.Key)
            {
                t.Value.GetMethod("AddModifier", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });
                return;
            }
        }
    }

    public static void EraseAllModifiers(this PlayerControl player)
    {
        foreach (var t in ModifierData.allModTypes)
        {
            t.Value.GetMethod("EraseModifier", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });
        }
    }
}