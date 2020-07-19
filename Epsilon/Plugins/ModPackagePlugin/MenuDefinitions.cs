using EpsilonLib.Menus;
using EpsilonLib.Shell;
using ModPackagePlugin.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModPackagePlugin
{
    public static class MenuDefinitions
    {
        [ExportMenuItem]
        public static MenuItemDefinition ModPackageMenu = new MenuItemDefinition(StandardMenus.MainMenu, null, "Mod Package", placeAfter: () => StandardMenus.ViewMenu);

        [ExportMenuItem]
        public static MenuItemDefinition CacheSelectionMenu = new CommandMenuItemDefinition<TagCacheCommandList>(ModPackageMenu, null);
    }
}
