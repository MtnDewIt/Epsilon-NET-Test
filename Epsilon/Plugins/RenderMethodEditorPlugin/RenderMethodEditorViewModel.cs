using CacheEditor;
using CacheEditor.TagEditing;
using CacheEditor.TagEditing.Messages;
using RenderMethodEditorPlugin.ShaderMethods;
using RenderMethodEditorPlugin.ShaderParameters;
using System.Collections.ObjectModel;
using TagTool.Cache;
using TagTool.Tags.Definitions;

namespace RenderMethodEditorPlugin
{
    class RenderMethodEditorViewModel : TagEditorPluginBase
    {
        private TagEditorContext _tagEditor;
        private RenderMethod _renderMethod;
        private GameCache _cache;
        private RenderMethodTemplate _renderMethodTemplate;
        private RenderMethodDefinition _renderMethodDefinition;
        private RenderMethod.RenderMethodPostprocessBlock _shaderProperty;
        private bool _isDataDirty = false;

        private ObservableCollection<GenericShaderParameter> _shaderParameters;
        private ObservableCollection<Method> _shaderMethods;

        public RenderMethodEditorViewModel(TagEditorContext tagEditor, GameCache cache, RenderMethod renderMethod)
        {
            _cache = cache;
            _tagEditor = tagEditor;
            Load(cache, renderMethod);
        }

        public TagEditorContext TagEditor => _tagEditor;

        public GameCache Cache => _cache;

        public ObservableCollection<Method> ShaderMethods
        {
            get => _shaderMethods;
            set => SetAndNotify(ref _shaderMethods, value);
        }

        public ObservableCollection<GenericShaderParameter> ShaderParameters
        {
            get => _shaderParameters;
            set
            {
                if (_shaderParameters != null)
                    UnregisterParameters();

                _shaderParameters = value;

                if (value != null)
                    RegisterParameters();

                NotifyOfPropertyChange();
            }
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

            var options = _renderMethod.Options.ConvertAll(x => (byte)x.OptionIndex);

            System.Collections.Generic.List<RenderMethodOption.ParameterBlock> rmopParameters;
            using (var stream = _cache.OpenCacheRead())
            {
                _renderMethodTemplate = cache.Deserialize<RenderMethodTemplate>(stream, _shaderProperty.Template);
                _renderMethodDefinition = cache.Deserialize<RenderMethodDefinition>(stream, renderMethod.BaseRenderMethod);
                // get parameters, auto macro and global parameters are ignored for now
                rmopParameters = TagTool.Shaders.ShaderGenerator.ShaderGeneratorNew.GatherParameters(cache, stream, _renderMethodDefinition, options.ToArray(), false);
            }

            var newMethods = new ObservableCollection<Method>();
            for (int i = 0; i < _renderMethodDefinition.Categories.Count; i++)
            {
                short optionIndex = 0; // assume an index of 0 for outdated tags
                
                if (i < _renderMethod.Options.Count)
                    optionIndex = _renderMethod.Options[i].OptionIndex;

                if (optionIndex < _renderMethodDefinition.Categories[i].ShaderOptions.Count)
                {
                    string categoryName = cache.StringTable.GetString(_renderMethodDefinition.Categories[i].Name);
                    string optionName = cache.StringTable.GetString(_renderMethodDefinition.Categories[i].ShaderOptions[optionIndex].Name);

                    newMethods.Add(new Method(categoryName, optionName, "Description N/A", i, optionIndex));
                }
            }
            ShaderMethods = newMethods;

            var newParameters = new ObservableCollection<GenericShaderParameter>();

            foreach (var parameter in rmopParameters)
            {
                var name = cache.StringTable.GetString(parameter.Name);

                switch (parameter.Type)
                {
                    case RenderMethodOption.ParameterBlock.OptionDataType.Real:
                        for (int i = 0; i < _renderMethodTemplate.RealParameterNames.Count; i++)
                        {
                            if (cache.StringTable.GetString(_renderMethodTemplate.RealParameterNames[i].Name) == name)
                            {
                                string description = $"1D vector for parameter \"{name}\"";
                                newParameters.Add(new FloatShaderParameter(_renderMethod.ShaderProperties[0], name, description, i));
                                break;
                            }
                        }
                        break;
                    case RenderMethodOption.ParameterBlock.OptionDataType.Color:
                    case RenderMethodOption.ParameterBlock.OptionDataType.ArgbColor:
                        for (int i = 0; i < _renderMethodTemplate.RealParameterNames.Count; i++)
                        {
                            if (cache.StringTable.GetString(_renderMethodTemplate.RealParameterNames[i].Name) == name)
                            {
                                string description = $"4D vector for parameter \"{name}\"";
                                newParameters.Add(new Float4ShaderParameter(_renderMethod.ShaderProperties[0], name, description, i));
                                break;
                            }
                        }
                        break;
                    case RenderMethodOption.ParameterBlock.OptionDataType.Bitmap:
                        for (int i = 0; i < _renderMethodTemplate.TextureParameterNames.Count; i++)
                        {
                            if (cache.StringTable.GetString(_renderMethodTemplate.TextureParameterNames[i].Name) == name)
                            {
                                string description = $"Bitmap tag for texture \"{name}\"";
                                newParameters.Add(new SamplerShaderParameter(this, _renderMethod.ShaderProperties[0], name, description, i));
                                break;
                            }
                        }
                        break;
                    //case RenderMethodOption.ParameterBlock.OptionDataType.Int:
                    //    for (int i = 0; i < _renderMethodTemplate.IntegerParameterNames.Count; i++)
                    //    {
                    //        if (cache.StringTable.GetString(_renderMethodTemplate.IntegerParameterNames[i].Name) == name)
                    //        {
                    //            string description = $"Whole integer for parameter \"{name}\"";
                    //            ShaderParameters.Add(new IntegerShaderParameter(_renderMethod.ShaderProperties[0], name, description, i));
                    //            break;
                    //        }
                    //    }
                    //    break;
                    case RenderMethodOption.ParameterBlock.OptionDataType.Bool:
                        for (int i = 0; i < _renderMethodTemplate.BooleanParameterNames.Count; i++)
                        {
                            if (cache.StringTable.GetString(_renderMethodTemplate.BooleanParameterNames[i].Name) == name)
                            {
                                string description = $"Checkbox for on/off parameter \"{name}\"";
                                newParameters.Add(new BooleanShaderParameter(_renderMethod.ShaderProperties[0], name, description, i, cache, _renderMethodTemplate.BooleanParameterNames));
                                break;
                            }
                        }
                        break;
                }

                // xform
                if (parameter.Type == RenderMethodOption.ParameterBlock.OptionDataType.Bitmap)
                {
                    for (int i = 0; i < _renderMethodTemplate.RealParameterNames.Count; i++)
                    {
                        if (cache.StringTable.GetString(_renderMethodTemplate.RealParameterNames[i].Name) == name)
                        {
                            string description = $"Transform vector for texture \"{name}\"";
                            newParameters.Add(new TransformShaderParameter(_renderMethod.ShaderProperties[0], name, description, i));
                            break;
                        }
                    }
                }
            }

            if (_renderMethodDefinition.Flags.HasFlag(RenderMethodDefinition.RenderMethodDefinitionFlags.UseAutomaticMacros))
            {
                for (int i = 0; i < _renderMethodDefinition.Categories.Count; i++)
                {
                    string name = cache.StringTable.GetString(_renderMethodDefinition.Categories[i].Name);

                    if (TagTool.Shaders.ShaderGenerator.ShaderGeneratorNew.AutoMacroIsParameter(name,
                        (HaloShaderGenerator.Globals.ShaderType)System.Enum.Parse(typeof(HaloShaderGenerator.Globals.ShaderType), templateType, true)))
                    {
                        for (int j = 0; j < _renderMethodTemplate.RealParameterNames.Count; j++)
                        {
                            if (cache.StringTable.GetString(_renderMethodTemplate.RealParameterNames[j].Name) == name)
                            {
                                string description = $"This value should match the option index for category \"{name}\"";
                                newParameters.Add(new CategoryShaderParameter(_renderMethod.ShaderProperties[0], "category_" + name, description, j));
                                break;
                            }
                        }
                    }
                }
            }
            ShaderParameters = newParameters;
        }

        public void Test()
        {
            PostMessage(this, new DefinitionDataChangedEvent(_renderMethod));
        }

        protected override void OnClose()
        {
            base.OnClose();
            _renderMethod = null;
            _cache = null;
            _renderMethodTemplate = null;
            _renderMethodDefinition = null;
            _shaderProperty = null;
            ShaderMethods = null;
            ShaderParameters = null;
        }

        private void RefreshParameters()
        {
            foreach (var parameter in _shaderParameters)
                parameter.Refresh();
        }

        private void RegisterParameters()
        {
            foreach (var parameter in _shaderParameters)
                parameter.ValueChanged += Parameter_ValueChanged;
        }

        private void UnregisterParameters()
        {
            foreach (var parameter in _shaderParameters)
                parameter.ValueChanged -= Parameter_ValueChanged;
        }

        private void Parameter_ValueChanged(object sender, System.EventArgs e)
        {
            PostMessage(this, new DefinitionDataChangedEvent(_renderMethod));
        }

        protected override void OnMessage(object sender, object message)
        {
            if (message is DefinitionDataChangedEvent dataChangedEvent)
            {
                if (dataChangedEvent.WasReloaded)
                {
                    Load(_cache, (RenderMethod)dataChangedEvent.NewData);
                }
                else
                {
                    if (IsActive)
                        RefreshParameters();
                    else
                        _isDataDirty = true;
                }
            }
        }

        protected override void OnActivate()
        {
            if (_isDataDirty)
            {
                _isDataDirty = false;
                RefreshParameters();
            }
        }
    }
}
