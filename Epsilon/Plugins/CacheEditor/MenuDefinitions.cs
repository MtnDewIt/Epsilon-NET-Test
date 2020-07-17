using CacheEditor.Commands;
using EpsilonLib.Menus;
using EpsilonLib.Shell;

namespace CacheEditor
{
    public static class MenuDefinitions
    {
        [ExportMenuItem]
        public static MenuItemDefinition ShowTagExplorerMenuItem = new CommandMenuItemDefinition<ShowTagExplorerCommand>(StandardMenus.ViewMenu, "CacheEditor.Tools");

        [ExportMenuItem]
        public static MenuItemDefinition ShowInfoWindowMenuItem = new CommandMenuItemDefinition<ShowInfoWindowCommand>(StandardMenus.ViewMenu, "CacheEditor.Tools");
    }
}
