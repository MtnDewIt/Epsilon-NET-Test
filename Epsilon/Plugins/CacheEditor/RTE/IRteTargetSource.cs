using System.Collections.Generic;

namespace CacheEditor.RTE
{
    public interface IRteTargetSource
    {
        IEnumerable<IRteTarget> FindTargets();
    }
}
