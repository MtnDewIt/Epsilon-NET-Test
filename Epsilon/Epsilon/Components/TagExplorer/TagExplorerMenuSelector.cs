using Epsilon.Behaviors;
using Epsilon.Menus;
using System.Windows.Controls;


namespace Epsilon
{
    class TagExplorerMenuSelector : IContextMenuSelector
    {
        public MenuItemDefinition SelectMenu(object target, ContextMenuEventArgs e)
        {
            return Components.TagTree.MenuDefinitions.ContextMenu;
        }
    }
}
