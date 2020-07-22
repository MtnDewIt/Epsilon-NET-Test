using EpsilonLib.Commands;

namespace ModPackagePlugin.Commands
{
    [ExportCommand]
    class NewModPackageCommand : CommandDefinition
    {
        public override string Name => "ModPackage.New";

        public override string DisplayText => "Mod Package...";
    }
}
