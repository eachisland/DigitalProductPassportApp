using System.ComponentModel.DataAnnotations;

namespace DigitalProductPass.Models;

public class Product
{
    public int Id { get; set; }

    public int BrandId { get; set; }
    public Brand Brand { get; set; } = null!;

    [Required, StringLength(200)]
    public string Name { get; set; } = "";

    [Required, StringLength(64)]
    public string Sku { get; set; } = "";

    /// <summary>GS1 GTIN normalised to 14 digits. Needed to build a GS1 Digital Link.</summary>
    [StringLength(14)]
    public string? Gtin { get; set; }

    /// <summary>Short alphanumeric code (max 6) used as the first block of every variant serial SKU.</summary>
    [StringLength(6)]
    public string? DifferentiationCode { get; set; }

    [StringLength(100)]
    public string? Category { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    /// <summary>Garment weight in kg; multiplied with the weighted material carbon index.</summary>
    public decimal? WeightKg { get; set; }

    /// <summary>Manually declared (whole-lifecycle) footprint. The calculated material footprint is shown next to it.</summary>
    public decimal? CarbonFootprintKg { get; set; }

    public decimal? WaterUsageLiters { get; set; }

    /// <summary>0-100.</summary>
    public int? RecyclabilityScore { get; set; }

    /// <summary>0-10.</summary>
    public decimal? RepairabilityIndex { get; set; }

    [StringLength(1000)]
    public string? WashInstructions { get; set; }

    [StringLength(1000)]
    public string? EndOfLifeGuidance { get; set; }

    // Legacy fields, still used as fallback when the new ones are empty.
    [StringLength(1000)]
    public string? CareInstructions { get; set; }

    [StringLength(1000)]
    public string? RecyclingInstructions { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Material> Materials { get; set; } = new();
    public List<SupplyChainStep> SupplyChainSteps { get; set; } = new();
    public List<ProductCertificate> Certificates { get; set; } = new();
    public List<ProductItem> Items { get; set; } = new();
}
