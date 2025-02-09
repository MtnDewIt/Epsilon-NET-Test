using Epsilon.Logging;
using Epsilon.Core;
using Epsilon.Logging;
using Stylet;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reflection;
using Epsilon.Editors;
using Epsilon;
using Epsilon.Commands;

namespace WpfApp20
{
    public class MefBootstrapper<TRootViewModel> : BootstrapperBase where TRootViewModel : class
    {
        private CompositionContainer _container;
        private List<Assembly> _assemblies = new List<Assembly>();

        private object _rootViewModel;
        protected virtual object RootViewModel
        {
            get { return this._rootViewModel ?? (this._rootViewModel = this.GetInstance(typeof(TRootViewModel))); }
        }


        protected virtual IEnumerable<Assembly> GetAssemblies() => new [] { Assembly.GetExecutingAssembly() };

        protected override void ConfigureBootstrapper()
        {
            Logger.RegisterLogger(new DefaultLogger());

            Logger.Info("Bootstrapping application...");

            _assemblies.AddRange(GetAssemblies());

			AggregateCatalog catalog = new AggregateCatalog(
                _assemblies.Select(x => new AssemblyCatalog(x))
                .OfType<ComposablePartCatalog>()
            );

			//typeof(ICacheEditorToolProvider),
            //typeof(ITagEditorPluginProvider),
            //typeof(IRteProvider),
            //typeof(IEditorProvider),
            //typeof(ICacheEditingService),
            //typeof(IEditorService),
            //typeof(IOptionsService),
            //typeof(IRteService),
            //typeof(ISettingsService),
            //typeof(IMenuFactory),
            //typeof(IMessageBoxViewModel),
            //typeof(ICommandRouter),
            //typeof(IOptionsPage),
            //typeof(ISessionStore),
            //typeof(ICommandRegistry),
            //typeof(IFileHistoryStore),
            //typeof(IShell),
            //typeof(ICacheEditorTool),

			_container = new CompositionContainer(catalog);
            GlobalServiceProvider.Initialize(_container);

			CompositionBatch batch = new CompositionBatch();

            this.DefaultConfigureIoC(batch);
            this.ConfigureIoC(batch);

            _container.Compose(batch);
        }

        public override object GetInstance(Type type)
        {
            string contract = AttributedModelServices.GetContractName(type);

            try
            {
				IEnumerable<Lazy<object>> exports = _container.GetExports<object>(contract);

                if (exports.Any())
                    return exports.First().Value;
            }
            catch(ReflectionTypeLoadException ex)
            {
				string exceptions = string.Join(Environment.NewLine, ex.LoaderExceptions.Select(x => x.ToString()));
                Logger.Error($"Unable to load one or more of the requested types while resolving Type '{contract}'. Exceptions:\n{exceptions}");
                throw;
            }

            throw new Exception(string.Format("Could not locate any instances of contract {0}.", contract));
        }

        public IEnumerable<T> GetInstances<T>()
        {
            return _container.GetExportedValues<T>();
        }

        public T GetInstance<T>()
        {
            return _container.GetExportedValue<T>();
        }

        protected override void Launch()
        {
            base.DisplayRootView(this.RootViewModel);
        }

        protected virtual void DefaultConfigureIoC(CompositionBatch batch)
        {
			// Mark these as weak-bindings, so the user can replace them if they want
			ViewManagerConfig viewManagerConfig = new ViewManagerConfig()
            {
                ViewFactory = this.GetInstance,
                ViewAssemblies = _assemblies
            };

			Epsilon.ViewManager viewManager = new Epsilon.ViewManager(viewManagerConfig);
            batch.AddExportedValue<IWindowManager>(new WindowManager(viewManager, () => throw new NotImplementedException(), this));
            batch.AddExportedValue<IEventAggregator>(new EventAggregator());
            batch.AddExportedValue<IViewManager>(viewManager);
            batch.AddExportedValue<IWindowManagerConfig>(this);
            batch.AddExport(new Export(typeof(IMessageBoxViewModel).FullName, () => new MessageBoxViewModel()));
            // add editor service
			batch.AddExport(new Export(typeof(IEditorService).FullName, () => new EditorService()));
			batch.AddExport(new Export(typeof(EditorService).FullName, () => new EditorService()));
			// add settings service
			batch.AddExport(new Export(typeof(ISettingsService).FullName, () => new SettingsService()));
			batch.AddExport(new Export(typeof(SettingsService).FullName, () => new SettingsService()));

			// gets to here before I can no longer figure out how to manually add the rest of the services

			//// add command router
			//batch.AddExport(new Export(typeof(ICommandRouter).FullName, () => new CommandRouter()));
			//batch.AddExport(new Export(typeof(CommandRouter).FullName, () => new CommandRouter()));
			//// add command service
			//batch.AddExport(new Export(typeof(ICommandHandler).FullName, () => new CommandService()));
			//batch.AddExport(new Export(typeof(CommandService).FullName, () => new CommandService()));
		}

		/// <summary>
		/// Override to add your own types to the IoC container.
		/// </summary>
		protected virtual void ConfigureIoC(CompositionBatch batch) { }

    }
}
