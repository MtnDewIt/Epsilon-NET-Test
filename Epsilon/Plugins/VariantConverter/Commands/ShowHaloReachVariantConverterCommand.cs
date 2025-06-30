using EpsilonLib.Commands;

namespace VariantConverter
{
    [ExportCommand]
    class ShowHaloReachVariantConverterCommand : CommandDefinition
    {
        public override string Name => "HaloReachVariantConverter.Show";

        public override string DisplayText => "Convert Halo Reach Variants";
    }
}
