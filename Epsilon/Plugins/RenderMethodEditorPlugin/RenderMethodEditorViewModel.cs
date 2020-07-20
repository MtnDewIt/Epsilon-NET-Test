using CacheEditor;
using CacheEditor.TagEditing;
using CacheEditor.TagEditing.Messages;
using HaloShaderGenerator.Generator;
using RenderMethodEditorPlugin.ShaderMethods;
using RenderMethodEditorPlugin.ShaderMethods.Shader;
using RenderMethodEditorPlugin.ShaderParameters;
using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagTool.Cache;
using TagTool.Tags.Definitions;

namespace RenderMethodEditorPlugin
{
    class RenderMethodEditorViewModel : TagEditorPluginBase
    {
        private RenderMethod _renderMethod;
        private GameCache _cache;
        private RenderMethodTemplate _renderMethodTemplate;
        private RenderMethod.ShaderProperty _shaderProperty;

        public ObservableCollection<BooleanConstant> BooleanConstants {get;} = new ObservableCollection<BooleanConstant>();
        public ObservableCollection<Method> ShaderMethods { get; } = new ObservableCollection<Method>();

        public ObservableCollection<GenericShaderParameter> ShaderParameters { get; } = new ObservableCollection<GenericShaderParameter>();

        public RenderMethodEditorViewModel(GameCache cache, RenderMethod renderMethod)
        {
            _cache = cache;
            _renderMethod = renderMethod;
            _shaderProperty = _renderMethod.ShaderProperties[0];
            var templateName = _shaderProperty.Template.Name;

            if (templateName == null)
                return;

            var templateTypeSplitIndex = 1;
            if (templateName.Contains("ms30"))
                templateTypeSplitIndex = 2;

            string templateType = templateName.Split('\\')[templateTypeSplitIndex];

            MethodParser methodParser;
            IShaderGenerator generator;

            if (templateType == "shader_templates")
            {
                using (var stream = _cache.OpenCacheRead())
                {
                    _renderMethodTemplate = cache.Deserialize<RenderMethodTemplate>(stream, _shaderProperty.Template);
                }

                switch (templateType)
                {
                    case "shader_templates":
                        methodParser = new ShaderMethod();
                        break;
                    default:
                        return;
                }

                generator = methodParser.BuildShaderGenerator(_renderMethod.RenderMethodDefinitionOptionIndices);

                for(int i = 0; i < _renderMethod.RenderMethodDefinitionOptionIndices.Count; i++)
                {
                    var optionIndex = _renderMethod.RenderMethodDefinitionOptionIndices[i].OptionIndex;
                    var methodInfo = methodParser.ParseMethod(i, optionIndex, generator);
                    if (methodInfo != null)
                        ShaderMethods.Add(methodInfo);
                }

                bool useRotation = false;
                if (generator is HaloShaderGenerator.Shader.ShaderGenerator && _renderMethod.RenderMethodDefinitionOptionIndices[9].OptionIndex == 3)
                    useRotation = true;

                ShaderParameters = ShaderParameterFactory.BuildShaderParameters(cache, generator.GetPixelShaderParameters(), _shaderProperty, _renderMethodTemplate, useRotation);

                //ParseBooleanArguments();
            }
            else
                return; // only shader templates, particle templates

            
        }

        // get boolean values from existing tag data

        private void ParseBooleanArguments()
        {
            for(int i = 0; i < _renderMethodTemplate.BooleanParameterNames.Count; i++)
            {
                string name = _cache.StringTable.GetString(_renderMethodTemplate.BooleanParameterNames[i].Name);
                BooleanConstants.Add(new BooleanConstant(_shaderProperty, name, FindDescriptionFromName(name), i));
            }
        }

        private string FindDescriptionFromName(string shaderArgName)
        {
            if(ShaderArgumentsDescription.ArgsDescription.ContainsKey(shaderArgName))
                return ShaderArgumentsDescription.ArgsDescription[shaderArgName];
            else
                return "Missing description";
        }

        public void Test()
        {
            PostMessage(this, new DefinitionDataChangedEvent(_renderMethod));
        }
    }

    class ShaderConstant
    {
        public RenderMethod.ShaderProperty Property;
        public string Name { get; set; }
        public string Description { get; set; }
        public int TemplateIndex;
        public ShaderConstant(RenderMethod.ShaderProperty property, string name, string desc, int templateIndex)
        {
            Name = ShaderStringConverter.ToPrettyFormat(name);
            TemplateIndex = templateIndex;
            Property = property;
            Description = desc;
        }
    }

    class BooleanConstant : ShaderConstant
    {
        public bool Value
        {
            get => (((int)(Property.BooleanConstants) >> TemplateIndex) & 1) == 1;
            set
            {
                if(value == true)
                    Property.BooleanConstants = (uint)((int)Property.BooleanConstants | (1 << TemplateIndex));
                else
                    Property.BooleanConstants = (uint)((int)Property.BooleanConstants & ~(1 << TemplateIndex));
            }
        }

        public BooleanConstant(RenderMethod.ShaderProperty property, string name, string desc, int templateIndex) : base(property, name, desc, templateIndex)
        {
        }
    }
}
