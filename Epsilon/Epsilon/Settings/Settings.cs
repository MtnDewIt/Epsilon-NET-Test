using Epsilon.Components.TagTree;

namespace Epsilon
{
    public static class Settings
    {
        public const string CollectionKey = "General";

        public static SettingDefinition DefaultTagCache = new SettingDefinition(CollectionKey, "DefaultTagCache", string.Empty);
        public static SettingDefinition DefaultPak = new SettingDefinition(CollectionKey, "DefaultModPackage", string.Empty);
        public static SettingDefinition DefaultPakCache = new SettingDefinition(CollectionKey, "DefaultModPackageCache", string.Empty);
		public static SettingDefinition StartupPositionLeft = new SettingDefinition(CollectionKey, "StartupPositionLeft", "0");
        public static SettingDefinition StartupPositionTop = new SettingDefinition(CollectionKey, "StartupPositionTop", "0");
        public static SettingDefinition StartupWidth = new SettingDefinition(CollectionKey, "StartupWidth", "0");
        public static SettingDefinition StartupHeight = new SettingDefinition(CollectionKey, "StartupHeight", "0");
        public static SettingDefinition AlwaysOnTop = new SettingDefinition(CollectionKey, "AlwaysOnTop", false.ToString());
        public static SettingDefinition AccentColor = new SettingDefinition(CollectionKey, "AccentColor", "#007ACC");


		//public const string CollectionKey = "TagResourceAndEpsilonSettings";
		public static SettingDefinition DisplayFieldTypesSetting = new SettingDefinition(CollectionKey, "DisplayFieldTypes", false.ToString());
		public static SettingDefinition DisplayFieldOffsetsSetting = new SettingDefinition(CollectionKey, "DisplayFieldOffsets", false.ToString());
		public static SettingDefinition CollapseBlocksSetting = new SettingDefinition(CollectionKey, "CollapseBlocks", false.ToString());

		public static void Load(Configuration config) {
			ISettingsCollection settings = SettingsService.GetCollection(CollectionKey);
			config.DisplayFieldTypes = settings.GetBool(DisplayFieldTypesSetting);
			config.DisplayFieldOffsets = settings.GetBool(DisplayFieldOffsetsSetting);
			config.CollapseBlocks = settings.GetBool(CollapseBlocksSetting);
		}

		//public const string CollectionKey = "Epsilon";

		public static SettingDefinition TagTreeViewModeSetting =        new SettingDefinition(CollectionKey, "TagTreeViewMode",            ((int)TagTreeViewMode.Groups).ToString());
		public static SettingDefinition TagTreeGroupDisplaySetting =    new SettingDefinition(CollectionKey, "TagTreeGroupDisplayMode",    ((int)TagTreeGroupDisplayMode.TagGroupName).ToString());
		public static SettingDefinition ShowTagGroupAltNamesSetting =   new SettingDefinition(CollectionKey, "ShowTagGroupAltNames",       false.ToString());
		public static SettingDefinition BaseCacheWarningsSetting =      new SettingDefinition(CollectionKey, "BaseCacheWarnings",          true.ToString());


	}
}
