using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagTool.Cache;

namespace Epsilon
{
	public interface ITagEditorPluginProvider
	{
        string DisplayName { get; }
        int SortOrder { get; }

		Task<ITagEditorPlugin> CreateAsync(TagEditorContext context);

		bool ValidForTag(ICacheFile cache, CachedTag tag);

	}
}
