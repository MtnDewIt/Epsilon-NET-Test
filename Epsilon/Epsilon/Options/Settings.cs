using EpsilonLib.Settings;

namespace Epsilon.Options
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
    }
}
