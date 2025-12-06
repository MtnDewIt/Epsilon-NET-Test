using EpsilonLib.Logging;
using TagTool.Tags.Definitions;

namespace RenderMethodEditorPlugin.ShaderParameters
{
    class IntegerShaderParameter : GenericShaderParameter
    {
        public uint Value
        {
            get => Property.IntegerConstants[TemplateIndex];
            set
            {
                Property.IntegerConstants[TemplateIndex] = value;
                Logger.LogCommand($"{Logger.ActiveTag.Name}.{Logger.ActiveTag.Group}", Name,
                    Logger.CommandEvent.CommandType.SetField, $"SetArgument {Name} {value}");

                NotifyValueChanged();
            }
        }

        public IntegerShaderParameter(RenderMethod.RenderMethodPostprocessBlock property, string name, string desc, int templateIndex) : base(property, name, desc, templateIndex)
        {
        }

        public override void Refresh()
        {
            NotifyOfPropertyChange(nameof(Value));
        }
    }
}
