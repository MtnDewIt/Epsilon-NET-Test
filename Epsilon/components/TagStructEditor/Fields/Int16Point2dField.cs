using TagTool.Common;

namespace TagStructEditor.Fields
{
    public class Int16Point2dField : ValueField
    {
        public short X { get; set; }
        public short Y { get; set; }

        public Int16Point2dField(ValueFieldInfo info) : base(info)
        {
        }

        public override void Accept(IFieldVisitor visitor)
        {
            visitor.Visit(this);
        }

        protected override void OnPopulate(object value)
        {
            var point = (Int16Point2d)value;
            X = point.X;
            Y = point.Y;
        }

        private void UpdateValue()
        {
            SetActualValue(new Int16Point2d(X, Y));
        }

        public void OnXChanged() => UpdateValue();
        public void OnYChanged() => UpdateValue();
    }
}
