using System.ComponentModel.DataAnnotations;

namespace DigitalProductPass.Models;

public class ProductCertificate
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    [Required, StringLength(200)]
    public string Name { get; set; } = "";

    [StringLength(200)]
    public string? Issuer { get; set; }

    public DateOnly? ValidUntil { get; set; }

    [StringLength(500)]
    public string? DocumentUrl { get; set; }
}
