using System;

namespace Epsilon.Fields
{
    public interface IFieldFactory
    {
        StructField CreateStruct(Type structType);
        ValueField CreateValueField(ValueFieldInfo info);
    }
}
