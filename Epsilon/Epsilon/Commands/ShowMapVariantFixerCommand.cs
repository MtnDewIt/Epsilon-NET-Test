namespace Epsilon.Commands
{
    [ExportCommand]
    class ShowMapVariantFixerCommand : CommandDefinition
    {
        public override string Name => "MapVariantFixer.Show";

        public override string DisplayText => "Update Map Variants";
    }
}
