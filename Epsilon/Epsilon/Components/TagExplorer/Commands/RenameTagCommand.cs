using Epsilon.Commands;

namespace Epsilon.Components.TagExplorer.Commands
{
    [ExportCommand]
    class RenameTagCommand : CommandDefinition
    {
        public override string Name => "Epsilon.RenameTag";

        public override string DisplayText => "Rename...";
    }
}
