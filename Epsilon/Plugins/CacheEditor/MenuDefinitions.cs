using CacheEditor.Commands;
using EpsilonLib.Menus;
using EpsilonLib.Shell;

namespace CacheEditor
{
    public static class MenuDefinitions
    {
        [ExportMenuItem]
        public static MenuItemDefinition ShowInfoWindowMenuItem = new CommandMenuItemDefinition<ShowInfoWindowCommand>(StandardMenus.ViewMenu, "CacheEditor.Tools");

        [ExportMenuItem]
        public static MenuItemDefinition ShowTagExplorerMenuItem = new CommandMenuItemDefinition<ShowTagExplorerCommand>(StandardMenus.ViewMenu, "CacheEditor.Tools");

        [ExportMenuItem]
        public static MenuItemDefinition ShowDependencyExplorerMenuItem = new CommandMenuItemDefinition<ShowDependencyExplorerCommand>(StandardMenus.ViewMenu, "CacheEditor.Tools");
        [ExportMenuItem]
        public static MenuItemDefinition ShowCommandLogMenuItem = new CommandMenuItemDefinition<ShowCommandLogCommand>(StandardMenus.ViewMenu, "CacheEditor.Tools");

        [ExportMenuItem]
        public static MenuItemDefinition GoToMenu = new MenuItemDefinition(StandardMenus.MainMenu, null, "Go To", placeAfter: () => StandardMenus.ViewMenu);
        [ExportMenuItem]
        public static MenuItemDefinition GlobalsMenuItem = new CommandMenuItemDefinition<GlobalTagsCommandList>(GoToMenu, null);
        [ExportMenuItem]
        public static MenuItemDefinition FavoritesMenuItem = new CommandMenuItemDefinition<FavoriteTagsCommandList>(GoToMenu, null);
    }
}
