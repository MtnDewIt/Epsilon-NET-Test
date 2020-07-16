using EpsilonLib.Menus;
using EpsilonLib.Shell.Commands;
using EpsilonLib.Shell.RecentFiles;

namespace EpsilonLib.Shell
{
    public static class StandardMenus
    {
        public static object DefaultMenuItemGroup = new object();
        public static object FileOpenMenuItemGroup = new object();
        public static object FileNewMenuItemGroup = new object();
        public static object FileExitMenuItemGroup = new object();
        public static object EditUndoRedoMenuItemGroup = new object();
        public static object EditCopyPasteMenuItemGroup = new object();

        [ExportMenuBar]
        public static MenuBarDefinition MainMenu = new MenuBarDefinition();

        [ExportMenu]
        public static MenuDefinition FileMenu = new MenuDefinition(MainMenu, DefaultMenuItemGroup, "File");
        [ExportMenu]
        public static MenuDefinition EditMenu = new MenuDefinition(MainMenu, DefaultMenuItemGroup, "Edit");
        [ExportMenu]
        public static MenuDefinition ViewMenu = new MenuDefinition(MainMenu, DefaultMenuItemGroup, "View");
        [ExportMenu]
        public static MenuDefinition ToolsMenu = new MenuDefinition(MainMenu, DefaultMenuItemGroup, "Tools");
        [ExportMenu]
        public static MenuDefinition FileOpenMenu = new MenuDefinition(FileMenu, FileOpenMenuItemGroup, "Open");

        [ExportMenuItem]
        public static MenuItemDefinition FileOpenTestMenuItem = new CommandMenuItemDefinition<OpenFileCommand>(FileOpenMenu, DefaultMenuItemGroup);
        [ExportMenuItem]
        public static TextMenuItemDefinition FileNewMenuItem = new TextMenuItemDefinition(FileMenu, FileOpenMenuItemGroup, "New", placeAfter: FileOpenMenu);
        [ExportMenuItem]
        public static CommandMenuItemDefinition FileRecentFilesMenu = new CommandMenuItemDefinition<RecentFilesCommandList>(FileMenu, "Recent");
        [ExportMenuItem]
        public static MenuItemDefinition FileExitMenuItem = new CommandMenuItemDefinition<ExitCommand>(FileMenu, FileExitMenuItemGroup);


        [ExportMenuItem]
        public static TextMenuItemDefinition EditUndoMenuItem = new TextMenuItemDefinition(EditMenu, EditUndoRedoMenuItemGroup, "Undo");
        [ExportMenuItem]
        public static TextMenuItemDefinition EditRedoMenuItem = new TextMenuItemDefinition(EditMenu, EditUndoRedoMenuItemGroup, "Redo");
        [ExportMenuItem]
        public static TextMenuItemDefinition EditCutMenuItem = new TextMenuItemDefinition(EditMenu, EditCopyPasteMenuItemGroup, "Cut");
        [ExportMenuItem]
        public static MenuItemDefinition EditCopyMenuItem = new CommandMenuItemDefinition<CopyCommand>(EditMenu, EditCopyPasteMenuItemGroup);
    }
}
