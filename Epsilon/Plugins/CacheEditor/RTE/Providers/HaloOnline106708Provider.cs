using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using TagTool.Cache;
using TagTool.Cache.HaloOnline;
using TagTool.Commands.Editing;
using TagTool.Common;
using TagTool.IO;
using TagTool.Serialization;
using TagTool.Tags;

namespace CacheEditor.RTE.Providers
{
    [Export(typeof(IRteProvider))]
    class HaloOnline106708Provider : IRteProvider
    {
        public IEnumerable<IRteTarget> FindTargets()
        {
            foreach (var process in Process.GetProcessesByName("eldorado"))
            {
                if (PCProcessTarget.TryCreate(this, process, out PCProcessTarget target))
                    yield return target;
            }
        }

        public bool ValidForCacheFile(ICacheFile cacheFile)
        {
            return cacheFile.Cache is GameCacheHaloOnlineBase;
        }

        void IRteProvider.PokeTag(IRteTarget target, GameCache cache, CachedTag instance, object definition, ref byte[] RuntimeTagData)
        {
            PokeTag(target, cache, definition, instance as CachedTagHaloOnline, ref RuntimeTagData);
        }

        private static bool IsPokeableValue(TagFieldAttribute attr, Type type)
        {
            if (type.IsValueType || typeof(IBlamType).IsAssignableFrom(type))
                return true;

            if (type == typeof(string))
                return true;

            if (type == typeof(CachedTag))
                return true;
     
            return false;
        }

        public void PokeValue(IRteTarget target, GameCache cache, CachedTag instance, uint address, TagFieldAttribute attr, Type valueType, object value)
        {
            attr ??= TagFieldAttribute.Default;

            var stream = new MemoryStream();
            var reader = new EndianReader(stream, cache.Endianness);
            var writer = new EndianWriter(stream, cache.Endianness);
            var dataContext = new DataSerializationContext(writer, CacheAddressType.Memory, false);

            using var processStream = CreateStream(target);

            if (!IsPokeableValue(attr, valueType))
                throw new RteProviderException($"Cannot poke fields of type '{valueType}'");

            if (value is CachedTagHaloOnline tagRef)
            {
                int newRuntimeTagIndex = ResolveTagIndex(tagRef.Index, cache);
                CheckTagLoaded(cache, processStream, tagRef.Index, newRuntimeTagIndex);

                value = new CachedTagHaloOnline(tagRef.TagCache, newRuntimeTagIndex, tagRef.Group, tagRef.Name);
            }

            var block = dataContext.CreateBlock();
            cache.Serializer.SerializeValue(dataContext, stream, block, value, attr, valueType);
            byte[] outData = block.Stream.ToArray();

            processStream.Seek(address, SeekOrigin.Begin);
            processStream.Write(outData, 0, outData.Length);
            processStream.Flush();
        }

        private void PokeTag(IRteTarget target, GameCache cache, object definition, CachedTagHaloOnline hoInstance, ref byte[] runtimeTagDataMap)
        {
            var process = Process.GetProcessById((int)target.Id);
            if (process == null)
                throw new RteTargetNotAvailableException(this, "Target process could not be found.");

            using var processStream = new ProcessMemoryStream(process);

            int tagIndex = ResolveTagIndex(hoInstance.Index, cache);
            uint address = GetTagAddress(processStream, tagIndex);
            if (address == 0)
                throw new RteProviderException(this, $"Tag '{hoInstance}' could not be located in the target process.");

            //first get a raw copy of the tag in the cache
            byte[] tagCacheData;
            using (var stream = cache.OpenCacheRead())
            {
                //deserialize the cache def then reserialize to a stream
                var cachedef = cache.Deserialize(stream, hoInstance);
                tagCacheData = SerializeToByteArray(cache, cachedef);
            }

            //then serialize the current version of the tag in the editor
            byte[] editorData = SerializeToByteArray(cache, definition);

            //length should make to make sure the serializer is consistent
            if (tagCacheData.Length != editorData.Length)
                throw new RteProviderException(this, $"Error: tag size changed or the serializer failed!");

            //pause the process during poking to prevent race conditions
            process.Suspend();

            try
            {
                uint runtimeTotalSize = 0;
                uint headerSize = 0;
                List<uint> tagRefFixups = [];
                using (var reader = new EndianReader(processStream))
                {
                    reader.BaseStream.Position = address;
                    ReadHeaderValues(reader, ref runtimeTotalSize, ref headerSize, ref tagRefFixups);
                }

                byte[] runtimeTagData = new byte[runtimeTotalSize - headerSize];
                processStream.Seek(address + headerSize, SeekOrigin.Begin);
                processStream.Read(runtimeTagData, 0, (int)(runtimeTotalSize - headerSize));

                if (runtimeTagData.Length != editorData.Length)
                    throw new RteProviderException(this, $"Error: Loaded tag size did not match cache tag size. Is this tag overwritten in a modpak? Is your modpak built on the most up to date cache?");

                //Store the process data before the first poke so we know which values are runtime values
                if (runtimeTagDataMap.Length == 0)
                {
                    runtimeTagDataMap = new byte[runtimeTagData.Length];
                    for (var i = 0; i < runtimeTagData.Length; i++)
                    {
                        //this will serve as a map of the tag data, with 1 being pokeable fields, and 0 being nonpokeable
                        if (runtimeTagData[i] == tagCacheData[i])
                            runtimeTagDataMap[i] = 1;
                    }
                }

                if (tagCacheData.Length != runtimeTagDataMap.Length)
                    throw new RteProviderException(this, $"Error: Loaded tag has changed size since initial poke! Try closing and reopening the tag.");

                //write diffed bytes only
                int patchedBytes = 0;
                for (var i = 0; i < tagCacheData.Length; i++)
                {
                    //patch anything that isn't a runtime modified field
                    if (runtimeTagDataMap[i] == 1)
                    {
                        runtimeTagData[i] = editorData[i];
                        patchedBytes++;
                    }
                }

                //validate and fixup tagrefs
                foreach (uint tagRefFixup in tagRefFixups)
                {
                    int offset = (int)(tagRefFixup - headerSize);
                    int editorTagIndex = BinaryPrimitives.ReadInt32LittleEndian(editorData.AsSpan(offset));
                    int runtimeTagIndex = BinaryPrimitives.ReadInt32LittleEndian(runtimeTagData.AsSpan(offset));

                    int newRuntimeTagIndex = ResolveTagIndex(editorTagIndex, cache);
                    CheckTagLoaded(cache, processStream, editorTagIndex, newRuntimeTagIndex);
   
                    BinaryPrimitives.WriteInt32LittleEndian(runtimeTagData.AsSpan(offset), newRuntimeTagIndex);
                }

                processStream.Seek(address + headerSize, SeekOrigin.Begin);
                processStream.Write(runtimeTagData, 0, runtimeTagData.Length);
                processStream.Flush();
            }
            finally
            {
                process.Resume();
            }
        }

        private void CheckTagLoaded(GameCache cache, ProcessMemoryStream processStream, int editorTagIndex, int newRuntimeTagIndex)
        {
            if (newRuntimeTagIndex != -1 && GetTagAddress(processStream, newRuntimeTagIndex) == 0)
                throw new RteProviderException(this, $"Error: Tried to poke tag that isn't loaded: {cache.TagCache.GetTag(editorTagIndex)}");
        }

        private static byte[] SerializeToByteArray(GameCache cache, object definition)
        {
            byte[] editorData;
            using (MemoryStream stream = new MemoryStream())
            using (EndianWriter writer = new EndianWriter(stream, EndianFormat.LittleEndian))
            {
                var dataContext = new DataSerializationContext(writer);
                cache.Serializer.Serialize(dataContext, definition);
                StreamUtil.Align(stream, 0x10);
                editorData = stream.ToArray();
            }

            return editorData;
        }

        public int ResolveTagIndex(int tagIndex, GameCache cache)
        {
            if (!cache.TagCache.TryGetCachedTag(tagIndex, out CachedTag tag))
                return -1;

            if (cache is GameCacheModPackage modPak && !((CachedTagHaloOnline)tag).IsEmpty())
            {
                if (modPak.BaseCacheReference.TagCache.TryGetTag(tag.ToString(), out CachedTag baseTag))
                    return baseTag.Index;

                int pakTagCount = 0;
                for (int i = 0; i < tagIndex; i++)
                {
                    if (!modPak.TagCacheGenHO.Tags[i].IsEmpty())
                        pakTagCount++;
                }

                return 0xFFFE - pakTagCount;
            }

            return tagIndex;
        }

        private void ReadHeaderValues(EndianReader reader, ref uint currenttotalsize, ref uint headersize, ref List<uint> TagReferenceFixups)
        {
            reader.Skip(4); // checksum
            currenttotalsize = reader.ReadUInt32();
            var numDependencies = reader.ReadInt16();              // 0x08 int16  dependencies count
            var numDataFixups = reader.ReadInt16();                // 0x0A int16  data fixup count
            var numResourceFixups = reader.ReadInt16();            // 0x0C int16  resource fixup count
            var numTagReferenceFixups = reader.ReadInt16();        // 0x0E int16  tag reference fixup count(was padding)

            reader.BaseStream.Position += 20; //skip to dependencies section
            reader.BaseStream.Position += 4 * numDependencies; //skip dependencies
            reader.BaseStream.Position += 4 * numDataFixups; //skip datafixups
            reader.BaseStream.Position += 4 * numResourceFixups; //skip resourcefixups
            for (var i = 0; i < numTagReferenceFixups; i++)
                TagReferenceFixups.Add(reader.ReadUInt32() - 0x40000000);

            headersize = CalculateMemoryHeaderSize(0x24, numDependencies, numDataFixups, numResourceFixups, numTagReferenceFixups);
        }

        private static uint CalculateMemoryHeaderSize(int TagHeaderSize, int numDependencies, int numDataFixups, int numResourceFixups, int numTagReferenceFixups)
        {
            var size = (uint)(TagHeaderSize + numDependencies * 4 + numDataFixups * 4 + numResourceFixups * 4 + numTagReferenceFixups * 4);
            return (uint)((size + 0xF) & ~0xF);  // align to 0x10
        }

        private static uint GetTagAddress(ProcessMemoryStream stream, int tagIndex)
        {
            // Read the tag count and validate the tag index
            var reader = new BinaryReader(stream);
            reader.BaseStream.Position = 0x22AB008;
            var maxIndex = 0xFFFF;
            if (tagIndex >= maxIndex)
                return 0;

            // Read the tag index table to get the index of the tag in the address table
            reader.BaseStream.Position = 0x22AAFFC;
            var tagIndexTableAddress = reader.ReadUInt32();
            if (tagIndexTableAddress == 0)
                return 0;
            reader.BaseStream.Position = tagIndexTableAddress + tagIndex * 4;
            var addressIndex = reader.ReadInt32();
            if (addressIndex < 0)
                return 0;

            // Read the tag's address in the address table
            reader.BaseStream.Position = 0x22AAFF8;
            var tagAddressTableAddress = reader.ReadUInt32();
            if (tagAddressTableAddress == 0)
                return 0;
            reader.BaseStream.Position = tagAddressTableAddress + addressIndex * 4;
            return reader.ReadUInt32();
        }

        public ProcessMemoryStream CreateStream(IRteTarget target)
        {
            var process = Process.GetProcessById((int)target.Id);
            if (process == null)
                throw new RteTargetNotAvailableException(this, "Target process could not be found.");

            return new ProcessMemoryStream(process);
        }

        public long GetTagMemoryAddress(ProcessMemoryStream stream, GameCache cache, CachedTag instance)
        {
            return GetTagAddress(stream, ResolveTagIndex(instance.Index, cache));
        }
    }
}
