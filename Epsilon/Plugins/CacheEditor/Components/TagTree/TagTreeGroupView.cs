using EpsilonLib.Shell.TreeModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TagTool.Cache;
using TagTool.Tags;

namespace CacheEditor.Components.TagTree
{
    class TagTreeGroupView : ITagTreeViewMode
    {
        public IEnumerable<ITreeNode> BuildTree(GameCache cache, Func<CachedTag, bool> filter)
        {
           return cache.TagCache
                .NonNull()
             .Where(filter)
             .GroupBy(tag => tag.Group, CreateTagNode)
             .Select(group => CreateGroupNode(cache, group))
             .OrderBy(node => node.Text);
        }

        private TagTreeTagNode CreateTagNode(CachedTag tag)
        {
            return new TagTreeTagNode(tag, () => FormatName(tag));
        }

        private TagTreeGroupNode CreateGroupNode(GameCache cache, IGrouping<TagGroup, TagTreeNode> group)
        {
            var groupNode = new TagTreeGroupNode()
            {
                Text = group.Key.ToString(),
                Tag = group.Key,
                Children = new ObservableCollection<ITreeNode>(group.OrderBy(node => node.Text))
            };
            foreach (var node in group)
                node.Parent = groupNode;

            return groupNode;
        }

        private string FormatName(CachedTag tag)
        {
            return tag.Name;
        }
    }

    class TagTreeGroupNode : TagTreeNode
    {

    }
}
