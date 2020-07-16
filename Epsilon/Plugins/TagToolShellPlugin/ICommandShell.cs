using System;
using System.Threading.Tasks;

namespace TagToolShellPlugin
{
    public interface ICommandShell
    {
        string ContextDisplayName { get; }
        event EventHandler ContextChanged;

        event EventHandler<CommandShellOutputEventArgs> OutputReceived;

        Task ExecuteAsync(string commandLine);
    }
}
