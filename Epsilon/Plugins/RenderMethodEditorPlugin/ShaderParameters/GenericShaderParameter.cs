using Stylet;
using System;
using TagTool.Tags.Definitions;

namespace RenderMethodEditorPlugin.ShaderParameters
{
    abstract class GenericShaderParameter : PropertyChangedBase
    {
        public RenderMethod.RenderMethodPostprocessBlock Property;
        public string Name { get; set; }
        public string PrettyName { get; set; }
        public string Description { get; set; }
        public int TemplateIndex;

        public event EventHandler ValueChanged;

        public GenericShaderParameter(RenderMethod.RenderMethodPostprocessBlock property, string name, string desc, int templateIndex)
        {
            Name = name;
            PrettyName = ShaderStringConverter.ToPrettyFormat(name);
            TemplateIndex = templateIndex;
            Property = property;
            Description = desc ?? "No description available";
        }

        protected virtual void NotifyValueChanged()
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        public abstract void Refresh();
    }
}
