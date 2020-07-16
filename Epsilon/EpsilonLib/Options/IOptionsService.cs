using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpsilonLib.Options
{
    public interface IOptionsService
    {
        IEnumerable<IOptionsPage> OptionPages { get; }
    }
}
