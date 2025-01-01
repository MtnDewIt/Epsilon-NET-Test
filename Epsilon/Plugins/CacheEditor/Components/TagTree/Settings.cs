using EpsilonLib.Settings;

namespace CacheEditor.Components.TagTree
{
    public static class Settings
    {

		public const string CollectionKey = "CacheEditor";

		public static SettingDefinition TagTreeViewModeSetting =        new SettingDefinition(CollectionKey, "TagTreeViewMode",            ((int)TagTreeViewMode.Groups).ToString());
        public static SettingDefinition TagTreeGroupDisplaySetting =    new SettingDefinition(CollectionKey, "TagTreeGroupDisplayMode",    ((int)TagTreeGroupDisplayMode.TagGroupName).ToString());
        public static SettingDefinition ShowTagGroupAltNamesSetting =   new SettingDefinition(CollectionKey, "ShowTagGroupAltNames",       false.ToString());
        public static SettingDefinition BaseCacheWarningsSetting =      new SettingDefinition(CollectionKey, "BaseCacheWarnings",          true.ToString());

    }
}
