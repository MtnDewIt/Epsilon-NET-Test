using Epsilon.Shell;
using Epsilon.Shell.TreeModels;
using System.ComponentModel.Composition;

namespace Epsilon
{
    public class TagExplorerViewModel : CacheEditorTool
    {
        public const string ToolName = "Epsilon.TagExplorer";

        public TreeModel TagTree { get; set; }

        public TagExplorerViewModel(TreeModel tagTree)
        {
            Name = ToolName;
            DisplayName = "Tags";
            PreferredLocation = PaneLocation.Left;
            PreferredWidth = 420;    
            TagTree = tagTree;
            IsVisible = true;
            IsActive = true;
        }
        protected override void OnClose()
        {
            base.OnClose();
            TagTree?.Dispose();
            TagTree = null;
        }
    }

    [Export(typeof(ICacheEditorToolProvider))]
    class TagExplorerToolProvider : ICacheEditorToolProvider
    {
        public int SortOrder => -1;

        public ICacheEditorTool CreateTool(ICacheEditor editor)
        {
            return new TagExplorerViewModel(editor.TagTree); 
        }

        public bool ValidForEditor(ICacheEditor editor)
        {
            return true;
        }
    }
}
