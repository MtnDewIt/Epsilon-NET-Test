using EpsilonLib.Options;
using EpsilonLib.Settings;
using System.IO;
using System.ComponentModel.Composition;
using System.Windows;

namespace Epsilon.Options
{
    [Export(typeof(IOptionsPage))]
    class GeneralOptionsViewModel : OptionPageBase
    {
        private readonly ISettingsCollection _settings;
        private string _defaultCachePath;
        private bool _defaultCachePathIsValid;
        private string _defaultPakPath;
        private bool _defaultPakPathIsValid;
        private string _startupPositionLeft;
        private string _startupPositionTop;
        private string _startupWidth;
        private string _startupHeight;
        private bool _alwaysOnTop;

        private string _defaultCacheShort;
        public string _defaultPakShort;

        [ImportingConstructor]
        public GeneralOptionsViewModel(ISettingsService settingsService) : base("General", "General")
        {
            _settings = settingsService.GetCollection(GeneralSettings.CollectionKey);
        }

        public string DefaultCachePath
        {
            get => _defaultCachePath;
            set
            {
                SetOptionAndNotify(ref _defaultCachePath, value);
                DefaultCacheShort = ShortenPath(value);
            }
        }

        public string DefaultCacheShort
        {
            get => _defaultCacheShort;
            set => SetOptionAndNotify(ref _defaultCacheShort, value);
        }

        public bool CachePathIsValid
        {
            get
            {
                _defaultCachePathIsValid = (File.Exists(@_defaultCachePath) && @_defaultCachePath.EndsWith(".dat") || @_defaultCachePath == "");
                return _defaultCachePathIsValid;
            }
            set => SetOptionAndNotify(ref _defaultCachePathIsValid, value);
        }

        public string DefaultPakPath
        {
            get => _defaultPakPath;
            set
            {
                SetOptionAndNotify(ref _defaultPakPath, value);
                DefaultPakShort = ShortenPath(value);
            }
        }

        public string DefaultPakShort
        {
            get => _defaultPakShort;
            set => SetOptionAndNotify(ref _defaultPakShort, value);
        }

        public bool PakPathIsValid
        {
            get
            {
                _defaultPakPathIsValid = (File.Exists(@_defaultPakPath) && @_defaultPakPath.EndsWith(".pak") || @_defaultPakPath == "");
                return _defaultPakPathIsValid;
            }
            set => SetOptionAndNotify(ref _defaultCachePathIsValid, value);
        }

        public string StartupPositionLeft
        {
            get => _startupPositionLeft;
            set => SetOptionAndNotify(ref _startupPositionLeft, value);
        }

        public string StartupPositionTop
        {
            get => _startupPositionTop;
            set => SetOptionAndNotify(ref _startupPositionTop, value);
        }

        public string StartupWidth
        {
            get => _startupWidth;
            set => SetOptionAndNotify(ref _startupWidth, value);
        }

        public string StartupHeight
        {
            get => _startupHeight;
            set => SetOptionAndNotify(ref _startupHeight, value);
        }

        public bool AlwaysOnTop
        {
            get => _alwaysOnTop;
            set => SetOptionAndNotify(ref _alwaysOnTop, value);
        }

        public override void Apply()
        {
            if (CachePathIsValid)
                _settings.Set(GeneralSettings.DefaultTagCacheSetting.Key, DefaultCachePath);
            if (PakPathIsValid)
                _settings.Set(GeneralSettings.DefaultPakSetting.Key, DefaultPakPath);

            _settings.Set(GeneralSettings.StartupPositionLeftSetting.Key, StartupPositionLeft);
            _settings.Set(GeneralSettings.StartupPositionTopSetting.Key, StartupPositionTop);

            _settings.Set(GeneralSettings.StartupWidthSetting.Key, StartupWidth);
            _settings.Set(GeneralSettings.StartupHeightSetting.Key, StartupHeight);

            _settings.Set(GeneralSettings.AlwaysOnTopSetting.Key, AlwaysOnTop);
            Application.Current.Resources["AlwaysOnTop"] = AlwaysOnTop;
        }

        public override void Load()
        {
            DefaultCachePath = _settings.Get(GeneralSettings.DefaultTagCacheSetting.Key, "");
            DefaultPakPath = _settings.Get(GeneralSettings.DefaultPakSetting.Key, "");
            StartupPositionLeft = _settings.Get(GeneralSettings.StartupPositionLeftSetting.Key, "");
            StartupPositionTop = _settings.Get(GeneralSettings.StartupPositionTopSetting.Key, "");
            StartupWidth = _settings.Get(GeneralSettings.StartupWidthSetting.Key, "");
            StartupHeight = _settings.Get(GeneralSettings.StartupHeightSetting.Key, "");
            AlwaysOnTop = _settings.Get(GeneralSettings.AlwaysOnTopSetting.Key, false);
        }
    }
}
    