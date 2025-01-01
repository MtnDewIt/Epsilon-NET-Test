using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheEditor.TagEditing.Messages
{
	public enum DefinitionDataChangedEventType
	{

		UnspecifiedChange,
		ValueChanged,
		ValueAdded,
		ValueRemoved,
		ScriptChanged,
		RenderMethodChanged,
		ResourceChanged

	}
}
