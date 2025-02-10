using System.Collections.Generic;

namespace Epsilon.RTE
{

	public interface IRteService
	{
		IEnumerable<IRteProvider> Providers { get; }
		IRteTargetCollection GetTargetList(ICacheFile cacheFile);
	}

}
