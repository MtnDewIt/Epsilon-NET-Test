using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Epsilon.Shell.TreeModels
{
	public static class TreeViewBehavior
    {

        public static bool GetBringIntoViewWhenSelected(TreeViewItem treeViewItem)
        {
            return (bool)treeViewItem.GetValue(BringIntoViewWhenSelectedProperty);
        }

        public static void SetBringIntoViewWhenSelected(TreeViewItem treeViewItem, bool value)
        {
            treeViewItem.SetValue(BringIntoViewWhenSelectedProperty, value);
        }

		/// <summary>
		/// The BringIntoViewWhenSelected property is used to bring the TreeViewItem into view when it is selected.
		/// </summary>
		public static readonly DependencyProperty BringIntoViewWhenSelectedProperty =
			DependencyProperty.RegisterAttached("BringIntoViewWhenSelected", typeof(bool), typeof(TreeViewBehavior), new UIPropertyMetadata(false, OnBringIntoViewWhenSelectedChanged));

		/// <summary>
		/// This method is called when the BringIntoViewWhenSelected property is changed.<br/>
		/// It is used to bring the TreeViewItem into view when it is selected, but only if the new value is true.
		/// </summary>
		/// <param name="depObj"> The DependencyObject that the property is attached to. In practice, this is a TreeViewItem.</param>
		/// <param name="eventArgs"> The event arguments. Essentially the old and new values of the property.</param>
		static void OnBringIntoViewWhenSelectedChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs eventArgs) {
			if (depObj is TreeViewItem item && eventArgs.NewValue is bool newBoolValue && newBoolValue) { item.BringIntoView(); }
		}

		// In the broader context of WPF, a DependencyProperty is a property that is registered with the WPF property system.
		// This system allows for the property to be set in XAML, styled, data-bound, and animated.
		// Take, for instance, the Width property of a Button. This property is a DependencyProperty.
		// The DependencyProperty class is used to define a DependencyProperty, and it is used to register the property with the property system.
		// Once this is done, the property can be set in XAML, styled, data-bound, and animated.
		// Typically, on the XAML side of things, you'd see something that looks like this:
		// <Button Width="100" Height="50" Content="Click Me!" />
		// In this example, the Width property is set to 100. This is possible because the Width property is a DependencyProperty.
		// The DependencyProperty class has a RegisterAttached method that is used to register an attached property.
		// An attached property is a property that is defined by one class but attached to another class.

		// Let's more thoroughly review our particular use case:
		// The Model property is an attached property that is attached to the TreeView class.
		// The Model property is used to bind the TreeModel to the TreeView.
		// The Model property is registered as a DependencyProperty using the RegisterAttached method.
		// The RegisterAttached method takes four parameters: the name of the property, the type of the property, the type of the class that the property is attached to, and a PropertyMetadata object.
		// The PropertyMetadata object is used to specify a callback method that is called when the property is changed.
		// In this case, the OnModelChanged method is called when the Model property is changed.
		// The OnModelChanged method is used to bind the TreeModel to the TreeView.
		// The OnModelChanged method gets the TreeView from the DependencyObject and checks if the old value is an ITreeViewEventSink.
		// If the old value is an ITreeViewEventSink, it is disposed of.
		// The OnModelChanged method then checks if the new value is an ITreeViewEventSink.
		// If the new value is an ITreeViewEventSink, it is bound to the TreeView.
		// The TreeView is bound to the TreeModel's Nodes property.
		// The TreeView is attached to the TreeModel's events.
		// The likely reasoning behind the "ITreeViewEventSink" interface is to make it clear that the TreeModel should be able to handle events from the TreeView.
		// However, in this instance, referring to the TreeModel as an ITreeViewEventSink is a bit misleading.
		// Even when the TreeModel implemented the ITreeViewEventSink interface, it was never really an event *sink*, it handled events from the TreeView in a completely standard way.
		// The TreeModel was never a "sink" for events, it was just a model that was bound to the TreeView.


		// The key thing to realize here is that the DependencyProperty pattern is a fundamental part of WPF.
		// It allows properties to be set in XAML, styled, data-bound, and animated.
		// In this case, the Model property is used to bind the TreeModel to the TreeView.
		// This allows the TreeView to display the nodes of the TreeModel and respond to events from the TreeModel.

		public static object GetModel(DependencyObject obj)
        {
            return (object)obj.GetValue(ModelProperty);
        }

        public static void SetModel(DependencyObject obj, object value)
        {
            obj.SetValue(ModelProperty, value);
        }

        // Using a DependencyProperty as the backing store for Model.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ModelProperty =
            DependencyProperty.RegisterAttached("Model", typeof(object), typeof(TreeViewBehavior), new PropertyMetadata(OnModelChanged));

		/// <summary>
		/// This method is called when the Model property is changed. It is used to bind the <see cref="TreeModel"/> to the <see cref="TreeView"/>.<br/>
		/// In detail, it binds the <see cref="TreeModel"/> to the <see cref="TreeView"/>'s ItemsSource property.
		/// </summary>
		/// <param name="obj"> The DependencyObject that the property is attached to. In practice, this is a TreeView.</param>
		/// <param name="eventArgs"> The event arguments. Essentially the old and new values of the property.</param>
		/// <remarks>
		/// Implementation Explanation:<br/>
		/// If the old value is an <see cref="TreeModel"/>, it is disposed of.<br/>
		/// If the new value is an <see cref="TreeModel"/>, it is bound to the TreeView.<br/>
		/// The TreeView is bound to the TreeModel's Nodes property.<br/>
		/// The TreeView is attached to the TreeModel's events.
		/// </remarks>
		private static void OnModelChanged(DependencyObject obj, DependencyPropertyChangedEventArgs eventArgs)
        {
			/// Handle binding and disposal if the DependencyObject is a <see cref="TreeView"/>.
			if (obj is TreeView treeView) {

				/// If the old value is a <see cref="TreeModel"/>, dispose of it.
				if (eventArgs.OldValue is TreeModel oldModel) {
                    if (oldModel.Source is IDisposable disposable) { disposable.Dispose(); }
					oldModel.Source = null;
                }

				/// If the new value is a <see cref="TreeModel"/>, bind it to the TreeView.
				if (eventArgs.NewValue is TreeModel newModel) {
					newModel.Source = new TreeViewModelBinding(treeView, newModel);
                }
            }

		}


		public static readonly DependencyProperty MultiSelectionEnabledProperty =
			DependencyProperty.RegisterAttached(
				"MultiSelectionEnabled",
				typeof(bool),
				typeof(TreeViewBehavior),
				new PropertyMetadata(false, OnMultiSelectionEnabledChanged)
			);

		public static void SetMultiSelectionEnabled(DependencyObject obj, bool value) {
			obj.SetValue(MultiSelectionEnabledProperty, value);
		}
		public static bool GetMultiSelectionEnabled(DependencyObject obj) {
			return (bool)obj.GetValue(MultiSelectionEnabledProperty);
		}

		private static void OnMultiSelectionEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is TreeView tv) {
				bool enable = (bool)e.NewValue;
				if (enable) {
					tv.SelectedItemChanged += OnSelectedItemChanged;
					tv.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
				}
				else {
					tv.SelectedItemChanged -= OnSelectedItemChanged;
					tv.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
				}
			}
		}

		private static void OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
			e.Handled = true; // block default selection logic
			//if (e.Source is TreeNode node) {
			//	Console.WriteLine(node.IsSelected);
			//}
		}

		private static void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			e.Handled = true;
			//if (sender is TreeView tv) {
			//	var tvi = VisualUpwardSearch(e.OriginalSource as DependencyObject) as TreeViewItem;
			//	if (tvi != null) {
			//		bool isCtrlPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
			//		// Add your multi-selection logic here
			//		e.Handled = true;
			//	}
			//}
		}

		private static DependencyObject VisualUpwardSearch(DependencyObject source) {
			while (source != null && !( source is TreeViewItem ))
				source = VisualTreeHelper.GetParent(source);
			return source;
		}

	}

}
