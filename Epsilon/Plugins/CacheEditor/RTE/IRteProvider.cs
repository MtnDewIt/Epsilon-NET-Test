using TagTool.Cache;
using TagTool.Cache.HaloOnline;
using TagTool.IO;

namespace CacheEditor.RTE
{
    public interface IRteProvider : IRteTargetSource
    {
        bool ValidForCacheFile(ICacheFile cacheFile);
        void PokeDefinition(IRteTarget target, GameCache cache, CachedTagHaloOnline instance, object definition, ref byte[] runtimeTagData);
        ProcessMemoryStream CreateStream(IRteTarget target);
        long GetTagMemoryAddress(ProcessMemoryStream stream, GameCache cache, CachedTag instance);
    }
}
