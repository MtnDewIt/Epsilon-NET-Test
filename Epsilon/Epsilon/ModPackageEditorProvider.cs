using Epsilon.Dialogs;
using Epsilon.Editors;
using Shared;
using Stylet;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using TagTool.Cache;

namespace Epsilon
{
	[Export(typeof(IEditorProvider))]
    public class ModPackageEditorProvider : IEditorProvider
    {
        private readonly ICacheEditingService _editingService;

        public string DisplayName => "Mod Package";

        public static Guid EditorId = new Guid("{BECDF82F-CF23-4DBD-BE7A-A44DBF943BFB}");

        public Guid Id => EditorId;

        public IReadOnlyList<string> FileExtensions => new[] { ".pak" };

        [ImportingConstructor]
        public ModPackageEditorProvider(ICacheEditingService editingService)
        {
            _editingService = editingService;
        }

        public async Task OpenFileAsync(IShell shell, params string[] paths)
        {

            string modPath = null, modCachePath = null;

            if (paths == null || paths.Length == 0) { 
                throw new ArgumentException($"{nameof(ModPackageEditorProvider)}.{nameof(OpenFileAsync)} requires at least one valid file path."); 
            }
            else { 
                modPath = paths[0]; 
                if (paths.Length > 1) { modCachePath = paths[1]; }
            }

			FileInfo modFileInfo = new FileInfo(modPath);

            FileInfo modCacheFileInfo;
            if (string.IsNullOrEmpty(modCachePath)) { modCacheFileInfo = new FileInfo(Path.Combine(modFileInfo.Directory.FullName, "..\\maps\\tags.dat")); }
            else { modCacheFileInfo = new FileInfo(modCachePath); }

            if (!modCacheFileInfo.Exists)
            {
                if (!FileDialogs.RunBrowseCacheDialog(out modCacheFileInfo))
                    return;
            }

            if (!modFileInfo.Exists)
            {
				AlertDialogViewModel alert = new AlertDialogViewModel
                {
                    AlertType = Alert.Error,
                    DisplayName = "File Not Found",
                    Message = $"Mod package not found at this location:",
                    SubMessage = modPath
                };
                shell.ShowDialog(alert);

                return;
            }

            try
            {
				GameCacheModPackage cache = await Task.Run(() => new GameCacheModPackage((GameCacheHaloOnlineBase)GameCache.Open(modCacheFileInfo), modFileInfo));
                shell.ActiveDocument = (IScreen)_editingService.CreateEditor(new ModPackageCacheFile(modFileInfo, cache));
            }
            catch
            {
				AlertDialogViewModel alert = new AlertDialogViewModel
                {
                    AlertType = Alert.Error,
                    DisplayName = "Failed to Open",
                    Message = $"An error occurred while opening this mod package; it may be incompatible or corrupted.",
                    SubMessage = modPath
                };
                shell.ShowDialog(alert);

                return;
            }
        }
    }
}
