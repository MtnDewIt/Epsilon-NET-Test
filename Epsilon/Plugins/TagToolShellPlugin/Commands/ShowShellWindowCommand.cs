using EpsilonLib.Commands;
using System.Windows.Input;

namespace TagToolShellPlugin.Commands
{
    [ExportCommand]
    public class ShowShellWindowCommand : CommandDefinition
    {
        public override string Name => "CacheEditor.ShowShell";

        public override string DisplayText => "Shell";

        public override KeyShortcut KeyShortcut => new KeyShortcut(ModifierKeys.Control, Key.OemQuestion); //or Oem2
    }
}
