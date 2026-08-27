using EpsilonLib.Commands;

namespace CacheEditor
{
    [ExportCommand]
    class FavoriteTagsCommandList : CommandListDefinition
    {
        public override string Name => "Favorites";

        public override string DisplayText => "Favorites";
    }
}
