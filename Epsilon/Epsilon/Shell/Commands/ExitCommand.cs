using Epsilon.Commands;
using System.Windows.Input;

namespace Epsilon.Shell.Commands
{
    [ExportCommand]
    public class ExitCommand : CommandDefinition
    {
        public override string Name => "Application.Exit";

        public override string DisplayText => "Exit";

        public override KeyShortcut KeyShortcut { get; } = new KeyShortcut(ModifierKeys.Alt, Key.F4);
    }
}
