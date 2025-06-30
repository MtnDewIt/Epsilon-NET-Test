using EpsilonLib.Commands;

namespace VariantConverter
{
    [ExportCommand]
    class ShowHalo3VariantConverterCommand : CommandDefinition
    {
        public override string Name => "Halo3VariantConverter.Show";

        public override string DisplayText => "Convert Halo 3 Variants";
    }
}
