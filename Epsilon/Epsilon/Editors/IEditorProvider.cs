using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Editors
{
	public interface IEditorProvider
	{
		string DisplayName { get; }

		Guid Id { get; }

		IReadOnlyList<string> FileExtensions { get; }

		Task OpenFileAsync(IShell shell, params string[] paths);
	}
}
