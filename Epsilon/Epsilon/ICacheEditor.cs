using Epsilon.Shell.TreeModels;
using System;
using System.Collections.Generic;
using TagTool.Cache;

namespace Epsilon
{
    public interface ICacheEditor
    {
		ICacheFile CacheFile { get; }
        TreeModel TagTree { get; }
        ICacheEditorTool GetTool(string name);
        IDictionary<string, object> PluginStorage { get; }
        void OpenTag(CachedTag tag);
        CachedTag RunBrowseTagDialog();
        void Reload();

        CachedTag CurrentTag { get; }
        event EventHandler CurrentTagChanged;
        void ReloadCurrentTag();
        List<string> GetOpenTagNames();
    }
}
