namespace Epsilon.Commands
{
	[ExportCommand]
    class ShowTagExplorerCommand : CommandDefinition
    {
        public override string Name => "Epsilon.ShowTagExplorer";

        public override string DisplayText => "Tag Explorer";
    }
}
