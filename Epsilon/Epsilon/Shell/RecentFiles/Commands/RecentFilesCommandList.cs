using Epsilon.Commands;

namespace Epsilon.Shell.RecentFiles
{
    [ExportCommand]
    class RecentFilesCommandList : CommandListDefinition
    {
        public override string Name => "File.RecentFiles";

        public override string DisplayText => "Recent Files";
    }
}
