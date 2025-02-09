using System;

namespace Epsilon
{
    public interface ISettingsService
    {
        event EventHandler<SettingChangedEventArgs> SettingChanged;

        ISettingsCollection GetCollection(string key);
    }
}
