using CacheEditor;
using EpsilonLib.Commands;
using Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using TagTool.Cache;

namespace CacheEditor.Commands
{
    [ExportCommandHandler]
    class FavoriteTagsCommandListHandler : 
        ICommandHandler<FavoriteTagsCommandList>, 
        ICommandListPopulator<FavoriteTagsCommandList>
    {
        private readonly Lazy<ICacheEditingService> _cacheEditingService;
        private readonly Lazy<IFavoritesService> _favoritesService;

        [ImportingConstructor]
        public FavoriteTagsCommandListHandler(Lazy<ICacheEditingService> cacheEditingService, Lazy<IFavoritesService> favoritesService)
        {
            _cacheEditingService = cacheEditingService;
            _favoritesService = favoritesService;
        }

        private ICacheEditor ActiveEditor => _cacheEditingService.Value?.ActiveCacheEditor;

        public void ExecuteCommand(Command command)
        {
            if (command.Tag is not null)
                ActiveEditor.OpenTag((CachedTag)command.Tag);
        }

        public IEnumerable<Command> PopulateCommandList(Command command)
        {
            var cache = ActiveEditor.CacheFile.Cache;
            if(_favoritesService.Value.Favorites.TryGetValue(new(cache.DisplayName), out var favoriteList))
            {
                foreach (var record in favoriteList)
                {
                    string displayText = "null";
                    if (cache.TagCache.TryGetCachedTag(record.TagName, out CachedTag tag))
                        displayText = $"{tag.Group.Tag} - " + tag.Name.Replace("_", "__");

                    yield return new Command(command.Definition) { 
                        RequiresUpdate = true, 
                        Tag = tag,
                        IsEnabled = tag is not null,
                        DisplayText = displayText };
                }
            }
            else
            {
                yield return new Command(command.Definition) { IsEnabled = false, DisplayText = "(empty)" };
            }
        }

        public void UpdateCommand(Command command)
        {
            var cache = ActiveEditor?.CacheFile?.Cache as GameCache;

            if (command.Tag == null)
                command.IsVisible = cache != null;
        }
    }
}
