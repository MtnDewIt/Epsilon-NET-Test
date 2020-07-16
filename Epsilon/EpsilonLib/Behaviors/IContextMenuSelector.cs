using EpsilonLib.Menus;
using System.Windows.Controls;

namespace EpsilonLib.Behaviors
{
    public interface IContextMenuSelector
    {
        MenuDefinition SelectMenu(object target, ContextMenuEventArgs e);
    }
}
