using Epsilon.Commands;

namespace Epsilon.Components.TagTree.Commands
{
    [ExportCommand]
    public class ToggleGroupTagNameViewCommand : CommandDefinition
    {
        public override string Name => "TagTree.ToggleGroupTagNameView";

        public override string DisplayText => "Abbreviated";
    }
}