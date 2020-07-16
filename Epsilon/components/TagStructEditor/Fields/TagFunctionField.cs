using TagStructEditor.Common;
using TagTool.Tags;

namespace TagStructEditor.Fields
{
    public class TagFunctionField : DataField, IExpandable
    {
        public TagFunctionField(ValueFieldInfo info) : base(info)
        {
        }

        protected override void OnPopulate(object value)
        {
            var function = (TagFunction)value;
            base.OnPopulate(function.Data);
        }

        public override void SetActualValue(object value)
        {
            base.SetActualValue(new TagFunction() { Data = (byte[])value });
        }
    }
}
