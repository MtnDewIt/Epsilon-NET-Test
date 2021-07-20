using Epsilon;
using Epsilon.Pages;
using EpsilonLib.Commands;
using EpsilonLib.Editors;
using EpsilonLib.Logging;
using EpsilonLib.Menus;
using EpsilonLib.Settings;
using Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
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
        private IEditorService _editorService;
        private ISettingsCollection _settings;
        private string DefaultCachePath;

        protected async override void Launch()
        {
            RegisterAdditionalLoggers();

            var startupTasks = new List<Task>();
            startupTasks.Add(_fileHistory.InitAsync());

            PrepareResources();

            await Task.WhenAll(startupTasks);

            App.Current.DispatcherUnhandledException += (o, args) =>
            {
                var dialog = new ExceptionDialog(args.Exception);
                dialog.Owner = App.Current.MainWindow;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                if(dialog.ShowDialog() == false)
                    args.Handled = true;
            };

            var providers = _editorService.EditorProviders.ToList();

            if (DefaultCachePath != null && DefaultCachePath.Length > 0)
            {
                OpenDefault(DefaultCachePath, providers.ElementAt(0));
            }

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
            var pluginManager = new PluginLoader();
            pluginManager.LoadPlugins();

            yield return Assembly.GetExecutingAssembly();
            yield return (typeof(IShell).Assembly); // EpsilonLib

            foreach (var file in pluginManager.Plugins)
                yield return file.Assembly;
        }

        private void PrepareResources()
        {
            foreach (var dict in GetInstances<ResourceDictionary>())
                App.Current.Resources.MergedDictionaries.Add(dict);

            _editorService = GetInstance<IEditorService>();
            _settings = GetInstance<ISettingsService>().GetCollection("General");
            DefaultCachePath = _settings.Get("DefaultTagCache", "");

            App.Current.Resources.Add(typeof(ICommandRegistry), GetInstance<ICommandRegistry>());
            App.Current.Resources.Add(typeof(IMenuFactory), GetInstance<IMenuFactory>());
            App.Current.Resources.Add(SystemParameters.MenuPopupAnimationKey, PopupAnimation.None);
        }

        private void PostLaunchInitShell()
        {
            FrameworkCompatibilityPreferences.KeepTextBoxDisplaySynchronizedWithTextProperty = false;

            // better font rendering
            TextOptions.TextFormattingModeProperty.OverrideMetadata(typeof(Window),
               new FrameworkPropertyMetadata(TextFormattingMode.Display,
               FrameworkPropertyMetadataOptions.AffectsMeasure |
               FrameworkPropertyMetadataOptions.AffectsRender |
               FrameworkPropertyMetadataOptions.Inherits));
        }

        private async void OpenDefault(string path, IEditorProvider editorProvider)
        {
            try
            {
                await _editorService.OpenFileWithEditorAsync(path, editorProvider.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Path to default cache is invalid: \n\"{path}\"", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Error(ex.ToString());
            }
        }
    }
}
