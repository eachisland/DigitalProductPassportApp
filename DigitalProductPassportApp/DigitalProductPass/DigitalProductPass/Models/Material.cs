using System.ComponentModel.DataAnnotations;

namespace DigitalProductPass.Models;

public class Material
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    [Required, StringLength(150)]
    public string Name { get; set; } = "";

    public decimal Percentage { get; set; }
    public bool IsRecycled { get; set; }

    [StringLength(100)]
    public string? Origin { get; set; }

    /// <summary>Cradle-to-gate carbon index of the material, kg CO2e per kg of material.</summary>
    public decimal? CarbonIndexKgPerKg { get; set; }

    /// <summary>Certificate / chain-of-custody code for this material (e.g. GOTS licence number).</summary>
    [StringLength(64)]
    public string? CertificateCode { get; set; }
}
