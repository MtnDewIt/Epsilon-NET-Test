using EpsilonLib.Commands;
using System.Windows.Input;

namespace CacheEditor
{
    [ExportCommand]
    class GlobalTagsCommandList : CommandListDefinition
    {
        public override string Name => "Globals";

        public override string DisplayText => "Globals";

        public override KeyShortcut KeyShortcut { get; }

        public GlobalTagsCommandList()
        {
            KeyShortcut = KeyShortcut.None;
        }

        public GlobalTagsCommandList(KeyShortcut shortcut)
        {
            KeyShortcut = shortcut;
        }
    }
}
