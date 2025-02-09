using Epsilon;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using TagTool.Tags;


namespace Epsilon
{
    [Export(typeof(ICacheEditingService))]
    class CacheEditingService : ICacheEditingService, IDisposable
	{
        private ICacheEditor _activeEditor;
        private Lazy<IShell> _shell;

        public const string SettingsKey = "CacheEditor";
		public ISettingsCollection Settings { get; }
        public IReadOnlyList<ITagEditorPluginProvider> TagEditorPlugins { get; }
        public ICacheEditor ActiveEditor 
        {
            get => _activeEditor;
            set => _activeEditor = value;
        }

        public IEnumerable<ICacheEditorToolProvider> Tools { get; set; }

		[ImportingConstructor]
        public CacheEditingService(
            Lazy<IShell> shell,
			ISettingsService settingsService,
            [ImportMany] IEnumerable<ITagEditorPluginProvider> tagEditorPlugins,
            [ImportMany] IEnumerable<ICacheEditorToolProvider> tools)
        {
            _shell = shell;
            Settings = settingsService.GetCollection(SettingsKey);
            TagEditorPlugins = tagEditorPlugins.OrderBy(x => x.SortOrder).ToList();
            Tools = tools;
        }

        public ICacheEditor CreateEditor(ICacheFile cacheFile)
        {
            return new CacheEditorViewModel(_shell.Value, this, cacheFile);
        }

        public void Dispose()
        {
            ActiveEditor = null;
        }
    }
}
