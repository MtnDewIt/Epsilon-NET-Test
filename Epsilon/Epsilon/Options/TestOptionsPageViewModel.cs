using EpsilonLib.Options;
using Stylet;
using System.ComponentModel.Composition;

namespace Epsilon.Options
{
    [Export(typeof(IOptionsPage))]
    class TestOptionsPageViewModel : Screen, IOptionsPage
    {
        public string Category => "Testing";

        public bool IsDirty { get; set; }

        public TestOptionsPageViewModel()
        {
            DisplayName = "Test";
        }

        public void Apply()
        {
            
        }

        public void Load()
        {
            
        }
    }
}
