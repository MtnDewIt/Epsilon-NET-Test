using Epsilon.Commands;

namespace Epsilon.Components.TagTree.Commands
{
    [ExportCommand]
    public class ExtractBitmapCommand : CommandDefinition
    {
        public override string Name => "TagTree.ExtractBitmap";

        public override string DisplayText => "Extract Bitmap...";
    }
}
