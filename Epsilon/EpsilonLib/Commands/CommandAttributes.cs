using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpsilonLib.Commands
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
