using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Epsilon.Fields;
using Epsilon.Helpers;
using TagTool.Cache;
using TagTool.IO;
using TagTool.Tags;

namespace Epsilon.RTE
{

	public class RTEFieldHelper
	{
		private IRteTarget _target;

		private ICacheFile _cache;

		private Type _mainStructType;

		private CachedTag _tagInstance;

		public RTEFieldHelper(IRteTarget target, ICacheFile cache, Type mainStructType, CachedTag tagInstance) {
			_target = target;
			_cache = cache;
			_mainStructType = mainStructType;
			_tagInstance = tagInstance;
		}

		public uint GetFieldMemoryAddress(IField field) {
			string fieldPath = FieldHelper.GetFieldPath(field);
			using (ProcessMemoryStream stream = _target.Provider.CreateStream(_target)) {
				uint tagAddress = (uint)_target.Provider.GetTagMemoryAddress(stream, _cache.Cache, _tagInstance);
				if (tagAddress == 0) {
					return 0u;
				}
				stream.Seek(tagAddress + 16, SeekOrigin.Begin);
				EndianReader reader = new EndianReader(stream);
				uint mainStructOffset = reader.ReadUInt32();
				return GetFieldMemoryAddress(reader, tagAddress + mainStructOffset, _mainStructType, fieldPath);
			}
		}

		private uint GetFieldMemoryAddress(EndianReader reader, uint address, Type type, string path) {
			if (path.Length == 0) {
				return address;
			}
			string[] parts = path.Split('.');
			string[] array = parts;
			foreach (string part in array) {
				string[] subparts = part.Split(new char[2] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
				string name = subparts[0];
				int index = -1;
				if (subparts.Length > 1) {
					index = int.Parse(subparts[1]);
				}
				TagFieldInfo field = TagStructure.GetTagFieldEnumerable(type, _cache.Cache.Version, _cache.Cache.Platform).FirstOrDefault((TagFieldInfo x) => x.Name == name);
				if (index != -1) {
					if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>)) {
						reader.SeekTo(address + field.Offset);
						int count = reader.ReadInt32();
						uint blockAddress = reader.ReadUInt32();
						address = blockAddress + (uint)( index * (int)GetElementSize(field.FieldType.GenericTypeArguments[0]) );
						type = field.FieldType.GenericTypeArguments[0];
					}
					else {
						if (!field.FieldType.IsArray) {
							throw new NotImplementedException();
						}
						address += (uint)( (int)field.Offset + index * (int)GetElementSize(field.FieldType.GetElementType()) );
						type = field.FieldType.GetElementType();
					}
				}
				else if (typeof(TagStructure).IsAssignableFrom(field.FieldType)) {
					address += field.Offset;
					type = field.FieldType;
				}
				else {
					address += field.Offset;
				}
			}
			return address;
		}

		private uint GetElementSize(Type type) {
			return TagStructure.GetStructureSize(type, _cache.Cache.Version, _cache.Cache.Platform);
		}
	}
}