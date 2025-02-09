using Epsilon.Commands;

namespace Epsilon.Components.TagTree.Commands
{
    [ExportCommand]
    public class ExportPhmoJMSCommand : CommandDefinition
    {
        public override string Name => "TagTree.ExportModeJMS";

        public override string DisplayText => "Export phmo JMS...";
    }
}
