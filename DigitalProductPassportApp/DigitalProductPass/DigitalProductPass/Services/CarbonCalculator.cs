using DigitalProductPass.Models;

namespace DigitalProductPass.Services;

public sealed record CarbonResult(
    decimal TotalPercentage,
    bool PercentagesValid,
    bool AllIndicesKnown,
    decimal? IndexKgPerKg,
    decimal? CalculatedKg,
    decimal? DeclaredKg)
{
    /// <summary>Calculated value wins; the declared whole-lifecycle value is the fallback.</summary>
    public decimal? HeadlineKg => CalculatedKg ?? DeclaredKg;
    public bool IsCalculated => CalculatedKg.HasValue;
}

public static class CarbonCalculator
{
    public const decimal Tolerance = 0.01m;

    public static bool IsHundred(decimal total) => Math.Abs(total - 100m) <= Tolerance;

    /// <summary>
    /// Weighted material carbon index = Σ (share% / 100 × material index)  [kg CO2e per kg of fabric].
    /// Only computed when the shares sum to 100% and every material has an index.
    /// Footprint (kg CO2e) = weighted index × garment weight (kg).
    /// </summary>
    public static CarbonResult Compute(Product p)
    {
        var mats = p.Materials;
        var total = mats.Sum(m => m.Percentage);
        var valid = mats.Count > 0 && IsHundred(total);
        var known = mats.Count > 0 && mats.All(m => m.CarbonIndexKgPerKg.HasValue);

        decimal? index = null;
        decimal? calculated = null;
        if (valid && known)
        {
            index = Math.Round(mats.Sum(m => m.Percentage / 100m * m.CarbonIndexKgPerKg!.Value), 3);
            if (p.WeightKg.HasValue && p.WeightKg.Value > 0)
                calculated = Math.Round(index.Value * p.WeightKg.Value, 2);
        }

        return new CarbonResult(total, valid, known, index, calculated, p.CarbonFootprintKg);
    }
}
