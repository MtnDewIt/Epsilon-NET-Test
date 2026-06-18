using System;
using TagTool.Cache;

namespace Shared;

public class FavoriteChangedEventArgs : EventArgs
{
    /// <summary>
    /// The cache whose favorites were modified
    /// </summary>
    public string CacheName { get; set; }

    /// <summary>
    /// The tag affected
    /// </summary>
    public CachedTag Tag { get; set; }

    /// <summary>
    /// The result of the change
    /// </summary>
    public bool Favorited { get; set; }

    public FavoriteChangedEventArgs(string cacheName, CachedTag tag, bool isFavorited)
    {
        CacheName = cacheName;
        Tag = tag;
        Favorited = isFavorited;
    }
}
