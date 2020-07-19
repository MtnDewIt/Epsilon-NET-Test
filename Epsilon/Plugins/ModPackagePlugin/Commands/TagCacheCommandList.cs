using EpsilonLib.Commands;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModPackagePlugin.Commands
{
    [ExportCommand]
    class TagCacheCommandList : CommandListDefinition
    {
        public override string Name => "ModPackage.SelectTagCache";

        public override string DisplayText => "Tag Cache";
    }
}
