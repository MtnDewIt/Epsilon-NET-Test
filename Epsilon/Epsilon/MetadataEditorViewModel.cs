using Epsilon;
using TagTool.Cache;

namespace Epsilon
{
    class MetadataEditorViewModel : CacheEditorTool
    {
        public const string ToolName = "ModPackage.MetadataEditorTool";

        public MetadataEditorViewModel(ModPackage package)
        {
            DisplayName = "Metadata";
            PreferredLocation = Epsilon.Shell.PaneLocation.Right;
            PreferredWidth = 420;
            Name = ToolName;
            Metadata = new MetadataModel(package.Metadata, package.Header);
            IsVisible = true;
        }

        public MetadataModel Metadata { get; }

        public override bool InitialAutoHidden => true;
    }
}
