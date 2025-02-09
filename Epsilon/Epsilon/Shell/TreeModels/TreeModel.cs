using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using TagTool.Cache;

namespace Epsilon.Shell.TreeModels
{
	public class TreeModel : PropertyChangedBase, IDisposable
	{

		/// <summary>
		/// Event fired when the node is <i><b>selected</b></i> (highlighted). This may be independed of <see cref="NodeActivated"/>.
		/// </summary>
		public event EventHandler<TreeNodeEventArgs> NodeSelected;
		public void TriggerNodeSelected(TreeNode node) { NodeSelected?.Invoke(this, new TreeNodeEventArgs(TreeViewItem.SelectedEvent, this, node)); }
		/// <summary>
		/// Event fired when the node is <i><b>activated</b></i>.<br/>
		/// Currently it is assumed that the node can only be activated while selected.
		/// </summary>
		public event EventHandler<TreeNodeEventArgs> NodeActivated;
		public void TriggerNodeActivated(TreeNode node) { NodeActivated?.Invoke(this, new TreeNodeEventArgs(TreeViewItem.SelectedEvent, this, node)); }

		#region CacheNodes

		/// <summary>
		/// The <see cref="TreeNode"/>s loaded from the cache.
		/// </summary>
		public List<TreeNode> CacheNodes { get; set; } = new List<TreeNode>();
		/// <summary>
		/// The <see cref="TreeNode"/>s loaded from the cache, mapped by <see cref="CachedTag"/>.
		/// </summary>
		public Dictionary<CachedTag, TreeNode> NodesByTag { get; set; } = new Dictionary<CachedTag, TreeNode>();

		#endregion

		#region Nodes

		/// <summary>
		/// Set the root nodes of the tree, including proper initialization of Event handlers and the <see cref="FlattenedHierarchy"/>.
		/// </summary>
		/// <param name="nodes"> The root nodes of the tree. </param>
		public void SetNodesCollection(IList<TreeNode> nodes) {
			ObservableCollection<TreeNode> newNodes = new ObservableCollection<TreeNode>(nodes);
			_flattenedHierarchy.Clear();
			//newNodes.CollectionChanged += Nodes_CollectionChanged;
			Nodes = newNodes;
			PopulateSpecializedNodeViews(Nodes.GetEnumerator());
		}
		/// <summary>
		/// The root nodes of the tree.
		/// </summary>
		public ObservableCollection<TreeNode> Nodes {
			get { 
				if (_nodes == null) { _nodes = new ObservableCollection<TreeNode>(); }
				return _nodes;
			}
			private set => SetAndNotify(ref _nodes, value);
		}
		private ObservableCollection<TreeNode> _nodes;

		/// <summary>
		/// Recursively populate the <see cref="FlattenedHierarchy"/> list with all nodes in the tree.
		/// </summary>
		/// <param name="nodes"> The enumerator for the nodes to add to the list. </param>
		private void PopulateSpecializedNodeViews(IEnumerator<TreeNode> nodes) {
			while (nodes.MoveNext()) {
				_flattenedHierarchy.Add(nodes.Current);
				//if (nodes.Current is GroupNode) { GroupNodes.Add(nodes.Current as GroupNode); }
				if (nodes.Current.Children != null) {
					PopulateSpecializedNodeViews(nodes.Current.Children.GetEnumerator());
				}
			}
		}

		/// <summary>
		/// The flattened hierarchy of all nodes in the tree.
		/// </summary>
		public List<TreeNode> FlattenedHierarchy { get => _flattenedHierarchy; }
		private List<TreeNode> _flattenedHierarchy = new List<TreeNode>();

		/// <summary>
		/// The specialized view of the tree nodes, organized by <see cref="GroupNode"/>s.
		/// </summary>
		public List<GroupNode> GroupNodes { get => _groupNodes; }
		private List<GroupNode> _groupNodes = new List<GroupNode>();

		#endregion

		#region Selected Nodes

		/// <summary>
		/// Whether multiple nodes can be selected at once.
		/// </summary>
		public virtual bool MultiSelectionEnabled { get; protected set; } = true;
		/// <summary>
		/// The index of the last node selected in a multi-selection operation.<br/>
		/// Used for determining the range of SHIFT and CTRL+SHIFT multi-selection operations.
		/// </summary>
		public int MultiSelectionLastIndex { get; protected set; } = -1;

		//TODO review all uses of SelectedNode and revise all logic to handle or explicitly not handle multi-selection
		/// <summary>
		/// The collection of selected nodes in the tree.
		/// </summary>
		public ObservableCollection<TreeNode> SelectedNodes {
			get { 
				if (_selectedNodes == null) { 
					_selectedNodes = new ObservableCollection<TreeNode>();
					_selectedNodes.CollectionChanged += SelectedNodes_CollectionChanged;
				}
				return _selectedNodes;
			}
			private set => SetAndNotify(ref _selectedNodes, value);
		}
		private ObservableCollection<TreeNode> _selectedNodes;

		/// <summary>
		/// Clear the <see cref="SelectedNodes"/> collection and reset the <see cref="MultiSelectionLastIndex"/>.
		/// </summary>
		public void ClearSelection() { SelectedNodes.Clear(); MultiSelectionLastIndex = -1; }

		/// <summary>
		/// Returns <see langword="true"/> if there is at least one visible node where <see cref="TreeNode.IsSelected"/> is <see langword="true"/>.
		/// </summary>
		public bool HasSelection => FlattenedHierarchy.Any(node => node.IsSelected);

		/// <summary>
		/// Returns <see langword="true"/> if there is a valid <see cref="MultiSelectionLastIndex"/> and the <see cref="FlattenedHierarchy"/> contains a node at that index.
		/// </summary>
		public bool HasLastSelection => MultiSelectionLastIndex != -1 && FlattenedHierarchy.Count > MultiSelectionLastIndex;
		/// <summary>
		/// Returns the last selected node based on <see cref="MultiSelectionLastIndex"/> tracking.
		/// </summary>
		public TreeNode LastSelection => HasLastSelection ? FlattenedHierarchy[MultiSelectionLastIndex] : null;

		/// <summary>
		/// The first selected node in the <see cref="SelectedNodes"/> collection.
		/// </summary>
		public TreeNode SelectedNode {
			get { _selectedNode = SelectedNodes.FirstOrDefault(); return _selectedNode; }
			set { 
				if (value is TreeNode) { ClearSelection(); SelectedNodes.Add(value); }				
				SetAndNotify(ref _selectedNode, value);
			}
		}
		private TreeNode _selectedNode;

		/// <summary>
		/// Returns <see langword="true"/> if there is only one node selected in the <see cref="SelectedNodes"/> collection.
		/// </summary>
		public bool HasSingleSelection => SelectedNodes.Count == 1;

		/// <summary>
		/// Event handler for changes to the <see cref="SelectedNodes"/> collection.
		/// </summary>
		/// <param name="sender"> The object that fired the event. </param>
		/// <param name="e"> The event arguments. </param>
		/// <exception cref="NotImplementedException"> Thrown when the collection change operation is not supported. </exception>
		private void SelectedNodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {

			// Set the IsSelected property of the nodes in the collection
			switch (e.Action) {
				case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
					if (e?.NewItems == null) { return; }
					try { foreach (TreeNode node in e.NewItems) { node.IsSelected = true; } } catch { }
					break;
				case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
					if (e?.OldItems == null) { return; }
					try { foreach (TreeNode node in e.OldItems) { node.IsSelected = false; } } catch { }
					break;
				case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
				case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
					try { foreach (TreeNode node in CacheNodes) { node.IsSelected = false; } } catch { }
					break;
				default:
					break;
			}

		}

		/// <summary>
		/// Deselect all nodes recursively.
		/// </summary>
		private void DeselectRecursive(TreeNode node) {
			if (node == null) { return; }
			node.IsSelected = false;
			if (node.Children == null) { return; }
			foreach (TreeNode child in node.Children) { DeselectRecursive(child); }
		}

		#endregion

		/// <summary>
		/// The <see cref="TreeViewModelBinding"/> that is associated with this <see cref="TreeModel"/>.
		/// </summary>
		public object Source { get; set; }


		/// <summary>
		/// Find all nodes in the tree with the specified tag.
		/// </summary>
		/// <param name="tag"> The tag to search for. </param>
		/// <param name="nodes"> The nodes to search. </param>
		/// <returns> An enumerable collection of nodes with the specified tag. </returns>
		public IEnumerable<TreeNode> FindNodesWithTag(object tag, IEnumerable<TreeNode> nodes) {
			foreach (TreeNode node in nodes) {

				if (node.Tag == tag) { yield return node; }

				if (node.Children == null) { continue; }

				foreach (TreeNode child in FindNodesWithTag(tag, node.Children)) {
					yield return child;
				}
			}
		}


		/// <summary>
		/// Implement event-handling for "Select" actions on the tree nodes.
		/// </summary>
		/// <param name="e"> Event arguments. </param>
		public void OnNodeSelected(TreeNodeEventArgs e) {

			// If the node is not a valid leaf node, return
			if (!( e.Node is TreeNode node )) { return; }

			// If the node is a group node, expand or collapse it
			if (e.Node is GroupNode groupNode) {
				if (e.Key != Key.None) {
					// GroupNode fired a Key Event
					if (e.Key == Key.Left && groupNode.IsExpanded) { groupNode.IsExpanded = false; return; }
					if (( e.Key == Key.Right || e.Key == Key.Enter ) && !groupNode.IsExpanded) { groupNode.IsExpanded = true; return; }
					int index = FlattenedHierarchy.IndexOf(groupNode);
					TreeNode navNode = null;
					if (index != -1) {
						// Get the Node above this one
						if (e.Key == Key.Up && index != 0) { navNode = FlattenedHierarchy[index - 1]; }
						// Get the Node below this one
						if (e.Key == Key.Down && index != FlattenedHierarchy.Count - 1) { navNode = FlattenedHierarchy[index + 1]; }
						// Perform highlighting to match the navigation action
						if (navNode != null) {
							if (navNode != groupNode) { SelectedNodes.Remove(groupNode); };
							SelectedNodes.Add(navNode);
							return;
						}
					}
					else { return; }
				}
				else {
					// GroupNode was clicked
					groupNode.IsExpanded = !groupNode.IsExpanded;
					ClearSelection(); SelectedNodes.Add(groupNode);
					return;
				}
			}

			// If not multi-selecting, clear the selection and select the clicked node
			if (!MultiSelectionEnabled) {
				ClearSelection();
				SelectedNodes.Add(node);
				//NodeSelected?.Invoke(this, new TreeNodeEventArgs(TreeViewItem.SelectedEvent, this, node));
				return;
			}

			if (e.Modifiers.HasFlag(ModifierKeys.Shift)) {

				int indexA, indexB, nodeIndex;

				if (MultiSelectionLastIndex == -1 && HasSelection) {
					MultiSelectionLastIndex = FlattenedHierarchy.IndexOf(TreeNode.LastSelected);
				}

				// Ensure we have a valid starting index for the range
				if (MultiSelectionLastIndex == -1) {
					if (!HasSelection) { return; }
					MultiSelectionLastIndex = FlattenedHierarchy.IndexOf(SelectedNode);
					// If the last index is still invalid, return
					if (MultiSelectionLastIndex == -1) { return; }
				}

				// get the range of nodes to add to the selection
				indexA = MultiSelectionLastIndex;
				indexB = FlattenedHierarchy.IndexOf(node);

				// this is not a valid range, return
				if (indexA == indexB) { return; }

				// if the clicked node was already selected, we'll be removing the range from the selection
				bool removingRange = SelectedNodes.Contains(node);

				// determine the direction of the range
				int increment = (indexA < indexB) ? 1 : -1;

				// if the range is being removed, we need to iterate over the range and remove each node
				if (removingRange) {
					try {
						indexB -= 1;
						for (nodeIndex = indexA; nodeIndex != indexB; nodeIndex += increment) {
							SelectedNodes.Remove(FlattenedHierarchy[nodeIndex]);
						}
						if (!HasSelection) { MultiSelectionLastIndex = -1; }
					}
					catch { }
				}

				// if the range is being added, we need to iterate over the range and add each node
				else {
					try {
						indexB += 1;
						for (nodeIndex = indexA; nodeIndex != indexB; nodeIndex += increment) {
							if (!SelectedNodes.Contains(FlattenedHierarchy[nodeIndex])) {
								SelectedNodes.Add(FlattenedHierarchy[nodeIndex]);
								//NodeSelected?.Invoke(this, new TreeNodeEventArgs(TreeViewItem.SelectedEvent, this, flatList[nodeIndex]));
							}
						}
					}
					catch { }
				}
				return;

			}

			// If not multi-selecting, clear the selection and select the clicked node
			if (!e.Modifiers.HasFlag(ModifierKeys.Control)) {
				// Clear the selection and select the node
				ClearSelection();
				SelectedNodes.Add(node);
				MultiSelectionLastIndex = FlattenedHierarchy.IndexOf(node);
				//NodeSelected?.Invoke(this, new TreeNodeEventArgs(TreeViewItem.SelectedEvent, this, node));
				return;
			}

			// If multi-selecting with only CTRL key modifier, add or remove the clicked node from the selection
			if (e.Modifiers.HasFlag(ModifierKeys.Control)) {
				if (SelectedNodes.Contains(node)) {
					SelectedNodes.Remove(node);
					if (!HasSelection) { MultiSelectionLastIndex = -1; }
					return;
				}
				else {
					SelectedNodes.Add(node);
					MultiSelectionLastIndex = FlattenedHierarchy.IndexOf(node);
					//NodeSelected?.Invoke(this, new TreeNodeEventArgs(TreeViewItem.SelectedEvent, this, node));
					return;
				}
			}

		}

		/// <summary>
		/// Implement event-handling for Double-Click / Enter-Key activation attempts on the tree nodes.
		/// </summary>
		/// <param name="e"> Event arguments. </param>
		public void OnNodeAttemptActivate(TreeNodeEventArgs e) {

			if (e?.Node == null) { return; }
			NodeActivated?.Invoke(this, e);

			//TODO review whether we want to define any behavior for Activation events with multiple nodes
			// There is currently no defined behavior for Activation events with multiple selected nodes
			//if (MultiSelectionEnabled && SelectedNodes.Count > 1) { return; }

			// If the selected node matches the event node, invoke the NodeActivated event
			//if (SelectedNode != null && SelectedNode == e.Node) {
			//	NodeActivated?.Invoke(this, e);
			//}

		}

		/// <summary>
		/// Implement event-handling for key-down events fired while tree nodes have focus.
		/// </summary>
		/// <param name="e"> Event arguments. </param>
		public void OnNodeKeyDown(TreeNodeEventArgs e) {

			//TODO review whether we want to define any behavior for KeyDown events with multiple nodes
			// There is currently no defined behavior for KeyDown events with multiple selected nodes
			//if (MultiSelectionEnabled && SelectedNodes.Count > 1) { return; }

			// Return if the event context is not valid
			//if (SelectedNode == null || e.Node == null || ( SelectedNode != e.Node )) { return; }

			// If the Enter key was pressed, invoke the NodeActivated event
			if (e.Key == Key.Enter && e?.Node != null) { NodeActivated?.Invoke(this, e); }

		}


		/// <summary>
		/// Dispose of the <see cref="TreeModel"/> and its resources.
		/// </summary>
		public void Dispose() { Nodes = null; }

	}
}
