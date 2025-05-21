using System.Collections.Generic;

namespace CacheEditor.RTE
{
    internal class AggregateTargetSource : List<IRteTargetSource>, IRteTargetSource
    {
        public IEnumerable<IRteTarget> FindTargets()
        {
            foreach (IRteTargetSource source in (List<IRteTargetSource>)this)
            {
                foreach (IRteTarget target in source.FindTargets())
                    yield return target;
            }
        }
    }
}
