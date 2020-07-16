using CacheEditor.Commands;
using EpsilonLib.Menus;
using EpsilonLib.Shell;

namespace CacheEditor
{
    public static class CacheEditorMenus
    {
        [ExportMenu]
        public static MenuDefinition CacheMenu = new MenuDefinition(StandardMenus.MainMenu, null, "Cache", 
            placeAfter: StandardMenus.ViewMenu, initiallyVisible: false);

        [ExportMenuItem]
        public static MenuItemDefinition ShowTagsWindowMenuItem = new CommandMenuItemDefinition<ShowTagExplorerCommand>(StandardMenus.ViewMenu, "CacheTools");
    }
}
