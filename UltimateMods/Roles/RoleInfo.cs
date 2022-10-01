using System.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using UltimateMods.Modules;
using static UltimateMods.Roles.CrewmateRoles;
using static UltimateMods.Roles.NeutralRoles;

namespace UltimateMods.Roles
{
    class RoleInfo
    {
        public Color color;
        public virtual string name { get { return ModTranslation.getString(nameKey); } }
        public virtual string nameColored { get { return Helpers.cs(color, name); } }
        public virtual string introDescription { get { return ModTranslation.getString(nameKey + "Intro"); } }
        public virtual string shortDescription { get { return ModTranslation.getString(nameKey + "Short"); } }
        public virtual string fullDescription { get { return ModTranslation.getString(nameKey + "Full"); } }
        public virtual string blurb { get { return ModTranslation.getString(nameKey + "Blurb"); } }
        public virtual string roleOptions
        {
            get
            {
                return GameOptionsDataPatch.optionsToString(baseOption, true);
            }
        }

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

        public static RoleInfo jester = new RoleInfo("Jester", Jester.color, CustomRolesH.JesterRate, RoleType.Jester);
        public static RoleInfo sheriff = new RoleInfo("Sheriff", Sheriff.color, CustomRolesH.SheriffRate, RoleType.Sheriff);
        public static RoleInfo impostor = new RoleInfo("Impostor", Palette.ImpostorRed, null, RoleType.Impostor);
        public static RoleInfo crewmate = new RoleInfo("Crewmate", Palette.CrewmateBlue, null, RoleType.Crewmate);

        public static List<RoleInfo> allRoleInfos = new()
        {
            impostor,
            jester,
            crewmate,
            sheriff,
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

            string roleName;
            var roleInfo = getRoleInfoForPlayer(p, excludeRoles, includeHidden);
            roleName = String.Join(" ", roleInfo.Select(x => useColors ? Helpers.cs(x.color, x.name) : x.name).ToArray());
            return roleName;
        }
    }
}