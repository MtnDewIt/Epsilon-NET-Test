using System.Collections.Generic;
using System.IO;
using TagTool.Cache;
using TagTool.Serialization;

namespace CacheEditor.RTE
{
    public interface IRteProvider : IRteTargetSource
    {
        bool ValidForCacheFile(ICacheFile cacheFile);
        void PokeTag(IRteTarget target, GameCache cache, CachedTag instance, object definition, ref byte[] RuntimeTagData);
    }
}
