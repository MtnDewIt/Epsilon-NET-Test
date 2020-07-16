using System;
using System.Collections.Generic;
using System.ComponentModel;
using TagStructEditor.Fields;

namespace DefinitionEditor
{
    class BlockEditorViewModel : IField
    {
        public BlockField Field { get; set; }

        public IField Template => Field.Template;
        public List<string> Elements { get; set; }
        public IField Parent { get; set; }

        public BlockEditorViewModel(BlockField field)
        {
            Field = field;
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void Populate(object owner, object value = null)
        {
            throw new NotImplementedException();
        }

        public void Accept(IFieldVisitor visitor)
        {
            Field.Accept(visitor);
        }
    }
}
