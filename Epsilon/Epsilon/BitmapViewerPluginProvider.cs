using Epsilon;
using Shared;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using TagTool.Cache;
using TagTool.Tags.Definitions;

namespace Epsilon
{
    [Export(typeof(ITagEditorPluginProvider))]
    public class BitmapViewerPluginProvider : ITagEditorPluginProvider
    {

        public string DisplayName => "Bitmap";
		public int SortOrder => 6;

		public BitmapViewerPluginProvider() { }
        public async Task<ITagEditorPlugin> CreateAsync(TagEditorContext context) { return new BitmapViewerViewModel(context); }
        public bool ValidForTag(ICacheFile cache, CachedTag tag) { return tag.IsInGroup("bitm"); }
    }
}
