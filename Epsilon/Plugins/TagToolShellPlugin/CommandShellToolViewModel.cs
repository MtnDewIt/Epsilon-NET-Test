using CacheEditor;
using EpsilonLib.Shell;
using System.ComponentModel.Composition;

namespace TagToolShellPlugin
{
    [Export(typeof(ICacheEditorTool))]
    class CommandShellToolViewModel : CacheEditorTool, ICacheEditorTool
    {
        public static readonly string ToolName = "CacheEditor.Tools.CommandShell";

        private CommandShellViewModel _shellViewModel;
        public CommandShellViewModel ShellViewModel
        {
            get => _shellViewModel;
            set => SetAndNotify(ref _shellViewModel, value);
        }

        [ImportingConstructor]
        public CommandShellToolViewModel(ICacheEditor editor)
        {
            DisplayName = "Shell";
            Name = ToolName;
            PreferredLocation = PaneLocation.Bottom;
            PreferredHeight = 300;

            editor.CacheFile.Reloaded += (s, e) =>
            {
                ShellViewModel = CreateShellViewModel(editor);
            };

            ShellViewModel = CreateShellViewModel(editor);
        }

        private CommandShellViewModel CreateShellViewModel(ICacheEditor editor)
        {
            return new CommandShellViewModel(new TagToolCommandShell(editor.CacheFile.Cache));
        }

        public override bool InitialAutoHidden => true;
    }

    [Export(typeof(ICacheEditorToolProvider))]
    class CommandShellToolProvider : ICacheEditorToolProvider
    {
        public int SortOrder => -1;

        public ICacheEditorTool CreateTool(ICacheEditor editor)
        {
            return new CommandShellToolViewModel(editor);
        }

        public bool ValidForEditor(ICacheEditor editor)
        {
            return true;
        }
    }
}
