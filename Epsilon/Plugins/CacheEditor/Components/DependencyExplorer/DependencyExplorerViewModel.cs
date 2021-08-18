using EpsilonLib.Commands;
using EpsilonLib.Shell.TreeModels;
using Stylet;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TagTool.Cache;
using TagTool.Cache.HaloOnline;

namespace CacheEditor.Components.DependencyExplorer
{
    class DependencyExplorerViewModel : CacheEditorTool
    {
        public const string ToolName = "CacheEditor.DependencyExplore";

        private ICacheEditor _editor;

        public DependencyExplorerViewModel(ICacheEditor editor)
        {
            _editor = editor;

            Name = ToolName;
            DisplayName = "Dependency Explorer";
            PreferredLocation = EpsilonLib.Shell.PaneLocation.Right;
            PreferredWidth = 450;
            var tagTree = (editor.TagTree as Components.TagTree.TagTreeViewModel);
            tagTree.NodeSelected += TagTree_NodeSelected;
        }

        public override bool InitialAutoHidden => true;

        public IObservableCollection<DependencyItem> Dependencies { get; } = new BindableCollection<DependencyItem>();
        public IObservableCollection<DependencyItem> Dependents { get; } = new BindableCollection<DependencyItem>();


        private void TagTree_NodeSelected(object sender, TreeNodeEventArgs e)
        {
            if (e.Node.Tag is CachedTagHaloOnline instance)
                Populate(instance);
        }

        internal void OnItemDoubleClicked(DependencyItem item)
        {
            _editor.OpenTag(item.Tag);
            if (item.Tag is CachedTagHaloOnline instance)
                Populate(instance);
        }

        private void Populate(CachedTagHaloOnline instance)
        {
            var cache = _editor.CacheFile.Cache;

            var dependencies = instance.Dependencies
                .Select(tagIndex => cache.TagCache.GetTag(tagIndex))
                .Select(CreateItem);

            var dependents = cache.TagCache.NonNull()
                .Cast<CachedTagHaloOnline>()
                .Where(tag => tag.Dependencies.Contains(instance.Index))
                .Select(CreateItem);

            Dependencies.Clear();
            Dependencies.AddRange(dependencies);

            Dependents.Clear();
            Dependents.AddRange(dependents);
        }

        private DependencyItem CreateItem(CachedTag instance)
        {
            return new DependencyItem() { Tag = instance, DisplayName = $"{instance.Name}.{instance.Group.Tag}" };
        }

        public class DependencyItem
        {
            public CachedTag Tag { get; set; }
            public string DisplayName { get; set; }

            public ICommand CopyTagNameCommand { get; set; }

            public DependencyItem()
            {
                CopyTagNameCommand = new DelegateCommand(CopyTagName_Execute);
            }

            private void CopyTagName_Execute()
                => Clipboard.SetText(Tag.ToString());
        }
    }
}
