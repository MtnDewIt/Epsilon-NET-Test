using System;

namespace TagStructEditor.Fields
{
	public class ValueChangedSink : IFieldsValueChangeSink
	{
		public event EventHandler<ValueChangedEventArgs> ValueChanged;
		public void Invoke(ValueChangedEventArgs e) {
			ValueChanged?.Invoke(this, e);
		}
	}

}
