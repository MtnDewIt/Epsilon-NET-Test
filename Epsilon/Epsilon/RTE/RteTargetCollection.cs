using System;
using System.Collections.Generic;
using System.Linq;
using Epsilon.RTE;

namespace Epsilon.RTE
{
	internal class RteTargetCollection : IRteTargetCollection
	{
		private IRteTargetSource _source;

		private HashSet<IRteTarget> _targets;

		public event Action<IRteTarget> TargetAdded;

		public event Action<IRteTarget> TargetRemoved;

		public RteTargetCollection(IRteTargetSource source) {
			_source = source;
			_targets = new HashSet<IRteTarget>(EqualityComparer<IRteTarget>.Default);
		}

		public void Refresh() {
			IEnumerable<IRteTarget> found = _source.FindTargets();
			HashSet<IRteTarget> foundSet = new HashSet<IRteTarget>(found);
			foreach (IRteTarget target in found) {
				if (_targets.Add(target)) {
					OnTargetAdded(target);
				}
			}
			foreach (IRteTarget target2 in _targets.ToList()) {
				if (!foundSet.Contains(target2)) {
					_targets.Remove(target2);
					OnTargetRemoved(target2);
				}
			}
		}

		private void OnTargetRemoved(IRteTarget target) {
			this.TargetRemoved?.Invoke(target);
		}

		private void OnTargetAdded(IRteTarget target) {
			this.TargetAdded?.Invoke(target);
		}
	}
}
