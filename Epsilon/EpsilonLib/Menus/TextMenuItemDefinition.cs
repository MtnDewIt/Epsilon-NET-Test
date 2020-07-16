namespace EpsilonLib.Menus
{
    public class TextMenuItemDefinition : MenuItemDefinition
    {
        public TextMenuItemDefinition(object parent, object group, string text, IMenuChild placeAfter = null)
        {
            Parent = parent;
            Group = group;
            Text = text;
            PlaceAfterChild = placeAfter;
        }
    }
}
