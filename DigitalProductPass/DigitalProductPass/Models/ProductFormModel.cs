using System.ComponentModel.DataAnnotations;

namespace DigitalProductPass.Models;

public class ProductFormModel
{
    [Display(Name = "Марка")]
    [Range(1, int.MaxValue, ErrorMessage = "Избери марка.")]
    public int BrandId { get; set; }

    [Display(Name = "Име"), Required(ErrorMessage = "Въведи име."), StringLength(200)]
    public string Name { get; set; } = "";

    [Display(Name = "Артикулен номер (SKU)"), Required(ErrorMessage = "Въведи SKU."), StringLength(64)]
    public string Sku { get; set; } = "";

    [Display(Name = "Категория"), StringLength(100)]
    public string? Category { get; set; }

    [Display(Name = "Описание"), StringLength(2000)]
    public string? Description { get; set; }

    [Display(Name = "URL на снимка"), StringLength(500), Url(ErrorMessage = "Невалиден URL.")]
    public string? ImageUrl { get; set; }

    [Display(Name = "Въглероден отпечатък (kg CO₂e)"), Range(0, 100000)]
    public decimal? CarbonFootprintKg { get; set; }

    [Display(Name = "Грижа за дрехата"), StringLength(1000)]
    public string? CareInstructions { get; set; }

    [Display(Name = "Рециклиране"), StringLength(1000)]
    public string? RecyclingInstructions { get; set; }
}
