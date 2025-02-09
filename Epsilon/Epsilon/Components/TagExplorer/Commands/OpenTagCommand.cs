using Epsilon.Commands;

namespace Epsilon.Components.TagExplorer.Commands
{
    [ExportCommand]
    class OpenTagCommand : CommandDefinition
    {
        public override string Name => "Epsilon.OpenTag";

        public override string DisplayText => "Open Tag";
    }
}
