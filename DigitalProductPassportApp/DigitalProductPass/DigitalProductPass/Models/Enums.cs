using DigitalProductPass.Services;

namespace DigitalProductPass.Models;

public enum SupplyChainStage
{
    RawMaterial = 0,
    Spinning = 1,
    Weaving = 2,
    Dyeing = 3,
    Assembly = 4,
    Packaging = 5,
    Distribution = 6
}

public static class EnumLabels
{
    /// <summary>Localised label (EN / TR / DE) via the key "stage.&lt;Name&gt;".</summary>
    public static string Label(this SupplyChainStage stage, UiText t) => t["stage." + stage];
}
