using EpsilonLib.Settings;
using Shared;
using System.Collections.Generic;
using System.Threading.Tasks;
using TagTool.Cache;

namespace CacheEditor
{
    public interface ICacheEditingService
    {
        ICacheEditor ActiveCacheEditor { get; set; }
        ISettingsCollection Settings { get; }
        IReadOnlyList<ITagEditorPluginProvider> TagEditorPlugins { get; }
        IEnumerable<ICacheEditorToolProvider> Tools { get; }
        ICacheEditor CreateEditor(ICacheFile cacheFile);
    }
}
