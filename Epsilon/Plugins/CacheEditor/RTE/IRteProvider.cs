using System.IO;
using TagTool.Cache;
using TagTool.IO;

namespace CacheEditor.RTE
{
    public interface IRteProvider : IRteTargetSource
    {
        bool ValidForCacheFile(ICacheFile cacheFile);
        ProcessMemoryStream CreateStream(IRteTarget target);
        long GetTagMemoryAddress(ProcessMemoryStream stream, GameCache cache, CachedTag instance);
    }
}
