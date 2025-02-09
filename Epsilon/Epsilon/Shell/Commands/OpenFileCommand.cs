using Epsilon.Commands;
using System.Windows.Input;

namespace Epsilon.Shell.Commands
{
    [ExportCommand]
    public class OpenFileCommand : CommandDefinition
    {
        public override string Name => "File.OpenFile";

        public override string DisplayText => "Open...";

        public override KeyShortcut KeyShortcut => new KeyShortcut(ModifierKeys.Control, Key.O);
    }
}
