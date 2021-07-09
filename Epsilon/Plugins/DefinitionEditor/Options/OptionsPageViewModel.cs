using EpsilonLib.Options;
using EpsilonLib.Settings;
using System.ComponentModel.Composition;

namespace DefinitionEditor.Options
{
    [Export(typeof(IOptionsPage))]
    class OptionsPageViewModel : OptionPageBase
    {
        private readonly ISettingsCollection _settings;
        private bool _isDisplayFieldTypesEnabled;
        private bool _isCollapseBlocksEnabled;

        [ImportingConstructor]
        public OptionsPageViewModel(ISettingsService settingsService) : base("Cache Editor", "Definition Editor")
        {
            _settings = settingsService.GetCollection(Settings.CollectionKey);
        }

        public bool IsDisplayFieldTypesChecked
        {
            get => _isDisplayFieldTypesEnabled;
            set => SetOptionAndNotify(ref _isDisplayFieldTypesEnabled, value);
        }

        public bool IsCollapseBlocksChecked
        {
            get => _isCollapseBlocksEnabled;
            set => SetOptionAndNotify(ref _isCollapseBlocksEnabled, value);
        }

        public override void Apply()
        {
            _settings.Set(Settings.DisplayFieldTypesSetting.Key, IsDisplayFieldTypesChecked);
            _settings.Set(Settings.CollapseBlocksSetting.Key, IsCollapseBlocksChecked);
        }

        public override void Load()
        {
            IsDisplayFieldTypesChecked = _settings.Get(Settings.DisplayFieldTypesSetting.Key, (bool)Settings.DisplayFieldTypesSetting.DefaultValue);
            IsCollapseBlocksChecked = _settings.Get(Settings.CollapseBlocksSetting.Key, (bool)Settings.CollapseBlocksSetting.DefaultValue);
        }
    }
}
