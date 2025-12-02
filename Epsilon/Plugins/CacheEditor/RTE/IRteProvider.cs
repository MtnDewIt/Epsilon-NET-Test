using System.IO;
using TagTool.Cache;
using TagTool.IO;

namespace CacheEditor.RTE
{
    public interface IRteProvider : IRteTargetSource
    {
        bool ValidForCacheFile(ICacheFile cacheFile);
        void PokeTag(IRteTarget target, GameCache cache, CachedTag instance, object definition, ref byte[] RuntimeTagData);
        ProcessMemoryStream CreateStream(IRteTarget target);
        long GetTagMemoryAddress(ProcessMemoryStream stream, GameCache cache, CachedTag instance);
    }
}
