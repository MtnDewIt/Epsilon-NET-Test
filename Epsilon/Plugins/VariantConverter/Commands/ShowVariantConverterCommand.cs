using EpsilonLib.Commands;

namespace VariantConverter
{
    [ExportCommand]
    class ShowVariantConverterCommand : CommandDefinition
    {
        public override string Name => "VariantConverter.Show";

        public override string DisplayText => "Convert 0.6 Variants";
    }
}
