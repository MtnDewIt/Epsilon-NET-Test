using System;
using TagTool.Cache;
using TagTool.Common;

namespace TagStructEditor.Fields
{
    public class StringIdField : ValueField
    {
        public readonly StringTable _stringTable;

        public string Value { get; set; }

        public StringIdField(StringTable stringTable, ValueFieldInfo info) : base(info)
        {
            _stringTable = stringTable;
        }

        public override void Accept(IFieldVisitor visitor)
        {
            visitor.Visit(this);
        }

        protected override void OnPopulate(object value)
        {
            var stringId = (StringId)value;
            if (stringId == StringId.Invalid)
                Value = "";
            else
                Value = _stringTable.GetString(stringId);
        }

        public void OnValueChanged()
        {
            StringId stringId = StringId.Invalid;
            if (!string.IsNullOrEmpty(Value))
            {
                stringId = _stringTable.GetStringId(Value);
                if (string.IsNullOrWhiteSpace(Value) || stringId == StringId.Invalid)
                   throw new ArgumentException(nameof(Value));
            }

            SetActualValue(stringId);
        }
    }
}
