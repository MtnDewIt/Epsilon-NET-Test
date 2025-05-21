using CacheEditor.RTE;
using CacheEditor;
using System.Collections.Generic;
using System.IO;
using System;
using TagStructEditor.Fields;
using TagStructEditor.Helpers;
using TagTool.Cache;
using TagTool.IO;
using TagTool.Tags;
using System.Net;
using System.Linq;

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
            string fieldPath = FieldHelper.GetFieldPath(field);
            using (ProcessMemoryStream stream = _target.Provider.CreateStream(_target))
            {
                uint tagMemoryAddress = (uint)_target.Provider.GetTagMemoryAddress(stream, _cache.Cache, _tagInstance);

                if (tagMemoryAddress == 0x0)
                    return 0;

                stream.Seek(tagMemoryAddress + 0x10, SeekOrigin.Begin);
                EndianReader reader = new EndianReader(stream, EndianFormat.LittleEndian);
                uint structureOffset = reader.ReadUInt32();

                return GetFieldMemoryAddress(reader, tagMemoryAddress + structureOffset, _mainStructType, fieldPath);
            }
        }

        private uint GetFieldMemoryAddress(EndianReader reader, uint address, Type type, string path)
        {
            if (path.Length == 0)
                return address;

            var parts = path.Split('.');

            foreach (string part in parts)
            {
                string[] subParts = part.Split(new char[2] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);

                string name = subParts[0];

                int index = -1;

                if (subParts.Length > 1)
                    index = int.Parse(subParts[1]);

                TagFieldInfo tagFieldInfo = TagStructure.GetTagFieldEnumerable(type, _cache.Cache.Version, this._cache.Cache.Platform).FirstOrDefault(x => x.Name == name);

                if (index != -1)
                {
                    if (tagFieldInfo.FieldType.IsGenericType && tagFieldInfo.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        reader.SeekTo(address + tagFieldInfo.Offset);
                        reader.ReadInt32();
                        address = reader.ReadUInt32() + (uint)index * GetElementSize(tagFieldInfo.FieldType.GenericTypeArguments[0]);
                        type = tagFieldInfo.FieldType.GenericTypeArguments[0];
                    }
                    else
                    {
                        if (!tagFieldInfo.FieldType.IsArray)
                            throw new NotImplementedException();

                        address += tagFieldInfo.Offset + (uint)index * GetElementSize(tagFieldInfo.FieldType.GetElementType());

                        type = tagFieldInfo.FieldType.GetElementType();
                    }
                }
                else if (typeof(TagStructure).IsAssignableFrom(tagFieldInfo.FieldType))
                {
                    address += tagFieldInfo.Offset;
                    type = tagFieldInfo.FieldType;
                }
                else
                {
                    address += tagFieldInfo.Offset;
                }
            }

            return address;
        }

        private uint GetElementSize(Type type)
        {
            return TagStructure.GetStructureSize(type, _cache.Cache.Version, _cache.Cache.Platform);
        }
    }
}
