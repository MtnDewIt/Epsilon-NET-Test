namespace Epsilon.Commands
{
	[ExportCommand]
    class ShowCommandLogCommand : CommandDefinition
    {
        public override string Name => "Epsilon.ShowCommandLog";

        public override string DisplayText => "Command Log";
    }
}
