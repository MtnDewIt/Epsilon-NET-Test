using System.IO;
using TagTool.Cache;

namespace Epsilon
{
    public class GenericCacheFile : CacheFileBase
    {
        public GenericCacheFile(FileInfo file, GameCache cache) : base(file, cache)
        {
        }
    }
}
