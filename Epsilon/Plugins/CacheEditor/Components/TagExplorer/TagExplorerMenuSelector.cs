using EpsilonLib.Behaviors;
using EpsilonLib.Menus;
using System.Windows.Controls;


namespace CacheEditor
{
    class TagExplorerMenuSelector : IContextMenuSelector
    {
        public MenuDefinition SelectMenu(object target, ContextMenuEventArgs e)
        {
            return Components.TagTree.MenuDefinitions.ContextMenu;
        }
    }
}
