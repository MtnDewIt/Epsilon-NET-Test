using CacheEditor.Properties;
using EpsilonLib.Settings;
using Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Threading;


namespace CacheEditor
{
    [Export(typeof(ICacheEditingService))]
    class CacheEditingService : ICacheEditingService
    {
        private ICacheEditor _activeEditor;

        public ISettingsCollection Settings { get; }
        public IReadOnlyList<ITagEditorPluginProvider> TagEditorPlugins { get; }
        public ICacheEditor ActiveCacheEditor 
        {
            get => _activeEditor;
            set => _activeEditor = value;
        }

        public IEnumerable<ICacheEditorToolProvider> Tools { get; }

        [ImportingConstructor]
        public CacheEditingService(
            ISettingsService settingsService,
            [ImportMany] IEnumerable<ITagEditorPluginProvider> tagEditorPlugins,
            [ImportMany] IEnumerable<ICacheEditorToolProvider> tools)
        {
            Settings = settingsService.GetCollection("CacheEditor");
            TagEditorPlugins = tagEditorPlugins.OrderBy(x => x.SortOrder).ToList();
            Tools = tools;
        }
    }
}
