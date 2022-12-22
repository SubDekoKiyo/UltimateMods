namespace UltimateMods.Roles;

public class RoleInfo
{
    public Color RoleColor;
    public virtual string Name { get { return LocalizationManager.GetString(NameKey); } }
    public virtual string ColorName { get { return Helpers.cs(RoleColor, Name); } }
    public virtual string IntroDescription { get { return LocalizationManager.GetString(NameKey + "Intro"); } }
    public virtual string ShortDescription { get { return LocalizationManager.GetString(NameKey + "Short"); } }
    public virtual string FullDescription { get { return LocalizationManager.GetString(NameKey + "Full"); } }
    public virtual string RoleOptions { get { return GameOptionsDataPatch.optionsToString(BaseOption, true); } }

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