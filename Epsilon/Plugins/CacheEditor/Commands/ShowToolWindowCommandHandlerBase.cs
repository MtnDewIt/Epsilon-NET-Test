using EpsilonLib.Commands;
using System.ComponentModel.Composition;

namespace CacheEditor.Commands
{
    public abstract class ShowToolWindowCommandHandlerBase<T> : ICommandHandler<T> where T : CommandDefinition
    {
        private ICacheEditingService _cacheEditingService;

        public abstract string ToolName { get; }

        public ShowToolWindowCommandHandlerBase(ICacheEditingService cacheEditingService)
        {
            _cacheEditingService = cacheEditingService;
        }

        public ICacheEditor CurrentEditor => _cacheEditingService.ActiveCacheEditor;

        public void ExecuteCommand(Command command)
        {
            var tool = CurrentEditor.GetTool(ToolName);
            tool.Show(!tool.IsVisible, true);
        }

        public void UpdateCommand(Command command)
        {
            command.IsVisible = _cacheEditingService.ActiveCacheEditor != null;
            if (_cacheEditingService.ActiveCacheEditor != null)
            {
                command.IsEnabled = _cacheEditingService.ActiveCacheEditor != null;
                command.IsChecked = _cacheEditingService.ActiveCacheEditor.GetTool(ToolName).IsVisible;
            }
        }
    }
}
