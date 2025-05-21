using System;
using System.Collections.Generic;
using System.Linq;

namespace CacheEditor.RTE
{
    internal class RteTargetCollection : IRteTargetCollection
    {
        private IRteTargetSource _source;
        private HashSet<IRteTarget> _targets;

        public event Action<IRteTarget> TargetAdded;

        public event Action<IRteTarget> TargetRemoved;

        public RteTargetCollection(IRteTargetSource source)
        {
            _source = source;
            _targets = new HashSet<IRteTarget>(EqualityComparer<IRteTarget>.Default);
        }

        public void Refresh()
        {
            IEnumerable<IRteTarget> targets = _source.FindTargets();

            HashSet<IRteTarget> rteTargetSet = new HashSet<IRteTarget>(targets);

            foreach (IRteTarget target in targets)
            {
                if (_targets.Add(target))
                    OnTargetAdded(target);
            }

            foreach (IRteTarget target in _targets.ToList())
            {
                if (!rteTargetSet.Contains(target))
                {
                    _targets.Remove(target);
                    OnTargetRemoved(target);
                }
            }
        }

        private void OnTargetRemoved(IRteTarget target) => TargetRemoved?.Invoke(target);

        private void OnTargetAdded(IRteTarget target) => TargetAdded?.Invoke(target);
    }
}
