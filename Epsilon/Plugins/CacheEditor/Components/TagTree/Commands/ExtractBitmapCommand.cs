using EpsilonLib.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheEditor.Components.TagTree.Commands
{
    [ExportCommand]
    public class ExtractBitmapCommand : CommandDefinition
    {
        public override string Name => "TagTree.ExtractBitmap";

        public override string DisplayText => "Extract Bitmap...";
    }
}
