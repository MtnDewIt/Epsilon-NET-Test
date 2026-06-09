using EpsilonLib.Commands;

namespace CacheEditor.Components.TagTree.Commands
{
    [ExportCommand]
    public class CopyChildTagNamesCommand : CommandDefinition
    {
        public override string Name => "TagTree.CopyChildTagNames";

        public override string DisplayText => "Copy Child Tag Names";
    }
}
