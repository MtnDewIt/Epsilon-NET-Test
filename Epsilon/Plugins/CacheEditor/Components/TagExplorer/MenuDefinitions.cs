using CacheEditor.Components.TagExplorer.Commands;
using CacheEditor.Components.TagTree.Commands;
using EpsilonLib.Menus;
using EpsilonLib.Shell;
using EpsilonLib.Shell.Commands;

namespace CacheEditor.Components.TagExplorer
{
    public static class MenuDefinitions
    {
        [ExportMenu]
        public static MenuDefinition ContextMenu = new MenuDefinition(null, null, null,
         placeAfter: StandardMenus.ViewMenu, initiallyVisible: false);

        [ExportMenuItem]
        public static MenuItemDefinition CopyMenuItem = new CommandMenuItemDefinition<CopyCommand>(ContextMenu, "Copy");

        [ExportMenuItem]
        public static MenuItemDefinition OpenTagMenuItem = new CommandMenuItemDefinition<OpenTagCommand>(ContextMenu, "Open");


        [ExportMenuItem]
        public static MenuItemDefinition RenameTagMenuItem = new CommandMenuItemDefinition<RenameTagCommand>(ContextMenu, null);

        [ExportMenuItem]
        public static MenuItemDefinition DuplicateTagMenuItem = new CommandMenuItemDefinition<DuplicateTagCommand>(ContextMenu, null);

        [ExportMenuItem]
        public static MenuItemDefinition ExtractTagMenuItem = new CommandMenuItemDefinition<ExtractTagCommand>(ContextMenu, null);

        [ExportMenuItem]
        public static MenuItemDefinition DeleteTagMenuItem = new CommandMenuItemDefinition<DeleteTagCommand>(ContextMenu, null);


        [ExportMenu]
        public static MenuDefinition ViewMenu = new MenuDefinition(ContextMenu, "View", "View", placeAfter: OpenTagMenuItem);
        [ExportMenuItem]
        public static MenuItemDefinition ToggleFoldersViewMenuItem = new CommandMenuItemDefinition<ToggleFoldersViewCommand>(ViewMenu, null);
        [ExportMenuItem]
        public static MenuItemDefinition ToggleGroupsViewMenuItem = new CommandMenuItemDefinition<ToggleGroupsViewCommand>(ViewMenu, null);
    }
}
