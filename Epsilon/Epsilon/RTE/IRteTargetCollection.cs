using System;

namespace Epsilon.RTE
{

	public interface IRteTargetCollection
	{
		event Action<IRteTarget> TargetAdded;

		event Action<IRteTarget> TargetRemoved;

		void Refresh();
	}


}
