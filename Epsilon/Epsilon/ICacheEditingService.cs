using System;
using System.Collections.Generic;

namespace Epsilon
{
	public interface ICacheEditingService : IDisposable
	{
        ICacheEditor ActiveEditor { get; set; }
        ISettingsCollection Settings { get; }
        IReadOnlyList<ITagEditorPluginProvider> TagEditorPlugins { get; }
        IEnumerable<ICacheEditorToolProvider> Tools { get; }
		ICacheEditor CreateEditor(ICacheFile cacheFile);
	}
}
