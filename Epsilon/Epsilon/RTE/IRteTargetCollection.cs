using System;
using Epsilon.RTE;

namespace Epsilon.RTE
{

	public interface IRteTargetCollection
	{
		event Action<IRteTarget> TargetAdded;

		event Action<IRteTarget> TargetRemoved;

		void Refresh();
	}


}
