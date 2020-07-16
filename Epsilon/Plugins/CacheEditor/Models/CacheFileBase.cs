using System;
using System.IO;
using System.Threading.Tasks;
using TagTool.Cache;

namespace CacheEditor
{
    class CacheFileBase : ICacheFile
    {
        public event EventHandler Reloaded;

        public CacheFileBase(FileInfo file, GameCache cache)
        {
            File = file;
            Cache = cache;
        }

        protected void Reload()
        {
            Cache = GameCache.Open(File);
            Reloaded?.Invoke(this, EventArgs.Empty);
        }

        public FileInfo File { get; }
        public GameCache Cache { get; protected set; }

        public virtual bool CanExtractTag => false;
        public virtual bool CanRenameTag => false;
        public virtual bool CanDeleteTag => false;
        public virtual bool CanDuplicateTag => false;
        public virtual bool CanSerializeTags => false;

        public virtual void DeleteTag(CachedTag tag) => throw new NotSupportedException();
        public virtual void ExtractTag(CachedTag tag, string filePath) => throw new NotSupportedException();
        public virtual void RenameTag(CachedTag tag, string newName) => throw new NotSupportedException();
        public virtual void DuplicateTag(CachedTag tag, string newName) => throw new NotSupportedException();
        public virtual Task SerializeTagAsync(CachedTag instance, object definition) => throw new NotSupportedException();
    }
}
