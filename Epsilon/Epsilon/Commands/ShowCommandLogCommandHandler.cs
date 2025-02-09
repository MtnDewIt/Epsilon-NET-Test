using Epsilon.Commands;
using System.ComponentModel.Composition;

namespace Epsilon.Commands
{
    [ExportCommandHandler]
    class ShowCommandLogCommandHandler : ShowToolWindowCommandHandlerBase<ShowCommandLogCommand>
    {
        [ImportingConstructor]
        public ShowCommandLogCommandHandler(ICacheEditingService cacheEditingService) : base(cacheEditingService)
        {
        }

        public override string ToolName => Components.CommandLog.CommandLogViewModel.ToolName;
    }
}
