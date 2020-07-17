using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpsilonLib.Menus
{
    public interface IMenuFactory
    {
        MenuItemViewModel GetMenu(MenuItemDefinition definition);
        IEnumerable<MenuItemViewModel> GetMenuBar(MenuBarDefinition definition);
    }
}
