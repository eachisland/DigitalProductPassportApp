using System.ComponentModel.DataAnnotations;

namespace DigitalProductPass.Models;

public class SupplyChainStep
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Order { get; set; }
    public SupplyChainStage Stage { get; set; }

    [Required, StringLength(200)]
    public string FacilityName { get; set; } = "";

    [Required, StringLength(100)]
    public string Country { get; set; } = "";

    public DateOnly? Date { get; set; }
}
