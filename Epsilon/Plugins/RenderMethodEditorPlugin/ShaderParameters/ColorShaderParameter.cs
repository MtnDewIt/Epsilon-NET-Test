using EpsilonLib.Logging;
using TagTool.Tags.Definitions;

namespace RenderMethodEditorPlugin.ShaderParameters
{
    class ColorShaderParameter : GenericShaderParameter
    {
        public float Red
        {
            get => Property.RealConstants[TemplateIndex].Arg0;
            set
            {
                Property.RealConstants[TemplateIndex].Arg0 = value;
                Logger.LogCommand($"{Logger.ActiveTag.Name}.{Logger.ActiveTag.Group}", Name,
                    Logger.CommandEvent.CommandType.SetField,
                    $"SetArgument {Name} {value} {Property.RealConstants[TemplateIndex].Arg1} " +
                    $"{Property.RealConstants[TemplateIndex].Arg2}");

                NotifyValueChanged();
            }
        }

        public float Green
        {
            get => Property.RealConstants[TemplateIndex].Arg1;
            set
            {
                Property.RealConstants[TemplateIndex].Arg1 = value;
                Logger.LogCommand($"{Logger.ActiveTag.Name}.{Logger.ActiveTag.Group}", Name,
                    Logger.CommandEvent.CommandType.SetField,
                    $"SetArgument {Name} {Property.RealConstants[TemplateIndex].Arg0} {value} " +
                    $"{Property.RealConstants[TemplateIndex].Arg2}");

                NotifyValueChanged();
            }
        }

        public float Blue
        {
            get => Property.RealConstants[TemplateIndex].Arg2;
            set
            {
                Property.RealConstants[TemplateIndex].Arg2 = value;
                Logger.LogCommand($"{Logger.ActiveTag.Name}.{Logger.ActiveTag.Group}", Name,
                    Logger.CommandEvent.CommandType.SetField,
                    $"SetArgument {Name} {Property.RealConstants[TemplateIndex].Arg0} {Property.RealConstants[TemplateIndex].Arg1} " +
                    $"{value}");

                NotifyValueChanged();
            }
        }

        public ColorShaderParameter(RenderMethod.RenderMethodPostprocessBlock property, string name, string desc, int templateIndex) : base(property, name, desc, templateIndex)
        {
        }
        public override void Refresh()
        {
            NotifyOfPropertyChange(nameof(Red));
            NotifyOfPropertyChange(nameof(Green));
            NotifyOfPropertyChange(nameof(Blue));
        }
    }
}
