using TagTool.Tags.Definitions;

namespace Epsilon.ShaderParameters
{
    class SamplerShaderParameter : GenericShaderParameter
    {
        private string temp;

        public string Value
        {
            get => Property.TextureConstants[TemplateIndex].Bitmap.Name;
            set => temp = value;
        }

        public SamplerShaderParameter(RenderMethod.RenderMethodPostprocessBlock property, string name, string desc, int templateIndex) : base(property, name, desc, templateIndex)
        {
        }
    }
}
