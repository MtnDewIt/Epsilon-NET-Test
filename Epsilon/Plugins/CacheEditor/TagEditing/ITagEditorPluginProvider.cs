using System.Threading.Tasks;

namespace CacheEditor
{
    public interface ITagEditorPluginProvider
    {
        string DisplayName { get; }

        int SortOrder { get; }

        Task<ITagEditorPlugin> CreateAsync(TagEditorContext context);
    }
}
