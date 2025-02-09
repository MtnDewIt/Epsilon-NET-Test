using System.Collections.Generic;

namespace Epsilon.Menus
{
    public interface IMenuFactory
    {
        MenuItemViewModel GetMenu(MenuItemDefinition definition);
        IEnumerable<MenuItemViewModel> GetMenuBar(MenuBarDefinition definition);
    }
}
