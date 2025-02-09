using Epsilon.Menus;
using System.Windows.Controls;

namespace Epsilon.Behaviors
{
    public interface IContextMenuSelector
    {
        MenuItemDefinition SelectMenu(object target, ContextMenuEventArgs e);
    }
}
