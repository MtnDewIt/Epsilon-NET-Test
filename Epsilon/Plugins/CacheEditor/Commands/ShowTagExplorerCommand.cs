using EpsilonLib.Commands;

namespace CacheEditor.Commands
{
    [ExportCommand]
    class ShowTagExplorerCommand : CommandDefinition
    {
        public override string Name => "CacheEditor.ToggleTagExplorer";

        public override string DisplayText => "Tags Window";
    }
}
