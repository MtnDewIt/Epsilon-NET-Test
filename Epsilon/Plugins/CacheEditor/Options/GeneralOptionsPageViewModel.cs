using CacheEditor.Components.TagTree;
using EpsilonLib.Options;
using EpsilonLib.Settings;
using System.ComponentModel.Composition;

namespace CacheEditor.Options
{
    [Export(typeof(IOptionsPage))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    class GeneralOptionsPageViewModel : OptionPageBase
    {
        private TagTreeViewMode _tagTreeViewMode;
        private TagTreeGroupDisplayMode _tagTreeGroupDisplayMode;
        private ISettingsCollection _settings;

        [ImportingConstructor]
        public GeneralOptionsPageViewModel(ISettingsService settingsService) : base("Cache Editor", "General")
        {
            _settings = settingsService.GetCollection(Settings.CollectionKey);
        }

        public TagTreeViewMode TagTreeViewMode
        {
            get => _tagTreeViewMode;
            set => SetOptionAndNotify(ref _tagTreeViewMode, value);
        }

        public TagTreeGroupDisplayMode TagTreeGroupDisplayMode
        {
            get => _tagTreeGroupDisplayMode;
            set => SetOptionAndNotify(ref _tagTreeGroupDisplayMode, value);
        }

        public override void Apply()
        {
            _settings.Set(Components.TagTree.Settings.TagTreeViewModeSetting.Key, TagTreeViewMode);
            _settings.Set(Components.TagTree.Settings.TagTreeGroupDisplaySetting.Key, TagTreeGroupDisplayMode);
        }

        public override void Load()
        {
            TagTreeViewMode = _settings.Get<TagTreeViewMode>(Components.TagTree.Settings.TagTreeViewModeSetting);
            TagTreeGroupDisplayMode = _settings.Get<TagTreeGroupDisplayMode>(Components.TagTree.Settings.TagTreeGroupDisplaySetting);
        }
    }
}
