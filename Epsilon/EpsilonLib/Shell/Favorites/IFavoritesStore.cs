using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shared
{
    public interface IFavoritesStore
    {
        Task StoreRecords(IDictionary<FavoritesCacheRecord, List<TagRecord>> records);
        Task<IDictionary<FavoritesCacheRecord, List<TagRecord>>> FetchRecords();
        public bool Writing { get; }
    }
}
