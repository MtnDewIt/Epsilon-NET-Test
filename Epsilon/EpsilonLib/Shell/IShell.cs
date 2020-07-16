using EpsilonLib.Shell;
using Stylet;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public interface IShell
    {
        IObservableCollection<IScreen> Documents { get; }
        IScreen ActiveDocument { get; set; }
        IStatusBar StatusBar { get; }

        IProgressReporter CreateProgressScope();
        bool? ShowDialog(object viewModel);
    }
}
