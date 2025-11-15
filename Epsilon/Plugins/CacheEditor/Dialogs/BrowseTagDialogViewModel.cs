using CacheEditor.Components.TagTree;
using EpsilonLib.Shell.TreeModels;
using Stylet;
using System;
using TagTool.Cache;

namespace CacheEditor
{
    class BrowseTagDialogViewModel : Screen
    {
        public TagTreeViewModel TagTree { get; }

        public BrowseTagDialogViewModel(ICacheEditingService cacheEditingService, ICacheFile cacheFile, BrowseTagOptions options)
        {
            Func<CachedTag, bool> filter = options.ValidGroups.Length == 0 ? null : (tag => tag.IsInGroup(options.ValidGroups));
            TagTree = new TagTreeViewModel(cacheEditingService, cacheFile, filter: filter);

            TagTree.NodeDoubleClicked += TagTree_NodeDoubleClicked;
            DisplayName = "Tag Browser";
        }

        private void TagTree_NodeDoubleClicked(object sender, TreeNodeEventArgs e)
        {
            if(e.Node.Tag is CachedTag)
                RequestClose(true);
        }
    }
}