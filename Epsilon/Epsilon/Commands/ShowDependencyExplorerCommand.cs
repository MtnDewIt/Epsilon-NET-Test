using Epsilon.Commands;

namespace Epsilon.Commands
{
    [ExportCommand]
    class ShowDependencyExplorerCommand : CommandDefinition
    {
        public override string Name => "Epsilon.ShowDependencyExplorer";

        public override string DisplayText => "Dependency Explorer";
    }
}
