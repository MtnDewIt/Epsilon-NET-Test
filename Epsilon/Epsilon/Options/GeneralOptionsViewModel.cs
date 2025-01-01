using EpsilonLib.Options;
using EpsilonLib.Settings;
using System.IO;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Media;
using WpfApp20;
using System.Windows.Controls;
using System;
using System.Linq;
using Epsilon.Pages;
using Epsilon.Properties;

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
		private string _defaultPakCachePath;
		private bool _defaultPakCachePathIsValid;
		private string _startupPositionLeft;
        private string _startupPositionTop;
        private string _startupWidth;
        private string _startupHeight;
        private bool _alwaysOnTop;

        private string _defaultCacheShort;
        private string _defaultPakShort;
		private string _defaultPakCacheShort;

		private string _accentColorHex;
        private string _theme;

        [ImportingConstructor]
        public GeneralOptionsViewModel(ISettingsService settingsService) : base(Settings.CollectionKey, Settings.CollectionKey)
        {
            _settings = settingsService.GetCollection(Settings.CollectionKey);
        }

        public string AccentColorHex
        {
            get => _accentColorHex;
            set
            {
                SetOptionAndNotify(ref _accentColorHex, value);
                UpdateAppearance(AccentColorHex, Theme);
            }
        }

        public string Theme = "Default";

        private void UpdateAppearance(string accentColorHex, string theme)
        {
            Application.Current.Resources["AccentColor"] = (Color)ColorConverter.ConvertFromString(accentColorHex);

            var mergedDictionary = Application.Current.Resources.MergedDictionaries;
            mergedDictionary.RemoveAt(mergedDictionary.Count - 1);

            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("/Epsilon;component/Themes/" + theme + ".xaml", UriKind.Relative)
            });
        }

        public void RevertAppearance()
        {
            string og_accent = _settings.Get(Settings.AccentColor);
			string og_theme = "Default";
            UpdateAppearance(og_accent, og_theme);
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

		public string DefaultPakCachePath {
			get => _defaultPakCachePath;
			set {
				SetOptionAndNotify(ref _defaultPakCachePath, value);
				DefaultPakCacheShort = ShortenPath(value);
			}
		}

		public string DefaultPakCacheShort {
			get => _defaultPakCacheShort;
			set => SetOptionAndNotify(ref _defaultPakCacheShort, value);
		}

		public bool PakCachePathIsValid {
			get {
				_defaultPakCachePathIsValid = ( File.Exists(@_defaultPakCachePath) && @_defaultPakCachePath.EndsWith(".dat") || @_defaultPakCachePath == "" );
				return _defaultPakCachePathIsValid;
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

		public override void Save() { Apply(); }

		public override void Apply()
        {
            if (CachePathIsValid)
                _settings.Set(Settings.DefaultTagCache.Key, DefaultCachePath);
            if (PakPathIsValid)
                _settings.Set(Settings.DefaultPak.Key, DefaultPakPath);
			if (PakCachePathIsValid)
				_settings.Set(Settings.DefaultPakCache.Key, DefaultPakCachePath);

			_settings.Set(Settings.StartupPositionLeft.Key, StartupPositionLeft);
            _settings.Set(Settings.StartupPositionTop.Key, StartupPositionTop);

            _settings.Set(Settings.StartupWidth.Key, StartupWidth);
            _settings.Set(Settings.StartupHeight.Key, StartupHeight);

            _settings.SetBool(Settings.AlwaysOnTop.Key, AlwaysOnTop);
            _settings.Set(Settings.AccentColor.Key, AccentColorHex);

            Application.Current.Resources["AlwaysOnTop"] = AlwaysOnTop;
        }

        public override void Load()
        {
            DefaultCachePath = _settings.Get(Settings.DefaultTagCache);
            DefaultPakPath = _settings.Get(Settings.DefaultPak);
			DefaultPakCachePath = _settings.Get(Settings.DefaultPakCache);
			StartupPositionLeft = _settings.Get(Settings.StartupPositionLeft);
            StartupPositionTop = _settings.Get(Settings.StartupPositionTop);
            StartupWidth = _settings.Get(Settings.StartupWidth);
            StartupHeight = _settings.Get(Settings.StartupHeight);
            AlwaysOnTop = _settings.GetBool(Settings.AlwaysOnTop);
            AccentColorHex = _settings.Get(Settings.AccentColor);
        }
    }
}
    