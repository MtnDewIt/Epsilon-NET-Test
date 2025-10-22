using CacheEditor;
using EpsilonLib.Commands;
using Shared;
using System;
using System.ComponentModel.Composition;
using TagTool.Cache;
using VariantConverter.Components.HaloReachVariantConverter;

namespace VariantConverter
{
    [ExportCommandHandler]
    class ShowHaloReachVariantConverterCommandHandler : ICommandHandler<ShowHaloReachVariantConverterCommand>
    {
        private readonly Lazy<IShell> _shell;
        private readonly ICacheEditingService _cacheEditingService;

        [ImportingConstructor]
        public ShowHaloReachVariantConverterCommandHandler(Lazy<IShell> shell, ICacheEditingService cacheEditingService)
        {
            _shell = shell;
            _cacheEditingService = cacheEditingService;
        }

        public void ExecuteCommand(Command command)
        {
            var editor = _cacheEditingService.ActiveCacheEditor;
            if (editor == null)
                return;

            _shell.Value.ActiveDocument = new HaloReachVariantConverterViewModel(_shell.Value, editor.CacheFile);
        }

        public void UpdateCommand(Command command)
        {
            command.IsVisible = _cacheEditingService.ActiveCacheEditor != null &&
                _cacheEditingService.ActiveCacheEditor.CacheFile.Cache.Version == TagTool.Cache.CacheVersion.EldoradoED
                && !(_cacheEditingService.ActiveCacheEditor.CacheFile.Cache is GameCacheModPackage);
        }
    }
}
