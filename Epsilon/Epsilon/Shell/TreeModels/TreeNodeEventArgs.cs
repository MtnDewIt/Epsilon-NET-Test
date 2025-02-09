using System.Windows;
using System.Windows.Input;

namespace Epsilon.Shell.TreeModels
{
	public class TreeNodeEventArgs : RoutedEventArgs
	{
		public TreeNode Node { get; }
		public Key Key { get; } = Key.None;
		public ModifierKeys Modifiers { get; set; } = ModifierKeys.None;

		public TreeNodeEventArgs(RoutedEvent routedEvent, object source, TreeNode node) : base(routedEvent, source) { Node = node; }
		public TreeNodeEventArgs(RoutedEvent routedEvent, object source, TreeNode node, Key key, ModifierKeys modifiers) : base(routedEvent, source) { Node = node; Key = key; Modifiers = modifiers; }
	}

}
