namespace TagStructEditor.Fields
{
    public class RealQuaternionField : ValueField
    {
        public float I { get; set; }
        public float J { get; set; }
        public float K { get; set; }
        public float W { get; set; }

        public RealQuaternionField(ValueFieldInfo info) : base(info)
        {
        }

        public override void Accept(IFieldVisitor visitor)
        {
            visitor.Visit(this);
        }

        protected override void OnPopulate(object value)
        {
            var plane = (TagTool.Common.RealQuaternion)value;
            I = plane.I;
            J = plane.J;
            K = plane.K;
            W = plane.W;
        }

        private void UpdateValue()
        {
            SetActualValue(new TagTool.Common.RealQuaternion(I, J, K, W));
        }

        public void OnIChanged() => UpdateValue();
        public void OnJChanged() => UpdateValue();
        public void OnKChanged() => UpdateValue();
        public void OnWChanged() => UpdateValue();
    }
}
