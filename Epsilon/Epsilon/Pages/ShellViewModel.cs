﻿using Epsilon.Components;
using EpsilonLib.Editors;
using EpsilonLib.Menus;
using EpsilonLib.Shell;
using Shared;
using Stylet;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reflection;

namespace Epsilon.Pages
{
    [Export]
    [Export(typeof(IShell))]
    public class ShellViewModel : Conductor<IScreen>.Collection.OneActive, IShell
    {
        private readonly IWindowManager _windowManager;
        private readonly IMenuFactory _menuFactory;

        [ImportingConstructor]
        public ShellViewModel(
            [ImportMany] IEnumerable<IEditorProvider> editorProviders,
            IWindowManager windowManager,
            IMenuFactory menuFactory)
        {
            _windowManager = windowManager;
            _menuFactory = menuFactory;
            StatusBar = new StatusBarModel();

            var assemblyInfo = Assembly.GetExecutingAssembly().GetName();
            Title = $"{assemblyInfo.Name}";
            MainMenu = new BindableCollection<MenuItemViewModel>(_menuFactory.GetMenuBar(StandardMenus.MainMenu));
            RefreshMainMenu();
        }

        public string Title { get;  }

        public IObservableCollection<MenuItemViewModel> MainMenu { get; }

        public IStatusBar StatusBar { get; }

        public IObservableCollection<IScreen> Documents => Items;

        public IScreen ActiveDocument
        {
            get => ActiveItem;
            set
            {
                ActiveItem = value;
                RefreshMainMenu();
                NotifyOfPropertyChange(nameof(ActiveDocument));
            }
        }

        protected override void OnInitialActivate()
        {
            base.OnInitialActivate();

            RefreshMainMenu();
        }

        public override void ActivateItem(IScreen item)
        {
            if (item != null && item.ScreenState == ScreenState.Closed)
                return;

            base.ActivateItem(item);
        }

        private void RefreshMainMenu()
        {
            foreach (var menu in MainMenu)
                menu.Refresh();
        }

        public IProgressReporter CreateProgressScope()
        {
            return new ShellProgressReporter(StatusBar);
        }

        public bool? ShowDialog(object viewModel)
        {
            return _windowManager.ShowDialog(viewModel);
        }
    }
}
