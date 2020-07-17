using EpsilonLib.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Commands
{
    [ExportCommand]
    class ShowAboutCommand : CommandDefinition
    {
        public override string Name => "Help.About";

        public override string DisplayText => "About";
    }
}
