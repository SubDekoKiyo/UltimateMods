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
        public virtual string Blurb { get { return ModTranslation.getString(nameKey + "Blurb"); } }
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

        public static RoleInfo jester = new RoleInfo("Jester", JesterPink, CustomRolesH.JesterRate, RoleType.Jester);
        public static RoleInfo sheriff = new RoleInfo("Sheriff", SheriffYellow, CustomRolesH.SheriffRate, RoleType.Sheriff);
        public static RoleInfo engineer = new RoleInfo("Engineer", EngineerBlue, CustomRolesH.EngineerRate, RoleType.Engineer);
        public static RoleInfo customImpostor = new RoleInfo("CustomImpostor", ImpostorRed, CustomRolesH.CustomImpostorRate, RoleType.CustomImpostor);
        public static RoleInfo underTaker = new RoleInfo("UnderTaker", ImpostorRed, CustomRolesH.UnderTakerRate, RoleType.UnderTaker);
        public static RoleInfo impostor = new RoleInfo("Impostor", ImpostorRed, null, RoleType.Impostor);
        public static RoleInfo crewmate = new RoleInfo("Crewmate", CrewmateBlue, null, RoleType.Crewmate);

        public static List<RoleInfo> allRoleInfos = new()
        {
            impostor,
            jester,
            crewmate,
            sheriff,
            engineer,
            customImpostor,
            underTaker,
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