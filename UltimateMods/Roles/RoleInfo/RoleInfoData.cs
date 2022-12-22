namespace UltimateMods.Roles;

public class RoleInfo
{
    public Color RoleColor;
    public virtual string Name { get { return ModTranslation.getString(NameKey); } }
    public virtual string ColorName { get { return Helpers.cs(RoleColor, Name); } }
    public virtual string IntroDescription { get { return ModTranslation.getString(NameKey + "IntroDesc"); } }
    public virtual string ShortDescription { get { return ModTranslation.getString(NameKey + "ShortDesc"); } }
    public virtual string FullDescription { get { return ModTranslation.getString(NameKey + "FullDesc"); } }
    public virtual string RoleOptions { get { return GameOptionsDataPatch.optionsToString(BaseOption, true); } }

    public bool Enabled { get { return CustomOptionsH.ActivateModRoles.getBool() && (BaseOption == null || BaseOption.enabled); } }
    public RoleId RoleId;

    public string NameKey;
    public CustomOption BaseOption;

    public RoleInfo(string Name, Color RoleColor, CustomOption BaseOption, RoleId RoleId)
    {
        this.RoleColor = RoleColor;
        this.NameKey = Name;
        this.BaseOption = BaseOption;
        this.RoleId = RoleId;
    }
}