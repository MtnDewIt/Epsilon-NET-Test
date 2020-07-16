using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpsilonLib.Core
{
    public class ExplicitViewAttribute : Attribute
    {
        public Type ViewType { get; }

        public ExplicitViewAttribute(Type viewType)
        {
            ViewType = viewType;
        }
    }
}
