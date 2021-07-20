using EpsilonLib.Options;
using EpsilonLib.Settings;
using System.IO;
using System.ComponentModel.Composition;

namespace Epsilon.Options
{
    [Export(typeof(IOptionsPage))]
    class GeneralOptionsViewModel : OptionPageBase
    {
        private readonly ISettingsCollection _settings;
        private string _defaultCachePath;
        private bool _defaultCachePathIsValid;

        [ImportingConstructor]
        public GeneralOptionsViewModel(ISettingsService settingsService) : base("General", "General")
        {
            _settings = settingsService.GetCollection(GeneralSettings.CollectionKey);
        }

        public string DefaultCachePath
        {
            get => _defaultCachePath;
            set => SetOptionAndNotify(ref _defaultCachePath, value);
        }
        public bool PathIsValid
        {
            get
            {
                _defaultCachePathIsValid = File.Exists(@_defaultCachePath) && @_defaultCachePath.EndsWith(".dat");
                return _defaultCachePathIsValid;
            }
            set => SetOptionAndNotify(ref _defaultCachePathIsValid, value);
        }

        public override void Apply()
        {
            if (PathIsValid)
                _settings.Set(GeneralSettings.DefaultTagCacheSetting.Key, DefaultCachePath);
        }

        public override void Load()
        {
            DefaultCachePath = _settings.Get(GeneralSettings.DefaultTagCacheSetting.Key, "");
        }
    }
}
