using System;

namespace TagStructEditor.Fields
{
    public interface IFieldsValueChangeSink
    {
        event EventHandler<ValueChangedEventArgs> ValueChanged;
    }
}
