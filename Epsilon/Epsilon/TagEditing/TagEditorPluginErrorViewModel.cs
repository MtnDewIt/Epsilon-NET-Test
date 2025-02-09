using Stylet;
using System;

namespace Epsilon
{
    internal class TagEditorPluginErrorViewModel : Screen
    {
        public string ErrorMessage { get; }

        public TagEditorPluginErrorViewModel(Exception ex)
        {
            ErrorMessage = ex.ToString();
        }
    }
}