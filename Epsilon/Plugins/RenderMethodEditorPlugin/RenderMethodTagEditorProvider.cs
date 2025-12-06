using CacheEditor;
using CacheEditor.RTE;
using Shared;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using TagTool.Cache;
using TagTool.Tags.Definitions;

namespace RenderMethodEditorPlugin
{
    [Export(typeof(ITagEditorPluginProvider))]
    class RenderMethodTagEditorProvider : ITagEditorPluginProvider
    {
        private readonly IRteService _rteService;
        private readonly Lazy<IShell> _shell;

        [ImportingConstructor]
        public RenderMethodTagEditorProvider(Lazy<IShell> shell, IRteService rteService)
        {
            _shell = shell;
            _rteService = rteService;
        }

        public string DisplayName => "Render Method Editor";

        public int SortOrder => 1;

        public async Task<ITagEditorPlugin> CreateAsync(TagEditorContext context)
        {
            RenderMethod definitionData = null;

            if (context.Instance.IsInGroup("rm  "))
                definitionData = (RenderMethod)(await context.DefinitionData);
            else if (context.Instance.IsInGroup("prt3"))
                definitionData = ((Particle)(await context.DefinitionData)).RenderMethod;

            var cache = context.CacheEditor.CacheFile.Cache;
            return new RenderMethodEditorViewModel(context, cache, definitionData);
        }

        public bool ValidForTag(ICacheFile cache, CachedTag tag)
        {
            if( cache.Cache is GameCacheGen3 || cache.Cache is GameCacheHaloOnlineBase)
                if (tag.IsInGroup("rm  ") || tag.IsInGroup("prt3"))
                    return true;
            return false;
        }
    }
}
