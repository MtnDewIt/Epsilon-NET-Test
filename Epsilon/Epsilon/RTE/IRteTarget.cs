using System;
using Epsilon.RTE;

namespace Epsilon.RTE
{

	public interface IRteTarget : IEquatable<IRteTarget>
	{
		IRteProvider Provider { get; }

		object Id { get; }

		string DisplayName { get; }
	}

}
