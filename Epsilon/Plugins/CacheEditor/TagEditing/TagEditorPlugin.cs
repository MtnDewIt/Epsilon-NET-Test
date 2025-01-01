using Shared;
using Stylet;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using TagTool.Cache;
using TagTool.Tags;

namespace CacheEditor
{

	/// <summary>
	/// Base class for tag editor plugins.<br/>
	/// </summary>
	/// <remarks>
	/// <b>No asynchronous code should be run directly within the constructor of any inheriting types.</b><br/><br/>
	/// The <see cref="ITagEditorPluginProvider.CreateAsync(TagEditorContext)"/> method should be used for running any required asynchronous code.<br/><br/>
	/// For an example of how to properly handle asynchronous execution while constructing a plugin, see the implementation for<br/>
	/// <see cref="BlamScriptEditorPlugin.ScriptTagEditorPluginProvider.CreateAsync"/>.<br/>
	/// • It constructs and stores a reference to the <see cref="BlamScriptEditorPlugin.ScriptTagEditorViewModel"/> using its constructor,<br/>
	/// • It then calls the <see cref="BlamScriptEditorPlugin.ScriptTagEditorViewModel.LoadAsync"/> method to perform the asynchronous work.<br/>
	/// • Finally, the <see cref="BlamScriptEditorPlugin.ScriptTagEditorViewModel"/> object is returned.
	/// </remarks>
	public abstract class TagEditorPlugin : Screen, ITagEditorPlugin {

		public TagEditorPlugin(TagEditorContext context, params object[] args) {
			TagEditorContext = context;
			if (context.IsValid) { _definition = TagEditorContext.DefinitionData as TagStructure; }
			_cacheEditor = TagEditorContext.CacheEditor;
			_instance = TagEditorContext.Instance;
			_client = TagEditorContext.ViewModel;
			_shell = TagEditorContext.Shell;
		}
		
		public TagEditorContext TagEditorContext { get; protected set; }

		public IShell Shell { get { return _shell; } set { _shell = value; } }
		protected IShell _shell;
		public bool HasShell { get { return _shell != null; } }

		public CachedTag Instance { get { return _instance; } set { _instance = value; } }
		protected CachedTag _instance;
		public bool HasInstance { get { return _instance != null; } }

		public virtual TagStructure Definition { get { return _definition; } set { _definition = value; } }
		protected TagStructure _definition;
		public bool HasDefinition { get { return _definition != null && _definition is TagStructure; } }

		public ICacheEditor CacheEditor { get { return _cacheEditor; } set { _cacheEditor = value; } }
		protected ICacheEditor _cacheEditor;
		public bool HasCacheEditor { get { return _cacheEditor != null; } }

		public ITagEditorPluginClient Client { get { return _client; } set { _client = value; } }
		protected ITagEditorPluginClient _client;
		public bool HasClient { get { return _client != null; } }

		public new string DisplayName { get { return _displayName; } }
		protected string _displayName;

		public int SortOrder { get { return _sortOrder; } }
		protected int _sortOrder;

		public virtual void OnMessage(object sender, object message) { OnMessage(sender, message); }
        public virtual void PostMessage(object sender, object message) { _client?.PostMessage(sender, message); }


		public virtual bool ValidForTag(ICacheFile cache, CachedTag tag) {
			return false;
		}
	}
}
