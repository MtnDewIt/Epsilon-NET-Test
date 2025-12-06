using CacheEditor.RTE.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Threading;

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

        public IRteSession CreateSession(ICacheFile cacheFile)
        {
            var source = new AggregateTargetSource();
            foreach (var provider in Providers)
            {
                if (!provider.ValidForCacheFile(cacheFile))
                    continue;

                source.Add(provider);
            }

            return new RteSession(cacheFile, source);
        }
    }

    class RteSession : IRteSession
    {
        private ICacheFile _cacheFile;
        private TargetListModel _targetList;
        private DispatcherTimer _timer;

        public RteSession(ICacheFile cacheFile, IRteTargetSource source)
        {
            _cacheFile = cacheFile;
            _targetList = new TargetListModel(new RteTargetCollection(source));
            _timer = new DispatcherTimer(TimeSpan.FromSeconds(5), DispatcherPriority.Background, OnRefreshTimerTicked, Dispatcher.CurrentDispatcher);
            _timer.Start();
        }

        private void OnRefreshTimerTicked(object sender, EventArgs e)
        {
            _targetList.Refresh();
        }

        public IRteTargetList TargetList => _targetList;

        public void Dispose()
        {
            _timer?.Stop();
            _timer = null;
            _cacheFile = null;
            _targetList = null;
        }
    }
}
