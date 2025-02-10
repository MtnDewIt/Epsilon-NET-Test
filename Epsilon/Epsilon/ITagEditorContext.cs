namespace Epsilon
{
	public interface ITagEditorContext
	{
		bool IsValid { get; }
		ICacheEditor CacheEditor { get; set; }
		Shared.IShell Shell { get; set; }
		TagTool.Cache.CachedTag Instance { get; set; }
		object DefinitionData { get; }
		TagEditorViewModel ViewModel { get; set; }

	}
}
