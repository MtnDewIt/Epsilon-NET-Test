using EpsilonLib.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheEditor.Components.TagExplorer.Commands
{
    [ExportCommand]
    class ImportTagCommand : CommandDefinition
    {
        public override string Name => "CacheEditor.ImportTag";

        public override string DisplayText => "Import...";
    }
}
