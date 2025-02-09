using System;

namespace Epsilon.Fields
{
    public interface IFieldsValueChangeSink
    {
        event EventHandler<ValueChangedEventArgs> ValueChanged;
    }
}
