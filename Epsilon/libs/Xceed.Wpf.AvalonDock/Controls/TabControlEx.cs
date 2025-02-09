using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace Xceed.Wpf.AvalonDock.Controls
{
	/// <summary>
	/// This control added to mitigate issue with tab (document) switching speed
	/// See this https://stackoverflow.com/questions/2080764/how-to-preserve-control-state-within-tab-items-in-a-tabcontrol
	/// and this https://stackoverflow.com/questions/31030293/cefsharp-in-tabcontrol-not-working/37171847#37171847
	/// 
	/// by implmenting an option to enable virtualization for tabbed document containers.
	/// </summary>
	[TemplatePart(Name = "PART_ItemsHolder", Type = typeof(Panel))]
	[TemplatePart(Name = "PART_SelectedContentHost", Type = typeof(ContentPresenter))]
	public class TabControlEx : TabControl
	{
		private Panel ItemsHolderPanel = null;

		/// <summary> Gets whether the control and its inheriting classes are virtualizing their items or not. </summary>
		public bool IsVirtualiting => _IsVirtualizing;
		private readonly bool _IsVirtualizing;

		/// <summary> Class constructor from virtualization parameter. </summary>
		/// <param name="isVirtualizing">Whether tabbed items are virtualized or not.</param>
		public TabControlEx(bool isVirtualizing) : this() { _IsVirtualizing = isVirtualizing; }

		/// <summary> Class constructor </summary>
		protected TabControlEx() : base() {
			_IsVirtualizing = false;
			// This is necessary so that we get the initial databound selected item
			ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
		}

		/// <summary> Get the ItemsHolder and generate any children </summary>
		public override void OnApplyTemplate() {
			base.OnApplyTemplate();
			if (_IsVirtualizing) { return; } // required only if virtualization is turned ON

			ItemsHolderPanel = CreateItemsHolder();
			// exchange ContentPresenter for Grid
			var contentPresenter = GetTemplateChild("PART_SelectedContentHost");
			if (contentPresenter == null) { throw new Exception($"{nameof(TabControlEx)}: PART_SelectedContentHost not found"); }
			var border = VisualTreeHelper.GetParent(contentPresenter) as Border;
			if (border == null) { throw new Exception($"{nameof(TabControlEx)}: PART_SelectedContentHost parent not found"); }
			border.Child = ItemsHolderPanel;
			UpdateSelectedItem();
		}

		/// <summary> When the items change we remove any generated panel children and add any new ones as necessary </summary>
		protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e) {
			base.OnItemsChanged(e);
			if (_IsVirtualizing) { return; } // required only if virtualization is turned ON

			if (ItemsHolderPanel == null)
				return;

			switch (e.Action) {
				case NotifyCollectionChangedAction.Reset:
					ItemsHolderPanel.Children.Clear();
					break;

				case NotifyCollectionChangedAction.Add:
				case NotifyCollectionChangedAction.Remove:
					if (e.OldItems != null) {
						foreach (var item in e.OldItems) {
							ContentPresenter cp = FindChildContentPresenter(item);
							if (cp != null) { ItemsHolderPanel.Children.Remove(cp); }
						}
					}

					// Don't do anything with new items because we don't want to
					// create visuals that aren't being shown

					UpdateSelectedItem();
					break;

				case NotifyCollectionChangedAction.Replace:
					throw new NotImplementedException("Replace not implemented yet");
			}
		}

		/// <summary> Raises the <see cref="System.Windows.Controls.Primitives.Selector.SelectionChanged"/> routed event. </summary>
		/// <param name="e">Provides data for <see cref="SelectionChangedEventArgs"/>.</param>
		protected override void OnSelectionChanged(SelectionChangedEventArgs e) {
			base.OnSelectionChanged(e);
			if (_IsVirtualizing) { return; } // required only if virtualization is turned ON
			UpdateSelectedItem();
		}

		/// <summary> Gets the currently selected item (including its generation if Virtualization is currently switched on). </summary>
		protected TabItem GetSelectedTabItem() {
			if (_IsVirtualizing) { return SelectedItem as TabItem; } // required only if virtualization is turned ON
			if (SelectedItem == null) { return null; }

			TabItem tabItem = SelectedItem as TabItem;
			if (tabItem == null) {
				tabItem = ItemContainerGenerator.ContainerFromIndex(SelectedIndex) as TabItem;
			}
			return tabItem;
		}

		/// <summary> If containers are done, generate the selected item </summary>
		private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e) {
			if (this.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated) {
				this.ItemContainerGenerator.StatusChanged -= ItemContainerGenerator_StatusChanged;
				UpdateSelectedItem();
			}
		}

		private Grid CreateItemsHolder() {
			var grid = new Grid();
			Binding binding = new Binding(PaddingProperty.Name);
			binding.Source = this;  // view model?
			grid.SetBinding(Grid.MarginProperty, binding);

			binding = new Binding(SnapsToDevicePixelsProperty.Name);
			binding.Source = this;  // view model?
			grid.SetBinding(Grid.SnapsToDevicePixelsProperty, binding);

			return grid;
		}

		private void UpdateSelectedItem() {
			if (ItemsHolderPanel == null)
				return;

			// Generate a ContentPresenter if necessary
			TabItem item = GetSelectedTabItem();
			if (item != null)
				CreateChildContentPresenter(item);

			// show the right child
			foreach (ContentPresenter child in ItemsHolderPanel.Children)
				child.Visibility = ( ( child.Tag as TabItem ).IsSelected ) ? Visibility.Visible : Visibility.Collapsed;
		}

		private ContentPresenter CreateChildContentPresenter(object item) {
			if (item == null)
				return null;

			ContentPresenter cp = FindChildContentPresenter(item);

			if (cp != null)
				return cp;

			// the actual child to be added.  cp.Tag is a reference to the TabItem
			cp = new ContentPresenter();
			cp.Content = ( item is TabItem ) ? ( item as TabItem ).Content : item;
			cp.ContentTemplate = this.SelectedContentTemplate;
			cp.ContentTemplateSelector = this.SelectedContentTemplateSelector;
			cp.ContentStringFormat = this.SelectedContentStringFormat;
			cp.Visibility = Visibility.Collapsed;
			cp.Tag = ( item is TabItem ) ? item : ( this.ItemContainerGenerator.ContainerFromItem(item) );
			ItemsHolderPanel.Children.Add(cp);
			return cp;
		}

		private ContentPresenter FindChildContentPresenter(object data) {
			if (data is TabItem tabItem) { data = tabItem.Content; }

			if (data == null)
				return null;

			if (ItemsHolderPanel == null)
				return null;

			foreach (ContentPresenter cp in ItemsHolderPanel.Children) {
				if (cp.Content == data)
					return cp;
			}

			return null;
		}
	}
}
