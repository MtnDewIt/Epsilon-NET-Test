using EpsilonLib.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Options
{
    public static class GeneralSettings
    {
        public const string CollectionKey = "General";

        public static SettingDefinition DefaultTagCacheSetting = new SettingDefinition("DefaultTagCache", "");
    }
}
