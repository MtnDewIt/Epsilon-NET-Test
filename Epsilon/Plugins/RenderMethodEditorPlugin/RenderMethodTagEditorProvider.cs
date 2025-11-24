using CacheEditor;
using CacheEditor.RTE;
using DefinitionEditor;
using Shared;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading.Tasks;
using TagStructEditor.Fields;
using TagTool.Cache;
using TagTool.Tags.Definitions;
using TagList = TagStructEditor.Common.TagList;

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
            var valueChangeSink = new ValueChangedSink();

            // All we need to track are changed values in the structure
            var config = new TagStructEditor.Configuration()
            {
                OpenTag = null,
                BrowseTag = null,
                ValueChanged = valueChangeSink.Invoke,
                DisplayFieldTypes = false,
                DisplayFieldOffsets = false,
                CollapseBlocks = false
            };

            var ctx = GetDefinitionEditorContext(context);
            var factory = new FieldFactory(ctx.Cache, ctx.TagList, config);

            RenderMethod definitionData = null;

            if (context.Instance.IsInGroup("rm  "))
                definitionData = (RenderMethod)(await context.DefinitionData);
            else if (context.Instance.IsInGroup("prt3"))
                definitionData = ((Particle)(await context.DefinitionData)).RenderMethod;

            var field = await Task.Run(() => CreateField(context, factory, definitionData));

            return new RenderMethodEditorViewModel(
                _shell.Value,
                _rteService,
                context.CacheEditor,
                context.CacheEditor.CacheFile,
                context.Instance,
                definitionData,
                field,
                valueChangeSink);
        }

        private static StructField CreateField(TagEditorContext context, FieldFactory factory, object definitionData)
        {
            var cache = context.CacheEditor.CacheFile.Cache;
            var structType = cache.TagCache.TagDefinitions.GetTagDefinitionType(context.Instance.Group);

            var stopWatch = new Stopwatch();

            stopWatch.Start();

            var field = factory.CreateStruct(structType);
            Debug.WriteLine($"Create took {stopWatch.ElapsedMilliseconds}ms");

            stopWatch.Restart();

            field.Populate(definitionData);
            Debug.WriteLine($"populate took {stopWatch.ElapsedMilliseconds}ms");

            return field;
        }

        private static PerCacheDefinitionEditorContext GetDefinitionEditorContext(TagEditorContext context)
        {
            var cacheEditor = context.CacheEditor;
            var cache = cacheEditor.CacheFile.Cache;

            return new PerCacheDefinitionEditorContext(cache);
        }

        public bool ValidForTag(ICacheFile cache, CachedTag tag)
        {
            if( cache.Cache is GameCacheGen3 || cache.Cache is GameCacheHaloOnlineBase)
                if (tag.IsInGroup("rm  ") || tag.IsInGroup("prt3"))
                    return true;
            return false;
        }

        class PerCacheDefinitionEditorContext
        {
            public GameCache Cache { get; }
            public TagList TagList { get; }

            public PerCacheDefinitionEditorContext(GameCache cache)
            {
                Cache = cache;
                TagList = new TagList(cache);
            }
        }

        class ValueChangedSink : IFieldsValueChangeSink
        {
            public event EventHandler<ValueChangedEventArgs> ValueChanged;

            public void Invoke(ValueChangedEventArgs e)
            {
                ValueChanged?.Invoke(this, e);
            }
        }
    }
}
