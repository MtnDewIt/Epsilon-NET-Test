using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TagStructEditor.Helpers;

namespace TagStructEditor.Fields
{
    public class EnumField : ValueField
    {
        public EnumMember Value { get; set; }
        public ObservableCollection<EnumMember> Values { get; }

        public EnumField(ValueFieldInfo info) : base(info)
        {
            Values = new ObservableCollection<EnumMember>(GenerateMemberList(info.FieldType));
        }

        private IEnumerable<EnumMember> GenerateMemberList(Type enumType)
        {
            var values = Enum.GetValues(enumType);
            var names = Enum.GetNames(enumType);

            for (int i = 0; i < values.Length; i++)
                yield return new EnumMember(Utils.DemangleName(names[i]), (Enum)values.GetValue(i));
        }

        public override void Accept(IFieldVisitor visitor)
        {
            visitor.Visit(this);
        }

        protected override void OnPopulate(object value)
        {
            Value = Values.FirstOrDefault(member => member.Value.Equals((Enum)value));
        }

        public void OnValueChanged() => SetActualValue(Value?.Value);

        public class EnumMember
        {
            public string Name { get; }
            public Enum Value { get; }

            public EnumMember(string name, Enum value)
            {
                Name = name;
                Value = value;
            }
        }
    }
}
