using System.Collections.Generic;
using System.Linq;
using TagStructEditor.Common;
using TagTool.Common;

namespace TagStructEditor.Fields
{
    public class GroupTagField : ValueField
    {
        public TagGroupItem Value { get; set; }
        public IList<TagGroupItem> Groups { get; }

        public GroupTagField(TagList tagList, ValueFieldInfo info) : base(info)
        {
            Groups = tagList.Groups;
        }

        public override void Accept(IFieldVisitor visitor)
        {
            visitor.Visit(this);
        }

        protected override void OnPopulate(object value)
        {
            Value = Groups.FirstOrDefault(item => item.Group.Tag == (Tag)value);
        }

        public void OnValueChanged() => SetActualValue(Value.Group.Tag);
    }
}
