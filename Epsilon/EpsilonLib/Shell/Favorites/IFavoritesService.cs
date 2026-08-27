using System;
using System.Collections.Generic;
using TagTool.Cache;

namespace Shared
{
    public interface IFavoritesService
    {
        public event EventHandler<FavoriteChangedEventArgs> FavoriteChanged;
        IDictionary<FavoritesCacheRecord, List<TagRecord>> Favorites { get; }
        bool TryGetCacheFavorites(in GameCache cache, out List<TagRecord> favorites);
        void ToggleCacheFavorite(in GameCache cache, in CachedTag tag);
        bool TagIsFavorited(in GameCache cache, in CachedTag tag);
        public string GetCommandText(bool favorited);
        public bool IsReady { get; }
    }
}
