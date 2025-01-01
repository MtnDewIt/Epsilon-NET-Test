using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheEditor
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
