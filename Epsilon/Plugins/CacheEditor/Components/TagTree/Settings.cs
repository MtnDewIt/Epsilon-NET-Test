using EpsilonLib.Settings;

namespace CacheEditor.Components.TagTree
{
    public static class Settings
    {
        public static SettingDefinition TagTreeViewModeSetting = new SettingDefinition("TagTreeViewMode", TagTreeViewMode.Groups);
        public static SettingDefinition TagTreeGroupDisplaySetting = new SettingDefinition("TagTreeGroupDisplayMode", TagTreeGroupDisplayMode.TagGroupName);
    }
}
