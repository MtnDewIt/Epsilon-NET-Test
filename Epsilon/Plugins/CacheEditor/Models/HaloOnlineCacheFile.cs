using System.IO;
using System.Threading.Tasks;
using TagTool.Cache;
using TagTool.Cache.HaloOnline;

namespace CacheEditor
{
    class HaloOnlineCacheFile : CacheFileBase
    {
        private new GameCacheHaloOnline Cache => (GameCacheHaloOnline) base.Cache;
 
        public HaloOnlineCacheFile(FileInfo file, GameCache cache) : base(file, cache)
        {
        }

        public override bool CanDeleteTag => true;
        public override bool CanExtractTag => true;
        public override bool CanRenameTag => true;
        public override bool CanDuplicateTag => true;
        public override bool CanSerializeTags => true;

        public override void DeleteTag(CachedTag tag)
        {
            using (var stream = Cache.OpenCacheReadWrite())
            {
                Cache.TagCacheGenHO.Tags[tag.Index] = null;
                Cache.TagCacheGenHO.SetTagDataRaw(stream, (CachedTagHaloOnline)tag, new byte[] { });
            }
            Cache.SaveTagNames();

            //base.Reload();
        }

        public override void ExtractTag(CachedTag tag, string filePath)
        {
            using (var stream = Cache.OpenCacheRead())
            {
                var data = Cache.TagCacheGenHO.ExtractTagRaw(stream, (CachedTagHaloOnline)tag);
                using (var outStream = System.IO.File.Create(filePath))
                {
                    outStream.Write(data, 0, data.Length);
                }
            }
        }

        public override void DuplicateTag(CachedTag tag, string newName)
        {
            var newTag = Cache.TagCache.AllocateTag(tag.Group, newName);
            Cache.SaveTagNames();
            using (var stream = Cache.OpenCacheReadWrite())
            {
                var originalDefinition = Cache.Deserialize(stream, tag);
                Cache.Serialize(stream, newTag, originalDefinition);
            }

            //base.Reload();
        }

        public async override Task SerializeTagAsync(CachedTag instance, object definition)
        {
            await Task.Delay(1000);
            //using (var stream = Cache.OpenCacheReadWrite())
                //await Task.Run(() => Cache.Serialize(stream, instance, definition));
        }

        public override void RenameTag(CachedTag tag, string newName)
        {
            tag.Name = newName;
            Cache.SaveTagNames();
        }
    }
}
