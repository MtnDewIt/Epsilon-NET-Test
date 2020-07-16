using EpsilonLib.Options;
using Stylet;
using System.ComponentModel.Composition;

namespace DefinitionEditor.Options
{
    [Export(typeof(IOptionsPage))]
    class OptionsPageViewModel : Screen, IOptionsPage
    {
        public string Category => "Cache Editor";

        public bool IsDirty { get; set; }

        public OptionsPageViewModel()
        {
            DisplayName = "Definition Editor";
        }

        public void Apply()
        {
            
        }

        public void Load()
        {
            
        }
    }
}
