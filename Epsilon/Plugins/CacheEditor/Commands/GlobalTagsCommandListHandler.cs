using CacheEditor;
using EpsilonLib.Commands;
using EpsilonLib.Dialogs;
using Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;
using TagTool.Cache;
using TagTool.Cache.Gen3;
using TagTool.Cache.HaloOnline;
using TagTool.Tags.Definitions;

namespace CacheEditor.Commands
{
    [ExportCommandHandler]
    class GlobalTagsCommandListHandler : 
        ICommandHandler<GlobalTagsCommandList>, 
        ICommandListPopulator<GlobalTagsCommandList>
    {
        private readonly Lazy<ICacheEditingService> _cacheEditingService;

        [ImportingConstructor]
        public GlobalTagsCommandListHandler(Lazy<ICacheEditingService> cacheEditingService)
        {
            _cacheEditingService = cacheEditingService;
        }

        private ICacheEditor ActiveEditor => _cacheEditingService.Value?.ActiveCacheEditor;

        public void ExecuteCommand(Command command)
        {
            if (command.Tag is not null)
                ActiveEditor.OpenTag((CachedTag)command.Tag);
        }

        public IEnumerable<Command> PopulateCommandList(Command command)
        {
            if(ActiveEditor.CacheFile.Cache is not null)
            {
                var cache = ActiveEditor.CacheFile.Cache;
                var cacheName = cache.DisplayName;

                List<CachedTag> globalTags = GetGlobalTags();

                if (globalTags.Count == 0)
                    yield return new Command(command.Definition) { IsEnabled = false, DisplayText = "(empty)" };

                for (int i = 0; i < globalTags.Count; i++)
                {
                    CachedTag tag = globalTags[i];
                    string displayText = tag is not null
                        ? $"{tag.Group.Tag} - {tag.Name.Replace("_","__")}"
                        : "not available";

                    yield return new Command(command.Definition) 
                    { 
                        RequiresUpdate = true, 
                        Tag = tag,
                        DisplayText = displayText,
                        IsEnabled = tag is not null
                    };
                }
            }
        }

        public void UpdateCommand(Command command)
        {
            var cache = ActiveEditor?.CacheFile?.Cache as GameCache;

            if(command.Tag == null)
                command.IsVisible = cache != null;
        }

        private List<CachedTag> GetGlobalTags()
        {
            List<string> globals = ["matg", "mulg"];

            var cache = ActiveEditor.CacheFile.Cache;

            if (cache is GameCacheHaloOnlineBase)
            {
                globals.AddRange(["modg", "forg"]);
            }
            else
            {
                globals.AddRange(["chgd", "scnr", "ugh!", "zone"]);
            }

            globals.AddRange(["aigl", "smdt"]);

            List<CachedTag> tags = [];
            foreach (var group in globals)
            {
                if (cache.TagCache.TryGetCachedTag($"*.{group}", out CachedTag tag))
                    tags.Add(tag);
            }

            return tags;
        }
    }
}
