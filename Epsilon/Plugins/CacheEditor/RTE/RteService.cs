using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace CacheEditor.RTE
{
    [Export(typeof(IRteService))]
    class RteService : IRteService
    {
        public IEnumerable<IRteProvider> Providers { get; }

        [ImportingConstructor]
        public RteService([ImportMany] IEnumerable<IRteProvider> providers)
        {
            Providers = providers;
        }

        public IRteTargetCollection GetTargetList(ICacheFile cacheFile)
        {
            var source = new AggregateTargetSource();
            foreach (var provider in Providers)
            {
                if (!provider.ValidForCacheFile(cacheFile))
                    continue;

                source.Add(provider);
            }

            return new RteTargetCollection(source);
        }
    }
}
