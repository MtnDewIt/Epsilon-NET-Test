using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Epsilon.Menus
{

	/// <summary>
	/// The Node class represents a menu structure that can be used to build complex hierarchical menus in a WPF application.<br/>
	/// It provides functionality to create menu items, headers, submenus, and separators, and to group menu items logically.
	/// </summary>
	public class Node
	{

		#region Constructors and Factory Methods

		/// <summary>
		/// Creates a new <see cref="Type.Root"/> <see cref="Node"/> with the specified key and title.
		/// </summary>
		/// <param name="key"> The <see cref="NodeKey"/> of the new <see cref="Node"/>. </param>
		/// <param name="title"> The <see cref="Text"/> of the new <see cref="Node"/>. </param>
		/// <returns> A new <see cref="Node"/> with the specified <see cref="NodeKey"/> and <see cref="Text"/>. </returns>
		public static Node NewRoot(string key, string title) {
			Node menu = new Node(null, key, title, Type.Root);
			return menu;
		}

		/// <summary>
		/// Creates a new <see cref="Node"/> with the specified properties.
		/// </summary>
		/// <param name="parent"> The <see cref="Parent"/> <see cref="Node"/> of the new <see cref="Node"/>. </param>
		/// <param name="nodeName"> The <see cref="NodeKey"/> of the new <see cref="Node"/>. </param>
		/// <param name="text"> The <see cref="Text"/> of the new <see cref="Node"/>. </param>
		/// <param name="nodeType"> The <see cref="Type"/> of the new <see cref="Node"/>. </param>
		/// <param name="command"> The <see cref="ICommand"/> associated with the new <see cref="Node"/>. </param>
		/// <param name="tooltip"> The <see cref="ToolTip"/> of the new <see cref="Node"/>. </param>
		/// <param name="shortcut"> The <see cref="Shortcut"/> of the new <see cref="Node"/>. </param>
		/// <param name="disabled"> The <see cref="Attributes.Disabled"/> state of the new <see cref="Node"/>. </param>
		private Node(Node parent, string nodeName, string text, Type nodeType, ICommand command = null, string tooltip = null, string shortcut = null, bool disabled = false) {
			_Parent = parent;
			NodeKey = nodeName;
			Text = text;
			Command = command;
			ToolTip = tooltip;
			Shortcut = shortcut;
			NodeType = nodeType;
			SetAttribute(Attributes.Disabled, disabled);
		}


		/// <summary>
		/// Returns or creates a <see cref="Type.Submenu"/> <see cref="Node"/> with the specified key and title.<br/>
		/// If a <see cref="Node"/> with the specified key already exists, the existing <see cref="Node"/> is returned.
		/// </summary>
		/// <param name="key"> The unique key of the <see cref="Node"/>. </param>
		/// <returns> The <see cref="Node"/> with the specified key. </returns>
		public Node this[string key] { get { return Submenu(key); } }

		/// <summary>
		/// Returns or creates a <see cref="Type.Submenu"/> <see cref="Node"/> with the specified key and title.<br/>
		/// If a <see cref="Node"/> with the specified key already exists, the existing <see cref="Node"/> is returned.<br/>
		/// By referencing the <see cref="Node"/> with the specified key, you can add items to the submenu using the <see cref="Add"/> and <see cref="AddSeparator"/> methods.<br/>
		/// You can also reference the submenu <see cref="Node"/> directly using bracket notation, e.g. <c>rootNode["SubmenuKey"].Add(...);</c>
		/// </summary>
		/// <param name="key"> The unique key of the <see cref="Node"/>. </param>
		/// <param name="title"> The text displayed in the menu for the <see cref="Node"/>. </param>
		/// <returns> The <see cref="Node"/> with the specified key. </returns>
		public Node Submenu(string key, string title = null) {
			Node menu = Nodes.FirstOrDefault(x => x.NodeKey == key);
			if (string.IsNullOrEmpty(title)) { title = key; }
			if (menu == null) {
				menu = new Node(this, key, title, nodeType: Type.Submenu);
				_Nodes.Add(menu);
			}
			return menu;
		}


		/// <summary>
		/// Adds a <see cref="Node"/> of <see cref="Type.Item"/> with the specified properties to the current <see cref="Node"/>'s list of children.<br/>
		/// The <paramref name="text"/> must be unique within the current <see cref="Node"/>'s list of children, or an <see cref="ArgumentException"/> is thrown.
		/// </summary>
		/// <param name="text"> The text displayed in the menu for the <see cref="Node"/>. </param>
		/// <param name="command"> The <see cref="ICommand"/> associated with the <see cref="Node"/>. </param>
		/// <param name="tooltip"> The tooltip text displayed when the mouse hovers over the <see cref="Node"/>. </param>
		/// <param name="shortcut"> The shortcut key associated with the <see cref="Node"/> in the context menu. </param>
		/// <param name="disabled"> The <see cref="Attributes.Disabled"/> state of the <see cref="Node"/>. </param>
		/// <exception cref="ArgumentException"> Thrown if the <paramref name="text"/> is null or empty, or if the <paramref name="text"/> is a duplicate key. </exception>
		public Node Add(string text, ICommand command = null, string tooltip = null, string shortcut = null, bool disabled = false) {
			if (string.IsNullOrEmpty(text) || Nodes.Any(x => x.Text == text)) { throw new ArgumentException("Invalid text or duplicate key."); }
			_Nodes.Add(new Node(this, NodeKey, text, Type.Item, command, tooltip, shortcut, disabled));
			return this;
		}

		/// <summary>
		/// Adds a <see cref="Type.Separator"/> <see cref="Node"/> to the current <see cref="Node"/>'s list of children.
		/// </summary>
		public Node AddSeparator() {
			_Nodes.Add(new Node(this, NodeKey, string.Empty, Type.Separator));
			return this;
		}

		/// <summary>
		/// Adds a <see cref="Type.Header"/> <see cref="Node"/> to the current <see cref="Node"/>'s list of children.
		/// </summary>
		/// <param name="text"> The text displayed in the menu for the <see cref="Node"/>. </param>
		/// <returns> The current <see cref="Node"/>. </returns>
		public Node AddHeader(string text) {
			_Nodes.Add(new Node(this, NodeKey, text, Type.Header));
			return this;
		}

		#endregion

		#region Node Properties

		/// <summary>
		/// The unique key of the <see cref="Node"/>. Used to identify the <see cref="Node"/> in the root <see cref="Node"/>'s hierarchy.<br/>
		/// This property is only relevant for <see cref="Type.Submenu"/> <see cref="Node"/>s.
		/// </summary>
		public string NodeKey { get; private set; } = string.Empty;
		/// <summary>
		/// The text displayed in the menu for the <see cref="Node"/>.
		/// </summary>
		public string Text { get; set; } = string.Empty;
		/// <summary>
		/// The <see cref="ICommand"/> associated with the <see cref="Node"/>. If the <see cref="Node"/> is a <see cref="Type.Separator"/> or <see cref="Type.Header"/>, this property is null.
		/// </summary>
		public ICommand Command { get; set; } = null;
		/// <summary>
		/// The shortcut key associated with the <see cref="Node"/> in the context menu. If the <see cref="Node"/> is a <see cref="Type.Separator"/> or <see cref="Type.Header"/>, this property is null.
		/// </summary>
		public string Shortcut { get; set; } = null;
		/// <summary>
		/// The tooltip text displayed when the mouse hovers over the <see cref="Node"/>. If the <see cref="Node"/> is a <see cref="Type.Separator"/>, this property is null.
		/// </summary>
		public string ToolTip { get; set; } = null;
		/// <summary>
		/// The path to the icon image displayed next to the <see cref="Node"/> text.
		/// </summary>
		public string IconPath { get; private set; } = null;

		/// <summary>
		/// The type of the <see cref="Node"/>. This property is handled internally.
		/// </summary>
		public Type NodeType { get; private set; } = Type.Item;
		/// <summary>
		/// The attributes of the <see cref="Node"/>. This property is handled internally.
		/// </summary>
		public Attributes NodeAttributes { get; private set; } = 0;

		#endregion

		#region Node Hierarchy

		private readonly Node _Parent = null;
		/// <summary>
		/// Returns the parent <see cref="Node"/> of this <see cref="Node"/>. Returns <see langword="null"/> if this is the root <see cref="Node"/>.
		/// </summary>
		protected Node Parent => _Parent;

		private readonly List<Node> _Nodes = new List<Node>();
		/// <summary>
		/// Returns a list of all child <see cref="Node"/>s of this <see cref="Node"/> that are not null.
		/// </summary>
		protected List<Node> Nodes => _Nodes.Where(e => e != null).ToList();

		/// <summary>
		/// Returns the index of this <see cref="Node"/> in the parent's list of nodes, or -1 for the root <see cref="Node"/>.
		/// </summary>
		public int MyIndex { get { return Parent?.Nodes.IndexOf(this) ?? -1; } }
		/// <summary>
		/// Indicates whether the node could reasonably be hidden from the menu:<br/>
		/// <see langword="true"/> if the node is a <see cref="Type.Separator"/>, if the node has no <see cref="Command"/>, or if the <see cref="Command"/> cannot execute.
		/// </summary>
		public bool CouldHide { get { return IsSeparator || IsHeader || IsHidden || IsDisabled || IsDimmed || Command == null || Command.CanExecute("idk") == false; } }
		/// <summary>
		/// Returns <see langword="true"/> if the <see cref="Node"/> has any children.
		/// </summary>
		public bool HasChildren => Nodes.Any();

		#endregion

		#region Node Type and Attributes

		/// <summary>
		/// The type of the <see cref="Node"/>.
		/// </summary>
		public enum Type
		{
			Root = 0,
			Separator = 1,
			Header = 2,
			Item = 3,
			Submenu = 4
		}

		/// <summary>
		/// Returns <see langword="true"/> if the <see cref="Node"/> is a root <see cref="Node"/>.
		/// </summary>
		public bool IsRoot => NodeType == Type.Root;
		/// <summary>
		/// Returns <see langword="true"/> if the <see cref="Node"/> is a submenu <see cref="Node"/>.
		/// </summary>
		public bool IsSubMenu => NodeType == Type.Submenu;
		/// <summary>
		/// Returns <see langword="true"/> if the <see cref="Node"/> is a separator <see cref="Node"/>.
		/// </summary>
		public bool IsSeparator => NodeType == Type.Separator;
		/// <summary>
		/// Returns <see langword="true"/> if the <see cref="Node"/> is a header <see cref="Node"/>.
		/// </summary>
		public bool IsHeader => NodeType == Type.Header;
		/// <summary>
		/// Returns <see langword="true"/> if the <see cref="Node"/> is an item <see cref="Node"/>.
		/// </summary>
		public bool IsItem => NodeType == Type.Item;

		[Flags]
		/// <summary>
		/// The attributes of the <see cref="Node"/>.
		/// </summary>		
		public enum Attributes
		{
			Hidden = 1,
			Disabled = 2,
			Dimmed = 4,
		}

		/// <summary>
		/// Sets the specified <paramref name="attribute"/> to the specified <paramref name="value"/> for the <see cref="Node"/>.
		/// </summary>
		public void SetAttribute(Attributes attribute, bool value) {
			if (value) { NodeAttributes |= attribute; }
			else { NodeAttributes &= ~attribute; }
		}
		/// <summary>
		/// Returns the boolean value of the specified <paramref name="attribute"/> for the <see cref="Node"/>.
		/// </summary>
		public bool GetAttribute(Attributes attribute) { return NodeAttributes.HasFlag(attribute); }
		/// <summary>
		/// Returns <see langword="true"/> if the <see cref="Node"/> is hidden.
		/// </summary>
		public bool IsHidden => GetAttribute(Attributes.Hidden);
		/// <summary>
		/// Returns <see langword="true"/> if the <see cref="Node"/> is disabled.
		/// </summary>
		public bool IsDisabled => GetAttribute(Attributes.Disabled);
		/// <summary>
		/// Returns <see langword="true"/> if the <see cref="Node"/> is dimmed.
		/// </summary>
		public bool IsDimmed => GetAttribute(Attributes.Dimmed);

		#endregion

		#region Menu Output

		/// <summary>
		/// Attempts to populate an <see cref="ItemCollection"/> hierarchy using the supplied <paramref name="input"/> node.<br/>
		/// If the <paramref name="input"/> is a single submenu, the hierarchy is flattened.<br/>
		/// If a submenu contains only inactive items, the submenu is dimmed.<br/>
		/// If the entire hierarchy contains no active elements or controls, the menu is hidden.<br/>
		/// The method returns <see langword="true"/> if the output menu is populated with at least one usable item, and <see langword="false"/> if the output menu is empty.
		/// </summary>
		/// <exception cref="ArgumentException"> Thrown if the <paramref name="input"/> node is not a root node with child elements. </exception>
		public bool PopulateMenu(Node input, ItemCollection output) {
			
			if (!input.IsRoot || !input.HasChildren) { 
				throw new ArgumentException("Input menu must be a root menu with child elements."); 
			}
#if DEBUG
			Debug.WriteLine(GenerateHierarchyString());
#endif

			// Flag to track whether the menu contains at least one active item
			bool anyActive = false;

			for (int i = 0; i < input.Nodes.Count; i++) {
				if (input.Nodes[i].IsSubMenu) {
					if (input.Nodes[i].Nodes.All(x => x.CouldHide)) {
						// If the submenu contains only unusable items, dim the submenu
						input.Nodes[i].SetAttribute(Attributes.Dimmed, true);
					}
					else {
						// If the submenu contains at least one usable item,
						// promote the children to the same level as the submenu
						int startIndex = input.Nodes[i].MyIndex;
						input.Nodes[i].PromoteChildrenToSiblings(startIndex, true);
						anyActive = true;
					}
				}
				else if (!anyActive && !input.Nodes[i].CouldHide) { anyActive = true; }
			}

			MenuItem built = input.BuildMenu();

			// because of the way ItemCollection works we have to detach the items first
			List<object> items = built.Items.OfType<object>().ToList();

			if (anyActive) {
				built.Items.Clear();
				items.ForEach((object x) => output.Add(x));
				return true;
			}
			else {
				// Clear output if there were no active items
				// in the entire menu's hierarchy
				output.Clear();
				return false;
			}

		}

		/// <summary>
		/// Builds a <see cref="MenuItem"/> hierarchy from the current <see cref="Node"/> and its children.
		/// </summary>
		private MenuItem BuildMenu() {
			
			MenuItem built = new MenuItem
			{
				Header = Text,
				Command = Command,
				ToolTip = string.IsNullOrEmpty(ToolTip) ? null : ToolTip,
				InputGestureText = Shortcut,
				IsEnabled = !IsDisabled
			};

			if (NodeAttributes.HasFlag(Attributes.Dimmed)) {
				built.Foreground = System.Windows.Media.Brushes.Gray;
			}
			if (NodeAttributes.HasFlag(Attributes.Hidden)) {
				built.Visibility = System.Windows.Visibility.Collapsed;
			}
			if (NodeAttributes.HasFlag(Attributes.Disabled)) { 
				built.IsEnabled = false; 
			}

			if (!string.IsNullOrEmpty(IconPath)) {
				built.Icon = new Image { Source = new BitmapImage(new Uri(IconPath, UriKind.Relative)) };
			}

			List<object> builtSubitems = new List<object>();

			switch (NodeType) {
				case Type.Header:
					built.IsEnabled = false; built.IsTabStop = false;
					built.Foreground = System.Windows.Media.Brushes.DarkGoldenrod;
					break;
				case Type.Submenu:
				case Type.Root:
					for (int i = 0; i < Nodes.Count; i++) { 
						if (Nodes[i].IsSeparator) { builtSubitems.Add(new Separator()); continue; }
						builtSubitems.Add(Nodes[i].BuildMenu()); 
					}
					break;
			}

			foreach (object subitem in builtSubitems) { _ = built.Items.Add(subitem); }

			return built;

		}

		/// <summary>
		/// Promotes all children of the current node to the same level as the current node.<br/>
		/// If <paramref name="transitionNodeToHeaderNode"/> is true, the current node will be converted to a header node.<br/>
		/// If <paramref name="transitionNodeToHeaderNode"/> is false, the current node will be hidden.<br/>
		/// </summary>
		/// <param name="transitionNodeToHeaderNode"> If <see langword="true"/>, the current node will be converted to a header node. If <see langword="false"/>, the current node will be hidden.</param>
		private void PromoteChildrenToSiblings(int startIndex, bool transitionNodeToHeaderNode = true) {
			
			int separatorIndex = startIndex;

			foreach (Node node in Nodes) { 
				Parent._Nodes.Insert(++startIndex, node);
			}
			_Nodes.Clear();

			if (transitionNodeToHeaderNode) {
				// If we're going to use the node as a Header, add a separator after the header for clarity
				Parent._Nodes.Insert(++separatorIndex, new Node(Parent, string.Empty, string.Empty, Type.Separator));
				NodeType = Type.Header; 
			}
			else { SetAttribute(Attributes.Hidden, true); }

		}

		#endregion

		#region Helpers

		/// <summary>
		/// Returns the <see cref="Type.Root"/> <see cref="Node"/> of the current <see cref="Node"/> hierarchy.
		/// </summary>
		public Node GetRoot() {
			Node current = this;
			while (current.Parent != null) { current = current.Parent; }
			return current;
		}

		/// <summary>
		/// Generates a string representation of the current <see cref="Node"/> hierarchy.
		/// </summary>
		public string GenerateHierarchyString() {
			Node root = GetRoot();
			StringBuilder sb = new StringBuilder();
			GenerateHierarchyString(root, sb, string.Empty, true);
			return sb.ToString();
		}
		private void GenerateHierarchyString(Node node, StringBuilder sb, string indent, bool last) {
			#pragma warning disable IDE0058 // Expression value is never used
			sb.Append(indent);
			if (last) {
				sb.Append("└─ ");
				indent += "   ";
			}
			else {
				sb.Append("├─ ");
				indent += "│  ";
			}
			switch (node.NodeType) {
				case Type.Root: sb.AppendLine($"<<<ROOT:{node.Text}>>>"); break;
				case Type.Separator: sb.AppendLine("~~~~~~~~~~~~"); break;
				case Type.Header: sb.AppendLine($"[H:{node.Text}]"); break;
				case Type.Item: sb.AppendLine($"+ {node.Text}"); break;
				case Type.Submenu: sb.AppendLine($"{node.Text} -->"); break;
				default: sb.AppendLine($"UNKNOWN-->{node.Text}<--UNKNOWN"); break;
			}

			for (int i = 0; i < node.Nodes.Count; i++) {
				GenerateHierarchyString(node.Nodes[i], sb, indent, i == node.Nodes.Count - 1);
			}
			#pragma warning restore IDE0058 // Expression value is never used
		}

		#endregion

	}

}

