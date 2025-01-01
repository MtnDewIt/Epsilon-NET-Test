using TagStructEditor.Common;
using TagTool.Cache;

namespace TagStructEditor
{
	public class PerCacheDefinitionEditorContext
	{
		public GameCache Cache { get; }
		public TagList TagList { get; }

		public PerCacheDefinitionEditorContext(GameCache cache) {
			Cache = cache;
			TagList = new TagList(cache);
		}
	}

}
