using System;
using System.ComponentModel.Composition;

namespace Epsilon.Commands
{
    [AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ExportCommandHandlerAttribute : ExportAttribute
    {
        public ExportCommandHandlerAttribute() : base(typeof(ICommandTarget))
        {

        }
    }

    [AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ExportCommandAttribute : ExportAttribute
    {
        public ExportCommandAttribute() : base(typeof(CommandDefinition))
        {

        }
    }
}
