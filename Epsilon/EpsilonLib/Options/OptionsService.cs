using Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpsilonLib.Options
{
    [Export(typeof(IOptionsService))]
    class OptionsService : IOptionsService
    {
        public IEnumerable<IOptionsPage> OptionPages { get; }

        [ImportingConstructor]
        public OptionsService([ImportMany] IEnumerable<IOptionsPage> optionPages)
        {
            OptionPages = optionPages;
        }
    }
}
