﻿using System;
using System.Collections.Generic;
using Epsilon.Fields;

namespace Epsilon
{
	class BlockEditorViewModel : IField
    {
        public BlockField Field { get; set; }

        public IField Template => Field.Template;
        public List<string> Elements { get; set; }

        public BlockEditorViewModel(BlockField field)
        {
            Field = field;
        }

        public override void Populate(object owner, object value = null)
        {
            throw new NotImplementedException();
        }

        public override void Accept(IFieldVisitor visitor)
        {
            Field.Accept(visitor);
        }
    }
}
