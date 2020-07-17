using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpsilonLib.Editors
{
    public interface IEditorService
    {
        IEnumerable<IEditorProvider> EditorProviders { get; }

        Task OpenFileWithEditorAsync(string filePath, Guid editorProviderId);
        Task OpenFileAsync(string filePath);
    }
}
