#undef DEBUG

using Shared;
using System.Threading.Tasks;
using TagStructEditor.Fields;
using TagTool.Cache;
using TagTool.Tags;

namespace CacheEditor
{
    public class TagEditorContext : ITagEditorContext
    {

        public TagEditorContext(ICacheEditor cacheEditor, IShell shell, TagEditorViewModel viewModel, CachedTag instance, object definitionDataOrTaskObject) {
			
			if (cacheEditor == null) { throw new System.ArgumentNullException(nameof(cacheEditor)); }
			if (shell == null) { throw new System.ArgumentNullException(nameof(shell)); }

			CacheEditor = cacheEditor;
			Shell = shell;
			ViewModel = viewModel;
			Instance = instance;

			if (definitionDataOrTaskObject is Task<object>) {
				_definitionData = null;
				DefinitionTask = definitionDataOrTaskObject as Task<object>;
			}
			else {
				_definitionData = definitionDataOrTaskObject;
				DefinitionTask = Task.FromResult<object>(definitionDataOrTaskObject);
			}

		}
		public TagEditorContext(ICacheEditor cacheEditor, IShell shell, TagEditorViewModel viewModel, CachedTag instance) : this(cacheEditor, shell, viewModel, instance, null) { }
		public TagEditorContext(ICacheEditor cacheEditor, IShell shell, TagEditorViewModel viewModel) : this(cacheEditor, shell, viewModel, null) { }

		public bool IsValid { get { return DefinitionData is TagStructure; } }

		public ICacheEditor CacheEditor { get; set; }
        public IShell Shell { get; set; }
		public CachedTag Instance { get; set; }
        public TagEditorViewModel ViewModel { get; set; }
        
        private object _definitionData = null;
		public object DefinitionData {
			get {
				if (_definitionData == null) {
					if (DefinitionTask == null) { return _definitionData; } // No definition data will ever be available
					else {
						switch (DefinitionTask.Status) {
							
							case TaskStatus.RanToCompletion: 
								_definitionData = DefinitionTask.Result; 
								return _definitionData;

							case TaskStatus.Canceled:
							case TaskStatus.Faulted: 
								return _definitionData;

							case TaskStatus.Created:
							case TaskStatus.Running:
							case TaskStatus.WaitingToRun:
							case TaskStatus.WaitingForActivation:
							case TaskStatus.WaitingForChildrenToComplete:
								try { _definitionData = DefinitionTask.GetAwaiter().GetResult(); }
								catch { _definitionData = null; }
								break;
						}
					}
				}
				return _definitionData;
			}
		}
		private Task<object> DefinitionTask { get; set; } = null;

		public StructField CreateField(FieldFactory factory) {
			GameCache cache = CacheEditor.CacheFile.Cache;
			System.Type structType = cache.TagCache.TagDefinitions.GetTagDefinitionType(Instance.Group);

			#if DEBUG
			Stopwatch stopWatch = new Stopwatch();
			stopWatch.Start();
			#endif

			StructField field = factory.CreateStruct(structType);

			#if DEBUG
			Debug.WriteLine($"Create took {stopWatch.ElapsedMilliseconds}ms");
			stopWatch.Restart();
			#endif

			field.Populate(DefinitionData);

			#if DEBUG
			Debug.WriteLine($"Populate took {stopWatch.ElapsedMilliseconds}ms");
			#endif

			return field;
		}

		public const string ContextKey = "TagEditorContext_ContextKey";

		public TagStructEditor.PerCacheDefinitionEditorContext GetDefinitionEditorContext() {
			GameCache cache = CacheEditor.CacheFile.Cache;
			if (!CacheEditor.PluginStorage.TryGetValue(ContextKey, out object value) ||
				!ReferenceEquals(cache, ( value as TagStructEditor.PerCacheDefinitionEditorContext).Cache)) {
				value = new TagStructEditor.PerCacheDefinitionEditorContext(cache);
				CacheEditor.PluginStorage[ContextKey] = value;
			}
			return value as TagStructEditor.PerCacheDefinitionEditorContext;
		}

	}
}
