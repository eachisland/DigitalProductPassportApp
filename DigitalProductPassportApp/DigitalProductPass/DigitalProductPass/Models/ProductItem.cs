using System.ComponentModel.DataAnnotations;

namespace DigitalProductPass.Models;

/// <summary>One concrete garment (variant) with its own unique serial SKU and QR code.</summary>
public class ProductItem
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    /// <summary>Random identifier used in the QR resolver URL (not guessable).</summary>
    public Guid PublicId { get; set; } = Guid.NewGuid();

    /// <summary>Unique variant serial SKU, e.g. TSHORG-WHI-M-7K3M9 (also used as GS1 AI 21).</summary>
    [Required, StringLength(32)]
    public string SerialCode { get; set; } = "";

    [StringLength(6)]
    public string? DifferentiationCode { get; set; }

    [StringLength(20)]
    public string? Size { get; set; }

    [StringLength(50)]
    public string? Color { get; set; }

    /// <summary>Production batch / lot (GS1 AI 10).</summary>
    [StringLength(20)]
    public string? BatchNumber { get; set; }

    public DateOnly ProducedOn { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }

    public List<ScanEvent> ScanEvents { get; set; } = new();
}
