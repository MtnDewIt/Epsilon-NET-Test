using Epsilon.Commands;
using System.Windows;

namespace Epsilon.Shell.Commands
{
    [ExportCommandHandler]
    class ExitCommandHandler : ICommandHandler<ExitCommand>
    {
        public void ExecuteCommand(Command command)
        {
            Application.Current.MainWindow.Close();
        }

        public void UpdateCommand(Command command) { }
    }
}
