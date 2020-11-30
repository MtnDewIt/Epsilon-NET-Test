using EpsilonLib.Commands;
using EpsilonLib.Logging;
using Stylet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CacheEditor
{
    class TagEditorViewModel : Conductor<TagEditorPluginTabViewModel>.Collection.OneActive, ITagEditorPluginClient
    {
        private ICacheEditingService _cacheEditingService;

        public ICommand CloseCommand { get; set; }

        public TagEditorViewModel(ICacheEditingService cacheEditingService, TagEditorContext context)
        {
            _cacheEditingService = cacheEditingService;
            var instance = context.Instance;
            DisplayName = $"{Path.GetFileName(instance.Name)}.{instance.Group.Tag}";

            CloseCommand = new DelegateCommand(Close);

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
