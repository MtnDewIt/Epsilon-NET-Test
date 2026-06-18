using EpsilonLib.Commands;

namespace CacheEditor.Components.TagTree.Commands
{
    [ExportCommand]
    class SetFavoriteTagCommand : CommandDefinition
    {
        public override string Name => "CacheEditor.FavoritedTagUpdate";

        public override string DisplayText => "Update Favorited Tag";
    }
}
