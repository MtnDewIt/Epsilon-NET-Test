using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Epsilon.Options
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
