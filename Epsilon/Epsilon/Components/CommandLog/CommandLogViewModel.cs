using Epsilon.Commands;
using Epsilon.Logging;
using System.Windows.Input;

namespace Epsilon.Components.CommandLog
{
	class CommandLogViewModel : CacheEditorTool
    {
        public const string ToolName = "Epsilon.CommandLog";
        private ICacheEditor _editor;

        public CommandLogViewModel(ICacheEditor editor)
        {
            _editor = editor;

            Name = ToolName;
            DisplayName = "Command Log";
            PreferredLocation = Epsilon.Shell.PaneLocation.Right;
            PreferredWidth = 450;
            IsVisible = true;
        }

        public class CommandLogControls
        {
            public ICommand ClearCommandLogCommand { get; set; }

            public CommandLogControls()
            {
                ClearCommandLogCommand = new DelegateCommand(() => Logger.ClearCommandLog());
            }
        }

        public override bool InitialAutoHidden => true;
    }
}
