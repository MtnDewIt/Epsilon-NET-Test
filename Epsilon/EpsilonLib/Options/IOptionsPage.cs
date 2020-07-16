using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpsilonLib.Options
{
    public interface IOptionsPage : IScreen
    {
        string Category { get; }

        bool IsDirty { get; set; }

        void Apply();

        void Load();
    }

    public class ProvideOptionsPageAttribute
    {
        public string Category { get; set; }
    }
}
