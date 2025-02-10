using System.ComponentModel.Composition;
using System.Threading.Tasks;
using TagTool.Cache;
using TagTool.Common;

namespace Epsilon
{
	[Export(typeof(ITagEditorPluginProvider))]
	public class TagResourceEditorPluginProvider : ITagEditorPluginProvider
	{
		// list of tags that have resources
		private static readonly Tag[] TagsWithResources =
		{
			new Tag("sbsp"),
			new Tag("sLdT"),
			new Tag("Lbsp"),
			new Tag("bitm"),
			new Tag("mode"),
			new Tag("pmdf"),
			new Tag("coll"),
			new Tag("snd!"),
			new Tag("jmad"),
			new Tag("bink"),
		};

		public string DisplayName => "Resources";
		public int SortOrder => 4;

		public TagResourceEditorPluginProvider() { }

		public async Task<ITagEditorPlugin> CreateAsync(TagEditorContext context) {
			TagResourceEditorViewModel editor = new TagResourceEditorViewModel(context);
			await editor.LoadAsync(context.DefinitionData);
			return editor;
		}

		public bool ValidForTag(ICacheFile cache, CachedTag tag) {
			return tag.IsInGroup(TagsWithResources);
		}
	}
}
