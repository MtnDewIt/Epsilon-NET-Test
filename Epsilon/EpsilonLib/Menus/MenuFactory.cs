using EpsilonLib.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace EpsilonLib.Menus
{
    [Export(typeof(IMenuFactory))]
    public class MenuFactory : IMenuFactory
    {
        private readonly ICommandRegistry _commandRegistry;
        private readonly MenuDefinition[] _menus;
        private readonly MenuItemDefinition[] _menuItems;
        private Dictionary<object, MenuItemViewModel> _menuViewModels = new Dictionary<object, MenuItemViewModel>();

        [ImportingConstructor]
        public MenuFactory(
            [Import] ICommandRegistry commandRegistry,
            [ImportMany] MenuDefinition[] menus,
            [ImportMany] MenuItemDefinition[] menuItems)
        {
            _commandRegistry = commandRegistry;
            _menus = menus;
            _menuItems = menuItems;
        }

        public MenuItemViewModel GetMenu(MenuDefinition definition)
        {
            if (_menuViewModels.TryGetValue(definition, out MenuItemViewModel menuViewModel))
                return menuViewModel;

            menuViewModel = new MenuItemViewModel() { Text = definition.Text, IsVisible = definition.InitiallyVisible };
            _menuViewModels.Add(definition, menuViewModel);

            var children = _menus.Cast<IMenuChild>().Concat(_menuItems)
                .Where(x => x.Parent == definition)
                .InPreferedOrder();

            var groups = children.GroupBy(x => x.Group).ToList();

            for(int i = 0; i < groups.Count; i++)
            {
                var group = groups[i];

                foreach (var child in group)
                {
                    var childViewModel = CreateChild(child);
                    menuViewModel.AddChild(childViewModel, group.Key);
                }

                if (i < groups.Count - 1)
                    menuViewModel.AddChild(MenuItemViewModel.Separator, group.Key);
            }

            return menuViewModel;
        }

        private MenuItemViewModel CreateChild(IMenuChild child)
        {
            switch (child)
            {
                case MenuDefinition submenu:
                    return GetMenu(submenu);
                case TextMenuItemDefinition item:
                    return new MenuItemViewModel() { Text = item.Text };
                case CommandMenuItemDefinition item:
                    {
                        var command = _commandRegistry.GetCommand(item.CommandType);
                        return new CommandMenuItemViewModel(command);
                    }
                default:
                    throw new NotSupportedException();
            }
        }

        public IEnumerable<MenuItemViewModel> GetMenuBar(MenuBarDefinition definition)
        {
            var children = _menus
                .Where(x => x.Parent == definition)
                .InPreferedOrder();

            foreach (var child in children)
                yield return GetMenu(child);
        }
    }

    static class Helpers
    {
        public static IEnumerable<T> InPreferedOrder<T>(this IEnumerable<T> input) where T : IMenuChild
        {
            var sorted = input.ToList<T>();
            for (int i = 0; i < sorted.Count; i++)
            {
                if (sorted[i].PlaceAfterChild != null)
                {
                    var source = sorted[i];
                    var target = (T)source.PlaceAfterChild;
                    sorted.RemoveAt(i);
                    sorted.Insert(sorted.IndexOf(target) + 1, source);
                }
            }
            return sorted;
        }
    }
}
