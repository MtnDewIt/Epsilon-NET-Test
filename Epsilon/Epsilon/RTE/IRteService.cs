using System.Collections.Generic;
using Epsilon;
using Epsilon.RTE;

namespace Epsilon.RTE
{

	public interface IRteService
	{
		IEnumerable<IRteProvider> Providers { get; }
		IRteTargetCollection GetTargetList(ICacheFile cacheFile);
	}

}
