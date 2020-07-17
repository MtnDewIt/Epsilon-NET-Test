using EpsilonLib.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DefinitionEditor
{
    public static class Settings
    {
        public const string CollectionKey = "DefinitionEditor";

        public static SettingDefinition DisplayFieldTypesSetting = new SettingDefinition("DisplayFieldTypes", false);
    }
}
