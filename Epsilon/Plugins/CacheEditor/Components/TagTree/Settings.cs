using EpsilonLib.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheEditor.Components.TagTree
{
    public static class Settings
    {
        public static SettingDefinition TagTreeViewModeSetting = new SettingDefinition("TagTreeViewMode", TagTreeViewMode.Groups);
        public static SettingDefinition TagTreeGroupDisplaySetting = new SettingDefinition("TagTreeGroupDisplayMode", TagTreeGroupDisplayMode.TagGroupName);
    }
}
