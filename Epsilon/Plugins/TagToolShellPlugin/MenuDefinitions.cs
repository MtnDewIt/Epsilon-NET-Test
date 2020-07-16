using EpsilonLib.Menus;
using EpsilonLib.Shell;
using TagToolShellPlugin.Commands;

namespace TagToolShellPlugin
{
    public static class MenuDefinitions
    {
        [ExportMenu]
        public static MenuDefinition CacheMenu = new MenuDefinition(StandardMenus.MainMenu, null, "Cache",
            placeAfter: StandardMenus.ViewMenu, initiallyVisible: false);

        [ExportMenuItem]
        public static MenuItemDefinition ShowTagsWindowMenuItem = new CommandMenuItemDefinition<ShowShellWindowCommand>(StandardMenus.ViewMenu, "CacheTools");
    }
}
