using System;
using System.Collections;
using System.Collections.Generic;
using TagTool.Cache;

namespace Shared
{
    public record FavoritesCacheRecord
    {
        public string CacheFileName { get; init; }
        public DateTime LastOpened { get; set; } = DateTime.Now;

        private readonly int _hashCode;

        public FavoritesCacheRecord(string cacheDisplayName, DateTime lastUsed = default)
        {
            CacheFileName = cacheDisplayName;
            LastOpened = lastUsed;
            _hashCode = CacheFileName.GetHashCode();
        }

        public FavoritesCacheRecord(in GameCache cache) : this(cache.DisplayName)
        {
        }

        public override int GetHashCode() => _hashCode;
        public virtual bool Equals(FavoritesCacheRecord rec)
        {
            return rec is not null && _hashCode == rec.GetHashCode();
        }
    }

    public record struct TagRecord(string TagName)
    {
        public TagRecord(CachedTag tag) : this(tag.ToString())
        {
        }
    }
}
