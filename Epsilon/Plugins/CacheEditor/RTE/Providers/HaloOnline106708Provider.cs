using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TagTool.Cache;
using TagTool.Cache.Eldorado;
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
            return cacheFile.Cache is GameCacheEldoradoBase;
        }

        private static int ResolveTagIndex(CachedTagHaloOnline instance, GameCacheHaloOnlineBase cache)
        {
            if(cache is GameCacheModPackage modPak)
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
    }
}
