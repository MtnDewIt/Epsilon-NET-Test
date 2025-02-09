using System;

namespace Epsilon.Fields
{
	public class ValueChangedSink : IFieldsValueChangeSink
	{
		public event EventHandler<ValueChangedEventArgs> ValueChanged;
		public void Invoke(ValueChangedEventArgs e) {
			ValueChanged?.Invoke(this, e);
		}
	}

}
