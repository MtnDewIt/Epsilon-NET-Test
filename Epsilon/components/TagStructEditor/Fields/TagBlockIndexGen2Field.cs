using TagTool.Common;

namespace TagStructEditor.Fields
{
    public class TagBlockIndexGen2Field : ValueField
    {
        public ushort Offset { get; set; }
        public ushort Count { get; set; }

        public TagBlockIndexGen2Field(ValueFieldInfo info) : base(info)
        {
        }

        public override void Accept(IFieldVisitor visitor)
        {
            return;
        }

        protected override void OnPopulate(object value)
        {
            var packedInt = (TagBlockIndexGen2)value;
            Offset = packedInt.Offset;
            Count = packedInt.Count;
        }

        protected void OnOffsetChanged() => UpdateValue();
        protected void OnCountChanged() => UpdateValue();

        void UpdateValue()
        {
            SetActualValue(new TagBlockIndexGen2() { Offset = Offset, Count = Count });
        }
    }
}
