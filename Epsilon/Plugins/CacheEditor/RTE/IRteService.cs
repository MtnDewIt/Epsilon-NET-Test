using CacheEditor.RTE.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CacheEditor.RTE
{
    public interface IRteService
    {
        IEnumerable<IRteProvider> Providers { get; }

        IRteSession CreateSession(ICacheFile cacheFile);
    }

    public interface IRteSession : IDisposable
    {
        IRteTargetList TargetList { get; }
    }
}
