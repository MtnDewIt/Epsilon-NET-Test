using CacheEditor;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagTool.Cache;
using TagTool.Tags.Definitions;

namespace RenderMethodEditorPlugin
{
    [Export(typeof(ITagEditorPluginProvider))]
    class RenderMethodTagEditorProvider : ITagEditorPluginProvider
    {
        public string DisplayName => "Render Method Editor";

        public int SortOrder => 1;

        public async Task<ITagEditorPlugin> CreateAsync(TagEditorContext context)
        {
            RenderMethod rm = (RenderMethod)(await context.DefinitionData);
            var cache = context.CacheEditor.CacheFile.Cache;
            return new RenderMethodEditorViewModel(cache, rm);
        }

        public bool ValidForTag(ICacheFile cache, CachedTag tag)
        {
            if( cache.Cache is GameCacheGen3 || cache.Cache is GameCacheHaloOnlineBase)
                if (tag.IsInGroup("rm  ") )
                    return true;
            return false;
        }
    }
}
