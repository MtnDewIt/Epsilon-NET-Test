using System.Collections.Generic;

namespace CacheEditor.RTE
{
    public interface IRteService
    {
        IEnumerable<IRteProvider> Providers { get; }

        IRteTargetCollection GetTargetList(ICacheFile cacheFile);
    }
}
