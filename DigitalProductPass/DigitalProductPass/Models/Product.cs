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

    [StringLength(100)]
    public string? Category { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public decimal? CarbonFootprintKg { get; set; }

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
