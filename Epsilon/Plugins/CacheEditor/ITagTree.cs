using EpsilonLib.Shell.TreeModels;

namespace CacheEditor
{
    public interface ITagTree
    {
        ITreeNode SelectedNode { get; }

        void UpdateNodeAppearance(ITagTree node);
    }
}
