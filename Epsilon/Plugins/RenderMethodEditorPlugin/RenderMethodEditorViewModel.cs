using CacheEditor;
using CacheEditor.RTE;
using CacheEditor.RTE.UI;
using CacheEditor.TagEditing;
using CacheEditor.TagEditing.Messages;
using DefinitionEditor;
using EpsilonLib.Commands;
using EpsilonLib.Dialogs;
using EpsilonLib.Logging;
using EpsilonLib.Utils;
using RenderMethodEditorPlugin.ShaderMethods;
using RenderMethodEditorPlugin.ShaderParameters;
using Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TagStructEditor.Fields;
using TagTool.Cache;
using TagTool.IO;
using TagTool.Serialization;
using TagTool.Tags;
using TagTool.Tags.Definitions;

namespace RenderMethodEditorPlugin
{
    class RenderMethodEditorViewModel : TagEditorPluginBase
    {
        private IShell _shell;
        private ICacheEditor _cacheEditor;
        private ICacheFile _cacheFile;
        private CachedTag _instance;
        private RenderMethod _definitionData;
        private IDisposable _rteRefreshTimer;
        private IFieldsValueChangeSink _changeSink;

        // Fields that have changed since the last poke
        private HashSet<ValueField> _changedFields = [];
        // Fields that have been poked this session
        private HashSet<ValueField> _pokedFields = [];

        private RenderMethodTemplate _renderMethodTemplate;
        private RenderMethodDefinition _renderMethodDefinition;
        private RenderMethod.RenderMethodPostprocessBlock _shaderProperty;

        public ObservableCollection<BooleanConstant> BooleanConstants { get; private set;  } = new ObservableCollection<BooleanConstant>();
        public ObservableCollection<Method> ShaderMethods { get; private set; } = new ObservableCollection<Method>();
        public ObservableCollection<GenericShaderParameter> ShaderParameters { get; private set; } = new ObservableCollection<GenericShaderParameter>();

        public StructField StructField { get; set; }
        public IRteService RteService { get; }
        public TargetListModel RteTargetList { get; }
        public TargetListItem SelectedRteTargetItem { get; set; }
        public bool RteHasTargets { get; set; }

        public DelegateCommand SaveCommand { get; }
        public DelegateCommand PokeCommand { get; }

        public RenderMethodEditorViewModel(
            IShell shell,
            IRteService rteService,
            ICacheEditor cacheEditor,
            ICacheFile cacheFile,
            CachedTag instance,
            RenderMethod definitionData,
            StructField structField,
            IFieldsValueChangeSink changeSink)
        {
            Load(cacheFile.Cache, definitionData);

            _shell = shell;
            _definitionData = definitionData;
            _cacheEditor = cacheEditor;
            _cacheFile = cacheFile;
            _changeSink = changeSink;
            _changeSink.ValueChanged += Field_ValueChanged;
            _instance = instance;

            RteService = rteService;
            StructField = structField;

            PokeCommand = new DelegateCommand(PokeChanges, () => RteTargetList.Any());
            SaveCommand = new DelegateCommand(SaveChanges, () => _cacheFile.CanSerializeTags);

            RteTargetList = new TargetListModel(rteService.GetTargetList(cacheFile));
            RteHasTargets = RteTargetList.Any();
        }

        private void Load(GameCache cache, RenderMethod renderMethod)
        {
            _definitionData = renderMethod;
            _shaderProperty = _definitionData.ShaderProperties[0];
            var templateName = _shaderProperty.Template.Name;

            if (templateName == null)
                return;

            var templateTypeSplitIndex = 1;
            if (templateName.Contains("ms30"))
                templateTypeSplitIndex = 2;

            string templateType = templateName.Split('\\')[templateTypeSplitIndex].Split('_')[0];

            var options = _definitionData.Options.ConvertAll(x => (byte)x.OptionIndex);

            List<RenderMethodOption.ParameterBlock> rmopParameters;
            using (var stream = cache.OpenCacheRead())
            {
                _renderMethodTemplate = cache.Deserialize<RenderMethodTemplate>(stream, _shaderProperty.Template);
                _renderMethodDefinition = cache.Deserialize<RenderMethodDefinition>(stream, renderMethod.BaseRenderMethod);
                // get parameters, auto macro and global parameters are ignored for now
                rmopParameters = TagTool.Shaders.ShaderGenerator.ShaderGeneratorNew.GatherParameters(cache, stream, _renderMethodDefinition, options.ToArray(), false);
            }

            ShaderMethods = new ObservableCollection<Method>();
            for (int i = 0; i < _renderMethodDefinition.Categories.Count; i++)
            {
                short optionIndex = 0; // assume an index of 0 for outdated tags
                
                if (i < _definitionData.Options.Count)
                    optionIndex = _definitionData.Options[i].OptionIndex;

                if (optionIndex < _renderMethodDefinition.Categories[i].ShaderOptions.Count)
                {
                    string categoryName = cache.StringTable.GetString(_renderMethodDefinition.Categories[i].Name);
                    string optionName = cache.StringTable.GetString(_renderMethodDefinition.Categories[i].ShaderOptions[optionIndex].Name);

                    ShaderMethods.Add(new Method(categoryName, optionName, "Description N/A", i, optionIndex));
                }
            }

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
                                ShaderParameters.Add(new FloatShaderParameter(_definitionData.ShaderProperties[0], name, description, i));
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
                                ShaderParameters.Add(new Float4ShaderParameter(_definitionData.ShaderProperties[0], name, description, i));
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
                                ShaderParameters.Add(new SamplerShaderParameter(_definitionData.ShaderProperties[0], name, description, i));
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
                                ShaderParameters.Add(new BooleanShaderParameter(_definitionData.ShaderProperties[0], name, description, i, cache, _renderMethodTemplate.BooleanParameterNames));
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
                            ShaderParameters.Add(new TransformShaderParameter(_definitionData.ShaderProperties[0], name, description, i));
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
                                ShaderParameters.Add(new CategoryShaderParameter(_definitionData.ShaderProperties[0], "category_" + name, description, j));
                                break;
                            }
                        }
                    }
                }
            }

            this.NotifyOfPropertyChange(nameof(ShaderMethods));
            this.NotifyOfPropertyChange(nameof(ShaderParameters));
        }

        // get boolean values from existing tag data

        private void ParseBooleanArguments()
        {
            for(int i = 0; i < _renderMethodTemplate.BooleanParameterNames.Count; i++)
            {
                string name = _cacheFile.Cache.StringTable.GetString(_renderMethodTemplate.BooleanParameterNames[i].Name);
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

        private void Field_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (e.Field is ValueField valueField)
                _changedFields.Add(valueField);
        }

        private void PokeField(ValueField field)
        {
            if (SelectedRteTargetItem == null)
            {
                var alert = new AlertDialogViewModel
                {
                    AlertType = Alert.Error,
                    DisplayName = "Failed to Poke",
                    Message = $"Not attached to game instance!",
                };

                _shell.ShowDialog(alert);
                return;
            }
            var helper = new RTEFieldHelper(SelectedRteTargetItem.Target, _cacheFile, _definitionData.GetType(), _instance);
            uint address = helper.GetFieldMemoryAddress(field);
            if (address == 0)
            {
                var alert = new AlertDialogViewModel
                {
                    AlertType = Alert.Error,
                    DisplayName = "Failed to Poke",
                    Message = $"Tag not loaded!",
                };

                _shell.ShowDialog(alert);
                return;
            }

            if (field is CachedTagField tagRefField)
            {
                // Ensure the tag being poked is loaded to avoid crashing.

                CachedTag instance = tagRefField.SelectedInstance.Instance;

                if (instance != null && !helper.IsTagLoaded(instance))
                {
                    var alert = new AlertDialogViewModel
                    {
                        AlertType = Alert.Error,
                        DisplayName = "Failed to Poke tag reference",
                        Message = $"Tag '{instance}' is not loaded!",
                    };

                    _shell.ShowDialog(alert);
                    return;
                }
            }

            GameCache editorCache = _cacheEditor.CacheFile.Cache;

            //handle field types that will break things
            var valueType = field.FieldType;
            var fieldValue = field.FieldInfo.ValueGetter(field.Owner);

            var stream = new MemoryStream();
            var writer = new EndianWriter(stream, editorCache.Endianness);
            var dataContext = new DataSerializationContext(writer, CacheAddressType.Memory, false);

            var block = dataContext.CreateBlock();

            _cacheEditor.CacheFile.Cache.Serializer.SerializeValue(dataContext, stream,
                block, fieldValue, new TagFieldAttribute(),
            field.FieldInfo.FieldType);
            byte[] outData = block.Stream.ToArray();

            using (var processStream = SelectedRteTargetItem.Target.Provider.CreateStream(SelectedRteTargetItem.Target))
            {
                processStream.Seek(address, SeekOrigin.Begin);
                processStream.Write(outData, 0, outData.Length);
                processStream.Flush();
            }

            _changedFields.Remove(field);
            _pokedFields.Add(field);
        }

        private void RefreshRteTargets()
        {
            RteTargetList.Refresh();
            RteHasTargets = RteTargetList.Any();
            if (SelectedRteTargetItem == null || !(RteTargetList).Contains(SelectedRteTargetItem))
                SelectedRteTargetItem = RteTargetList.FirstOrDefault();
            PokeCommand.RaiseCanExecuteChanged();
        }

        private async void SaveChanges()
        {
            if (!_cacheFile.CanSerializeTags)
                throw new NotSupportedException();

            try
            {
                using (var progress = _shell.CreateProgressScope())
                {
                    progress.Report($"Serializing tag '{_instance}'...");
                    await _cacheFile.SerializeTagAsync(_instance, _definitionData);
                }
                _shell.StatusBar.ShowStatusText("Saved Changes");
                Logger.LogCommand($"{_instance.Name}.{_instance.Group}", null, Logger.CommandEvent.CommandType.None, "SaveTagChanges");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                var alert = new AlertDialogViewModel
                {
                    AlertType = Alert.Error,
                    Message = $"An exception was thrown while attempting to save tag changes.",
                    SubMessage = ex.Message
                };

                _shell.ShowDialog(alert);
            }
        }

        private void PokeChanges()
        {
            if (SelectedRteTargetItem == null)
                return;

            try
            {
                IRteTarget target = SelectedRteTargetItem.Target;
                int changedFieldCount = _changedFields.Count;

                foreach (ValueField field in _changedFields)
                    PokeField(field);

                _shell.StatusBar.ShowStatusText($"Poked {changedFieldCount} fields");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                var alert = new AlertDialogViewModel
                {
                    AlertType = Alert.Error,
                    Message = $"An exception was thrown while attempting to poke tag changes.",
                    SubMessage = ex.Message
                };

                _shell.ShowDialog(alert);
            }
        }

        protected override void OnMessage(object sender, object message)
        {
            if (message is DefinitionDataChangedEvent e)
            {
                // Restore potentially changed fields on the next poke
                _changedFields = _pokedFields;
                _pokedFields = [];

                Load(_cacheFile.Cache, (RenderMethod)e.NewData);
            }
        }

        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();

            if (_rteRefreshTimer == null)
                _rteRefreshTimer = DispatcherEx.CreateTimer(TimeSpan.FromSeconds(5), RefreshRteTargets);

            RefreshRteTargets();
        }

        protected override void OnClose()
        {
            base.OnClose();
            _cacheFile = null;
            _cacheEditor = null;
            _instance = null;
            _definitionData = null;
            _renderMethodTemplate = null;
            _renderMethodDefinition = null;
            _shaderProperty = null;
            BooleanConstants.Clear();
            ShaderMethods.Clear();
            ShaderParameters.Clear();
            StructField?.Dispose();
            StructField = null;

            _changeSink.ValueChanged -= Field_ValueChanged;

            if (_rteRefreshTimer != null)
            {
                _rteRefreshTimer.Dispose();
                _rteRefreshTimer = null;
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
