using Epsilon.Commands;

namespace Epsilon.Components.TagExplorer.Commands
{
    [ExportCommand]
    class ExtractTagCommand : CommandDefinition
    {
        public override string Name => "Epsilon.ExtractTag";

        public override string DisplayText => "Extract...";
    }
}
