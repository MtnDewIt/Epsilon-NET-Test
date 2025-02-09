using Epsilon;
using System.IO;
using System.Threading.Tasks;
using TagTool.Cache;

namespace Epsilon
{
    internal class ModPackageCacheFile : HaloOnlineCacheFile
    {
        private new GameCacheModPackage Cache => (GameCacheModPackage)base.Cache;

        public ModPackageCacheFile(FileInfo file, GameCacheModPackage cache) : base(file, cache) { }

        public async override Task SerializeTagAsync(CachedTag instance, object definition)
        {
            await base.SerializeTagAsync(instance, definition);

            // Temporary
            await Task.Run(() => Cache.SaveModPackage(File));
        }
    }
}