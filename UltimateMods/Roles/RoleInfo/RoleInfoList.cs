namespace UltimateMods.Roles;

public static class RoleInfoList
{
    public static List<RoleInfo> AllRoleInfos; // RoleInfoを全て格納するList

    public static RoleInfo Impostor;
    public static RoleInfo Crewmate;
    public static RoleInfo Engineer;
    public static RoleInfo Scientist;
    public static RoleInfo ShapeShifter;

    public static RoleInfo Sheriff;
    public static RoleInfo ProEngineer;
    public static RoleInfo Madmate;
    public static RoleInfo Bakery;
    public static RoleInfo Snitch;
    public static RoleInfo Seer;
    public static RoleInfo Lighter;
    public static RoleInfo Altruist;
    public static RoleInfo Mayor;
    public static RoleInfo CustomImpostor;
    public static RoleInfo UnderTaker;
    public static RoleInfo BountyHunter;
    public static RoleInfo Teleporter;
    public static RoleInfo EvilHacker;
    public static RoleInfo Adversity;
    public static RoleInfo Jester;
    public static RoleInfo Jackal;
    public static RoleInfo Sidekick;
    public static RoleInfo Arsonist;

    public static void Load()
    {
        AllRoleInfos = new()
        {
            Impostor,
            Crewmate,
            Engineer,
            Scientist,
            ShapeShifter,
            Sheriff,
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
        };

        Impostor = new("Impostor", ImpostorRed, CustomRolesH.AdversityRate, RoleId.Impostor);
        Crewmate = new("Crewmate", CrewmateBlue, CustomRolesH.AdversityRate, RoleId.Crewmate);
        Engineer = new("Engineer", EngineerOrange, CustomRolesH.AdversityRate, RoleId.Engineer);
        Scientist = new("Scientist", ScientistBlue, CustomRolesH.AdversityRate, RoleId.Scientist);
        ShapeShifter = new("ShapeShifter", ImpostorRed, CustomRolesH.AdversityRate, RoleId.ShapeShifter);

        Sheriff = new("Sheriff", SheriffYellow, CustomRolesH.SheriffRate, RoleId.Sheriff);
        ProEngineer = new("ProEngineer", EngineerBlue, CustomRolesH.EngineerRate, RoleId.ProEngineer);
        Madmate = new("Madmate", ImpostorRed, CustomRolesH.MadmateRate, RoleId.Madmate);
        Bakery = new("Bakery", BakeryYellow, CustomRolesH.BakeryRate, RoleId.Bakery);
        Snitch = new("Snitch", SnitchGreen, CustomRolesH.SnitchRate, RoleId.Snitch);
        Seer = new("Seer", SeerGreen, CustomRolesH.SeerRate, RoleId.Seer);
        Lighter = new("Lighter", LighterYellow, CustomRolesH.LighterRate, RoleId.Lighter);
        Altruist = new("Altruist", AltruistRed, CustomRolesH.AltruistRate, RoleId.Altruist);
        Mayor = new("Mayor", MayorGreen, CustomRolesH.MayorRate, RoleId.Mayor);
        CustomImpostor = new("CustomImpostor", ImpostorRed, CustomRolesH.CustomImpostorRate, RoleId.CustomImpostor);
        UnderTaker = new("UnderTaker", ImpostorRed, CustomRolesH.UnderTakerRate, RoleId.UnderTaker);
        BountyHunter = new("BountyHunter", ImpostorRed, CustomRolesH.BountyHunterRate, RoleId.BountyHunter);
        Teleporter = new("Teleporter", ImpostorRed, CustomRolesH.TeleporterRate, RoleId.Teleporter);
        EvilHacker = new("EvilHacker", ImpostorRed, CustomRolesH.EvilHackerRate, RoleId.EvilHacker);
        Adversity = new("Adversity", ImpostorRed, CustomRolesH.AdversityRate, RoleId.Adversity);
        Jester = new("Jester", JesterPink, CustomRolesH.JesterRate, RoleId.Jester);
        Jackal = new("Jackal", JackalBlue, CustomRolesH.JackalRate, RoleId.Jackal);
        Sidekick = new("Sidekick", JackalBlue, CustomRolesH.JackalRate, RoleId.Sidekick);
        Arsonist = new("Arsonist", ArsonistOrange, CustomRolesH.ArsonistRate, RoleId.Arsonist);
    }

    public static List<RoleInfo> GetRoleInfoForPlayer(PlayerControl p, RoleId[] excludeRoles = null, bool includeHidden = false)
    {
        List<RoleInfo> infos = new();
        if (p == null) return infos;

        if (p.IsRole(RoleId.Engineer)) infos.Add(Engineer);
        if (p.IsRole(RoleId.Scientist)) infos.Add(Scientist);
        if (p.IsRole(RoleId.ShapeShifter)) infos.Add(ShapeShifter);
        if (p.IsRole(RoleId.Sheriff)) infos.Add(Sheriff);
        if (p.IsRole(RoleId.ProEngineer)) infos.Add(ProEngineer);
        if (p.IsRole(RoleId.Madmate)) infos.Add(Madmate);
        if (p.IsRole(RoleId.Bakery)) infos.Add(Bakery);
        if (p.IsRole(RoleId.Snitch)) infos.Add(Snitch);
        if (p.IsRole(RoleId.Seer)) infos.Add(Seer);
        if (p.IsRole(RoleId.Lighter)) infos.Add(Lighter);
        if (p.IsRole(RoleId.Altruist)) infos.Add(Altruist);
        if (p.IsRole(RoleId.Mayor)) infos.Add(Mayor);
        if (p.IsRole(RoleId.CustomImpostor)) infos.Add(CustomImpostor);
        if (p.IsRole(RoleId.UnderTaker)) infos.Add(UnderTaker);
        if (p.IsRole(RoleId.BountyHunter)) infos.Add(BountyHunter);
        if (p.IsRole(RoleId.Teleporter)) infos.Add(Teleporter);
        if (p.IsRole(RoleId.EvilHacker)) infos.Add(EvilHacker);
        if (p.IsRole(RoleId.Adversity)) infos.Add(Adversity);
        if (p.IsRole(RoleId.Jester)) infos.Add(Jester);
        if (p.IsRole(RoleId.Jackal)) infos.Add(Jackal);
        if (p.IsRole(RoleId.Sidekick)) infos.Add(Sidekick);
        if (p.IsRole(RoleId.Arsonist)) infos.Add(Arsonist);

        if (infos.Count == 0 && p.Data.Role != null && p.Data.Role.IsImpostor) infos.Add(Impostor); // Just Impostor
        if (infos.Count == 0 && p.Data.Role != null && !p.Data.Role.IsImpostor) infos.Add(Crewmate); // Just Crewmate

        if (excludeRoles != null)
            infos.RemoveAll(x => excludeRoles.Contains(x.RoleId));

        return infos;
    }

    public static String GetRolesString(PlayerControl p, bool useColors, RoleId[] excludeRoles = null, bool includeHidden = false)
    {
        if (p?.Data?.Disconnected != false) return "";

        var roleInfo = GetRoleInfoForPlayer(p, excludeRoles, includeHidden);
        string roleName = String.Join(" ", roleInfo.Select(x => Helpers.cs(x.RoleColor, x.Name)).ToArray());

        if (p.HasModifier(ModifierId.Opportunist))
        {
            string postfix = Helpers.cs(OpportunistGreen, p.GetModifierPostfixString(ModifierId.Opportunist));
            roleName = roleName + postfix;
        }
        if (p.HasModifier(ModifierId.Watcher))
        {
            string postfix = Helpers.cs(WatcherPurple, p.GetModifierPostfixString(ModifierId.Watcher));
            roleName = roleName + postfix;
        }
        if (p.HasModifier(ModifierId.Sunglasses))
        {
            string postfix = Helpers.cs(SunglassesGray, p.GetModifierPostfixString(ModifierId.Watcher));
            roleName = roleName + postfix;
        }

        return roleName;
    }
}