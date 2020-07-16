using CacheEditor.Components.TagTree;
using EpsilonLib.Options;
using EpsilonLib.Settings;
using Shared;
using Stylet;
using System;
using System.ComponentModel.Composition;
using System.Runtime.CompilerServices;

namespace CacheEditor.Options
{
    [Export(typeof(IOptionsPage))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    class GeneralOptionsPageViewModel : Screen, IOptionsPage
    {
        public string Category => "Cache Editor";
        public bool IsDirty { get; set; }

        private TagTreeViewMode _tagTreeViewMode;
        private ISettingsCollection _settings;

        public TagTreeViewMode TagTreeViewMode
        {
            get => _tagTreeViewMode;
            set => SetOptionAndNotify(ref _tagTreeViewMode, value);
        }

        protected virtual void SetOptionAndNotify<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if(SetAndNotify(ref field, value, propertyName))
                IsDirty = true;
        }

        [ImportingConstructor]
        public GeneralOptionsPageViewModel(ISettingsService settingsService)
        {
            DisplayName = "General";
            _settings = settingsService.GetCollection("CacheEditor");
            
        }

        public void Apply()
        {
            _settings.Set(Components.TagTree.Settings.TagTreeViewModeSetting.Key, TagTreeViewMode);
        }


        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            TagTreeViewMode = _settings.Get(
                Components.TagTree.Settings.TagTreeViewModeSetting.Key,
                (TagTreeViewMode)Components.TagTree.Settings.TagTreeViewModeSetting.DefaultValue);
        }

        public void Load()
        {
            
        }
    }
}
