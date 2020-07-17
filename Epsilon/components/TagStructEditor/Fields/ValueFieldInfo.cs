using System;
using static TagTool.Tags.TagFieldInfo;

namespace TagStructEditor.Fields
{
    /// <summary>
    /// Info used to construct a <see cref="ValueField"/>
    /// </summary>
    public class ValueFieldInfo
    {
        public ValueFieldFlags Flags { get; set; } = ValueFieldFlags.Default;
        public Type FieldType { get; set; }
        public string Name { get; set; }
        public uint Offset { get; set; }
        public int Length { get; set; }
        public ValueSetter ValueSetter;
        public ValueGetter ValueGetter;
        public Action<ValueChangedEventArgs> ValueChanged;
    }

    [Flags]
    public enum ValueFieldFlags
    {
        None = 0,
        ShowType = (1 << 0),

        Default = ShowType
    }
}
