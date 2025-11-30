using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TagTool.Cache;
using TagTool.Cache.HaloOnline;
using TagTool.Commands.Editing;
using TagTool.IO;
using TagTool.Serialization;

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

        private static int ResolveTagIndex(CachedTagHaloOnline instance, GameCacheHaloOnlineBase cache)
        {
            if (cache is GameCacheModPackage modPak)
            {
                if (modPak.BaseCacheReference.TagCache.TryGetTag(instance.ToString(), out CachedTag baseInstance))
                {
                    return baseInstance.Index;
                }
                else
                {
                    int paktagcount = 0;
                    for (var i = 0; i < instance.Index; i++)
                    {
                        if (!cache.TagCacheGenHO.Tags[i].IsEmpty())
                            paktagcount++;
                    }
                    return 0xFFFE - paktagcount;
                }
            }

            return instance.Index;
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
            return GetTagAddress(stream, ResolveTagIndex((CachedTagHaloOnline)instance, (GameCacheHaloOnlineBase)cache));
        }
        
        // We technically shouldn't need these but since we don't track the modified fields in every plugin context, its easier if we just poke the whole definition to memory
        public void PokeDefinition(IRteTarget target, GameCache cache, CachedTagHaloOnline instance, object definition, ref byte[] runtimeTagData)
        {
            var process = Process.GetProcessById((int)target.Id);
            if (process == null)
                throw new RteTargetNotAvailableException(this, "Target process could not be found.");

            using (var processStream = new ProcessMemoryStream(process))
            {
                bool isModPackage = false;
                //bool isModPackageTag = false;
                GameCacheModPackage modpak = null;
                if (cache is GameCacheModPackage)
                {
                    modpak = (GameCacheModPackage)cache;
                    isModPackage = true;
                }

                var tagindex = ResolveTagIndex(instance, modpak);
                var address = GetTagAddress(processStream, tagindex);
                if (address == 0)
                    throw new RteProviderException(this, $"Tag '{instance}' could not be located in the target process.");

                //first get a raw copy of the tag in the cache
                byte[] tagcachedata;
                using (var stream = cache.OpenCacheRead())
                using (var outstream = new MemoryStream())
                using (EndianWriter writer = new EndianWriter(outstream, EndianFormat.LittleEndian))
                {
                    //deserialize the cache def then reserialize to a stream
                    var cachedef = cache.Deserialize(stream, instance);
                    var dataContext = new DataSerializationContext(writer);
                    cache.Serializer.Serialize(dataContext, cachedef);
                    StreamUtil.Align(outstream, 0x10);
                    tagcachedata = outstream.ToArray();
                }

                //then serialize the current version of the tag in the editor
                byte[] editordata;
                using (MemoryStream stream = new MemoryStream())
                using (EndianWriter writer = new EndianWriter(stream, EndianFormat.LittleEndian))
                {
                    var dataContext = new DataSerializationContext(writer);
                    cache.Serializer.Serialize(dataContext, definition);
                    StreamUtil.Align(stream, 0x10);
                    editordata = stream.ToArray();
                }

                //length should make to make sure the serializer is consistent
                if (tagcachedata.Length != editordata.Length)
                {
                    throw new RteProviderException(this, $"Error: tag size changed or the serializer failed!");
                }

                //some very rare tags have a size that doesn't match our serialized version, need to fix root cause
                if (!isModPackage && tagcachedata.Length != instance.TotalSize - instance.CalculateHeaderSize())
                {
                    throw new RteProviderException(this, $"Sorry can't poke this specific tag yet (only happens with very rare specific tags), go bug a dev");
                }

                process.Suspend();

                uint currenttotalsize = 0;
                uint headersize = 0;
                List<uint> TagReferenceFixups = new List<uint>();
                using (EndianReader reader = new EndianReader(processStream))
                {
                    CachedTagHaloOnline runtimetag = new CachedTagHaloOnline();
                    processStream.Seek(address + 4, SeekOrigin.Begin);
                    ReadHeaderValues(reader, ref currenttotalsize, ref headersize, ref TagReferenceFixups);
                }

                byte[] CurrentRuntimeTagData = new byte[currenttotalsize - headersize];
                processStream.Seek(address + headersize, SeekOrigin.Begin);
                processStream.ReadExactly(CurrentRuntimeTagData, 0, (int)(currenttotalsize - headersize));

                if (currenttotalsize - headersize != tagcachedata.Length)
                {
                    process.Resume();
                    throw new RteProviderException(this, $"Error: Loaded tag size did not match cache tag size. Is this tag overwritten in a modpak? Is your modpak built on the most up to date cache?");
                }

                //Store the process data before the first poke so we know which values are runtime values
                if (runtimeTagData.Length == 0)
                {
                    runtimeTagData = new byte[CurrentRuntimeTagData.Length];
                    for (var i = 0; i < CurrentRuntimeTagData.Length; i++)
                    {
                        //this will serve as a map of the tag data, with 1 being pokeable fields, and 0 being nonpokeable
                        if (CurrentRuntimeTagData[i] == tagcachedata[i])
                            runtimeTagData[i] = 1;
                    }
                }

                if (tagcachedata.Length != runtimeTagData.Length)
                {
                    process.Resume();
                    throw new RteProviderException(this, $"Error: Loaded tag has changed size since initial poke! Try closing and reopening the tag.");
                }

                //write diffed bytes only
                int patchedbytes = 0;
                for (var i = 0; i < tagcachedata.Length; i++)
                {
                    //patch anything that isn't a runtime modified field
                    if (runtimeTagData[i] == 1)
                    {
                        CurrentRuntimeTagData[i] = editordata[i];
                        patchedbytes++;
                    }
                }

                processStream.Seek(address + headersize, SeekOrigin.Begin);
                processStream.Write(CurrentRuntimeTagData, 0, CurrentRuntimeTagData.Length);
                processStream.Flush();

                process.Resume();
            }
        }

        private static void ReadHeaderValues(EndianReader reader, ref uint currenttotalsize, ref uint headersize, ref List<uint> TagReferenceFixups)
        {
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
    }
}
