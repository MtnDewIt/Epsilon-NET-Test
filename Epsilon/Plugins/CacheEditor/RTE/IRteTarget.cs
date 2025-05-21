using System;

namespace CacheEditor.RTE
{
    public interface IRteTarget : IEquatable<IRteTarget>
    {
        IRteProvider Provider { get; }

        object Id { get; }

        string DisplayName { get; }
    }
}
