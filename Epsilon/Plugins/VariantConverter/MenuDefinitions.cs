using EpsilonLib.Menus;
using EpsilonLib.Shell;

namespace VariantConverter
{
    static class MenuDefinitions
    {
        [ExportMenuItem]
        public static MenuItemDefinition VariantConverterMenuItem = new CommandMenuItemDefinition<ShowVariantConverterCommand>(StandardMenus.ToolsMenu, null);

        [ExportMenuItem]
        public static MenuItemDefinition Halo3VariantConverterMenuItem = new CommandMenuItemDefinition<ShowHalo3VariantConverterCommand>(StandardMenus.ToolsMenu, null);

        [ExportMenuItem]
        public static MenuItemDefinition HaloReachVariantConverterMenuItem = new CommandMenuItemDefinition<ShowHaloReachVariantConverterCommand>(StandardMenus.ToolsMenu, null);
    }
}
