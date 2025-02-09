using Epsilon.Commands;

namespace Epsilon.Components.TagExplorer.Commands
{
    [ExportCommand]
    class ImportTagCommand : CommandDefinition
    {
        public override string Name => "Epsilon.ImportTag";

        public override string DisplayText => "Import...";
    }
}
