using EpsilonLib.Options;
using EpsilonLib.Settings;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace Epsilon.Options
{
    enum Theme
    {
        Default,
        Solid,
        Frosted,
        Transparent,
        Steam,
        HotWheels,
        ClownWorld,
        Dark1,
        Dark2,
        Dark3
    }

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
        private string _defaultPakShort;

        private string _accentColorHex;
        private Theme _theme;

        [ImportingConstructor]
        public GeneralOptionsViewModel(ISettingsService settingsService) : base("General", "General")
        {
            _settings = settingsService.GetCollection(GeneralSettings.CollectionKey);
        }

        public string AccentColorHex
        {
            get => _accentColorHex;
            set
            {
                SetOptionAndNotify(ref _accentColorHex, value);
                UpdateAccentColor(AccentColorHex);
            }
        }

        public Theme Theme 
        {
            get => _theme;
            set
            {
                SetOptionAndNotify(ref _theme, value, nameof(Theme));
                UpdateTheme(Theme);
            }
        }

        private void UpdateAccentColor(string accentColorHex)
        {
            Application.Current.Resources["AccentColor"] = (Color)ColorConverter.ConvertFromString(accentColorHex);

            var mergedDictionary = Application.Current.Resources.MergedDictionaries;
            mergedDictionary.RemoveAt(mergedDictionary.Count - 1);

            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("/Epsilon;component/Themes/" + Theme + ".xaml", UriKind.Relative)
            });
        }

        private void UpdateTheme(Theme theme)
        {
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("/Epsilon;component/Themes/" + theme + ".xaml", UriKind.Relative)
            });
        }

        public void RevertAppearance()
        {
            string og_accent = _settings.Get(GeneralSettings.AccentColorSetting.Key, "#007ACC");
            Theme og_theme = _settings.Get(GeneralSettings.ThemeSetting.Key, Theme.Default);
            UpdateAccentColor(og_accent);
            UpdateTheme(og_theme);
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
            _settings.Set(GeneralSettings.AccentColorSetting.Key, AccentColorHex);
            _settings.Set(GeneralSettings.ThemeSetting.Key, Theme);

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
            AccentColorHex = _settings.Get(GeneralSettings.AccentColorSetting.Key, "#007ACC");
            Theme = _settings.Get(GeneralSettings.ThemeSetting.Key, Theme.Default);
        }
    }
}
    