using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Epsilon.Converters
{
    public class BoolToVisibilityConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
			return (bool)value == true ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }

	public class BoolToNodeVisibilityConverter : MarkupExtension, IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			return (bool)value == true ? Visibility.Visible : Visibility.Hidden;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}

		public override object ProvideValue(IServiceProvider serviceProvider) {
			return this;
		}
	}

    public class TreeNodeSelectedToForegroundConverter : MarkupExtension, IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			return (bool)value == true ? SystemColors.HighlightTextBrushKey : SystemColors.ControlTextBrushKey;
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
		public override object ProvideValue(IServiceProvider serviceProvider) {
			return this;
		}
	}

	public class TreeNodeSelectedToBackgroundConverter : MarkupExtension, IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			return (bool)value == true ? SystemColors.HighlightBrushKey : SystemColors.ControlBrushKey;
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
		public override object ProvideValue(IServiceProvider serviceProvider) {
			return this;
		}
	}

}
