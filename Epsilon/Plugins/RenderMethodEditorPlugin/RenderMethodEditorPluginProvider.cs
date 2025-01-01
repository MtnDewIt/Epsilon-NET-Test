using CacheEditor;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using TagTool.Cache;

namespace RenderMethodEditorPlugin
{
	[Export(typeof(ITagEditorPluginProvider))]
	public class RenderMethodEditorPluginProvider : ITagEditorPluginProvider
	{

		public string DisplayName => "Render Method Editor";
		public int SortOrder => 1;

		public RenderMethodEditorPluginProvider() { }
		public async Task<ITagEditorPlugin> CreateAsync(TagEditorContext context) { 
			return new RenderMethodEditorViewModel(context);
		}
		public bool ValidForTag(ICacheFile cache, CachedTag tag) {
			return tag.IsInGroup("rm  ", "prt3") && ( cache.Cache is GameCacheGen3 || cache.Cache is GameCacheHaloOnlineBase );
		}
	}
}
