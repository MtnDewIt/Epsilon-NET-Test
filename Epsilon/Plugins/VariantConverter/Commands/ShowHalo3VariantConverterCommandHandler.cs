using CacheEditor;
using EpsilonLib.Commands;
using Shared;
using System;
using System.ComponentModel.Composition;
using TagTool.Cache;
using VariantConverter.Components.Halo3VariantConverter;

namespace VariantConverter
{
    [ExportCommandHandler]
    class ShowHalo3VariantConverterCommandHandler : ICommandHandler<ShowHalo3VariantConverterCommand>
    {
        private readonly Lazy<IShell> _shell;
        private readonly ICacheEditingService _cacheEditingService;

        [ImportingConstructor]
        public ShowHalo3VariantConverterCommandHandler(Lazy<IShell> shell, ICacheEditingService cacheEditingService)
        {
            _shell = shell;
            _cacheEditingService = cacheEditingService;
        }

        public void ExecuteCommand(Command command)
        {
            var editor = _cacheEditingService.ActiveCacheEditor;
            if (editor == null)
                return;

            _shell.Value.ActiveDocument = new Halo3VariantConverterViewModel(_shell.Value, editor.CacheFile);
        }

        public void UpdateCommand(Command command)
        {
            command.IsVisible = _cacheEditingService.ActiveCacheEditor != null &&
                _cacheEditingService.ActiveCacheEditor.CacheFile.Cache.Version == TagTool.Cache.CacheVersion.HaloOnlineED
                && !(_cacheEditingService.ActiveCacheEditor.CacheFile.Cache is GameCacheModPackage);
        }
    }
}
