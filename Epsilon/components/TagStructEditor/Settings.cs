using EpsilonLib.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagStructEditor
{
    public static class Settings
    {

		public const string CollectionKey = "TagResourceAndDefinitionEditorSettings";

		public static SettingDefinition DisplayFieldTypesSetting = new SettingDefinition(CollectionKey, "DisplayFieldTypes", false.ToString());
		public static SettingDefinition DisplayFieldOffsetsSetting = new SettingDefinition(CollectionKey, "DisplayFieldOffsets", false.ToString());
		public static SettingDefinition CollapseBlocksSetting = new SettingDefinition(CollectionKey, "CollapseBlocks", false.ToString());

		public static void Load(Configuration config) {
			ISettingsCollection settings = SettingsService.GetCollection(CollectionKey);
			config.DisplayFieldTypes = settings.GetBool(DisplayFieldTypesSetting);
			config.DisplayFieldOffsets = settings.GetBool(DisplayFieldOffsetsSetting);
			config.CollapseBlocks = settings.GetBool(CollapseBlocksSetting);
		}

	}
}
