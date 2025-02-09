using System.ComponentModel.Composition;

namespace Epsilon.Commands
{
    [ExportCommandHandler]
    class ShowShellWindowCommandHandler : ShowToolWindowCommandHandlerBase<ShowShellWindowCommand>
    {
        [ImportingConstructor]
        public ShowShellWindowCommandHandler(ICacheEditingService editingService) : base(editingService)
        {
        }

        public override string ToolName => CommandShellToolViewModel.ToolName;
    }
}
