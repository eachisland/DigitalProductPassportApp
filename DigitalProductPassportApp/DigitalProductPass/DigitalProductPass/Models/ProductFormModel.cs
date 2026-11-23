using System.ComponentModel.DataAnnotations;

namespace DigitalProductPass.Models;

public class ProductFormModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Choose a brand.")]
    public int BrandId { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = "";

    [Required, StringLength(64)]
    public string Sku { get; set; } = "";

    [Required, RegularExpression("^[A-Za-z0-9]{2,6}$", ErrorMessage = "2-6 letters or digits.")]
    public string DifferentiationCode { get; set; } = "";

    public string? Gtin { get; set; }

    [StringLength(100)]
    public string? Category { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [StringLength(500), Url(ErrorMessage = "Invalid URL.")]
    public string? ImageUrl { get; set; }

    [Range(0.001, 100, ErrorMessage = "0.001 - 100 kg.")]
    public decimal? WeightKg { get; set; }

    [Range(0, 100000)]
    public decimal? CarbonFootprintKg { get; set; }

    [Range(0, 10000000)]
    public decimal? WaterUsageLiters { get; set; }

    [Range(0, 100)]
    public int? RecyclabilityScore { get; set; }

    [Range(0, 10)]
    public decimal? RepairabilityIndex { get; set; }

    [StringLength(1000)]
    public string? WashInstructions { get; set; }

    [StringLength(1000)]
    public string? EndOfLifeGuidance { get; set; }
}
