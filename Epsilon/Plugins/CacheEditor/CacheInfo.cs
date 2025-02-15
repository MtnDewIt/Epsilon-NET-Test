﻿using System.IO;
using System.Linq;
using TagTool.Cache;
using TagTool.IO;

namespace CacheEditor
{
    class CacheInfo
    {
        public string CacheDisplayName { get; set; }
        public int LocaleCount { get; set; }
        public int StringCount { get; set; }
        public int TagCount { get; private set; }
        public DirectoryInfo CacheDirectory { get; set; }
        public CacheVersion Version { get; set; }
        public EndianFormat Endianess { get; set; }

        public CacheInfo(ICacheFile cacheFile)
        {
            var cache = cacheFile.Cache;
            CacheDisplayName = cache.DisplayName;
            LocaleCount = cache.LocaleTables?.Count ?? 0;
            StringCount = cache.StringTable?.Count ?? 0;
            TagCount = cache.TagCache.TagTable.Count();
            CacheDirectory = cache.Directory;
            Version = cache.Version;
            Endianess = cache.Endianness;
        }

		public override string ToString() {
			return $"Cache: {CacheDisplayName}\n" +
				   $"Directory: {CacheDirectory.FullName}\n" +
				   $"Version: {Version}\n" +
				   $"Tag Count: {TagCount}\n" +
				   $"String Count: {StringCount}\n" +
				   $"Locale Count: {LocaleCount}\n" +
				   $"Endianess: {Endianess}";
		}

	}
}
