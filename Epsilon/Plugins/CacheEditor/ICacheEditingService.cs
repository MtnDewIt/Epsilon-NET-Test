using EpsilonLib.Settings;
using Shared;
using System.Collections.Generic;

namespace CacheEditor
{
    public interface ICacheEditingService
    {
        ICacheEditor ActiveCacheEditor { get; set; }
        ISettingsCollection Settings { get; }
        IReadOnlyList<ITagEditorPluginProvider> TagEditorPlugins { get; }
        IEnumerable<ICacheEditorToolProvider> Tools { get; }
    }
}
