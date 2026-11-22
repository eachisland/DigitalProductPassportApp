using System.ComponentModel.DataAnnotations;

namespace DigitalProductPass.Models;

/// <summary>Една конкретна дреха с уникален QR код.</summary>
public class ProductItem
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    /// <summary>Случаен идентификатор, който отива в QR кода (не се налучква).</summary>
    public Guid PublicId { get; set; } = Guid.NewGuid();

    /// <summary>Четим код, отпечатан на етикета, напр. DPP-7K3M-9QXA.</summary>
    [Required, StringLength(32)]
    public string SerialCode { get; set; } = "";

    [StringLength(20)]
    public string? Size { get; set; }

    [StringLength(50)]
    public string? Color { get; set; }

    public DateOnly ProducedOn { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public List<ScanEvent> ScanEvents { get; set; } = new();
}
