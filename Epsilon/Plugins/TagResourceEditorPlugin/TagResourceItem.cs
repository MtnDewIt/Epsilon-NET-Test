using System;
using TagTool.Tags;

namespace TagResourceEditorPlugin
{
    public class TagResourceItem : TagStructure
    {
        public string DisplayName { get; set; }
        public TagResourceReference Resource { get; set; }
        public Type DefinitionType { get; set; }
    }
}
