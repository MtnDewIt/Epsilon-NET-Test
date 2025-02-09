using System.Collections.Generic;

namespace Epsilon.Options
{
    public interface IOptionsService
    {
        IEnumerable<IOptionsPage> OptionPages { get; }
    }
}
