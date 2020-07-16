using Stylet;
using System.Threading.Tasks;

namespace CacheEditor
{
    class TagEditorPluginTabViewModel : Screen
    {
        private IScreen _content;

        public IScreen Content
        {
            get => _content;
            set => SetAndNotify(ref _content, value);
        }

        public TagEditorPluginTabViewModel(Task<ITagEditorPlugin> futurePlugin)
        {
            DoLoadingAsync(futurePlugin); 
        }

        private async void DoLoadingAsync(Task<ITagEditorPlugin> futurePlugin)
        {
            var plugin = await futurePlugin;
            Content = plugin;
        }

        protected override void OnClose()
        {
            Content?.Close();
            _content = null;
        }

        ~TagEditorPluginTabViewModel()
        {

        }
    }
}
