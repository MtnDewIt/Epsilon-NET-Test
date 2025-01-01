using EpsilonLib.Settings;

namespace DefinitionEditor
{
	public static class Settings
	{
		public const string CollectionKey = "DefinitionEditor";

		public static SettingDefinition DisplayFieldTypesSetting = new SettingDefinition(CollectionKey, "DisplayFieldTypes", false.ToString());

		public static SettingDefinition DisplayFieldOffsetsSetting = new SettingDefinition(CollectionKey, "DisplayFieldOffsets", false.ToString());

		public static SettingDefinition CollapseBlocksSetting = new SettingDefinition(CollectionKey, "CollapseBlocks", false.ToString());
	}
}
