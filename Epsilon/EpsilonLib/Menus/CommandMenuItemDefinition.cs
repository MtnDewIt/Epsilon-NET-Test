using EpsilonLib.Commands;
using System;

namespace EpsilonLib.Menus
{
    public abstract class CommandMenuItemDefinition : MenuItemDefinition
    {
        public Type CommandType { get; set; }

        public CommandMenuItemDefinition(Type commandType)
        {
            CommandType = commandType;
        }
    }

    public class CommandMenuItemDefinition<T> : CommandMenuItemDefinition where T : CommandDefinition
    {
        public CommandMenuItemDefinition(object parent, object group, IMenuChild placeAfter = null) : base(typeof(T))
        {
            Parent = parent;
            Group = group;
            PlaceAfterChild = placeAfter;
        }
    }
}
