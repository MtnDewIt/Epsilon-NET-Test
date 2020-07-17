using CacheEditor.Commands;
using EpsilonLib.Commands;
using System.ComponentModel.Composition;

namespace CacheEditor
{
    [ExportCommandHandler]
    class ShowTagExplorerCommandHandler : ICommandHandler<ShowTagExplorerCommand>
    {
        private ICacheEditingService _cacheEditingService;

        [ImportingConstructor]
        public ShowTagExplorerCommandHandler(ICacheEditingService cacheEditingService)
        {
            _cacheEditingService = cacheEditingService;
        }

        public ICacheEditor CurrentEditor => _cacheEditingService.ActiveCacheEditor;

        public void ExecuteCommand(Command command)
        {
            var tool = CurrentEditor.GetTool(TagExplorerViewModel.ToolName);
            tool.Show(!tool.IsVisible, true);
        }

        public void UpdateCommand(Command command)
        {
            command.IsVisible = _cacheEditingService.ActiveCacheEditor != null;
            if (_cacheEditingService.ActiveCacheEditor != null)
            {
                command.IsEnabled = _cacheEditingService.ActiveCacheEditor != null;
                command.IsChecked = _cacheEditingService.ActiveCacheEditor.GetTool(TagExplorerViewModel.ToolName).IsVisible;
            }
        }
    }
}
