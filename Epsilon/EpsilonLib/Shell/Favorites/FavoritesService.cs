using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using TagTool.Cache;
using TagTool.Serialization;

namespace Shared
{
    public class FavoritesService : IFavoritesService
    {
        public event EventHandler<FavoriteChangedEventArgs> FavoriteChanged;

        private readonly IFavoritesStore _store;
        private Dictionary<FavoritesCacheRecord, List<TagRecord>> _favorites;
        public IDictionary<FavoritesCacheRecord, List<TagRecord>> Favorites => _favorites;
        public bool IsReady { get => !_store.Writing; }


        [ImportingConstructor]
        public FavoritesService(IFavoritesStore store)
        {
            _store = store;
        }

        public async Task InitAsync()
        {
            _favorites = new(await _store.FetchRecords());
        }

        public void ToggleCacheFavorite(in GameCache cache, in CachedTag tag)
        {
            TagRecord record = new(tag);

            if (TryGetCacheFavorites(cache, out List<TagRecord> tagRecords))
            {
                if (!tagRecords.Remove(record))
                    tagRecords.Add(record);
            }
            else
            {
                _favorites[new(GetCacheId(cache))] = [record];
            }
            
            _store.StoreRecords(_favorites);
            OnFavoriteChanged(cache, tag);
        }

        protected virtual void OnFavoriteChanged(in GameCache cache, in CachedTag tag)
        {
            bool favorited = TagIsFavorited(cache, tag);

            FavoriteChangedEventArgs eventArgs = new(GetCacheId(cache), tag, favorited);
            FavoriteChanged?.Invoke(this, eventArgs);
        }

        public bool TagIsFavorited(in GameCache cache, in CachedTag tag)
        {
            if (TryGetCacheFavorites(cache, out List<TagRecord> tagRecords))
            {
                if (tagRecords.Contains(new(tag.ToString())))
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryGetCacheFavorites(in GameCache cache, out List<TagRecord> tagRecords)
        {
            FavoritesCacheRecord testCache = new(GetCacheId(cache));
            return _favorites.TryGetValue(testCache, out tagRecords);
        }

        private string GetCacheId(in GameCache cache)
        {
            return cache.DisplayName;
        }

        public string GetCommandText(bool favorited) => favorited ? "Unfavorite" : "Add to Favorites";
    }
}
