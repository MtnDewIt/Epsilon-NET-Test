using CacheEditor;
using CacheEditor.RTE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TagStructEditor.Fields;
using TagTool.Cache;
using TagTool.IO;
using TagTool.Tags;

namespace DefinitionEditor
{
    internal class RTEFieldHelper
    {
        private IRteTarget _target;
        private ICacheFile _cache;
        private Type _mainStructType;
        private CachedTag _tagInstance;

        public RTEFieldHelper(IRteTarget target, ICacheFile cache, Type mainStructType, CachedTag tagInstance)
        {
            _target = target;
            _cache = cache;
            _mainStructType = mainStructType;
            _tagInstance = tagInstance;
        }

        public uint GetFieldMemoryAddress(IField field)
        {
            string fieldPath = GetFieldPath(field);
            


            using (var stream = _target.Provider.CreateStream(_target))
            {
                uint tagAddress = (uint)_target.Provider.GetTagMemoryAddress(stream, _cache.Cache, _tagInstance);
                if (tagAddress == 0)
                    return 0;

                stream.Seek(tagAddress + 0x10, SeekOrigin.Begin);
                var reader = new EndianReader(stream, EndianFormat.LittleEndian);
                uint mainStructOffset = reader.ReadUInt32();

                return GetFieldMemoryAddress(reader, tagAddress + mainStructOffset, _mainStructType, fieldPath);
            }
        }

        private uint GetFieldMemoryAddress(EndianReader reader, uint address, Type type, string path)
        {
            if (path.Length == 0)
                return address;

            var parts = path.Split('.');
            foreach (var part in parts)
            {
                var subparts = part.Split(new[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                var name = subparts[0];

                var index = -1;
                if (subparts.Length > 1)
                    index = int.Parse(subparts[1]);

                var field = TagStructure.GetTagFieldEnumerable(type, _cache.Cache.Version, _cache.Cache.Platform)
                    .FirstOrDefault(x => x.Name == name);

                if (index != -1)
                {
                    if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        reader.SeekTo(address + field.Offset);
                        var count = reader.ReadInt32();
                        var blockAddress = reader.ReadUInt32();
                        address = blockAddress + (uint)index * GetElementSize(field.FieldType.GenericTypeArguments[0]);
                        type = field.FieldType.GenericTypeArguments[0];
                    }
                    else if (field.FieldType.IsArray)
                    {
                        address += field.Offset + (uint)index * GetElementSize(field.FieldType.GetElementType());
                        type = field.FieldType.GetElementType();
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                }
                else if (typeof(TagStructure).IsAssignableFrom(field.FieldType))
                {
                    address += field.Offset;
                    type = field.FieldType;
                }
                else
                {
                    address += field.Offset;
                }
            }

            return address;
        }


        private string GetFieldPath(IField field)
        {
            var parts = new List<string>();
            IField f = field;
            while (f != null)
            {
                if (f is ValueField vf)
                {
                    if (vf is BlockField bf && vf != field)
                        parts.Add($"{vf.FieldInfo.ActualName}[{bf.CurrentIndex}]");
                    else
                        parts.Add(vf.FieldInfo.ActualName);
                }

                f = f.Parent;
            }
            return string.Join(".", parts.AsEnumerable().Reverse().Where(x => !string.IsNullOrEmpty(x)));
        }

        private uint GetElementSize(Type type)
        {
            return TagStructure.GetStructureSize(type, _cache.Cache.Version, _cache.Cache.Platform);
        }
    }
}
