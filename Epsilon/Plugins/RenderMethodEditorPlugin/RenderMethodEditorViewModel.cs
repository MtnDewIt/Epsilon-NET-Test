using CacheEditor;
using Stylet;
using System;
using System.Collections.Generic;
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
        private RenderMethod.ShaderProperty _shaderProperties;

        private List<BooleanConstant> _booleanConstants = new List<BooleanConstant>();

        public RenderMethodEditorViewModel(GameCache cache, RenderMethod renderMethod)
        {
            _cache = cache;
            _renderMethod = renderMethod;
            _shaderProperties = _renderMethod.ShaderProperties[0];
            using (var stream = _cache.OpenCacheRead())
            {
                _renderMethodTemplate = cache.Deserialize<RenderMethodTemplate>(stream, _shaderProperties.Template);
            }
            
        }

        // magic shader code goes here

        private void ParseBooleanArguments()
        {
            for(int i = 0; i < _renderMethodTemplate.BooleanParameterNames.Count; i++)
            {
                string name = _cache.StringTable.GetString(_renderMethodTemplate.BooleanParameterNames[i].Name);
                bool value = ((int)(_shaderProperties.BooleanConstants) & (1 << i)) == 1;
                _booleanConstants.Add(new BooleanConstant(name, value, i));
            }
            
            // convert name to something pretty somehow
            
            // this should be a flag like a tagfield flag with name and checkbox
            // updating the value here would also update the UI and model value
            // also dynamically generated from the rmt2 since they heavily depend on it
        }

        private class ShaderConstant
        {
            public string Name;
            public int TemplateIndex;
            public ShaderConstant(string name, int templateIndex)
            {
                Name = name;
                TemplateIndex = templateIndex;
            }
        }

        private class BooleanConstant : ShaderConstant
        {
            public bool Value;

            public BooleanConstant(string name, bool value, int templateIndex) : base(name, templateIndex)
            {
                Value = value;
            }
        }

        private class FloatConstant : ShaderConstant
        {
            public float Value;

            public FloatConstant(string name, float value, int templateIndex) : base(name, templateIndex)
            {
                Value = value;
            }
        }
    }
}
