using Stylet;
using System.Threading.Tasks;

namespace Epsilon
{
	public class TagEditorPluginTabViewModel : Screen {

        private ITagEditorPlugin _content;
        public ITagEditorPlugin Content { get { return _content; } set { SetAndNotify(ref _content, value); } }

        public Task<ITagEditorPlugin> LoadTask { get; }
        private async void DoLoadingAsync(Task<ITagEditorPlugin> futurePlugin, TagEditorViewModel parent) { 
            Content = await futurePlugin;
			Content.Client = parent;
			parent.Content = _content;
		}
        
        public TagEditorPluginTabViewModel(Task<ITagEditorPlugin> futurePlugin, TagEditorViewModel parent) {
            LoadTask = futurePlugin;
            DoLoadingAsync(futurePlugin, parent);
		}

        protected override void OnClose() { _content?.Close(); _content = null; }

    }
}
