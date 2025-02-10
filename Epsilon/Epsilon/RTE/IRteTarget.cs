using System;

namespace Epsilon.RTE
{

	public interface IRteTarget : IEquatable<IRteTarget>
	{
		IRteProvider Provider { get; }

		object Id { get; }

		string DisplayName { get; }
	}

}
