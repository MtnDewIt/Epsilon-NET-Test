using Shared;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpsilonLib.Editors
{
    public interface IEditorProvider
    {
        string DisplayName { get; }

        Guid Id { get; }

        IReadOnlyList<string> FileExtensions { get; }

        Task OpenFileAsync(IShell shell, string fileName);
    }

    public interface IEditor : IScreen
    {

    }
}
