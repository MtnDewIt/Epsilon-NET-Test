using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagTool.Cache;

namespace CacheEditor.RTE
{
    public interface IRteTarget : IEquatable<IRteTarget>
    {
        IRteProvider Provider { get; }

        object Id { get; }

        string DisplayName { get; }
    }
}
