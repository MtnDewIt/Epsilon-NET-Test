using System.Collections.Generic;
using Epsilon.RTE;

namespace Epsilon.RTE
{
	public interface IRteTargetSource
	{
		IEnumerable<IRteTarget> FindTargets();
	}
}
