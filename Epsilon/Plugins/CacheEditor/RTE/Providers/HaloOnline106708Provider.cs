using EpsilonLib.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
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


        void IRteProvider.PokeTag(IRteTarget target, GameCache cache, CachedTag instance, object definition)
        {
            PokeTag(target, cache, definition, instance as CachedTagHaloOnline);
        }

        private void PokeTag(IRteTarget target, GameCache cache, object definition, CachedTagHaloOnline hoInstance)
        {
            var process = Process.GetProcessById((int)target.Id);
            if (process == null)
                throw new RteTargetNotAvailableException(this, "Target process could not be found.");

            using (var processStream = new ProcessMemoryStream(process))
            {
                int tagindex = hoInstance.Index;
                #if DEBUG
                {
                    if (cache is GameCacheModPackage modpackage && 
                        !modpackage.BaseCacheReference.TagCache.TryGetTag($"{hoInstance.Name}.{hoInstance.Group}", out var baseTag))
                    {
                        tagindex = 0xFFFE - tagindex;
                    }
                }
                #endif

                var address = GetTagAddress(processStream, tagindex);
                if (address == 0)
                    throw new RteProviderException(this, $"Tag '{hoInstance}' could not be located in the target process.");

                //first get a raw copy of the tag in the cache
                byte[] tagcachedata;
                using (var stream = cache.OpenCacheRead())
                using (var outstream = new MemoryStream())
                using (EndianWriter writer = new EndianWriter(outstream, EndianFormat.LittleEndian))
                {
                    //deserialize the cache def then reserialize to a stream
                    var cachedef = cache.Deserialize(stream, hoInstance);
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
                if (tagcachedata.Length != hoInstance.TotalSize - hoInstance.CalculateHeaderSize())
                {
                    throw new RteProviderException(this, $"Sorry can't poke this specific tag yet (only happens with very rare specific tags), go bug a dev");
                }

                //pause the process during poking to prevent race conditions
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                process.Suspend();

                //write diffed bytes only
                int patchedbytes = 0;
                int headersize = (int)hoInstance.CalculateHeaderSize();
                for (var i = 0; i < editordata.Length; i++)
                {
                    if (editordata[i] != tagcachedata[i])
                    {
                        processStream.Seek(address + headersize + i, SeekOrigin.Begin);
                        processStream.WriteByte(editordata[i]);
                        patchedbytes++;
                    }
                }
                processStream.Flush();

                process.Resume();
                stopWatch.Stop();

                Console.WriteLine($"Patched {patchedbytes} bytes in {stopWatch.ElapsedMilliseconds / 1000.0f} seconds");
            }
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
    }
}
