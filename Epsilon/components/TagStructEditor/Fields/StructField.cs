using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TagStructEditor.Fields
{
    public class StructField : IField
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public IField Parent { get; set; }

        public ObservableCollection<IField> Fields { get; set; }

        public StructField()
        {
            Fields = new ObservableCollection<IField>();
        }

        public void AddChild(IField child)
        {
            child.Parent = this;
            Fields.Add(child);
        }

        public void Accept(IFieldVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Populate(object owner, object value = null)
        {
            if (value == null)
                value = owner;
                
            foreach (var field in Fields)
                field.Populate(value);
        }
    }
}
