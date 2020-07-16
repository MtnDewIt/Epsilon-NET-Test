using System;
using System.Collections.Generic;
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
                yield return new PCProcessTarget(this, process);
            }   
        }

        public bool ValidForCacheFile(ICacheFile cacheFile)
        {
            return cacheFile.Cache.Version == CacheVersion.HaloOnline106708;
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
                var address = GetTagAddress(processStream, hoInstance.Index);
                if (address == 0)
                    throw new RteProviderException(this, $"Tag '{hoInstance}' could not be located in the target process.");

                // TODO: RuntimeSerializationContext should throw exceptions when it fails instead of just logging
                var runtimeContext = new RuntimeSerializationContext(
                    cache,
                    processStream,
                    address,
                    hoInstance.Offset,
                    hoInstance.CalculateHeaderSize(),
                    hoInstance.TotalSize);

                //pause the process during poking to prevent race conditions
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                process.Suspend();
                cache.Serializer.Serialize(runtimeContext, definition);
                process.Resume();
                stopWatch.Stop();

                Console.WriteLine($"Poked tag at 0x{address.ToString("X8")} in {stopWatch.ElapsedMilliseconds / 1000.0f} seconds");
            }
        }

        private static uint GetTagAddress(ProcessMemoryStream stream, int tagIndex)
        {
            // Read the tag count and validate the tag index
            var reader = new BinaryReader(stream);
            reader.BaseStream.Position = 0x22AB008;
            var maxIndex = reader.ReadInt32();
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
