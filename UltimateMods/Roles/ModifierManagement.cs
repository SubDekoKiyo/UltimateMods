namespace UltimateMods.Roles;

public static class ModifierManagement
{
    public static string GetModifierString(this PlayerControl player, ModifierId modId)
    {
        foreach (var mod in Modifier.allModifiers)
        {
            foreach (var t in ModifierData.allModTypes)
                if (modId == t.Key) return mod.ModifierName();
        }
        return "NoData";
    }

    public static string GetTranslatedModifierString(this PlayerControl player, ModifierId modId)
    {
        return ModTranslation.getString(player.GetModifierString(modId));
    }

    public static Color GetModifierColor(this PlayerControl player, ModifierId modId)
    {
        foreach (var mod in Modifier.allModifiers)
        {
            foreach (var t in ModifierData.allModTypes)
                if (modId == t.Key) return mod.ModifierColor();
        }
        return CrewmateBlue;
    }

    public static ModifierId GetModifierId(this PlayerControl player)
    {
        foreach (var t in ModifierData.allModTypes)
            if (player.HasModifier(t.Key)) return t.Key;
        return ModifierId.None;
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