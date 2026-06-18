using CacheEditor.RTE;
using EpsilonLib.Settings;
using Shared;
using System;
using System.Collections.Generic;

namespace CacheEditor
{
    public interface ICacheEditingService : IDisposable
    {
        IRteService Rte { get; }
        ICacheEditor ActiveCacheEditor { get; set; }
        ISettingsCollection Settings { get; }
        IFavoritesService Favorites { get; }
        IReadOnlyList<ITagEditorPluginProvider> TagEditorPlugins { get; }
        IEnumerable<ICacheEditorToolProvider> Tools { get; }
        ICacheEditor CreateEditor(ICacheFile cacheFile);
    }
}
