using Epsilon.Commands;

namespace Epsilon.Components.TagExplorer.Commands
{
    [ExportCommand]
    class DuplicateTagCommand : CommandDefinition
    {
        public override string Name => "Epsilon.DuplicateTag";

        public override string DisplayText => "Duplicate...";
    }
}
