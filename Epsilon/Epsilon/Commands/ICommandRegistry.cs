using System;
using System.Collections.Generic;

namespace Epsilon.Commands
{
    public interface ICommandRegistry
    {
        IEnumerable<Command> GetCommands();
        Command GetCommand(Type commandType);
    }
}
