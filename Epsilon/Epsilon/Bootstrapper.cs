using Epsilon;
using Epsilon.Pages;
using Epsilon.Commands;
using Epsilon.Editors;
using Epsilon.Logging;
using Epsilon.Menus;
using Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;


namespace WpfApp20
{
	public class Bootstrapper : MefBootstrapper<ShellViewModel>
	{
		private FileHistoryService _fileHistory;
		private IEditorService _editorService;
		private ISettingsCollection _settings;
		private string DefaultCachePath;
		private string DefaultPakPath;
		private string DefaultPakCachePath;
		private double StartupPositionLeft;
		private double StartupPositionTop;
		private double StartupWidth;
		private double StartupHeight;
		private bool AlwaysOnTop;
		private string AccentColor;

		protected async override void Launch() {
			RegisterAdditionalLoggers();

			List<Task> startupTasks = new List<Task>();
			startupTasks.Add(_fileHistory.InitAsync());

			PrepareResources();

			await Task.WhenAll(startupTasks);

			App.Current.DispatcherUnhandledException += UnhandledExceptionDisplay;

			List<IEditorProvider> providers = _editorService.EditorProviders.ToList();

			FrameworkCompatibilityPreferences.KeepTextBoxDisplaySynchronizedWithTextProperty = false;

			base.Launch();

			PostLaunchInitShell();

			await OpenDefault(providers.ElementAt(0), DefaultCachePath);
			await OpenDefault(providers.ElementAt(1), DefaultPakPath, DefaultPakCachePath);
		}

		private void RegisterAdditionalLoggers() {
			foreach (ILogHandler logger in GetInstances<ILogHandler>()) {
				Logger.RegisterLogger(logger);
			}
		}
		
		private void UnhandledExceptionDisplay(object sender, DispatcherUnhandledExceptionEventArgs args) {
			try {
				ExceptionDialog dialog = new ExceptionDialog(args.Exception);
				dialog.Owner = App.Current.MainWindow;
				dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
				if (dialog.ShowDialog() == false) {
					args.Handled = true;
				}
			}
			catch {
				// Extensive Console.WriteLine output for args.Exception:
				StringBuilder sb = new StringBuilder();
				sb.AppendLine("An unhandled exception occurred:");
				sb.AppendLine(args.Exception.Message);
				sb.AppendLine("Stack Trace:");
				sb.AppendLine(args.Exception.StackTrace);
				Exception inner = args.Exception.InnerException;
				while (inner != null) {
					sb.AppendLine("Inner Exception:");
					sb.AppendLine(inner.Message);
					sb.AppendLine(inner.StackTrace);
					inner = inner.InnerException;
				}
				Console.WriteLine(sb.ToString());
				MessageBox.Show("An unhandled exception occurred before the application finished initializing. Check the console for details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			args.Handled = true;
			return;
		}

		protected override void ConfigureIoC(CompositionBatch batch) {
			base.ConfigureIoC(batch);

			_fileHistory = new FileHistoryService(new XmlFileHistoryStore("filehistory.xml"));
			batch.AddExportedValue<IFileHistoryService>(_fileHistory);
		}

		private void PrepareResources() {

			foreach (ResourceDictionary dict in GetInstances<ResourceDictionary>()) {
				App.Current.Resources.MergedDictionaries.Add(dict);
			}
		
			_editorService = GetInstance<IEditorService>();
			_settings = GetInstance<ISettingsService>().GetCollection(Settings.CollectionKey);
			DefaultCachePath = _settings.Get(Settings.DefaultTagCache);
			DefaultPakPath = _settings.Get(Settings.DefaultPak);
			DefaultPakCachePath = _settings.Get(Settings.DefaultPakCache);
			AlwaysOnTop = _settings.GetBool(Settings.AlwaysOnTop);
			AccentColor = _settings.Get(Settings.AccentColor);
			
			App.Current.Resources.Add(typeof(ICommandRegistry), GetInstance<ICommandRegistry>());
			App.Current.Resources.Add(typeof(IMenuFactory), GetInstance<IMenuFactory>());
			App.Current.Resources.Add(SystemParameters.MenuPopupAnimationKey, PopupAnimation.None);
			App.Current.Resources[Settings.AlwaysOnTop.Key] = AlwaysOnTop;
		}

		private void PostLaunchInitShell() {

			// better font rendering

			TextOptions.TextFormattingModeProperty.OverrideMetadata(typeof(Window),
			   new FrameworkPropertyMetadata(TextFormattingMode.Display,
			   FrameworkPropertyMetadataOptions.AffectsMeasure |
			   FrameworkPropertyMetadataOptions.AffectsRender |
			   FrameworkPropertyMetadataOptions.Inherits));
			
			if (_settings.TryGetDouble(Settings.StartupPositionLeft, out StartupPositionLeft)
			&&  _settings.TryGetDouble(Settings.StartupPositionTop,  out StartupPositionTop)) {
				App.Current.MainWindow.Left = StartupPositionLeft;
				App.Current.MainWindow.Top = StartupPositionTop;
			}

			StartupWidth = _settings.GetDouble(Settings.StartupWidth);
			StartupHeight = _settings.GetDouble(Settings.StartupHeight);
			if (StartupWidth > 281 && StartupHeight > 500) {
				App.Current.MainWindow.Width = StartupWidth;
				App.Current.MainWindow.Height = StartupHeight;
			}

			InitAppearance();
		}

		private async Task OpenDefault(IEditorProvider editorProvider, params string[] paths) {
			if (paths == null || paths.Length == 0) { return; }
			string path = paths[0];
			if (string.IsNullOrWhiteSpace(path)) { return; }
			else if (File.Exists(path)) {
				if (paths.Length > 1) { await _editorService.OpenFileWithEditorAsync(editorProvider.Id, paths); }
				else { await _editorService.OpenFileWithEditorAsync(editorProvider.Id, path); }
			}
			else {
				MessageBox.Show($"Startup cache or mod package could not be found at the following location:" +
						$"\n\n{path}", "File Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void InitAppearance() {
			App.Current.Resources[Settings.AccentColor.Key] = (Color)ColorConverter.ConvertFromString(AccentColor);

			string epsilonTheme = "Default";
			Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary {
				Source = new Uri("/Epsilon;component/Themes/" + epsilonTheme.ToString() + ".xaml", UriKind.Relative)
			});
		}
	}
}
