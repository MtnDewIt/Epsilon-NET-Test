using System.Collections.Generic;

namespace CacheEditor.RTE
{
    internal class AggregateTargetSource : List<IRteTargetSource>, IRteTargetSource
    {
        public IEnumerable<IRteTarget> FindTargets()
        {
            foreach(var source in this)
            {
                foreach (var target in source.FindTargets())
                    yield return target;
            }
        }
    }
}