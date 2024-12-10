﻿using System;
using CacheEditor.RTE;

namespace CacheEditor.RTE
{

	public interface IRteTargetCollection
	{
		event Action<IRteTarget> TargetAdded;

		event Action<IRteTarget> TargetRemoved;

		void Refresh();
	}


}
