using Stylet;

namespace Epsilon
{
	/// <summary>
	/// This is typically the <see cref="TagEditorViewModel"/> that hosts the <see cref="ITagEditorPlugin"/>s.
	/// </summary>
	public interface ITagEditorPluginClient
	{

		/// <summary>
		/// The <see cref="ITagEditorPlugin"/> that is currently being hosted by the client.
		/// </summary>
		ITagEditorPlugin Content { get; set; }

		/// <summary>
		/// This method is called by the <see cref="ITagEditorPlugin"/> to post a message to the client (typically the <see cref="TagEditorViewModel"/>).
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="message"></param>
		void PostMessage(object sender, object message);
	}
}
