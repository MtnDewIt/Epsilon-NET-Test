using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpsilonLib.Settings
{
    public interface ISettingsService
    {
        event EventHandler<SettingChangedEventArgs> SettingChanged;

        ISettingsCollection GetCollection(string key);
    }
}
