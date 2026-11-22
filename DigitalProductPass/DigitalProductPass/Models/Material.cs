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
}
