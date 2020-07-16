namespace EpsilonLib.Menus
{
    public abstract class MenuItemDefinition : IMenuChild
    {
        public string Text { get; protected set; }
        public object Parent { get; protected set; }
        public object Group { get; protected set; }
        public IMenuChild PlaceAfterChild { get; protected set; }
    }
}
