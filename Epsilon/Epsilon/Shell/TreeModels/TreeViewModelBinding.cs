using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Data;

namespace Epsilon.Shell.TreeModels
{

	public class TreeViewModelBinding : IDisposable
	{

		private readonly TreeView _treeView;
		private readonly TreeModel _treeModel;
		private readonly Dictionary<RoutedEvent, RoutedEventHandler> _attachedHandlers;

		/// <summary>
		/// Initializes a new instance of the <see cref="TreeViewModelBinding"/> class.<br/>
		/// This class binds a <see cref="TreeView"/> to a <see cref="TreeModel"/> instance, and routes events to the model.
		/// </summary>
		/// <param name="treeView"> The <see cref="TreeView"/> to bind to the <paramref name="treeModel"/>. </param>
		/// <param name="treeModel"> The <see cref="TreeModel"/> to bind to the <paramref name="treeView"/>. </param>
		/// <exception cref="ArgumentNullException"> Thrown if either the <paramref name="treeView"/> or <paramref name="treeModel"/> are <see langword="null"/>. </exception>
		public TreeViewModelBinding(TreeView treeView, TreeModel treeModel) {
			
			if (treeView == null) { throw new ArgumentNullException(nameof(treeView)); }
			if (treeModel == null) { throw new ArgumentNullException(nameof(treeModel)); }

			_treeModel = treeModel;
			_treeView = treeView;

			_attachedHandlers = new Dictionary<RoutedEvent, RoutedEventHandler>();

			BindingOperations.SetBinding(
				_treeView, ItemsControl.ItemsSourceProperty,
				new Binding(nameof(TreeModel.Nodes)) { Source = _treeModel }
			);

			// The Control.MouseDoubleClickEvent is associated with the TreeModel's NodeActionAttempted event.
			AttachHandler(Control.MouseDoubleClickEvent, NodeDoubleClickHandler);
			// The TreeViewItem.SelectedEvent is associated with the TreeModel's NodeSelected event.
			AttachHandler(TreeViewItem.SelectedEvent, NodeSelectHandler);
			// The UIElement.KeyDownEvent is associated with the TreeModel's KeyDown event.
			AttachHandler(UIElement.PreviewKeyDownEvent, NodeKeyDownHandler);

		}


		/// <summary>
		/// Attaches a handler to a routed event.<br/>
		/// If the <paramref name="routedEvent"/> is null or the <paramref name="eventHandler"/> is null, an <see cref="Exception"/> is thrown.
		/// </summary>
		/// <param name="routedEvent"> The routed event to attach the handler to.</param>
		/// <param name="eventHandler"> The handler to attach to the routed event.</param>
		/// <exception cref="Exception"> Thrown when the <paramref name="routedEvent"/> is null or the <paramref name="eventHandler"/> is null.</exception>
		public void AttachHandler(RoutedEvent routedEvent, RoutedEventHandler eventHandler) {
			try {
				_attachedHandlers.Add(routedEvent, eventHandler);
				_treeView.AddHandler(routedEvent, eventHandler);
			}
			catch (Exception ex) {
				throw new Exception($"Failed to attach handler '{eventHandler?.ToString() ?? "NULL"}' for event '{routedEvent?.Name ?? "NULL"}'.", ex);
			}
		}

		/// <summary>
		/// Detaches a handler from a routed event.<br/>
		/// If the <paramref name="routedEvent"/> is null or the <paramref name="eventHandler"/> is null, an <see cref="Exception"/> is thrown.
		/// </summary>
		/// <param name="routedEvent"> The routed event to detach the handler from.</param>
		/// <param name="eventHandler"> The handler to detach from the routed event.</param>
		/// <exception cref="Exception"> Thrown when the <paramref name="routedEvent"/> is null or the <paramref name="eventHandler"/> is null.</exception>
		public void DetachHandler(RoutedEvent routedEvent, RoutedEventHandler eventHandler) {
			try {
				_treeView.RemoveHandler(routedEvent, eventHandler);
				_attachedHandlers.Remove(routedEvent);
			}
			catch (Exception ex) {
				throw new Exception($"Failed to detach handler '{eventHandler?.ToString() ?? "NULL"}' for event '{routedEvent?.Name ?? "NULL"}'.", ex);
			}
		}


		/// <summary>
		/// Route any double-click events to the first ancestor <see cref="TreeViewItem"/>, if one exists in the event path.<br/>
		/// This handler directly invokes the <see cref="TreeModel.OnNodeAttemptActivate"/> method.
		/// </summary>
		/// <param name="sender"> The object that fired the event. </param>
		/// <param name="e"> The event arguments. </param>
		public void NodeDoubleClickHandler(object sender, RoutedEventArgs e) {

			// If the original source is not a DependencyObject, return
			if (e == null || !(e.OriginalSource is DependencyObject dependencyObject)) { return; }

			// Get the TreeViewItem that was double-clicked, return if the event context is not valid
			TreeViewItem treeViewItem = FindAncestors<TreeViewItem>(dependencyObject).FirstOrDefault();
			if (treeViewItem == null || !treeViewItem.IsMouseOver || !( treeViewItem.DataContext is TreeNode node )) { return; }

			// Invoke the TreeModel's OnNodeAttemptActivate method
			_treeModel.OnNodeAttemptActivate(
				new TreeNodeEventArgs(Control.MouseDoubleClickEvent, sender, node, Key.None, Keyboard.Modifiers)
			);

		}

		/// <summary>
		/// Route any selection events to the first ancestor <see cref="TreeViewItem"/>, if one exists in the event path.<br/>
		/// This handler directly invokes the <see cref="TreeModel.OnNodeSelected"/> method.
		/// </summary>
		/// <param name="sender"> The object that fired the event. </param>
		/// <param name="e"> The event arguments. </param>
		public void NodeSelectHandler(object sender, RoutedEventArgs e) {

			// If the original source is not a DependencyObject, return
			if (e == null || !( e.OriginalSource is DependencyObject dependencyObject )) { return; }

			// Get the TreeViewItem that was selected, return if the event context is not valid
			TreeViewItem treeViewItem = FindAncestors<TreeViewItem>(dependencyObject).FirstOrDefault();
			if (treeViewItem == null || !(treeViewItem.DataContext is TreeNode node)) { return; }

			// Mark the event as handled
			e.Handled = true;

			// Get the key value to pass if this is in response to a key event
			Key keyValue = e is KeyEventArgs keyEvent ? keyEvent.Key : Key.None;

			// Invoke the TreeModel's OnNodeSelected method
			_treeModel.OnNodeSelected( 
				new TreeNodeEventArgs(e.RoutedEvent, sender, node, keyValue, Keyboard.Modifiers)
			);

		}

		/// <summary>
		/// Route any key down events to the first ancestor <see cref="TreeViewItem"/>, if one exists in the event path.<br/>
		/// Currently this handler only resolves <see cref="Key.Enter"/> presses, directly invoking the <see cref="TreeModel.OnNodeAttemptActivate"/> method.
		/// </summary>
		/// <param name="sender"> The object that fired the event. </param>
		/// <param name="e"> The event arguments. </param>
		public void NodeKeyDownHandler(object sender, RoutedEventArgs e) {

			// If this is not a KeyEvent, return
			if (!( e is KeyEventArgs keyEventArgs )) { return; }

			// If the original source is not a DependencyObject, return
			if (!( e.OriginalSource is DependencyObject dependencyObject )) { return; }

			// Get the TreeViewItem that was selected when the key was pressed
			TreeViewItem treeViewItem = FindAncestors<TreeViewItem>(dependencyObject).FirstOrDefault();

			// If the TreeViewItem is not focused, or the DataContext is not a TreeNode, return
			if (treeViewItem == null || !treeViewItem.IsKeyboardFocusWithin || !( treeViewItem.DataContext is TreeNode node )) { return; }

			switch (keyEventArgs.Key) {
				
				case Key.Enter:
					// Enter Key: Invoke the TreeModel's OnNodeAttemptActivate method
					_treeModel.OnNodeAttemptActivate(
						new TreeNodeEventArgs(e.RoutedEvent, sender, node, Key.Enter, Keyboard.Modifiers)
					);
					e.Handled = true;
					break;

				case Key.Left:
				case Key.Right:
				case Key.Up:
				case Key.Down:
					_treeModel.OnNodeSelected(
						new TreeNodeEventArgs(e.RoutedEvent, sender, node, keyEventArgs.Key, Keyboard.Modifiers)
					);
					e.Handled = true;
					break;

				default:
					break;

			}

		}


		/// <summary>
		/// Find all ancestors of a given type for a given <see cref="DependencyObject"/>.
		/// </summary>
		/// <typeparam name="T"> The type of ancestor to find. </typeparam>
		/// <param name="node"> The <see cref="DependencyObject"/> to find ancestors for. </param>
		/// <returns> An <see cref="IEnumerable{T}"/> of ancestors of the given type. </returns>
		public IEnumerable<T> FindAncestors<T>(DependencyObject node) where T : DependencyObject {
			while (node != null) {
				if (node is T nodeAsT) {
					yield return nodeAsT;
				}
				node = System.Windows.Media.VisualTreeHelper.GetParent(node);
			}
		}


		/// <summary>
		/// Disposes of the <see cref="TreeViewModelBinding"/> instance, clearing all bindings and attached handlers.
		/// </summary>
		public void Dispose() {
			try {
				BindingOperations.ClearBinding(_treeView, ItemsControl.ItemsSourceProperty);
				foreach (KeyValuePair<RoutedEvent, RoutedEventHandler> pair in _attachedHandlers.ToList()) {
					try { DetachHandler(pair.Key, pair.Value); }
					catch { continue; }
				}
			}
			catch { }
		}

	}

}
