using EpsilonLib.Logging;
using TagTool.Tags.Definitions;

namespace RenderMethodEditorPlugin.ShaderParameters
{
    class Float2ShaderParameter : GenericShaderParameter
    {
        public float Value1
        {
            get => Property.RealConstants[TemplateIndex].Arg0;
            set
            {
                Property.RealConstants[TemplateIndex].Arg0 = value;
                Logger.LogCommand($"{Logger.ActiveTag.Name}.{Logger.ActiveTag.Group}", Name,
                    Logger.CommandEvent.CommandType.SetField,
                    $"SetArgument {Name} {value} {Property.RealConstants[TemplateIndex].Arg1}");

                NotifyValueChanged();
            }
        }

        public float Value2
        {
            get => Property.RealConstants[TemplateIndex].Arg1;
            set
            {
                Property.RealConstants[TemplateIndex].Arg1 = value;
                Logger.LogCommand($"{Logger.ActiveTag.Name}.{Logger.ActiveTag.Group}", Name,
                    Logger.CommandEvent.CommandType.SetField,
                    $"SetArgument {Name} {Property.RealConstants[TemplateIndex].Arg0} {value}");

                NotifyValueChanged();
            }
        }

        public Float2ShaderParameter(RenderMethod.RenderMethodPostprocessBlock property, string name, string desc, int templateIndex) : base(property, name, desc, templateIndex)
        {
        }

        public override void Refresh()
        {
            NotifyOfPropertyChange(nameof(Value1));
            NotifyOfPropertyChange(nameof(Value2));
        }
    }
}
