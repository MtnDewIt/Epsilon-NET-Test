using Epsilon.Commands;
using Epsilon.Menus;
using Epsilon.Shell;

namespace WpfApp20
{
    static class Menus
    {
        [ExportMenuItem]
        public static MenuItemDefinition GarbageCollectMenuItem = new CommandMenuItemDefinition<GarbageCollectCommand>(StandardMenus.ToolsMenu, null);

        [ExportMenuItem]
        public static MenuItemDefinition ToolsOptionsMenuItem = new CommandMenuItemDefinition<ShowOptionsCommand>(StandardMenus.ToolsMenu, null);

        [ExportMenuItem]
        public static MenuItemDefinition HelpAboutMenuItem = new CommandMenuItemDefinition<ShowAboutCommand>(StandardMenus.HelpMenu, null);

		[ExportMenuItem]
		public static MenuItemDefinition ShowTagExplorerMenuItem = new CommandMenuItemDefinition<ShowTagExplorerCommand>(StandardMenus.ViewMenu, "Epsilon.Tools");

		[ExportMenuItem]
		public static MenuItemDefinition ShowDependencyExplorerMenuItem = new CommandMenuItemDefinition<ShowDependencyExplorerCommand>(StandardMenus.ViewMenu, "Epsilon.Tools");

		[ExportMenuItem]
		public static MenuItemDefinition ShowCommandLogMenuItem = new CommandMenuItemDefinition<ShowCommandLogCommand>(StandardMenus.ViewMenu, "Epsilon.Tools");

		[ExportMenuItem]
		public static MenuItemDefinition ShowShellWindowMenuItem = new CommandMenuItemDefinition<ShowShellWindowCommand>(StandardMenus.ViewMenu, "Epsilon.Tools");

		[ExportMenuItem]
		public static MenuItemDefinition ServerJsonEditorMenuItem = new CommandMenuItemDefinition<ShowServerJsonEditorCommand>(StandardMenus.ToolsMenu, null);

		[ExportMenuItem]
		public static MenuItemDefinition MapVariantFixerMenuItem = new CommandMenuItemDefinition<ShowMapVariantFixerCommand>(StandardMenus.ToolsMenu, null);

		[ExportMenuItem]
		public static MenuItemDefinition ModPackageMenu = new MenuItemDefinition(StandardMenus.MainMenu, null, "Mod Package", placeAfter: () => StandardMenus.ViewMenu);

		[ExportMenuItem]
		public static MenuItemDefinition CacheSelectionMenu = new CommandMenuItemDefinition<TagCacheCommandList>(ModPackageMenu, null);

		[ExportMenuItem]
		public static MenuItemDefinition ReloadModPackageMenuItem = new CommandMenuItemDefinition<ReloadModPackageCommand>(ModPackageMenu, null);

		[ExportMenuItem]
		public static MenuItemDefinition NewModPackageMenuItem = new CommandMenuItemDefinition<NewModPackageCommand>(StandardMenus.FileNewMenu, null);

	}
}
