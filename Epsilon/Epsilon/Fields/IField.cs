using Epsilon.Menus;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using Epsilon.Common;

namespace Epsilon.Fields
{
	public abstract class IField : PropertyChangedNotifier, IDisposable
	{
		/// <summary>
		/// The Parent <see cref="IField" /> this field
		/// </summary>
		public IField Parent { get; set; }

		/// <summary>
		/// Populates the field recursively
		/// </summary>
		/// <param name="owner">The owner of used to get the field's value</param>
		/// <param name="value">The value to set. If null it will get the value from the Owner</param>
		public abstract void Populate(object owner, object value = null);

		/// <summary>
		/// Visitor Accept function - Allows functionality to be implemented without having to modify the tag field interface.
		/// </summary>
		/// <param name="visitor">The visitor</param>
		public abstract void Accept(IFieldVisitor visitor);

		/// <summary>
		/// Called when the context menu should be populated
		/// </summary>
		/// <param name="menu"></param>
		protected virtual void OnPopulateContextMenu(Node menu) { }

		/// <summary>
		/// Populate the context menu for this field and its parents.
		/// </summary>
		/// <param name="menu"></param>
		public void PopulateContextMenu(Node menu)
		{
			//var oldItemCount = menu.Items.Count;
			OnPopulateContextMenu(menu);

			//if (menu.Items.Count > oldItemCount && Parent != null)
			//	menu.Items.Add(new Separator());

			//if (Parent != null)
			//	Parent.PopulateContextMenu(menu);
		}

		/// <summary>
		/// Returns <see langword="true"/> if the field is not of the specified type - <typeparamref name="T0"/>; otherwise, <see langword="false"/>.
		/// </summary>
		/// <typeparam name="T0"> The <see cref="Type"/> to check. </typeparam>
		/// <returns> <see langword="true"/> if the field is not of the specified type - <typeparamref name="T0"/>; otherwise, <see langword="false"/>. </returns>
		public bool IsNot<T0>()
			where T0 : class { return !( this is T0 ); }
		/// <summary>
		/// Returns <see langword="true"/> if the field is not of any of any of the specified types - (<typeparamref name="T0"/>, <typeparamref name="T1"/>); otherwise, <see langword="false"/>.
		/// </summary>
		/// <typeparam name="T0"> The first <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T1"> The second <see cref="Type"/> to check. </typeparam>
		/// <returns> <see langword="true"/> if the field is not of any of any of the specified types - (<typeparamref name="T0"/>, <typeparamref name="T1"/>); otherwise, <see langword="false"/>. </returns>
		public bool IsNot<T0, T1>()
			where T0 : class where T1 : class { return !( this is T0 || this is T1 ); }
		/// <summary>
		/// Returns <see langword="true"/> if the field is not of any of any of the specified types - (<typeparamref name="T0"/>, <typeparamref name="T1"/>, <typeparamref name="T2"/>); otherwise, <see langword="false"/>.
		/// </summary>
		/// <typeparam name="T0"> The first <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T1"> The second <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T2"> The third <see cref="Type"/> to check. </typeparam>
		/// <returns> <see langword="true"/> if the field is not of any of any of the specified types - (<typeparamref name="T0"/>, <typeparamref name="T1"/>, <typeparamref name="T2"/>); otherwise, <see langword="false"/>. </returns>
		public bool IsNot<T0, T1, T2>()
			where T0 : class where T1 : class where T2 : class { return !( this is T0 || this is T1 || this is T2 ); }
		/// <summary>
		/// Returns <see langword="true"/> if the field is not of any of any of the specified types - (<typeparamref name="T0"/>, <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>); otherwise, <see langword="false"/>.
		/// </summary>
		/// <typeparam name="T0"> The first <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T1"> The second <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T2"> The third <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T3"> The fourth <see cref="Type"/> to check. </typeparam>
		/// <returns> <see langword="true"/> if the field is not of any of any of the specified types - (<typeparamref name="T0"/>, <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>); otherwise, <see langword="false"/>. </returns>
		public bool IsNot<T0, T1, T2, T3>()
			where T0 : class where T1 : class where T2 : class where T3 : class { return !( this is T0 || this is T1 || this is T2 || this is T3 ); }
		/// <summary>
		/// Returns <see langword="true"/> if the field is not of any of any of the specified types - (<typeparamref name="T0"/>, <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>); otherwise, <see langword="false"/>. </returns>
		/// </summary>
		/// <typeparam name="T0"> The first <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T1"> The second <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T2"> The third <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T3"> The fourth <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T4"> The fifth <see cref="Type"/> to check. </typeparam>
		/// <returns> <see langword="true"/> if the field is not of any of any of the specified types - (<typeparamref name="T0"/>, <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>); otherwise, <see langword="false"/>. </returns>
		public bool IsNot<T0, T1, T2, T3, T4>()
			where T0 : class where T1 : class where T2 : class where T3 : class where T4 : class { return !( this is T0 || this is T1 || this is T2 || this is T3 || this is T4 ); }
		/// <summary>
		/// Returns <see langword="true"/> if the field is not of any of any of the specified types - (<typeparamref name="T0"/>, <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>, <typeparamref name="T5"/>); otherwise, <see langword="false"/>. </returns>
		/// </summary>
		/// <typeparam name="T0"> The first <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T1"> The second <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T2"> The third <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T3"> The fourth <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T4"> The fifth <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T5"> The sixth <see cref="Type"/> to check. </typeparam>
		/// <returns> <see langword="true"/> if the field is not of any of any of the specified types - (<typeparamref name="T0"/>, <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>, <typeparamref name="T5"/>); otherwise, <see langword="false"/>. </returns>
		public bool IsNot<T0, T1, T2, T3, T4, T5>()
			where T0 : class where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class { return !( this is T0 || this is T1 || this is T2 || this is T3 || this is T4 || this is T5 ); }
		/// <summary>
		/// Returns <see langword="true"/> if the field is not of any of any of the specified types - (<typeparamref name="T0"/>, <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>, <typeparamref name="T5"/>, <typeparamref name="T6"/>); otherwise, <see langword="false"/>. </returns>
		/// </summary>
		/// <typeparam name="T0"> The first <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T1"> The second <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T2"> The third <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T3"> The fourth <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T4"> The fifth <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T5"> The sixth <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T6"> The seventh <see cref="Type"/> to check. </typeparam>
		/// <returns> <see langword="true"/> if the field is not of any of any of the specified types - (<typeparamref name="T0"/>, <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>, <typeparamref name="T5"/>, <typeparamref name="T6"/>); otherwise, <see langword="false"/>. </returns>
		public bool IsNot<T0, T1, T2, T3, T4, T5, T6>()
			where T0 : class where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class { return !( this is T0 || this is T1 || this is T2 || this is T3 || this is T4 || this is T5 || this is T6 ); }

		/// <summary>
		/// Returns <see langword="true"/> if the field is of the specified type - <typeparamref name="T0"/>; otherwise, <see langword="false"/>.
		/// </summary>
		/// <typeparam name="T0"> The <see cref="Type"/> to check. </typeparam>
		/// <returns> <see langword="true"/> if the field is of the specified type - <typeparamref name="T0"/>; otherwise, <see langword="false"/>. </returns>
		public bool Is<T0>()
			where T0 : class { return this is T0; }
		/// <summary>
		/// Returns <see langword="true"/> if the field is of any of the specified types - (<typeparamref name="T0"/>, <typeparamref name="T1"/>); otherwise, <see langword="false"/>.
		/// </summary>
		/// <typeparam name="T0"> The first <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T1"> The second <see cref="Type"/> to check. </typeparam>
		/// <returns> <see langword="true"/> if the field is of any of the specified types - (<typeparamref name="T0"/>, <typeparamref name="T1"/>); otherwise, <see langword="false"/>. </returns>
		public bool Is<T0, T1>()
			where T0 : class where T1 : class { return this is T0 || this is T1; }
		/// <summary>
		/// Returns <see langword="true"/> if the field is of any of the specified types - (<typeparamref name="T0"/>, <typeparamref name="T1"/>, <typeparamref name="T2"/>); otherwise, <see langword="false"/>.
		/// </summary>
		/// <typeparam name="T0"> The first <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T1"> The second <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T2"> The third <see cref="Type"/> to check. </typeparam>
		/// <returns> <see langword="true"/> if the field is of any of the specified types - (<typeparamref name="T0"/>, <typeparamref name="T1"/>, <typeparamref name="T2"/>); otherwise, <see langword="false"/>. </returns>
		public bool Is<T0, T1, T2>()
			where T0 : class where T1 : class where T2 : class { return this is T0 || this is T1 || this is T2; }
		/// <summary>
		/// Returns <see langword="true"/> if the field is of any of the specified types - (<typeparamref name="T0"/>, <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>); otherwise, <see langword="false"/>.
		/// </summary>
		/// <typeparam name="T0"> The first <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T1"> The second <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T2"> The third <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T3"> The fourth <see cref="Type"/> to check. </typeparam>
		/// <returns> <see langword="true"/> if the field is of any of the specified types - (<typeparamref name="T0"/>, <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>); otherwise, <see langword="false"/>. </returns>
		public bool Is<T0, T1, T2, T3>()
			where T0 : class where T1 : class where T2 : class where T3 : class { return this is T0 || this is T1 || this is T2 || this is T3; }
		/// <summary>
		/// Returns <see langword="true"/> if the field is of any of the specified types - (<typeparamref name="T0"/>, <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>); otherwise, <see langword="false"/>. </returns>
		/// </summary>
		/// <typeparam name="T0"> The first <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T1"> The second <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T2"> The third <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T3"> The fourth <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T4"> The fifth <see cref="Type"/> to check. </typeparam>
		/// <returns> <see langword="true"/> if the field is of any of the specified types - (<typeparamref name="T0"/>, <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>); otherwise, <see langword="false"/>. </returns>
		public bool Is<T0, T1, T2, T3, T4>()
			where T0 : class where T1 : class where T2 : class where T3 : class where T4 : class { return this is T0 || this is T1 || this is T2 || this is T3 || this is T4; }
		/// <summary>
		/// Returns <see langword="true"/> if the field is of any of the specified types - (<typeparamref name="T0"/>, <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>, <typeparamref name="T5"/>); otherwise, <see langword="false"/>. </returns>
		/// </summary>
		/// <typeparam name="T0"> The first <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T1"> The second <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T2"> The third <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T3"> The fourth <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T4"> The fifth <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T5"> The sixth <see cref="Type"/> to check. </typeparam>
		/// <returns> <see langword="true"/> if the field is of any of the specified types - (<typeparamref name="T0"/>, <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>, <typeparamref name="T5"/>); otherwise, <see langword="false"/>. </returns>
		public bool Is<T0, T1, T2, T3, T4, T5>()
			where T0 : class where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class { return this is T0 || this is T1 || this is T2 || this is T3 || this is T4 || this is T5; }
		/// <summary>
		/// Returns <see langword="true"/> if the field is of any of the specified types - (<typeparamref name="T0"/>, <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>, <typeparamref name="T5"/>, <typeparamref name="T6"/>); otherwise, <see langword="false"/>. </returns>
		/// </summary>
		/// <typeparam name="T0"> The first <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T1"> The second <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T2"> The third <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T3"> The fourth <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T4"> The fifth <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T5"> The sixth <see cref="Type"/> to check. </typeparam>
		/// <typeparam name="T6"> The seventh <see cref="Type"/> to check. </typeparam>
		/// <returns> <see langword="true"/> if the field is of any of the specified types - (<typeparamref name="T0"/>, <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, <typeparamref name="T4"/>, <typeparamref name="T5"/>, <typeparamref name="T6"/>); otherwise, <see langword="false"/>. </returns>
		public bool Is<T0, T1, T2, T3, T4, T5, T6>()
			where T0 : class where T1 : class where T2 : class where T3 : class where T4 : class where T5 : class where T6 : class { return this is T0 || this is T1 || this is T2 || this is T3 || this is T4 || this is T5 || this is T6; }

		public virtual void Dispose() { }
	}
}
