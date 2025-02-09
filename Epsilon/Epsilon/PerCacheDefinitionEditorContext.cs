using Epsilon.Common;
using TagTool.Cache;

namespace Epsilon
{
	public class PerCacheEpsilonContext
	{
		public GameCache Cache { get; }
		public TagList TagList { get; }

		public PerCacheEpsilonContext(GameCache cache) {
			Cache = cache;
			TagList = new TagList(cache);
		}
	}

}
