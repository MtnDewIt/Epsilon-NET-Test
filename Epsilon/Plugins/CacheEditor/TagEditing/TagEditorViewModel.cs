using EpsilonLib.Commands;
using EpsilonLib.Core;
using EpsilonLib.Shell;
using EpsilonLib.Shell.TreeModels;
using Stylet;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TagTool.Cache;

namespace CacheEditor
{
	public class TagEditorViewModel : Conductor<TagEditorPluginTabViewModel>.Collection.OneActive, ITagEditorPluginClient
    {

        public TagEditorContext tagEditorContext;

        private static readonly object LastOpenedTabKey = new object();
        private bool _pluginsLoaded = false;
        private ICacheEditingService _cacheEditingService;
        public IObservableCollection<TagEditorPluginTabViewModel> Documents => Items;

		private ITagEditorPlugin _content;
		public ITagEditorPlugin Content { 
            get { return _content; } 
            set { SetAndNotify(ref _content, value); }
		}

		private ICacheEditor _cacheEditor;
        public ICacheEditor CacheEditor { get { return _cacheEditor; } }

		public CachedTag Tag;
        public string FullName { get; set; }

        public ICommand CloseCommand { get; set; }
        public ICommand CopyTagNameCommand { get; set; }
        public ICommand CopyTagIndexCommand { get; set; }
        public ICommand TagTreeDeselect { get; set; }

        public TagEditorViewModel (TagEditorContext context, ICacheEditingService cacheEditingService)
        {
            _cacheEditingService = cacheEditingService;
			_cacheEditor = context.CacheEditor;
			Tag = context.Instance;
            context.ViewModel = this;
			DisplayName = $"{Path.GetFileName(Tag.Name)}.{Tag.Group.Tag}";
            FullName = $"{Tag.Name}.{Tag.Group.Tag}";

            CloseCommand = new DelegateCommand(Close);
            CopyTagNameCommand = new DelegateCommand(() => ClipboardEx.SetTextSafe($"{Tag}"));
            CopyTagIndexCommand = new DelegateCommand(() => ClipboardEx.SetTextSafe($"0x{Tag.Index:X08}"));
            TagTreeDeselect = new DelegateCommand(() => (context.CacheEditor.TagTree as TreeModel).SelectedNode = null);

            LoadPlugins(context);
		}

        private async void LoadPlugins(TagEditorContext context)
        {
            foreach (ITagEditorPluginProvider provider in _cacheEditingService.TagEditorPlugins) {
                
                if (!provider.ValidForTag(context.CacheEditor.CacheFile, context.Instance)) { continue; }

                if (provider is ITagEditorPluginProvider) {
                    Task<ITagEditorPlugin> futurePlugin = provider.CreateAsync(context);
					TagEditorPluginTabViewModel tab = new TagEditorPluginTabViewModel(futurePlugin, this) { DisplayName = provider.DisplayName };
					Items.Add(tab);					
                    // Instead of ->    Content = futurePlugin.Result;
					// The Content property is set in the constructor of TagEditorPluginTabViewModel
				}
			}

			// if a tab was opened previously and we have a tab with the same name, active that one. otherwse just activate the first.
			ISessionStore sessionStore = GlobalServiceProvider.GetService<ISessionStore>();
            sessionStore.TryGetItem(LastOpenedTabKey, out string lastOpenedTab);
			TagEditorPluginTabViewModel tabToActivate = Items.FirstOrDefault(x => x.DisplayName == lastOpenedTab) ?? Items.FirstOrDefault();

            ActiveItem = tabToActivate;
            _pluginsLoaded = true;
        }

        public void Close()
        {
            RequestClose();
            TagTreeDeselect.Execute(null);
        }

        protected override void OnClose()
        {
            base.OnClose();
            foreach (TagEditorPluginTabViewModel document in Documents)
                ((IScreen)document).Close();
            Documents.Clear();
        }

        protected override void OnActivate()
        {
            base.OnActivate();
        }

        public override void ActivateItem(TagEditorPluginTabViewModel item)
        {
            base.ActivateItem(item);

            if (item != null && _pluginsLoaded)
            {
				// store the last opened tab name
				ISessionStore sessionStore = GlobalServiceProvider.GetService<ISessionStore>();
                sessionStore.StoreItem(LastOpenedTabKey, item.DisplayName);
            }
        }

        async void ITagEditorPluginClient.PostMessage(object sender, object message)
        {
            foreach(TagEditorPluginTabViewModel tab in Items)
            {
                ITagEditorPlugin plugin =  tab.Content  ??  await tab.LoadTask;
				if (plugin != null && plugin != sender) { plugin.OnMessage(sender, message); }
            }
        }

        public override string ToString() { return Tag?.ToString() ?? string.Empty; }
    }
}
