using Epsilon.Commands;
using System.ComponentModel.Composition;

namespace Epsilon
{
	[ExportCommandHandler]
    class ShowTagExplorerCommandHandler : ShowToolWindowCommandHandlerBase<ShowTagExplorerCommand>
    {
        [ImportingConstructor]
        public ShowTagExplorerCommandHandler(ICacheEditingService cacheEditingService) : base(cacheEditingService)
        {
        }

        public override string ToolName => TagExplorerViewModel.ToolName;
    }
}
