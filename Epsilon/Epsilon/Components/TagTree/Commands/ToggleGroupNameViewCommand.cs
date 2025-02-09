using Epsilon.Commands;

namespace Epsilon.Components.TagTree.Commands
{
    [ExportCommand]
    public class ToggleGroupNameViewCommand : CommandDefinition
    {
        public override string Name => "TagTree.ToggleGroupNameView";

        public override string DisplayText => "Full Name";
    }
}