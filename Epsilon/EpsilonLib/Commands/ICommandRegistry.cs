using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpsilonLib.Commands
{
    public interface ICommandRegistry
    {
        IEnumerable<Command> GetCommands();
        Command GetCommand(Type commandType);
    }
}
