using CacheEditor;
using EpsilonLib.Commands;
using System.ComponentModel.Composition;

namespace TagToolShellPlugin.Commands
{
    [ExportCommandHandler]
    class ShowShellWindowCommandHandler : ICommandHandler<ShowShellWindowCommand>
    {
        private readonly ICacheEditingService _editingService;

        [ImportingConstructor]
        public ShowShellWindowCommandHandler(ICacheEditingService editingService)
        {
            _editingService = editingService;
        }

        private ICacheEditorTool GetTool() 
            => _editingService.ActiveCacheEditor?.GetTool(CommandShellToolViewModel.ToolName);

        public void ExecuteCommand(Command command)
        {
            var tool = GetTool();
            tool.Show(!tool.IsVisible, true);
        }

        public void UpdateCommand(Command command)
        {
            var tool = GetTool();
            command.IsChecked = tool != null && tool.IsVisible;
            command.IsVisible = tool != null;
        }
    }
}
