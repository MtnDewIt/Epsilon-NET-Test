using System;
using System.Collections.Generic;
using TagTool.Cache;
using TagTool.Serialization;

namespace CacheEditor.RTE
{
    public interface IRteService
    {
        IEnumerable<IRteProvider> Providers { get; }

        IRteTargetCollection GetTargetList(ICacheFile cacheFile);
    }
}
