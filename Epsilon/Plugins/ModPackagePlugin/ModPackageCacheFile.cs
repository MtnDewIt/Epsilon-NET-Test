using CacheEditor;
using System.IO;
using TagTool.Cache;

namespace ModPackagePlugin
{
    internal class ModPackageCacheFile : CacheFileBase
    {
        public ModPackageCacheFile(FileInfo file, GameCache cache) : base(file, cache)
        {
        }
    }
}