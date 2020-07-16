using Epsilon;
using Epsilon.Pages;
using EpsilonLib.Commands;
using EpsilonLib.Logging;
using EpsilonLib.Menus;
using Shared;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;


namespace WpfApp20
{
    public class Bootstrapper : MefBootstrapper<ShellViewModel>
    {
        private FileHistoryService _fileHistory;

        protected async override void Launch()
        {
            RegisterAdditionalLoggers();

            var startupTasks = new List<Task>();
            startupTasks.Add(_fileHistory.InitAsync());

            PrepareResources();

            await Task.WhenAll(startupTasks);

            base.Launch();

            PostLaunchInitShell();
        }

        private void RegisterAdditionalLoggers()
        {
            foreach (var logger in GetInstances<ILogHandler>())
                Logger.RegisterLogger(logger);
        }

        protected override void ConfigureIoC(CompositionBatch batch)
        {
            base.ConfigureIoC(batch);

            _fileHistory = new FileHistoryService(new XmlFileHistoryStore("filehistory.xml"));
            batch.AddExportedValue<IFileHistoryService>(_fileHistory);
        }

        protected override IEnumerable<Assembly> GetAssemblies()
        {
            var pluginManagaer = new PluginLoader();
            pluginManagaer.LoadPlugins();

            yield return Assembly.GetExecutingAssembly();
            yield return (typeof(IShell).Assembly); // EpsilonLib

            foreach (var file in pluginManagaer.Plugins)
                yield return file.Assembly;
        }

        private void PrepareResources()
        {
            foreach (var dict in GetInstances<ResourceDictionary>())
                App.Current.Resources.MergedDictionaries.Add(dict);

            App.Current.Resources.Add(typeof(IMenuFactory), GetInstance<IMenuFactory>());
            App.Current.Resources.Add(SystemParameters.MenuPopupAnimationKey, PopupAnimation.None);
        }

        private void PostLaunchInitShell()
        {
            // better font rendering
            TextOptions.TextFormattingModeProperty.OverrideMetadata(typeof(Window),
               new FrameworkPropertyMetadata(TextFormattingMode.Display,
               FrameworkPropertyMetadataOptions.AffectsMeasure |
               FrameworkPropertyMetadataOptions.AffectsRender |
               FrameworkPropertyMetadataOptions.Inherits));

            var window = App.Current.MainWindow;

            var commandRegistry = GetInstance<ICommandRegistry>();
            foreach (var command in commandRegistry.GetCommands())
            {
                if (command.Definition.KeyShortcut == KeyShortcut.None)
                    continue;

                window.InputBindings.Add(new System.Windows.Input.InputBinding(command, command.Definition.KeyShortcut.KeyGesture));
            }
        }
    }
}
