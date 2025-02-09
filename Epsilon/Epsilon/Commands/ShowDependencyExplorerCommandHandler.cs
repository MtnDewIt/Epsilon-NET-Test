using Epsilon.Commands;
using System.ComponentModel.Composition;

namespace Epsilon.Commands
{
    [ExportCommandHandler]
    class ShowDependnecyExplorerCommandHandler : ShowToolWindowCommandHandlerBase<ShowDependencyExplorerCommand>
    {
        [ImportingConstructor]
        public ShowDependnecyExplorerCommandHandler(ICacheEditingService cacheEditingService) : base(cacheEditingService)
        {
        }

        public override string ToolName => Components.DependencyExplorer.DependencyExplorerViewModel.ToolName;
    }
}
