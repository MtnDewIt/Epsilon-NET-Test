using CacheEditor;
using TagTool.Cache;

namespace ModPackagePlugin
{
    class MetadataEditorViewModel : CacheEditorTool
    {
        public const string ToolName = "ModPackage.MetadataEditorTool";

        public MetadataEditorViewModel(ModPackage package)
        {
            DisplayName = "Metadata";
            PreferredLocation = EpsilonLib.Shell.PaneLocation.Right;
            PreferredWidth = 420;
            Name = ToolName;
            Metadata = new MetadataModel(package.Metadata);
            IsVisible = true;
        }

        public MetadataModel Metadata { get; }

        public override bool InitialAutoHidden => true;
    }
}
