using Shared;
using Stylet;
using TagTool.Tags;

namespace Epsilon
{
	/// <summary>
	/// These are the views that are hosted by the <see cref="TagEditorViewModel"/>.<br/>
	/// For example, the views for the tag's <code><b>Definition</b>, <b>Script</b>, and <b>Render Method Editor</b></code>
	/// </summary>
	public interface ITagEditorPlugin : IScreen
	{

		/// <summary>
		/// The definition of the tag that is being edited.
		/// </summary>
		TagStructure Definition { get; set; }

		/// <summary>
		/// This is the client that the <see cref="ITagEditorPlugin"/> can use to post messages to the client.<br/>
		/// This is typically the <see cref="TagEditorViewModel"/> that hosts the <see cref="ITagEditorPlugin"/>s.
		/// </summary>
		ITagEditorPluginClient Client { get; set; }

		/// <summary>
		/// The <see cref="IShell"/> that hosts the <see cref="TagEditorViewModel"/> that hosts the <see cref="ITagEditorPlugin"/>s.
		/// </summary>
		IShell Shell { get; set; }

		/// <summary>
		/// This method is called by the <see cref="ITagEditorPluginClient"/> to post a message to the client (typically the <see cref="TagEditorViewModel"/>).
		/// </summary>
		/// <param name="sender"> An arbitrary object that may identify the sender or carry data. </param>
		/// <param name="message"> Typically a <see cref="Epsilon.TagEditing.Messages.DefinitionDataChangedEvent"/> or an arbitrary message / data object. </param>
		void OnMessage(object sender, object message);

		/// <summary>
		/// Implement this method to determine if the plugin is valid for the supplied <see cref="CachedTag"/>.
		/// </summary>
		/// <param name="cache"> The <see cref="ICacheFile"/> that contains the tag. </param>
		/// <param name="tag"> The <see cref="CachedTag"/> to validate. </param>
		/// <returns> <see langword="true"/> if the plugin is valid for the tag; otherwise, <see langword="false"/>. </returns>
		bool ValidForTag(ICacheFile cache, TagTool.Cache.CachedTag tag);

	}
}
