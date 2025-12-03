using System;
using System.IO;
using TagTool.Cache;
using TagTool.IO;
using TagTool.Tags;

namespace CacheEditor.RTE
{
    public interface IRteProvider : IRteTargetSource
    {
        bool ValidForCacheFile(ICacheFile cacheFile);
        void PokeTag(IRteTarget target, GameCache cache, CachedTag instance, object definition, ref byte[] RuntimeTagData);
        void PokeValue(IRteTarget target, GameCache cache, CachedTag instance, uint address, TagFieldAttribute attr, Type valueType, object value);
        ProcessMemoryStream CreateStream(IRteTarget target);
        long GetTagMemoryAddress(ProcessMemoryStream stream, GameCache cache, CachedTag instance);
    }
}
