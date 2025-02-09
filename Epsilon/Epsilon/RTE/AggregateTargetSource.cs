using System.Collections.Generic;
using Epsilon.RTE;

namespace Epsilon.RTE
{
	internal class AggregateTargetSource : List<IRteTargetSource>, IRteTargetSource
	{
		public IEnumerable<IRteTarget> FindTargets() {
			using (Enumerator enumerator = GetEnumerator()) {
				while (enumerator.MoveNext()) {
					IRteTargetSource source = enumerator.Current;
					foreach (IRteTarget item in source.FindTargets()) {
						yield return item;
					}
				}
			}
		}
	}
}
