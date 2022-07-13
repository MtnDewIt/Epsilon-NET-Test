using CacheEditor;
using CacheEditor.TagEditing;
using CacheEditor.TagEditing.Messages;
using HaloShaderGenerator.Generator;
using RenderMethodEditorPlugin.ShaderMethods;
using RenderMethodEditorPlugin.ShaderMethods.Particle;
using RenderMethodEditorPlugin.ShaderMethods.Shader;
using RenderMethodEditorPlugin.ShaderMethods.Halogram;
using RenderMethodEditorPlugin.ShaderMethods.Decal;
using RenderMethodEditorPlugin.ShaderMethods.Screen;
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
using System.ComponentModel;

namespace RenderMethodEditorPlugin
{
    class RenderMethodEditorViewModel : TagEditorPluginBase
    {
        private RenderMethod _renderMethod;
        private GameCache _cache;
        private RenderMethodTemplate _renderMethodTemplate;
        private RenderMethod.RenderMethodPostprocessBlock _shaderProperty;

        public ObservableCollection<BooleanConstant> BooleanConstants { get; private set;  } = new ObservableCollection<BooleanConstant>();
        public ObservableCollection<Method> ShaderMethods { get; private set; } = new ObservableCollection<Method>();
        public ObservableCollection<GenericShaderParameter> ShaderParameters { get; private set; } = new ObservableCollection<GenericShaderParameter>();

        public RenderMethodEditorViewModel(GameCache cache, RenderMethod renderMethod)
        {
            _cache = cache;
            Load(cache, renderMethod);
        }

        private void Load(GameCache cache, RenderMethod renderMethod)
        {
            _renderMethod = renderMethod;
            _shaderProperty = _renderMethod.ShaderProperties[0];
            var templateName = _shaderProperty.Template.Name;

            if (templateName == null)
                return;

            var templateTypeSplitIndex = 1;
            if (templateName.Contains("ms30"))
                templateTypeSplitIndex = 2;

            string templateType = templateName.Split('\\')[templateTypeSplitIndex].Split('_')[0];

            MethodParser methodParser = null;
            IShaderGenerator generator;
            RenderMethodDefinition rmdf = null;

            using (var stream = _cache.OpenCacheRead())
            {
                _renderMethodTemplate = cache.Deserialize<RenderMethodTemplate>(stream, _shaderProperty.Template);

                if (_renderMethod.BaseRenderMethod != null)
                {
                    rmdf = cache.Deserialize<RenderMethodDefinition>(stream, _renderMethod.BaseRenderMethod);
                }
            }

            byte[] options = _renderMethod.Options.ConvertAll(x => (byte)x.OptionIndex).ToArray();

            switch (templateType)
            {
                case "beam":
                    generator = new HaloShaderGenerator.Beam.BeamGenerator(options, true);
                    break;
                case "black":
                    generator = new HaloShaderGenerator.Black.ShaderBlackGenerator();
                    break;
                case "contrail":
                    generator = new HaloShaderGenerator.Contrail.ContrailGenerator(options, true);
                    break;
                case "cortana":
                    generator = new HaloShaderGenerator.Cortana.CortanaGenerator(options, true);
                    break;
                case "custom":
                    generator = new HaloShaderGenerator.Custom.CustomGenerator(options, true);
                    break;
                case "decal":
                    methodParser = new DecalMethod();
                    generator = new HaloShaderGenerator.Decal.DecalGenerator(options, true);
                    break;
                case "foliage":
                    generator = new HaloShaderGenerator.Foliage.FoliageGenerator(options, true);
                    break;
                case "halogram":
                    methodParser = new HalogramMethod();
                    generator = new HaloShaderGenerator.Halogram.HalogramGenerator(options, true);
                    break;
                case "light_volume":
                    generator = new HaloShaderGenerator.LightVolume.LightVolumeGenerator(options, true);
                    break;
                case "particle":
                    methodParser = new ParticleMethod();
                    generator = new HaloShaderGenerator.Particle.ParticleGenerator(options, true);
                    break;
                case "screen":
                    methodParser = new ScreenMethod();
                    generator = new HaloShaderGenerator.Screen.ScreenGenerator(options, true);
                    break;
                case "shader":
                    methodParser = new ShaderMethod();
                    generator = new HaloShaderGenerator.Shader.ShaderGenerator(options, true);
                    break;
                case "terrain":
                    generator = new HaloShaderGenerator.Terrain.TerrainGenerator(options, true);
                    break;
                case "water":
                    generator = new HaloShaderGenerator.Water.WaterGenerator(options, true);
                    break;
                case "zonly":
                    generator = new HaloShaderGenerator.ZOnly.ZOnlyGenerator(options, true);
                    break;
                default:
                    return;
            }

            ShaderMethods = new ObservableCollection<Method>();
            for (int i = 0; i < _renderMethod.Options.Count; i++)
            {
                var optionIndex = _renderMethod.Options[i].OptionIndex;

                if (methodParser != null)
                {
                    var methodInfo = methodParser.ParseMethod(i, optionIndex, generator);
                    if (methodInfo != null)
                        ShaderMethods.Add(methodInfo);
                }
                else if (rmdf != null) // pull from rmdf
                {
                    if (i >= 0 && i < generator.GetMethodCount() && i < rmdf.Categories.Count)
                    {
                        if (optionIndex >= 0 && optionIndex < generator.GetMethodOptionCount(i) && optionIndex < rmdf.Categories[i].ShaderOptions.Count)
                        {
                            var categoryName = cache.StringTable.GetString(rmdf.Categories[i].Name);
                            var optionName = cache.StringTable.GetString(rmdf.Categories[i].ShaderOptions[optionIndex].Name);

                            var methodInfo = new Method(categoryName, optionName, "Description N/A", i, optionIndex);

                            if (methodInfo != null)
                                ShaderMethods.Add(methodInfo);
                        }
                    }
                }
            }

            bool useRotation = false;
            if (generator is HaloShaderGenerator.Shader.ShaderGenerator && _renderMethod.Options[9].OptionIndex == 3)
                useRotation = true;
            var parameters = generator.GetPixelShaderParameters().Parameters;
            parameters.AddRange(generator.GetVertexShaderParameters().Parameters);
            ShaderParameters = ShaderParameterFactory.BuildShaderParameters(cache, parameters, _shaderProperty, _renderMethodTemplate, useRotation);

            this.NotifyOfPropertyChange(nameof(ShaderMethods));
            this.NotifyOfPropertyChange(nameof(ShaderParameters));
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

        protected override void OnMessage(object sender, object message)
        {
            if (message is DefinitionDataChangedEvent e)
            {
               Load(_cache, (RenderMethod)e.NewData);
            }
        }
    }

    class ShaderConstant
    {
        public RenderMethod.RenderMethodPostprocessBlock Property;
        public string Name { get; set; }
        public string Description { get; set; }
        public int TemplateIndex;
        public ShaderConstant(RenderMethod.RenderMethodPostprocessBlock property, string name, string desc, int templateIndex)
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

        public BooleanConstant(RenderMethod.RenderMethodPostprocessBlock property, string name, string desc, int templateIndex) : base(property, name, desc, templateIndex)
        {
        }
    }
}
