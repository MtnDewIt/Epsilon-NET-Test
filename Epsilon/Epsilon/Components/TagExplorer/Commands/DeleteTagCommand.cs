using Epsilon.Commands;

namespace Epsilon.Components.TagExplorer.Commands
{
    [ExportCommand]
    class DeleteTagCommand : CommandDefinition
    {
        public override string Name => "Epsilon.DeleteTag";

        public override string DisplayText => "Delete";
    }
}
