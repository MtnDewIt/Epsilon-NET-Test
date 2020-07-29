using CacheEditor;
using Shared;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using TagTool.Cache;
using TagTool.Common;

namespace TagResourceEditorPlugin
{
    [Export(typeof(ITagEditorPluginProvider))]
    class TagResourceEditorProvider : ITagEditorPluginProvider
    {
        // hard code for now
        private static readonly Tag[] TagsWithResources =
        {
            new Tag("sbsp"),
            new Tag("lbsp"),
            new Tag("bitm"),
            new Tag("mode"),
            new Tag("coll"),
            new Tag("snd!"),
            new Tag("jamd"),
            new Tag("bink")
        };

        private readonly Lazy<IShell> _shell;

        [ImportingConstructor]
        public TagResourceEditorProvider(Lazy<IShell> shell)
        {
            _shell = shell;
        }

        public string DisplayName => "Resources";

        public int SortOrder => 4;

        public async Task<ITagEditorPlugin> CreateAsync(TagEditorContext context)
        {
            var editor = new TagResourceEditorViewModel(_shell.Value, context.CacheEditor.CacheFile);
            await editor.LoadAsync(await context.DefinitionData);
            return editor;
        }

        public bool ValidForTag(ICacheFile cache, CachedTag tag)
        {
            return tag.IsInGroup(TagsWithResources);
        }
    }
}
