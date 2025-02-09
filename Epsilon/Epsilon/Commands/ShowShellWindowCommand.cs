namespace Epsilon.Commands
{
    [ExportCommand]
    public class ShowShellWindowCommand : CommandDefinition
    {
        public override string Name => "Epsilon.ShowShell";

        public override string DisplayText => "Shell";
    }
}
