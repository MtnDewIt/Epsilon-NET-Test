using EpsilonLib.Commands;
using EpsilonLib.Logging;
using EpsilonLib.Shell;
using Stylet;
using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using TagTool.Cache;

namespace CacheEditor
{
    class TagEditorViewModel : Conductor<TagEditorPluginTabViewModel>.Collection.OneActive, ITagEditorPluginClient
    {
        private ICacheEditingService _cacheEditingService;
        public CachedTag Tag;

        public ICommand CloseCommand { get; set; }
        public ICommand CopyTagNameCommand { get; set; }
        public ICommand CopyTagIndexCommand { get; set; }

        public TagEditorViewModel(ICacheEditingService cacheEditingService, TagEditorContext context)
        {
            _cacheEditingService = cacheEditingService;
            Tag = context.Instance;
            DisplayName = $"{Path.GetFileName(Tag.Name)}.{Tag.Group.Tag}";

            CloseCommand = new DelegateCommand(Close);
            CopyTagNameCommand = new DelegateCommand(() => ClipboardEx.SetTextSafe($"{Tag}"));
            CopyTagIndexCommand = new DelegateCommand(() => ClipboardEx.SetTextSafe($"0x{Tag.Index:X08}"));

            LoadPlugins(context);
        }

        private async void LoadPlugins(TagEditorContext context)
        {
            foreach(var provider in _cacheEditingService.TagEditorPlugins)
            {
                if (!provider.ValidForTag(context.CacheEditor.CacheFile, context.Instance))
                    continue;

                var futurePlugin = provider.CreateAsync(context);
                Items.Add(new TagEditorPluginTabViewModel(futurePlugin) { DisplayName = provider.DisplayName });

                try
                {
                    (await futurePlugin).Client = this;
                }
                catch(Exception ex)
                {
                    Logger.Error($"failed to load tag editor plugin '{provider.DisplayName}'. Exception: {ex}");
                }
            }

            ActiveItem = Items.FirstOrDefault();
        }

        public void Close()
        {
            RequestClose();
        }

        protected override void OnClose()
        {
            base.OnClose();
        }

        protected override void OnActivate()
        {
            base.OnActivate();
        }

        async void ITagEditorPluginClient.PostMessage(object sender, object message)
        {
            foreach(var tab in Items)
            {
                var plugin = await tab.LoadTask;

                if (plugin != sender)
                    plugin.OnMessage(sender, message);
            }
        }
    }
}
