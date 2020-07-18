using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheEditor.TagEditing.Messages
{
    public class DefinitionDataChangedEvent
    {
        public object NewData { get; }

        public DefinitionDataChangedEvent(object data)
        {
            NewData = data;
        }
    }
}
