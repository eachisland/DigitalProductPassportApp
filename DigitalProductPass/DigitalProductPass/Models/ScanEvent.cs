using System.ComponentModel.DataAnnotations;

namespace DigitalProductPass.Models;

public class ScanEvent
{
    public long Id { get; set; }
    public int ProductItemId { get; set; }
    public ProductItem ProductItem { get; set; } = null!;

    public DateTime ScannedAt { get; set; } = DateTime.UtcNow;

    [StringLength(300)]
    public string? UserAgent { get; set; }
}
