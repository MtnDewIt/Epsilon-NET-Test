using EpsilonLib.Shell.TreeModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TagTool.Cache;

namespace CacheEditor.Components.TagTree
{
    class TagTreeFolderView : ITagTreeViewMode
    {
        public IEnumerable<ITreeNode> BuildTree(GameCache cache, Func<CachedTag, bool> filter)
        {
            var tree = new List<ITreeNode>();

            var tags = cache.TagCache
                .NonNull()
                .Where(filter)
                .OrderBy(tag => tag.Name);

            foreach (var tag in tags)
                AddTag(tree, tag);

            return tree;
        }

        private void AddTag(IList<ITreeNode> roots, CachedTag tag)
        {
            var segments = tag.Name.Split('\\');
            for(int i = 0; i < segments.Length; i++)
            {
                if (i < segments.Length - 1)
                {
                    var node = FindNodeWithText(roots, segments[i]);
                    if(node == null)
                    {
                        node = CreateFolderNode(segments[i]);
                        roots.Insert(0, node);
                    }

                    roots = node.Children;
                }
                else
                {
                    var node = CreateTagNode(tag);
                    roots.Add(node);
                }
            }
        }

        private TagTreeNode FindNodeWithText(IList<ITreeNode> nodes, string text)
        {
            for (int j = 0; j < nodes.Count; j++)
                if (nodes[j] is TagTreeNode n && n.Text == text)
                    return n;

            return null;
        }

        private TagTreeFolderNode CreateFolderNode(string name)
        {
            return new TagTreeFolderNode() { Text = name };
        }

        private TagTreeTagNode CreateTagNode(CachedTag tag)
        {
            return new TagTreeTagNode(tag, () => FormatName(tag));
        }

        private string FormatName(CachedTag tag)
        {
            var fileName = Path.GetFileName(tag.Name);
            return $"{fileName}.{tag.Group}";
        }
    }

    interface ITagNameFormatter
    {
        string Format(CachedTag tag);
    }

    public class TagTreeFolderNode : TagTreeNode { }
}
