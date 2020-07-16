using EpsilonLib.Options;
using Shared;
using Stylet;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryingCatPlugin
{
    [Export(typeof(IOptionsPage))]
    class OptionsPageViewModel : Screen, IOptionsPage
    {
        public string Category => "Crying Cat";

        public bool IsDirty { get; set; }

        public OptionsPageViewModel()
        {
            DisplayName = "General";
        }

        public void Apply()
        {
            //throw new NotImplementedException();
        }

        public void Load()
        {
            //throw new NotImplementedException();
        }
    }
}
