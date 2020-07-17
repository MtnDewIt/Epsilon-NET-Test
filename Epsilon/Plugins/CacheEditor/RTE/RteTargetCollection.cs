using System;
using System.Collections.Generic;
using System.Linq;

namespace CacheEditor.RTE
{
    class RteTargetCollection : IRteTargetCollection
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
            var found = _source.FindTargets();

            var foundSet = new HashSet<IRteTarget>(found);

            foreach (var target in found)
            {
                if(_targets.Add(target))
                    OnTargetAdded(target);
            }

            foreach (var target in _targets.ToList())
            {
                if (!foundSet.Contains(target))
                {
                    _targets.Remove(target);
                    OnTargetRemoved(target);
                }    
            }
        }

        private void OnTargetRemoved(IRteTarget target)
        {
            TargetRemoved?.Invoke(target);
        }

        private void OnTargetAdded(IRteTarget target)
        {
            TargetAdded?.Invoke(target);
        }
    }
}
