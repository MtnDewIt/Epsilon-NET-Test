using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System;
using TagTool.Cache.HaloOnline;
using TagTool.Cache;
using TagTool.Commands.Editing;
using TagTool.IO;
using TagTool.Serialization;
using System.Linq;
using System.ComponentModel.Composition;

namespace CacheEditor.RTE.Providers
{
    [Export(typeof(IRteProvider))]
    internal class HaloOnline106708Provider : IRteProvider, IRteTargetSource
    {
        public IEnumerable<IRteTarget> FindTargets()
        {
            Process[] processArray = Process.GetProcessesByName("eldorado");

            for (int index = 0; index < processArray.Length; ++index)
            {
                Process process = processArray[index];

                if (PCProcessTarget.TryCreate(this, process, out var target))
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

        private void PokeTag(IRteTarget target, GameCache cache, object definition, CachedTagHaloOnline hoInstance, ref byte[] RuntimeTagDataMap)
        {
            Process processById = Process.GetProcessById((int)target.Id);

            if (processById == null)
                throw new RteTargetNotAvailableException(this, "Target process could not be found.");

            using (ProcessMemoryStream processStream = new ProcessMemoryStream(processById))
            {
                bool isModPackage = false;
                GameCacheModPackage modpak = null;

                if (cache is GameCacheModPackage)
                {
                    modpak = (GameCacheModPackage)cache;
                    isModPackage = true;
                }

                int tagIndex = ResolveTagIndex(hoInstance.Index, isModPackage, modpak);
                uint tagAddress = GetTagAddress(processStream, tagIndex);

                if (tagAddress == 0x0)
                    throw new RteProviderException(this, $"Tag '{hoInstance}' could not be located in the target process.");

                byte[] tagDataBuffer;

                using (Stream cacheStream = cache.OpenCacheRead())
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        // TODO: pull endian format from cache
                        using (EndianWriter endianWriter = new EndianWriter(memoryStream, EndianFormat.LittleEndian))
                        {
                            object cacheDefinition = cache.Deserialize(cacheStream, hoInstance);
                            DataSerializationContext serializationContext = new DataSerializationContext(endianWriter, CacheAddressType.Memory);
                            cache.Serializer.Serialize(serializationContext, cacheDefinition);
                            StreamUtil.Align(memoryStream, 0x10);
                            tagDataBuffer = memoryStream.ToArray();
                        }
                    }
                }

                byte[] memoryBuffer;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (EndianWriter endianWriter = new EndianWriter(memoryStream, EndianFormat.LittleEndian))
                    {
                        DataSerializationContext serializationContext = new DataSerializationContext(endianWriter, CacheAddressType.Memory);
                        cache.Serializer.Serialize(serializationContext, definition);
                        StreamUtil.Align(memoryStream, 0x10);
                        memoryBuffer = memoryStream.ToArray();
                    }
                }

                if (tagDataBuffer.Length != memoryBuffer.Length)
                    throw new RteProviderException(this, "Error: tag size changed or the serializer failed!");

                if (!isModPackage && tagDataBuffer.Length != hoInstance.TotalSize - hoInstance.CalculateHeaderSize())
                    throw new RteProviderException(this, "Sorry can't poke this specific tag yet (only happens with very rare specific tags), go bug a dev");

                Stopwatch stopwatch = new Stopwatch();

                stopwatch.Start();

                ProcessExtension.Suspend(processById);

                uint currenttotalsize = 0;
                uint headersize = 0;

                List<uint> TagReferenceFixups = new List<uint>();

                using (EndianReader reader = new EndianReader(processStream, EndianFormat.LittleEndian))
                {
                    CachedTagHaloOnline cachedTagHaloOnline = new CachedTagHaloOnline();
                    processStream.Seek(tagAddress + 0x4, SeekOrigin.Begin);
                    ReadHeaderValues(reader, ref currenttotalsize, ref headersize, ref TagReferenceFixups);
                }

                byte[] buffer = new byte[currenttotalsize - headersize];

                processStream.Seek(tagAddress + headersize, SeekOrigin.Begin);
                processStream.Read(buffer, 0, (int)currenttotalsize - (int)headersize);

                if (currenttotalsize - headersize != tagDataBuffer.Length)
                {
                    ProcessExtension.Resume(processById);
                    throw new RteProviderException(this, "Error: Loaded tag size did not match cache tag size. Is this tag overwritten in a modpak? Is your modpak built on the most up to date cache?");
                }

                if (RuntimeTagDataMap.Length == 0)
                {
                    RuntimeTagDataMap = new byte[buffer.Length];

                    for (int i = 0; i < buffer.Length; ++i)
                    {
                        if (buffer[i] == tagDataBuffer[i])
                            RuntimeTagDataMap[i] = 1;
                    }
                }

                if (tagDataBuffer.Length != RuntimeTagDataMap.Length)
                {
                    ProcessExtension.Resume(processById);
                    throw new RteProviderException(this, "Error: Loaded tag has changed size since initial poke! Try closing and reopening the tag.");
                }

                if (isModPackage)
                {
                    List<int> modPackagesIndicies = new List<int>();

                    for (int i = 0; i < modpak.TagCache.Count; ++i)
                    {
                        if (modpak.TagCache.TryGetCachedTag(i, out var cachedTag) && !((CachedTagHaloOnline)cachedTag).IsEmpty())
                            modPackagesIndicies.Add(i);
                    }

                    foreach (uint fixup in TagReferenceFixups)
                    {
                        int editortagref = BitConverter.ToInt32(memoryBuffer, (int)fixup - (int)headersize);

                        if (!modpak.BaseCacheReference.TagCache.TryGetCachedTag(editortagref, out var cachedTag))
                        {
                            editortagref = 0xFFFE - modPackagesIndicies.Count(x => x < editortagref);

                            BitConverter.GetBytes(editortagref).CopyTo(buffer, fixup - headersize);
                        }
                    }
                }

                int bytesPatched = 0;

                for (int i = 0; i < tagDataBuffer.Length; ++i)
                {
                    if (RuntimeTagDataMap[i] == 1)
                    {
                        buffer[i] = memoryBuffer[i];
                        ++bytesPatched;
                    }
                }

                processStream.Seek(tagAddress + headersize, SeekOrigin.Begin);
                processStream.Write(buffer, 0, buffer.Length);
                processStream.Flush();

                ProcessExtension.Resume(processById);

                stopwatch.Stop();

                Console.WriteLine($"Patched {bytesPatched} bytes in {stopwatch.ElapsedMilliseconds / 1000} seconds");
            }
        }

        private static int ResolveTagIndex(int tagindex, bool isModPackage, GameCacheModPackage modpak)
        {
            if (isModPackage && !modpak.BaseCacheReference.TagCache.TryGetCachedTag(tagindex, out var baseCachedTag))
            {
                int modPackagesIndicies = 0;

                for (int i = 0; i < tagindex; ++i)
                {
                    if (modpak.TagCache.TryGetCachedTag(i, out var cachedTag) && !((CachedTagHaloOnline)cachedTag).IsEmpty())
                        ++modPackagesIndicies;
                }

                tagindex = 0xFFFE - modPackagesIndicies;
            }

            return tagindex;
        }

        private void ReadHeaderValues( EndianReader reader, ref uint currenttotalsize, ref uint headersize, ref List<uint> TagReferenceFixups)
        {
            currenttotalsize = reader.ReadUInt32();

            short numDependencies = reader.ReadInt16();
            short numDataFixups = reader.ReadInt16();
            short numResourceFixups = reader.ReadInt16();
            short numTagReferenceFixups = reader.ReadInt16();

            reader.BaseStream.Position += 0x14;
            reader.BaseStream.Position += 4 * numDependencies;
            reader.BaseStream.Position += 4 * numDataFixups;
            reader.BaseStream.Position += 4 * numResourceFixups;

            for (int i = 0; i < numTagReferenceFixups; ++i)
                TagReferenceFixups.Add(reader.ReadUInt32() - 0x40000000);

            headersize = CalculateMemoryHeaderSize(36, numDependencies, numDataFixups, numResourceFixups, numTagReferenceFixups);
        }

        public uint CalculateMemoryHeaderSize(int TagHeaderSize, int numDependencies, int numDataFixups, int numResourceFixups, int numTagReferenceFixups)
        {
            return (uint)(((uint)(TagHeaderSize + numDependencies * 0x4 + numDataFixups * 0x4 + numResourceFixups * 0x4 + numTagReferenceFixups * 0x4) + 0xF) & 0xFFFFFFFFFFFFFFF0);
        }

        private static uint GetTagAddress(ProcessMemoryStream stream, int tagIndex)
        {
            BinaryReader binaryReader = new BinaryReader(stream);

            binaryReader.BaseStream.Position = 0x22AB008;

            if (tagIndex >= ushort.MaxValue)
                return 0;

            binaryReader.BaseStream.Position = 0x22AAFFC;

            uint tagIndexTableAddress = binaryReader.ReadUInt32();

            if (tagIndexTableAddress == 0x0)
                return 0;

            binaryReader.BaseStream.Position = tagIndexTableAddress + tagIndex * 4;

            int addressIndex = binaryReader.ReadInt32();

            if (addressIndex < 0x0)
                return 0;

            binaryReader.BaseStream.Position = 0x22AAFF8;

            uint tagAddressTableAddress = binaryReader.ReadUInt32();

            if (tagAddressTableAddress == 0x0)
                return 0;

            binaryReader.BaseStream.Position = tagAddressTableAddress + addressIndex * 4;

            return binaryReader.ReadUInt32();
        }

        public void DumpData(string filename, byte[] data)
        {
            using (FileStream fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write))
                fileStream.Write(data, 0, data.Length);
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
            GameCacheModPackage modpak = null;

            bool isModPackage = false;

            if (cache is GameCacheModPackage)
            {
                modpak = (GameCacheModPackage)cache;
                isModPackage = true;
            }

            return GetTagAddress(stream, ResolveTagIndex(instance.Index, isModPackage, modpak));
        }
    }
}
