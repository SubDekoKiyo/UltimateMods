using System.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using UltimateMods.Modules;
using static UltimateMods.ColorDictionary;

namespace UltimateMods.Roles
{
    class RoleInfo
    {
        public Color color;
        public virtual string Name { get { return ModTranslation.getString(nameKey); } }
        public virtual string NameColored { get { return Helpers.cs(color, Name); } }
        public virtual string IntroDescription { get { return ModTranslation.getString(nameKey + "Intro"); } }
        public virtual string ShortDescription { get { return ModTranslation.getString(nameKey + "Short"); } }
        public virtual string FullDescription { get { return ModTranslation.getString(nameKey + "Full"); } }
        public virtual string RoleOptions { get { return GameOptionsDataPatch.optionsToString(baseOption, true); } }

        public bool enabled { get { return CustomOptionsH.ActivateModRoles.getBool() && (baseOption == null || baseOption.enabled); } }
        public RoleType roleType;

        private string nameKey;
        private CustomOption baseOption;

        RoleInfo(string name, Color color, CustomOption baseOption, RoleType roleType)
        {
            this.color = color;
            this.nameKey = name;
            this.baseOption = baseOption;
            this.roleType = roleType;
        }

        public static RoleInfo jester = new("Jester", JesterPink, CustomRolesH.JesterRate, RoleType.Jester);
        public static RoleInfo sheriff = new("Sheriff", SheriffYellow, CustomRolesH.SheriffRate, RoleType.Sheriff);
        public static RoleInfo engineer = new("Engineer", EngineerBlue, CustomRolesH.EngineerRate, RoleType.Engineer);
        public static RoleInfo customImpostor = new("CustomImpostor", ImpostorRed, CustomRolesH.CustomImpostorRate, RoleType.CustomImpostor);
        public static RoleInfo underTaker = new("UnderTaker", ImpostorRed, CustomRolesH.UnderTakerRate, RoleType.UnderTaker);
        public static RoleInfo bountyHunter = new("BountyHunter", ImpostorRed, CustomRolesH.BountyHunterRate, RoleType.BountyHunter);
        public static RoleInfo madmate = new("Madmate", ImpostorRed, CustomRolesH.MadmateRate, RoleType.Madmate);
        public static RoleInfo bakery = new("Bakery", BakeryYellow, CustomRolesH.BakeryRate, RoleType.Bakery);
        public static RoleInfo teleporter = new("Teleporter", ImpostorRed, CustomRolesH.TeleporterRate, RoleType.Teleporter);
        public static RoleInfo altruist = new("Altruist", AltruistRed, CustomRolesH.AltruistRate, RoleType.Altruist);
        public static RoleInfo evilHacker = new("EvilHacker", ImpostorRed, CustomRolesH.EvilHackerRate, RoleType.EvilHacker);
        public static RoleInfo adversity = new("Adversity", ImpostorRed, CustomRolesH.AdversityRate, RoleType.Adversity);
        public static RoleInfo snitch = new("Snitch", SnitchGreen, CustomRolesH.SnitchRate, RoleType.Snitch);
        public static RoleInfo jackal = new("Jackal", JackalBlue, CustomRolesH.JackalRate, RoleType.Jackal);
        public static RoleInfo sidekick = new("Sidekick", JackalBlue, CustomRolesH.JackalRate, RoleType.Sidekick);
        public static RoleInfo seer = new("Seer", SeerGreen, CustomRolesH.SeerRate, RoleType.Seer);
        public static RoleInfo arsonist = new("Arsonist", ArsonistOrange, CustomRolesH.ArsonistRate, RoleType.Arsonist);
        public static RoleInfo lighter = new("Lighter", LighterYellow, CustomRolesH.LighterRate, RoleType.Lighter);
        public static RoleInfo impostor = new("Impostor", ImpostorRed, null, RoleType.Impostor);
        public static RoleInfo crewmate = new("Crewmate", CrewmateBlue, null, RoleType.Crewmate);

        public static List<RoleInfo> allRoleInfos = new()
        {
            impostor,
            jester,
            crewmate,
            sheriff,
            engineer,
            customImpostor,
            underTaker,
            bountyHunter,
            madmate,
            bakery,
            teleporter,
            altruist,
            evilHacker,
            adversity,
            snitch,
            jackal,
            sidekick,
            seer,
            arsonist,
            lighter,
        };

        public static string tl(string key)
        {
            return ModTranslation.getString(key);
        }

        public static List<RoleInfo> getRoleInfoForPlayer(PlayerControl p, RoleType[] excludeRoles = null, bool includeHidden = false)
        {
            List<RoleInfo> infos = new();
            if (p == null) return infos;

            // Special roles
            if (p.isRole(RoleType.Jester)) infos.Add(jester);
            if (p.isRole(RoleType.Sheriff)) infos.Add(sheriff);
            if (p.isRole(RoleType.Engineer)) infos.Add(engineer);
            if (p.isRole(RoleType.CustomImpostor)) infos.Add(customImpostor);
            if (p.isRole(RoleType.UnderTaker)) infos.Add(underTaker);
            if (p.isRole(RoleType.BountyHunter)) infos.Add(bountyHunter);
            if (p.isRole(RoleType.Madmate)) infos.Add(madmate);
            if (p.isRole(RoleType.Bakery)) infos.Add(bakery);
            if (p.isRole(RoleType.Teleporter)) infos.Add(teleporter);
            if (p.isRole(RoleType.Altruist)) infos.Add(altruist);
            if (p.isRole(RoleType.EvilHacker)) infos.Add(evilHacker);
            if (p.isRole(RoleType.Adversity)) infos.Add(adversity);
            if (p.isRole(RoleType.Snitch)) infos.Add(snitch);
            if (p.isRole(RoleType.Jackal)) infos.Add(jackal);
            if (p.isRole(RoleType.Sidekick)) infos.Add(sidekick);
            if (p.isRole(RoleType.Seer)) infos.Add(seer);
            if (p.isRole(RoleType.Arsonist)) infos.Add(arsonist);
            if (p.isRole(RoleType.Lighter)) infos.Add(lighter);

            // Default roles
            if (infos.Count == 0 && p.Data.Role.IsImpostor) infos.Add(impostor); // Just Impostor
            if (infos.Count == 0 && !p.Data.Role.IsImpostor) infos.Add(crewmate); // Just Crewmate

            if (excludeRoles != null)
                infos.RemoveAll(x => excludeRoles.Contains(x.roleType));

            return infos;
        }

        public static String GetRolesString(PlayerControl p, bool useColors, RoleType[] excludeRoles = null, bool includeHidden = false)
        {
            if (p?.Data?.Disconnected != false) return "";

            var roleInfo = getRoleInfoForPlayer(p, excludeRoles, includeHidden);
            string roleName = String.Join(" ", roleInfo.Select(x => useColors ? Helpers.cs(x.color, x.Name) : x.Name).ToArray());

            if (p.hasModifier(ModifierType.Opportunist))
            {
                string postfix = useColors ? Helpers.cs(OpportunistGreen, Opportunist.Postfix) : Opportunist.Postfix;
                roleName = roleName + postfix;
            }

            return roleName;
        }
    }
}