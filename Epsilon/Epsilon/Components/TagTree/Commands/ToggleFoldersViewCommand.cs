using Epsilon.Commands;

namespace Epsilon.Components.TagTree.Commands
{
    [ExportCommand]
    public class ToggleFoldersViewCommand : CommandDefinition
    {
        public override string Name => "TagTree.ToggleFoldersView";

        public override string DisplayText => "Folders";
    }
}