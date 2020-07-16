using System.Collections.Generic;

namespace EpsilonLib.Shell.TreeModels
{
    public interface ITreeNode
    {
        object Tag { get; }
        bool IsSelected { get; set; }
        bool IsExpanded { get; set; }

        IEnumerable<ITreeNode> Children { get; }
    }
}
