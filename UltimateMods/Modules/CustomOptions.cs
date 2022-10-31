using System.Collections.Generic;
using UnityEngine;
using BepInEx.Configuration;
using System;
using System.Linq;
using HarmonyLib;
using Hazel;
using System.Reflection;
using System.Text;
using UltimateMods.Utilities;
using UltimateMods.Roles;
using static UltimateMods.Modules.Assets;

namespace UltimateMods.Modules
{
    public class CustomOption
    {
        public enum CustomOptionType
        {
            General,
            Impostor,
            Neutral,
            Crewmate,
            Modifier,
            Other
        }

        public static List<CustomOption> options = new();
        public static int Preset = 0;

        public int id;
        public string name;
        public string format;
        public System.Object[] selections;

        public int defaultSelection;
        public ConfigEntry<int> entry;
        public int selection;
        public OptionBehaviour optionBehaviour;
        public CustomOption parent;
        public List<CustomOption> children;
        public bool isHeader;
        public bool isHidden;
        public CustomOptionType type;
        public Color Color;

        public virtual bool enabled
        {
            get
            {
                return CustomOptionsH.ActivateModRoles.getBool() && this.getBool();
            }
        }

        // Option creation
        public CustomOption() { }

        public CustomOption(int id, CustomOptionType type, Color Color, string name, System.Object[] selections, System.Object defaultValue, CustomOption parent, bool isHeader, bool isHidden, string format)
        {
            Init(id, type, Color, name, selections, defaultValue, parent, isHeader, isHidden, format);
        }

        public void Init(int id, CustomOptionType type, Color Color, string name, System.Object[] selections, System.Object defaultValue, CustomOption parent, bool isHeader, bool isHidden, string format)
        {
            this.id = id;
            this.name = name;
            this.format = format;
            this.selections = selections;
            int index = Array.IndexOf(selections, defaultValue);
            this.defaultSelection = index >= 0 ? index : 0;
            this.parent = parent;
            this.isHeader = isHeader;
            this.isHidden = isHidden;
            this.type = type;
            this.Color = Color;

            this.children = new List<CustomOption>();
            if (parent != null)
            {
                parent.children.Add(this);
            }

            selection = 0;
            if (id > 0)
            {
                entry = UltimateModsPlugin.Instance.Config.Bind($"Preset{Preset}", id.ToString(), defaultSelection);
                selection = Mathf.Clamp(entry.Value, 0, selections.Length - 1);

                if (options.Any(x => x.id == id))
                {
                    UltimateModsPlugin.Instance.Log.LogWarning($"CustomOption id {id} is used in multiple places.");
                }
            }
            options.Add(this);
        }

        public static CustomOption Create(int id, CustomOptionType type, Color Color, string name, string[] selections, CustomOption parent = null, bool isHeader = false, bool isHidden = false, string format = "")
        {
            return new CustomOption(id, type, Color, name, selections, "", parent, isHeader, isHidden, format);
        }

        public static CustomOption Create(int id, CustomOptionType type, Color Color, string name, float defaultValue, float min, float max, float step, CustomOption parent = null, bool isHeader = false, bool isHidden = false, string format = "")
        {
            List<object> selections = new();
            for (float s = min; s <= max; s += step)
                selections.Add(s);
            return new CustomOption(id, type, Color, name, selections.ToArray(), defaultValue, parent, isHeader, isHidden, format);
        }

        public static CustomOption Create(int id, CustomOptionType type, Color Color, string name, bool defaultValue, CustomOption parent = null, bool isHeader = false, bool isHidden = false, string format = "")
        {
            return new CustomOption(id, type, Color, name, new string[] { "OptionOff", "OptionOn" }, defaultValue ? "OptionOn" : "OptionOff", parent, isHeader, isHidden, format);
        }

        // Static behaviour

        public static void switchPreset(int newPreset)
        {
            CustomOption.Preset = newPreset;
            foreach (CustomOption option in CustomOption.options)
            {
                if (option.id <= 0) continue;

                option.entry = UltimateModsPlugin.Instance.Config.Bind($"Preset{Preset}", option.id.ToString(), option.defaultSelection);
                option.selection = Mathf.Clamp(option.entry.Value, 0, option.selections.Length - 1);
                if (option.optionBehaviour != null && option.optionBehaviour is StringOption stringOption)
                {
                    stringOption.oldValue = stringOption.Value = option.selection;
                    stringOption.ValueText.text = option.getString();
                }
            }
        }

        /*public static void ShareOptionSelections()
        {
            if (PlayerControl.AllPlayerControls.Count <= 1 || AmongUsClient.Instance?.AmHost == false && PlayerControl.LocalPlayer == null) return;

            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareOptions, Hazel.SendOption.Reliable);
            messageWriter.WritePacked((uint)CustomOption.options.Count);
            foreach (CustomOption option in CustomOption.options)
            {
                messageWriter.WritePacked((uint)option.id);
                messageWriter.WritePacked((uint)Convert.ToUInt32(option.selection));
            }
            messageWriter.EndMessage();
        }*/

        public static void ShareOptionSelections()
        {
            if (PlayerControl.AllPlayerControls.Count <= 1 || AmongUsClient.Instance!.AmHost == false && PlayerControl.LocalPlayer == null) return;

            var optionsList = new List<CustomOption>(CustomOption.options);
            while (optionsList.Any())
            {
                byte amount = (byte)Math.Min(optionsList.Count, 20);
                var writer = AmongUsClient.Instance!.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareOptions, SendOption.Reliable, -1);
                writer.Write(amount);
                for (int i = 0; i < amount; i++)
                {
                    var option = optionsList[0];
                    optionsList.RemoveAt(0);
                    writer.WritePacked((uint)option.id);
                    writer.WritePacked(Convert.ToUInt32(option.selection));
                }
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
        }

        // Getter

        public virtual int getSelection()
        {
            return selection;
        }

        public virtual bool getBool()
        {
            return selection > 0;
        }

        public virtual float getFloat()
        {
            return (float)selections[selection];
        }

        public int getQuantity()
        {
            return selection + 1;
        }

        public virtual string getString()
        {
            string sel = selections[selection].ToString();
            if (format != "")
            {
                return string.Format(ModTranslation.getString(format), sel);
            }
            return ModTranslation.getString(sel);
        }

        public virtual string getName()
        {
            return ModTranslation.getString(name);
        }

        public virtual Color getColor()
        {
            return Color;
        }

        // Option changes
        public virtual void updateSelection(int newSelection)
        {
            selection = Mathf.Clamp((newSelection + selections.Length) % selections.Length, 0, selections.Length - 1);
            if (optionBehaviour != null && optionBehaviour is StringOption stringOption)
            {
                stringOption.oldValue = stringOption.Value = selection;
                stringOption.ValueText.text = getString();

                if (AmongUsClient.Instance?.AmHost == true && PlayerControl.LocalPlayer)
                {
                    if (id == 0) switchPreset(selection); // Switch Presets
                    else if (entry != null) entry.Value = selection; // Save selection to config

                    ShareOptionSelections();// Share all selections
                }
            }
        }
    }

    public class CustomRoleOption : CustomOption
    {
        public CustomOption countOption = null;
        public bool roleEnabled = true;

        public override bool enabled
        {
            get
            {
                return CustomOptionsH.ActivateModRoles.getBool() && roleEnabled && selection > 0;
            }
        }

        public int rate
        {
            get
            {
                return enabled ? selection : 0;
            }
        }

        public int count
        {
            get
            {
                if (!enabled)
                    return 0;

                if (countOption != null)
                    return Mathf.RoundToInt(countOption.getFloat());

                return 1;
            }
        }

        public (int, int) data
        {
            get
            {
                return (rate, count);
            }
        }

        public CustomRoleOption(int id, CustomOptionType type, Color Color, string name, Color color, int max = 15, bool roleEnabled = true) :
            base(id, type, Color, Helpers.cs(color, name), CustomOptionsH.TenRates, "", null, true, false, "")
        {
            this.roleEnabled = roleEnabled;

            if (max <= 0 || !roleEnabled)
            {
                isHidden = true;
                this.roleEnabled = false;
            }

            if (max > 1)
                countOption = Create(id + 10000, type, Color, "RoleNumAssigned", 1f, 1f, 15f, 1f, this, false, isHidden, "FormatPlayer");
        }
    }

    public class CustomDualRoleOption : CustomRoleOption
    {
        public static List<CustomDualRoleOption> dualRoles = new();
        public CustomOption roleImpChance = null;
        public CustomOption roleAssignEqually = null;
        public RoleType roleType;

        public int impChance { get { return roleImpChance.getSelection(); } }

        public bool assignEqually { get { return roleAssignEqually.getSelection() == 0; } }

        public CustomDualRoleOption(int id, CustomOptionType type, Color Color, string name, Color color, RoleType roleType, int max = 15, bool roleEnabled = true) : base(id, type, Color, name, color, max, roleEnabled)
        {
            roleAssignEqually = new CustomOption(id + 15001, type, Color, "roleAssignEqually", new string[] { "OptionOn", "OptionOff" }, "OptionOff", this, false, isHidden, "");
            roleImpChance = Create(id + 15000, type, Color, "roleImpChance", CustomOptionsH.TenRates, roleAssignEqually, false, isHidden);

            this.roleType = roleType;
            dualRoles.Add(this);
        }
    }

    public class CustomTasksOption : CustomOption
    {
        public CustomOption commonTasksOption = null;
        public CustomOption longTasksOption = null;
        public CustomOption shortTasksOption = null;

        public int CommonTasks { get { return Mathf.RoundToInt(commonTasksOption.getSelection()); } }
        public int LongTasks { get { return Mathf.RoundToInt(longTasksOption.getSelection()); } }
        public int ShortTasks { get { return Mathf.RoundToInt(shortTasksOption.getSelection()); } }

        public List<byte> generateTasks()
        {
            return Helpers.GenerateTasks(CommonTasks, ShortTasks, LongTasks);
        }

        public CustomTasksOption(int id, CustomOptionType type, Color Color, int commonDef, int longDef, int shortDef, CustomOption parent = null)
        {
            commonTasksOption = Create(id + 20000, type, Color, "NumCommonTasks", commonDef, 0f, 4f, 1f, parent);
            longTasksOption = Create(id + 20001, type, Color, "NumLongTasks", longDef, 0f, 15f, 1f, parent);
            shortTasksOption = Create(id + 20002, type, Color, "NumShortTasks", shortDef, 0f, 23f, 1f, parent);
        }
    }

    public class CustomRoleSelectionOption : CustomOption
    {
        public List<RoleType> roleTypes;

        public RoleType role
        {
            get
            {
                return roleTypes[selection];
            }
        }

        public CustomRoleSelectionOption(int id, CustomOptionType type, Color Color, string name, List<RoleType> roleTypes = null, CustomOption parent = null)
        {
            if (roleTypes == null)
            {
                roleTypes = Enum.GetValues(typeof(RoleType)).Cast<RoleType>().ToList();
            }

            this.roleTypes = roleTypes;
            var strings = new string[] { "OptionOff" };

            Init(id, type, Color, name, strings, 0, parent, false, false, "");
        }

        public override void updateSelection(int newSelection)
        {
            if (roleTypes.Count > 0)
            {
                selections = roleTypes.Select(
                    x =>
                        x == RoleType.NoRole ? "OptionOff" :
                        RoleInfo.allRoleInfos.First(y => y.roleType == x).NameColored
                    ).ToArray();
            }

            base.updateSelection(newSelection);
        }
    }

    public class CustomOptionBlank : CustomOption
    {
        public CustomOptionBlank(CustomOption parent)
        {
            this.parent = parent;
            this.id = -1;
            this.name = "";
            this.isHeader = false;
            this.isHidden = true;
            this.children = new List<CustomOption>();
            this.selections = new string[] { "" };
            options.Add(this);
        }

        public override int getSelection()
        {
            return 0;
        }

        public override bool getBool()
        {
            return true;
        }

        public override float getFloat()
        {
            return 0f;
        }

        public override string getString()
        {
            return "";
        }

        public override void updateSelection(int newSelection)
        {
            return;
        }
    }

    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
    class GameOptionsMenuStartPatch
    {
        public static void Postfix(GameOptionsMenu __instance)
        {
            if (GameObject.Find("UMSettings") != null)
            { // Settings setup has already been performed, fixing the title of the tab and returning
                GameObject.Find("UMSettings").transform.FindChild("GameGroup").FindChild("Text").GetComponent<TMPro.TextMeshPro>().SetText(ModTranslation.getString("GeneralSettings"));
                return;
            }
            if (GameObject.Find("ImpostorSettings") != null)
            {
                GameObject.Find("ImpostorSettings").transform.FindChild("GameGroup").FindChild("Text").GetComponent<TMPro.TextMeshPro>().SetText(ModTranslation.getString("ImpostorSettings"));
                return;
            }
            if (GameObject.Find("NeutralSettings") != null)
            {
                GameObject.Find("NeutralSettings").transform.FindChild("GameGroup").FindChild("Text").GetComponent<TMPro.TextMeshPro>().SetText(ModTranslation.getString("NeutralSettings"));
                return;
            }
            if (GameObject.Find("CrewmateSettings") != null)
            {
                GameObject.Find("CrewmateSettings").transform.FindChild("GameGroup").FindChild("Text").GetComponent<TMPro.TextMeshPro>().SetText(ModTranslation.getString("CrewmateSettings"));
                return;
            }
            if (GameObject.Find("ModifierSettings") != null)
            {
                GameObject.Find("ModifierSettings").transform.FindChild("GameGroup").FindChild("Text").GetComponent<TMPro.TextMeshPro>().SetText(ModTranslation.getString("ModifierSettings"));
                return;
            }
            if (GameObject.Find("OtherSettings") != null)
            {
                GameObject.Find("OtherSettings").transform.FindChild("GameGroup").FindChild("Text").GetComponent<TMPro.TextMeshPro>().SetText(ModTranslation.getString("OtherSettings"));
                return;
            }

            // Setup UM tab
            var template = UnityEngine.Object.FindObjectsOfType<StringOption>().FirstOrDefault();
            if (template == null) return;
            var gameSettings = GameObject.Find("Game Settings");
            var gameSettingMenu = UnityEngine.Object.FindObjectsOfType<GameSettingMenu>().FirstOrDefault();
            var umSettings = UnityEngine.Object.Instantiate(gameSettings, gameSettings.transform.parent);
            var umMenu = umSettings.transform.FindChild("GameGroup").FindChild("SliderInner").GetComponent<GameOptionsMenu>();
            umSettings.name = "UMSettings";

            var impostorSettings = UnityEngine.Object.Instantiate(gameSettings, gameSettings.transform.parent);
            var impostorMenu = impostorSettings.transform.FindChild("GameGroup").FindChild("SliderInner").GetComponent<GameOptionsMenu>();
            impostorSettings.name = "ImpostorSettings";

            var neutralSettings = UnityEngine.Object.Instantiate(gameSettings, gameSettings.transform.parent);
            var neutralMenu = neutralSettings.transform.FindChild("GameGroup").FindChild("SliderInner").GetComponent<GameOptionsMenu>();
            neutralSettings.name = "NeutralSettings";

            var crewmateSettings = UnityEngine.Object.Instantiate(gameSettings, gameSettings.transform.parent);
            var crewmateMenu = crewmateSettings.transform.FindChild("GameGroup").FindChild("SliderInner").GetComponent<GameOptionsMenu>();
            crewmateSettings.name = "CrewmateSettings";

            var modifierSettings = UnityEngine.Object.Instantiate(gameSettings, gameSettings.transform.parent);
            var modifierMenu = modifierSettings.transform.FindChild("GameGroup").FindChild("SliderInner").GetComponent<GameOptionsMenu>();
            modifierSettings.name = "ModifierSettings";

            var otherSettings = UnityEngine.Object.Instantiate(gameSettings, gameSettings.transform.parent);
            var otherMenu = otherSettings.transform.FindChild("GameGroup").FindChild("SliderInner").GetComponent<GameOptionsMenu>();
            otherSettings.name = "OtherSettings";

            var roleTab = GameObject.Find("RoleTab");
            var gameTab = GameObject.Find("GameTab");

            var umTab = UnityEngine.Object.Instantiate(roleTab, roleTab.transform.parent);
            var umTabHighlight = umTab.transform.FindChild("Hat Button").FindChild("Tab Background").GetComponent<SpriteRenderer>();
            umTab.transform.FindChild("Hat Button").FindChild("Icon").GetComponent<SpriteRenderer>().sprite = Helpers.LoadSpriteFromTexture2D(TabSet, 100f);

            var impostorTab = UnityEngine.Object.Instantiate(roleTab, umTab.transform);
            var impostorTabHighlight = impostorTab.transform.FindChild("Hat Button").FindChild("Tab Background").GetComponent<SpriteRenderer>();
            impostorTab.transform.FindChild("Hat Button").FindChild("Icon").GetComponent<SpriteRenderer>().sprite = Helpers.LoadSpriteFromTexture2D(TabImp, 100f);
            impostorTab.name = "ImpostorTab";

            var neutralTab = UnityEngine.Object.Instantiate(roleTab, impostorTab.transform);
            var neutralTabHighlight = neutralTab.transform.FindChild("Hat Button").FindChild("Tab Background").GetComponent<SpriteRenderer>();
            neutralTab.transform.FindChild("Hat Button").FindChild("Icon").GetComponent<SpriteRenderer>().sprite = Helpers.LoadSpriteFromTexture2D(TabNeu, 100f);
            neutralTab.name = "NeutralTab";

            var crewmateTab = UnityEngine.Object.Instantiate(roleTab, neutralTab.transform);
            var crewmateTabHighlight = crewmateTab.transform.FindChild("Hat Button").FindChild("Tab Background").GetComponent<SpriteRenderer>();
            crewmateTab.transform.FindChild("Hat Button").FindChild("Icon").GetComponent<SpriteRenderer>().sprite = Helpers.LoadSpriteFromTexture2D(TabCrew, 100f);
            crewmateTab.name = "CrewmateTab";

            var modifierTab = UnityEngine.Object.Instantiate(roleTab, crewmateTab.transform);
            var modifierTabHighlight = modifierTab.transform.FindChild("Hat Button").FindChild("Tab Background").GetComponent<SpriteRenderer>();
            modifierTab.transform.FindChild("Hat Button").FindChild("Icon").GetComponent<SpriteRenderer>().sprite = Helpers.LoadSpriteFromTexture2D(TabMod, 100f);
            modifierTab.name = "ModifierTab";

            var otherTab = UnityEngine.Object.Instantiate(roleTab, modifierTab.transform);
            var otherTabHighlight = otherTab.transform.FindChild("Hat Button").FindChild("Tab Background").GetComponent<SpriteRenderer>();
            otherTab.transform.FindChild("Hat Button").FindChild("Icon").GetComponent<SpriteRenderer>().sprite = Helpers.LoadSpriteFromTexture2D(TabOth, 100f);
            otherTab.name = "OtherTab";

            // Position of Tab Icons
            gameTab.transform.position += Vector3.left * 3.2f;
            roleTab.transform.position += Vector3.left * 3.2f;
            umTab.transform.position += Vector3.left * 2.1f;
            impostorTab.transform.localPosition = Vector3.right * 0.9f;
            neutralTab.transform.localPosition = Vector3.right * 0.9f;
            crewmateTab.transform.localPosition = Vector3.right * 0.9f;
            modifierTab.transform.localPosition = Vector3.right * 0.9f;
            otherTab.transform.localPosition = Vector3.right * 0.9f;

            var tabs = new GameObject[] { gameTab, roleTab, umTab, impostorTab, neutralTab, crewmateTab, modifierTab, otherTab };
            for (int i = 0; i < tabs.Length; i++)
            {
                var button = tabs[i].GetComponentInChildren<PassiveButton>();
                if (button == null) continue;
                int copiedIndex = i;
                button.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
                {
                    gameSettingMenu.RegularGameSettings.SetActive(false);
                    gameSettingMenu.RolesSettings.gameObject.SetActive(false);
                    umSettings.gameObject.SetActive(false);
                    impostorSettings.gameObject.SetActive(false);
                    neutralSettings.gameObject.SetActive(false);
                    crewmateSettings.gameObject.SetActive(false);
                    modifierSettings.gameObject.SetActive(false);
                    otherSettings.gameObject.SetActive(false);
                    gameSettingMenu.GameSettingsHightlight.enabled = false;
                    gameSettingMenu.RolesSettingsHightlight.enabled = false;
                    umTabHighlight.enabled = false;
                    impostorTabHighlight.enabled = false;
                    neutralTabHighlight.enabled = false;
                    crewmateTabHighlight.enabled = false;
                    modifierTabHighlight.enabled = false;
                    otherTabHighlight.enabled = false;
                    if (copiedIndex == 0)
                    {
                        gameSettingMenu.RegularGameSettings.SetActive(true);
                        gameSettingMenu.GameSettingsHightlight.enabled = true;
                    }
                    else if (copiedIndex == 1)
                    {
                        gameSettingMenu.RolesSettings.gameObject.SetActive(true);
                        gameSettingMenu.RolesSettingsHightlight.enabled = true;
                    }
                    else if (copiedIndex == 2)
                    {
                        umSettings.gameObject.SetActive(true);
                        umTabHighlight.enabled = true;
                    }
                    else if (copiedIndex == 3)
                    {
                        impostorSettings.gameObject.SetActive(true);
                        impostorTabHighlight.enabled = true;
                    }
                    else if (copiedIndex == 4)
                    {
                        neutralSettings.gameObject.SetActive(true);
                        neutralTabHighlight.enabled = true;
                    }
                    else if (copiedIndex == 5)
                    {
                        crewmateSettings.gameObject.SetActive(true);
                        crewmateTabHighlight.enabled = true;
                    }
                    else if (copiedIndex == 6)
                    {
                        modifierSettings.gameObject.SetActive(true);
                        modifierTabHighlight.enabled = true;
                    }
                    else if (copiedIndex == 7)
                    {
                        otherSettings.gameObject.SetActive(true);
                        otherTabHighlight.enabled = true;
                    }
                }));
            }

            foreach (OptionBehaviour option in umMenu.GetComponentsInChildren<OptionBehaviour>())
                UnityEngine.Object.Destroy(option.gameObject);
            List<OptionBehaviour> umOptions = new();

            foreach (OptionBehaviour option in impostorMenu.GetComponentsInChildren<OptionBehaviour>())
                UnityEngine.Object.Destroy(option.gameObject);
            List<OptionBehaviour> impostorOptions = new();

            foreach (OptionBehaviour option in neutralMenu.GetComponentsInChildren<OptionBehaviour>())
                UnityEngine.Object.Destroy(option.gameObject);
            List<OptionBehaviour> neutralOptions = new();

            foreach (OptionBehaviour option in crewmateMenu.GetComponentsInChildren<OptionBehaviour>())
                UnityEngine.Object.Destroy(option.gameObject);
            List<OptionBehaviour> crewmateOptions = new();

            foreach (OptionBehaviour option in modifierMenu.GetComponentsInChildren<OptionBehaviour>())
                UnityEngine.Object.Destroy(option.gameObject);
            List<OptionBehaviour> modifierOptions = new();

            foreach (OptionBehaviour option in otherMenu.GetComponentsInChildren<OptionBehaviour>())
                UnityEngine.Object.Destroy(option.gameObject);
            List<OptionBehaviour> otherOptions = new();

            List<Transform> menus = new() { umMenu.transform, impostorMenu.transform, neutralMenu.transform, crewmateMenu.transform, modifierMenu.transform, otherMenu.transform };
            List<List<OptionBehaviour>> optionBehaviours = new() { umOptions, impostorOptions, neutralOptions, crewmateOptions, modifierOptions, otherOptions };

            for (int i = 0; i < CustomOption.options.Count; i++)
            {
                CustomOption option = CustomOption.options[i];
                if (option.optionBehaviour == null)
                {
                    StringOption stringOption = UnityEngine.Object.Instantiate(template, menus[(int)option.type]);
                    optionBehaviours[(int)option.type].Add(stringOption);
                    stringOption.OnValueChanged = new Action<OptionBehaviour>((o) => { });
                    stringOption.TitleText.text = option.name;
                    stringOption.Value = stringOption.oldValue = option.selection;
                    stringOption.ValueText.text = option.selections[option.selection].ToString();

                    option.optionBehaviour = stringOption;
                }
                option.optionBehaviour.gameObject.SetActive(true);
            }

            umMenu.Children = umOptions.ToArray();
            umSettings.gameObject.SetActive(false);

            impostorMenu.Children = impostorOptions.ToArray();
            impostorSettings.gameObject.SetActive(false);

            neutralMenu.Children = neutralOptions.ToArray();
            neutralSettings.gameObject.SetActive(false);

            crewmateMenu.Children = crewmateOptions.ToArray();
            crewmateSettings.gameObject.SetActive(false);

            modifierMenu.Children = modifierOptions.ToArray();
            modifierSettings.gameObject.SetActive(false);

            otherMenu.Children = otherOptions.ToArray();
            otherSettings.gameObject.SetActive(false);

            /*var numImpostorsOption = __instance.Children.FirstOrDefault(x => x.name == "NumImpostors").TryCast<NumberOption>();
            if (numImpostorsOption != null) numImpostorsOption.ValidRange = new FloatRange(0f, 15f);*/

            var killCoolOption = __instance.Children.FirstOrDefault(x => x.name == "KillCooldown").TryCast<NumberOption>();
            if (killCoolOption != null) killCoolOption.ValidRange = new FloatRange(2.5f, 60f);

            var commonTasksOption = __instance.Children.FirstOrDefault(x => x.name == "NumCommonTasks").TryCast<NumberOption>();
            if (commonTasksOption != null) commonTasksOption.ValidRange = new FloatRange(0f, 4f);

            var shortTasksOption = __instance.Children.FirstOrDefault(x => x.name == "NumShortTasks").TryCast<NumberOption>();
            if (shortTasksOption != null) shortTasksOption.ValidRange = new FloatRange(0f, 23f);

            var longTasksOption = __instance.Children.FirstOrDefault(x => x.name == "NumLongTasks").TryCast<NumberOption>();
            if (longTasksOption != null) longTasksOption.ValidRange = new FloatRange(0f, 15f);
        }
    }

    [HarmonyPatch(typeof(KeyValueOption), nameof(KeyValueOption.OnEnable))]
    public class KeyValueOptionEnablePatch
    {
        public static void Postfix(KeyValueOption __instance)
        {
            GameOptionsData gameOptions = PlayerControl.GameOptions;
            if (__instance.Title == StringNames.GameMapName)
            {
                __instance.Selected = gameOptions.MapId;
            }
            try
            {
                __instance.ValueText.text = __instance.Values[Mathf.Clamp(__instance.Selected, 0, __instance.Values.Count - 1)].Key;
            }
            catch { }
        }
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.OnEnable))]
    public class StringOptionEnablePatch
    {
        public static bool Prefix(StringOption __instance)
        {
            CustomOption option = CustomOption.options.FirstOrDefault(option => option.optionBehaviour == __instance);

            if (option == null) return true;

            SpriteRenderer Background = (option.optionBehaviour).transform.Find("Background").GetComponent<SpriteRenderer>();
            __instance.OnValueChanged = new Action<OptionBehaviour>((o) => { });
            __instance.TitleText.text = option.getName();
            __instance.Value = __instance.oldValue = option.selection;
            __instance.ValueText.text = option.getString();
            Background.color = option.Color;
            return false;
        }
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Increase))]
    public class StringOptionIncreasePatch
    {
        public static bool Prefix(StringOption __instance)
        {
            CustomOption option = CustomOption.options.FirstOrDefault(option => option.optionBehaviour == __instance);
            if (option == null) return true;
            option.updateSelection(option.selection + 1);
            return false;
        }
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Decrease))]
    public class StringOptionDecreasePatch
    {
        public static bool Prefix(StringOption __instance)
        {
            CustomOption option = CustomOption.options.FirstOrDefault(option => option.optionBehaviour == __instance);
            if (option == null) return true;
            option.updateSelection(option.selection - 1);
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
    public class RpcSyncSettingsPatch
    {
        public static void Postfix()
        {
            CustomOption.ShareOptionSelections();
        }
    }


    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
    class GameOptionsMenuUpdatePatch
    {
        private static float timer = 1f;
        public static void Postfix(GameOptionsMenu __instance)
        {
            foreach (var ob in __instance.Children)
            {
                switch (ob.Title)
                {
                    case StringNames.GameRecommendedSettings:
                        ob.enabled = false;
                        ob.gameObject.SetActive(false);
                        break;
                    default:
                        break;
                }
            }

            // Return Menu Update if in normal among us settings
            var gameSettingMenu = UnityEngine.Object.FindObjectsOfType<GameSettingMenu>().FirstOrDefault();
            if (gameSettingMenu.RegularGameSettings.active || gameSettingMenu.RolesSettings.gameObject.active) return;

            timer += Time.deltaTime;
            if (timer < 0.1f) return;
            timer = 0f;

            float numItems = __instance.Children.Length;

            float offset = 2.75f;
            foreach (CustomOption option in CustomOption.options)
            {
                if (GameObject.Find("UMSettings") && option.type != CustomOption.CustomOptionType.General)
                    continue;
                if (GameObject.Find("ImpostorSettings") && option.type != CustomOption.CustomOptionType.Impostor)
                    continue;
                if (GameObject.Find("NeutralSettings") && option.type != CustomOption.CustomOptionType.Neutral)
                    continue;
                if (GameObject.Find("CrewmateSettings") && option.type != CustomOption.CustomOptionType.Crewmate)
                    continue;
                if (GameObject.Find("ModifierSettings") && option.type != CustomOption.CustomOptionType.Modifier)
                    continue;
                if (GameObject.Find("OtherSettings") && option.type != CustomOption.CustomOptionType.Other)
                    continue;
                if (option?.optionBehaviour != null && option.optionBehaviour.gameObject != null)
                {
                    bool enabled = true;
                    var parent = option.parent;

                    if (option.isHidden)
                    {
                        enabled = false;
                    }

                    while (parent != null && enabled)
                    {
                        enabled = parent.enabled;
                        parent = parent.parent;
                    }

                    option.optionBehaviour.gameObject.SetActive(enabled);
                    if (enabled)
                    {
                        offset -= option.isHeader ? 0.75f : 0.5f;
                        option.optionBehaviour.transform.localPosition = new Vector3(option.optionBehaviour.transform.localPosition.x, offset, option.optionBehaviour.transform.localPosition.z);

                        if (option.isHeader)
                        {
                            numItems += 0.5f;
                        }
                    }
                    else
                    {
                        numItems--;
                    }
                }
            }
            __instance.GetComponentInParent<Scroller>().ContentYBounds.max = -4.0f + numItems * 0.5f;
        }
    }

    [HarmonyPatch]
    class GameOptionsDataPatch
    {
        public static int MaxPage;
        public static string tl(string key)
        {
            return ModTranslation.getString(key);
        }

        private static IEnumerable<MethodBase> TargetMethods()
        {
            return typeof(GameOptionsData).GetMethods().Where(x => x.ReturnType == typeof(string) && x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == typeof(int));
        }

        public static string optionToString(CustomOption option)
        {
            if (option == null) return "";
            return $"{option.getName()}: {option.getString()}";
        }

        public static string optionsToString(CustomOption option, bool skipFirst = false)
        {
            if (option == null)
            {
                Helpers.Log("no option?");
                return "";
            }

            List<string> options = new();
            if (!option.isHidden && !skipFirst) options.Add(optionToString(option));
            if (option.enabled)
            {
                foreach (CustomOption op in option.children)
                {
                    string str = optionsToString(op);
                    if (str != "") options.Add(str);
                }
            }
            return string.Join("\n", options);
        }

        private static void Postfix(ref string __result)
        {
            List<string> pages = new();
            pages.Add(__result);

            StringBuilder entry = new StringBuilder();
            List<string> entries = new();

            // First add the Presets and the role counts
            entries.Add(optionToString(CustomOptionsH.PresetSelection));
            entries.Add(optionToString(CustomOptionsH.ActivateModRoles));
            entries.Add(optionToString(CustomOptionsH.RandomGen));
            entries.Add(optionToString(CustomOptionsH.EnableMirrorMap));
            entries.Add(optionToString(CustomOptionsH.CanZoomInOutWhenPlayerIsDead));

            // entries.Add(optionToString(CustomOptionsH.RememberClassic));

            var optionName = CustomOptionsH.cs(new Color(204f / 255f, 204f / 255f, 0, 1f), tl("CrewmateRoles"));
            var min = CustomOptionsH.CrewmateRolesCountMin.getSelection();
            var max = CustomOptionsH.CrewmateRolesCountMax.getSelection();
            if (min > max) min = max;
            var optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
            entry.AppendLine($"{optionName}: {optionValue}");

            optionName = CustomOptionsH.cs(new Color(204f / 255f, 204f / 255f, 0, 1f), tl("NeutralRoles"));
            min = CustomOptionsH.NeutralRolesCountMin.getSelection();
            max = CustomOptionsH.NeutralRolesCountMax.getSelection();
            if (min > max) min = max;
            optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
            entry.AppendLine($"{optionName}: {optionValue}");

            optionName = CustomOptionsH.cs(new Color(204f / 255f, 204f / 255f, 0, 1f), tl("ImpostorRoles"));
            min = CustomOptionsH.ImpostorRolesCountMin.getSelection();
            max = CustomOptionsH.ImpostorRolesCountMax.getSelection();
            if (min > max) min = max;
            optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
            entry.AppendLine($"{optionName}: {optionValue}");

            optionName = CustomOptionsH.cs(new Color(204f / 255f, 204f / 255f, 0, 1f), tl("ModifierRoles"));
            min = CustomOptionsH.ModifierCountMin.getSelection();
            max = CustomOptionsH.ModifierCountMax.getSelection();
            if (min > max) min = max;
            optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
            entry.AppendLine($"{optionName}: {optionValue}");

            entries.Add(entry.ToString().Trim('\r', '\n'));

            void addChildren(CustomOption option, ref StringBuilder entry, bool indent = true)
            {
                if (!option.enabled) return;

                foreach (var child in option.children)
                {
                    if (!child.isHidden)
                        entry.AppendLine((indent ? "    " : "") + optionToString(child));
                    addChildren(child, ref entry, indent);
                }
            }

            foreach (CustomOption option in CustomOption.options)
            {
                if ((option == CustomOptionsH.PresetSelection) ||
                    (option == CustomOptionsH.ActivateModRoles) ||
                    (option == CustomOptionsH.RandomGen) ||
                    (option == CustomOptionsH.EnableMirrorMap) ||
                    (option == CustomOptionsH.CanZoomInOutWhenPlayerIsDead) ||
                    // (option == CustomOptionsH.RememberClassic) ||
                    (option == CustomOptionsH.CrewmateRolesCountMin) ||
                    (option == CustomOptionsH.CrewmateRolesCountMax) ||
                    (option == CustomOptionsH.NeutralRolesCountMin) ||
                    (option == CustomOptionsH.NeutralRolesCountMax) ||
                    (option == CustomOptionsH.ImpostorRolesCountMin) ||
                    (option == CustomOptionsH.ImpostorRolesCountMax) ||
                    (option == CustomOptionsH.ModifierCountMin) ||
                    (option == CustomOptionsH.ModifierCountMax))
                {
                    continue;
                }

                if (option.parent == null)
                {
                    if (!option.enabled)
                    {
                        continue;
                    }

                    entry = new StringBuilder();
                    if (!option.isHidden)
                        entry.AppendLine(optionToString(option));

                    addChildren(option, ref entry, !option.isHidden);
                    entries.Add(entry.ToString().Trim('\r', '\n'));
                }
            }

            int maxLines = 28;
            int lineCount = 0;
            string page = "";
            foreach (var e in entries)
            {
                int lines = e.Count(c => c == '\n') + 1;

                if (lineCount + lines > maxLines)
                {
                    pages.Add(page);
                    page = "";
                    lineCount = 0;
                }

                page = page + e + "\n\n";
                lineCount += lines + 1;
            }

            page = page.Trim('\r', '\n');
            if (page != "")
            {
                pages.Add(page);
            }

            int numPages = pages.Count;
            MaxPage = numPages;
            int counter = UltimateModsPlugin.OptionsPage = UltimateModsPlugin.OptionsPage % numPages;

            __result = pages[counter].Trim('\r', '\n') + "\n\n" + tl("ChangePage") + $" ({counter + 1}/{numPages})";
        }
    }

    [HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.GetAdjustedNumImpostors))]
    public static class GameOptionsGetAdjustedNumImpostorsPatch
    {
        public static bool Prefix(GameOptionsData __instance, ref int __result)
        {
            __result = PlayerControl.GameOptions.NumImpostors;
            return false;
        }
    }

    [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
    public static class GameOptionsNextPagePatch
    {
        public static void Postfix(KeyboardJoystick __instance)
        {
            int page = UltimateModsPlugin.OptionsPage;
            if (Input.GetKeyDown(KeyCode.Period))
            {
                UltimateModsPlugin.OptionsPage = (UltimateModsPlugin.OptionsPage + 1) % 7;
            }
            if (Input.GetKeyDown(KeyCode.Comma))
            {
                if (UltimateModsPlugin.OptionsPage > 0)
                    UltimateModsPlugin.OptionsPage -= 1;
                else if (UltimateModsPlugin.OptionsPage == 0)
                    UltimateModsPlugin.OptionsPage = GameOptionsDataPatch.MaxPage - 1;
            }
            if (page != UltimateModsPlugin.OptionsPage)
            {
                Vector3 position = (Vector3)FastDestroyableSingleton<HudManager>.Instance?.GameSettings?.transform.localPosition;
                FastDestroyableSingleton<HudManager>.Instance.GameSettings.transform.localPosition = new Vector3(position.x, 2.9f, position.z);
            }
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class GameSettingsScalePatch
    {
        public static void Prefix(HudManager __instance)
        {
            if (__instance.GameSettings != null) __instance.GameSettings.fontSize = 1.2f;
        }
    }
}