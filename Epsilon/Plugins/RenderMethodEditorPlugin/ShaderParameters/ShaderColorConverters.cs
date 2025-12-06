using ColorPicker.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace RenderMethodEditorPlugin.ShaderParameters
{
    public abstract class MultiBinderBase : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var targetProvider = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));
            var targetElement = targetProvider.TargetObject as FrameworkElement;
            var targetProperty = targetProvider.TargetProperty as DependencyProperty;

            if (targetElement != null && targetProperty != null)
            {
                // make sure that if the binding context changes then the binding gets updated.
                targetElement.DataContextChanged += (sender, args) => ApplyBinding(targetElement, targetProperty, args.NewValue);

                var binding = ApplyBinding(targetElement, targetProperty, targetElement.DataContext);
                return binding.ProvideValue(serviceProvider);
            }

            return Binding.DoNothing;
        }

        private BindingBase ApplyBinding(DependencyObject target, DependencyProperty property, object source)
        {
            BindingOperations.ClearBinding(target, property);
            return Bind(source);
        }

        public abstract MultiBinding Bind(object source);
    }

    public class ColorShaderParameterBinder : MultiBinderBase, IMultiValueConverter
    {
        public override MultiBinding Bind(object source)
        {
            var binding = new MultiBinding() { Mode = BindingMode.TwoWay, Converter = this };
            binding.Bindings.Add(new Binding(nameof(ColorShaderParameter.Red)) { Mode = BindingMode.TwoWay, Source = source, });
            binding.Bindings.Add(new Binding(nameof(ColorShaderParameter.Green)) { Mode = BindingMode.TwoWay, Source = source, });
            binding.Bindings.Add(new Binding(nameof(ColorShaderParameter.Blue)) { Mode = BindingMode.TwoWay, Source = source, });
            return binding;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var cs = new ColorState();
            cs.SetARGB(1.0, (float)values[0], (float)values[1], (float)values[2]);
            return cs;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            var cs = (ColorState)value;
            return new object[] { (float)cs.RGB_R, (float)cs.RGB_G, (float)cs.RGB_B };
        }
    }

    public class ArgbColorShaderParameterBinder : MultiBinderBase, IMultiValueConverter
    {
        public override MultiBinding Bind(object source)
        {
            var binding = new MultiBinding() { Mode = BindingMode.TwoWay, Converter = this };
            binding.Bindings.Add(new Binding(nameof(ArgbColorShaderParameter.Alpha)) { Mode = BindingMode.TwoWay, Source = source, });
            binding.Bindings.Add(new Binding(nameof(ArgbColorShaderParameter.Red)) { Mode = BindingMode.TwoWay, Source = source, });
            binding.Bindings.Add(new Binding(nameof(ArgbColorShaderParameter.Green)) { Mode = BindingMode.TwoWay, Source = source, });
            binding.Bindings.Add(new Binding(nameof(ArgbColorShaderParameter.Blue)) { Mode = BindingMode.TwoWay, Source = source, });
            return binding;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var cs = new ColorState();
            cs.SetARGB((float)values[0], (float)values[1], (float)values[2], (float)values[3]);
            return cs;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            var cs = (ColorState)value;
            return new object[] { (float)cs.A, (float)cs.RGB_R, (float)cs.RGB_G, (float)cs.RGB_B };
        }
    }
}
