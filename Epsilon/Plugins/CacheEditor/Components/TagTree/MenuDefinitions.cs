using CacheEditor.Components.TagTree.Commands;
using EpsilonLib.Menus;
using EpsilonLib.Shell.Commands;


namespace CacheEditor.Components.TagTree
{
    public static class MenuDefinitions
    {
        [ExportMenu]
        public static MenuDefinition ContextMenu = new MenuDefinition(null, null, null);

        [ExportMenuItem]
        public static MenuItemDefinition CopyMenuItem = new CommandMenuItemDefinition<CopyCommand>(ContextMenu, "Copy");

        [ExportMenu]
        public static MenuDefinition ViewMenu = new MenuDefinition(ContextMenu, "View", "View", placeAfter: CopyMenuItem);
        [ExportMenuItem]
        public static MenuItemDefinition ToggleFoldersViewMenuItem = new CommandMenuItemDefinition<ToggleFoldersViewCommand>(ViewMenu, null);
        [ExportMenuItem]
        public static MenuItemDefinition ToggleGroupsViewMenuItem = new CommandMenuItemDefinition<ToggleGroupsViewCommand>(ViewMenu, null);
    }
}
