using EpsilonLib.Logging;
using TagTool.Tags.Definitions;

namespace RenderMethodEditorPlugin.ShaderParameters
{
    class ArgbColorShaderParameter : GenericShaderParameter
    {
        public float Alpha
        {
            get => Property.RealConstants[TemplateIndex].Arg0;
            set
            {
                Property.RealConstants[TemplateIndex].Arg0 = value;
                Logger.LogCommand($"{Logger.ActiveTag.Name}.{Logger.ActiveTag.Group}", Name,
                    Logger.CommandEvent.CommandType.SetField,
                    $"SetArgument {Name} {value} {Property.RealConstants[TemplateIndex].Arg1} " +
                    $"{Property.RealConstants[TemplateIndex].Arg2} {Property.RealConstants[TemplateIndex].Arg3}");

                NotifyValueChanged();
            }
        }

        public float Red
        {
            get => Property.RealConstants[TemplateIndex].Arg1;
            set
            {
                Property.RealConstants[TemplateIndex].Arg1 = value;
                Logger.LogCommand($"{Logger.ActiveTag.Name}.{Logger.ActiveTag.Group}", Name,
                    Logger.CommandEvent.CommandType.SetField,
                    $"SetArgument {Name} {Property.RealConstants[TemplateIndex].Arg0} {value} " +
                    $"{Property.RealConstants[TemplateIndex].Arg2} {Property.RealConstants[TemplateIndex].Arg3}");

                NotifyValueChanged();
            }
        }

        public float Green
        {
            get => Property.RealConstants[TemplateIndex].Arg2;
            set
            {
                Property.RealConstants[TemplateIndex].Arg2 = value;
                Logger.LogCommand($"{Logger.ActiveTag.Name}.{Logger.ActiveTag.Group}", Name,
                    Logger.CommandEvent.CommandType.SetField,
                    $"SetArgument {Name} {Property.RealConstants[TemplateIndex].Arg0} {Property.RealConstants[TemplateIndex].Arg1} " +
                    $"{value} {Property.RealConstants[TemplateIndex].Arg3}");

                NotifyValueChanged();
            }
        }

        public float Blue
        {
            get => Property.RealConstants[TemplateIndex].Arg3;
            set
            {
                Property.RealConstants[TemplateIndex].Arg3 = value;
                Logger.LogCommand($"{Logger.ActiveTag.Name}.{Logger.ActiveTag.Group}", Name,
                    Logger.CommandEvent.CommandType.SetField,
                    $"SetArgument {Name} {Property.RealConstants[TemplateIndex].Arg0} {Property.RealConstants[TemplateIndex].Arg1} " +
                    $"{Property.RealConstants[TemplateIndex].Arg2} {value}");

                NotifyValueChanged();
            }
        }

        public ArgbColorShaderParameter(RenderMethod.RenderMethodPostprocessBlock property, string name, string desc, int templateIndex) : base(property, name, desc, templateIndex)
        {
        }

        public override void Refresh()
        {
            NotifyOfPropertyChange(nameof(Alpha));
            NotifyOfPropertyChange(nameof(Red));
            NotifyOfPropertyChange(nameof(Green));
            NotifyOfPropertyChange(nameof(Blue));
        }
    }
}
