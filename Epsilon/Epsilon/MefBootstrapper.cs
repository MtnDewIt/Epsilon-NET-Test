﻿using Epsilon.Logging;
using EpsilonLib.Core;
using EpsilonLib.Logging;
using Stylet;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reflection;

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
                .OfType<ComposablePartCatalog>());

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
        }

        /// <summary>
        /// Override to add your own types to the IoC container.
        /// </summary>
        protected virtual void ConfigureIoC(CompositionBatch batch) { }

    }
}
