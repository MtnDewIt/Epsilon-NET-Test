using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TagTool.Tags.Definitions.RenderMethodTemplate;

namespace TagStructEditor.Fields
{
    public class PackedInteger_10_6Field : ValueField
    {
        public ushort Offset { get; set; }
        public ushort Count { get; set; }

        public PackedInteger_10_6Field(ValueFieldInfo info) : base(info)
        {
        }

        public override void Accept(IFieldVisitor visitor)
        {
            return;
        }

        protected override void OnPopulate(object value)
        {
            var packedInt = (PackedInteger_10_6)value;
            Offset = packedInt.Offset;
            Count = packedInt.Count;
        }

        protected void OnOffsetChanged() => UpdateValue();
        protected void OnCountChanged() => UpdateValue();

        void UpdateValue()
        {
            SetActualValue(new PackedInteger_10_6() { Offset = Offset, Count = Count });
        }
    }
}
