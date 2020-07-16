namespace EpsilonLib.Menus
{
    public class MenuDefinition : IMenuChild
    {
        public object Parent { get; set; }
        public string Text { get; set; }
        public object Group { get; set; }
        public IMenuChild PlaceAfterChild { get; set; }
        public bool InitiallyVisible { get; }

        public MenuDefinition(object parent, object group, string text, IMenuChild placeAfter = null, bool initiallyVisible = true)
        {
            Parent = parent;
            Group = group;
            Text = text;
            PlaceAfterChild = placeAfter;
            InitiallyVisible = initiallyVisible;
        }
    }

}
