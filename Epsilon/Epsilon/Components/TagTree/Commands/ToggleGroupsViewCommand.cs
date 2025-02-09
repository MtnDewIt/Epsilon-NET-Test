using Epsilon.Commands;

namespace Epsilon.Components.TagTree.Commands
{
    [ExportCommand]
    public class ToggleGroupsViewCommand : CommandDefinition
    {
        public override string Name => "TagTree.ToggleGroupsView";

        public override string DisplayText => "Groups";
    }
}