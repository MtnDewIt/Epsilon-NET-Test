using CacheEditor;
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
    class RenderMethodEditorViewModel : Screen, ITagEditorPlugin
    {
        private RenderMethod _renderMethod;
        private GameCache _cache;
        private RenderMethodTemplate _renderMethodTemplate;
        private RenderMethod.ShaderProperty _shaderProperty;

        public ObservableCollection<BooleanConstant> BooleanConstants {get;} = new ObservableCollection<BooleanConstant>();
        // only properties can be bound to from xaml


        public RenderMethodEditorViewModel(GameCache cache, RenderMethod renderMethod)
        {
            /*
             * System.Windows.Data Error: 40 : BindingExpression path error: 'BooleanConstant' property not found on 'object' ''BooleanConstant' (HashCode=29753161)'. BindingExpression:Path=BooleanConstant.Name; DataItem='BooleanConstant' (HashCode=29753161); target element is 'TextBlock' (Name=''); target property is 'Text' (type 'String')
System.Windows.Data Error: 40 : BindingExpression path error: 'BooleanConstant' property not found on 'object' ''BooleanConstant' (HashCode=29753161)'. BindingExpression:Path=BooleanConstant.Value; DataItem='BooleanConstant' (HashCode=29753161); target element is 'CheckBox' (Name=''); target property is 'IsChecked' (type 'Nullable`1')
The thread 0xcc has exited with code 0 (0x0).
             * */
            _cache = cache;
            _renderMethod = renderMethod; // discord down? yeah now it kinda works but the name and value are broken for the class broken how? no string showing and box not ticked
            _shaderProperty = _renderMethod.ShaderProperties[0];
            using (var stream = _cache.OpenCacheRead())
            {
                _renderMethodTemplate = cache.Deserialize<RenderMethodTemplate>(stream, _shaderProperty.Template);
            }

            ParseBooleanArguments();
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
