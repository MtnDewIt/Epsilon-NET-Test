﻿using System;
using static TagTool.Tags.Definitions.Effect.Event.ParticleSystem.Emitter.RuntimeMGpuData;
using static TagTool.Tags.ParticlePropertyScalar;

namespace Epsilon.Fields
{
	public class EmitterGpuInnardsZField : ValueField
    {
        public OutputModifierValue ModifierIndex { get; set; }
        public ParticleStates InputIndexModifier { get; set; }
        public byte FunctionIndexGreen { get; set; }

        public static readonly Type OutputModifierValueEnum = typeof(OutputModifierValue);
        public static readonly Type ParticleStatesEnum = typeof(ParticleStates);

        public EmitterGpuInnardsZField(ValueFieldInfo info) : base(info)
        {
        }

        public override void Accept(IFieldVisitor visitor)
        {
            return;
        }

        protected override void OnPopulate(object value)
        {
            var innardsZ = (Property.InnardsZ)value;
            ModifierIndex = innardsZ.ModifierIndex;
            InputIndexModifier = innardsZ.InputIndexModifier;
            FunctionIndexGreen = innardsZ.FunctionIndexGreen;
        }

        protected void OnModifierIndexChanged() => UpdateValue();
        protected void OnInputIndexModifierChanged() => UpdateValue();
        protected void OnFunctionIndexGreenChanged() => UpdateValue();

        void UpdateValue()
        {
            SetActualValue(new Property.InnardsZ() { ModifierIndex = ModifierIndex, InputIndexModifier = InputIndexModifier, FunctionIndexGreen = FunctionIndexGreen });
        }
    }
}
