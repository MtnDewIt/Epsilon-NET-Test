using CacheEditor;
using Shared;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using TagTool.Cache;
using TagTool.Tags.Definitions;

namespace BlamScriptEditorPlugin
{
    [Export(typeof(ITagEditorPluginProvider))]
    public class ScriptTagEditorPluginProvider : ITagEditorPluginProvider
    {

		public string DisplayName => "Scripts";
		public int SortOrder => 2;

		public ScriptTagEditorPluginProvider() { }

        public async Task<ITagEditorPlugin> CreateAsync(TagEditorContext context)
        {
			ScriptTagEditorViewModel vm = new ScriptTagEditorViewModel(context);
            await vm.LoadAsync();
            return vm;
        }

        public bool ValidForTag(ICacheFile cache, CachedTag tag) { 
            return tag?.IsInGroup("scnr") ?? false; 
        }
    }
}
