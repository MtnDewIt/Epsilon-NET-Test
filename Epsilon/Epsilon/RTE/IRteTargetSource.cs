using System.Collections.Generic;

namespace Epsilon.RTE
{
	public interface IRteTargetSource
	{
		IEnumerable<IRteTarget> FindTargets();
	}
}
