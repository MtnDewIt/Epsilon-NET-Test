using Epsilon.Dialogs;
using Epsilon.Editors;
using Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using TagTool.Cache;

namespace Epsilon
{
    [Export(typeof(IEditorProvider))]
    public class CacheEditorProvider : IEditorProvider
    {

        [Import]    // property injection instead of constructor injection
        public ICacheEditingService EditingService { get; set; }

		public string DisplayName => "Cache File";

        public Guid Id => new Guid("{444EDD8C-F984-40BF-9CC6-4FEF104609E1}");

        public IReadOnlyList<string> FileExtensions => new[] { ".dat", ".map" };

		public CacheEditorProvider() {}

        async Task IEditorProvider.OpenFileAsync(IShell shell, params string[] paths)
        {
            if (paths == null || paths.Length != 1) { 
                throw new ArgumentException($"{nameof(CacheEditorProvider)} requires exactly one file path"); 
            }
            var fileName = paths[0];
			var file = new FileInfo(fileName);

            if(!file.Exists)
            {
                var error = new AlertDialogViewModel
                {
                    AlertType = Alert.Error,
                    DisplayName = "File Not Found",
                    Message = $"Tag cache not found at this location:",
                    SubMessage = $"{fileName}"
                };
                shell.ShowDialog(error);

                return;
            }

            var cache = await Task.Run(() => GameCache.Open(file));
            shell.ActiveDocument = new CacheEditorViewModel(shell, EditingService, CreateCacheFileDocument(file, cache));
        }

        ICacheFile CreateCacheFileDocument(FileInfo file, GameCache cache)
        {
            if (CacheVersionDetection.IsInGen(CacheGeneration.HaloOnline, cache.Version))
                return new HaloOnlineCacheFile(file, cache);
            else
                return new GenericCacheFile(file, cache);
        }
    }
}
