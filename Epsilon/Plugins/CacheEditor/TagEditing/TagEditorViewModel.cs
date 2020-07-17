using EpsilonLib.Commands;
using Stylet;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace CacheEditor
{
    class TagEditorViewModel : Conductor<TagEditorPluginTabViewModel>.Collection.OneActive
    {
        private ICacheEditingService _cacheEditingService;

        public ICommand CloseCommand { get; set; }

        public TagEditorViewModel(ICacheEditingService cacheEditingService, TagEditorContext context)
        {
            _cacheEditingService = cacheEditingService;
            var instance = context.Instance;
            DisplayName = $"{Path.GetFileName(instance.Name)}.{instance.Group}";

            CloseCommand = new DelegateCommand(Close);

            LoadPlugins(context);
        }


        private void LoadPlugins(TagEditorContext context)
        {
            Items.AddRange(
                _cacheEditingService.TagEditorPlugins
                .Where(provider => provider.ValidForTag(context.CacheEditor.CacheFile, context.Instance))
                .Select(provider => new TagEditorPluginTabViewModel(provider.CreateAsync(context)) { DisplayName = provider.DisplayName }));

            ActiveItem = Items.FirstOrDefault();
        }

        public void Close()
        {
            RequestClose();
        }

        protected override void OnClose()
        {
            base.OnClose();
        }

        protected override void OnActivate()
        {
            base.OnActivate();
        }
    }
}
