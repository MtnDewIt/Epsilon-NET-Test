using Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Epsilon.Editors
{
    [Export(typeof(IEditorService))]
    public class EditorService : IEditorService
    {
		[Import]
		private readonly Lazy<IShell> _shell;
		[Import]
		private readonly IFileHistoryService _fileHistory;
		[Import]
		private readonly IEditorProvider[] _providers;
        public IEnumerable<IEditorProvider> EditorProviders => _providers;

		//[ImportingConstructor]
        public EditorService()
            //[Import] Lazy<IShell> shell,
            //[ImportMany]IEditorProvider[] providers,
            //[Import] IFileHistoryService fileHistory)
        {
            //_shell = shell;
            //_providers = providers;
            //_fileHistory = fileHistory;
        }

        IEditorProvider GetProvider(Guid id)
        {
            return _providers.FirstOrDefault(x => x.Id == id);
        }

        public async Task OpenFileWithEditorAsync(Guid editorProviderId, params string[] paths)
        {
			var shell = _shell.Value;
            string filePath;

            if (paths == null || paths.Length == 0){ throw new NotSupportedException("Unable to open null file"); }
            else { filePath = paths[0]; }

			using (var progress = shell.CreateProgressScope())
            {
                progress.Report($"Loading '{filePath}'...");
				var provider = GetProvider(editorProviderId);
                if (provider == null){
                    throw new NotSupportedException("Unable to find editor for this file type");
                }

                await provider.OpenFileAsync(shell, paths);
                await _fileHistory.RecordFileOpened(editorProviderId, filePath);
            }
        }

        public Task OpenFileAsync(string filePath)
        {
			var file = new FileInfo(filePath);
            foreach(var provider in EditorProviders)
            {
                if (provider.FileExtensions.Contains(file.Extension)){ 
                    return OpenFileWithEditorAsync(provider.Id, filePath); 
                }
            }

            throw new NotSupportedException();
        }
    }
}
