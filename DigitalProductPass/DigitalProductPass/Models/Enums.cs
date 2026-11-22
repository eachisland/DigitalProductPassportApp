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
    public static string Label(this SupplyChainStage stage) => stage switch
    {
        SupplyChainStage.RawMaterial => "Суровина",
        SupplyChainStage.Spinning => "Предене",
        SupplyChainStage.Weaving => "Тъкане / плетене",
        SupplyChainStage.Dyeing => "Боядисване",
        SupplyChainStage.Assembly => "Шиене / сглобяване",
        SupplyChainStage.Packaging => "Опаковане",
        SupplyChainStage.Distribution => "Дистрибуция",
        _ => stage.ToString()
    };
}
