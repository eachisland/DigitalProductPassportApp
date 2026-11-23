using System.ComponentModel.DataAnnotations;

namespace DigitalProductPass.Models;

public class Brand
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = "";

    [StringLength(100)]
    public string? Country { get; set; }

    [StringLength(300)]
    public string? Website { get; set; }

    public List<Product> Products { get; set; } = new();
}
