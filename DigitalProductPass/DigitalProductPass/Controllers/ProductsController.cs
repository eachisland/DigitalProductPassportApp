using DigitalProductPass.Data;
using DigitalProductPass.Models;
using DigitalProductPass.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DigitalProductPass.Controllers;

// TODO (етап 3): [Authorize(Roles = "Brand,Admin")] + филтриране по марката на потребителя
public class ProductsController : Controller
{
    private readonly ApplicationDbContext _db;

    public ProductsController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var products = await _db.Products
            .AsNoTracking()
            .Include(p => p.Brand)
            .Include(p => p.Items)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadBrands();
        return View(new ProductFormModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormModel form)
    {
        if (await _db.Products.AnyAsync(p => p.BrandId == form.BrandId && p.Sku == form.Sku))
            ModelState.AddModelError(nameof(form.Sku), "Вече има продукт с този SKU за тази марка.");

        if (!ModelState.IsValid)
        {
            await LoadBrands();
            return View(form);
        }

        var product = new Product
        {
            BrandId = form.BrandId,
            Name = form.Name.Trim(),
            Sku = form.Sku.Trim(),
            Category = form.Category,
            Description = form.Description,
            ImageUrl = form.ImageUrl,
            CarbonFootprintKg = form.CarbonFootprintKg,
            CareInstructions = form.CareInstructions,
            RecyclingInstructions = form.RecyclingInstructions
        };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = product.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _db.Products
            .AsNoTracking()
            .Include(p => p.Brand)
            .Include(p => p.Materials)
            .Include(p => p.SupplyChainSteps)
            .Include(p => p.Items)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id);

        return product is null ? NotFound() : View(product);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMaterial(int id, string? name, decimal percentage, bool isRecycled, string? origin)
    {
        if (!await _db.Products.AnyAsync(p => p.Id == id)) return NotFound();

        if (string.IsNullOrWhiteSpace(name) || percentage <= 0 || percentage > 100)
        {
            TempData["Error"] = "Материалът трябва да има име и процент между 0 и 100.";
            return RedirectToAction(nameof(Details), new { id });
        }

        _db.Materials.Add(new Material
        {
            ProductId = id,
            Name = name.Trim(),
            Percentage = percentage,
            IsRecycled = isRecycled,
            Origin = string.IsNullOrWhiteSpace(origin) ? null : origin.Trim()
        });
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddStep(int id, SupplyChainStage stage, string? facilityName, string? country, DateOnly? date)
    {
        if (!await _db.Products.AnyAsync(p => p.Id == id)) return NotFound();

        if (string.IsNullOrWhiteSpace(facilityName) || string.IsNullOrWhiteSpace(country))
        {
            TempData["Error"] = "Въведи обект и държава.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var nextOrder = (await _db.SupplyChainSteps.Where(s => s.ProductId == id).MaxAsync(s => (int?)s.Order) ?? 0) + 1;

        _db.SupplyChainSteps.Add(new SupplyChainStep
        {
            ProductId = id,
            Order = nextOrder,
            Stage = stage,
            FacilityName = facilityName.Trim(),
            Country = country.Trim(),
            Date = date
        });
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateItems(int id, int count, string? size, string? color)
    {
        if (!await _db.Products.AnyAsync(p => p.Id == id)) return NotFound();

        if (count < 1 || count > 500)
        {
            TempData["Error"] = "Броят трябва да е между 1 и 500.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var codes = new HashSet<string>();
        while (codes.Count < count)
        {
            var batch = new HashSet<string>();
            while (codes.Count + batch.Count < count)
            {
                var c = SerialCodeGenerator.New();
                if (!codes.Contains(c)) batch.Add(c);
            }

            // Изхвърляме кодове, които вече съществуват в базата.
            var taken = await _db.ProductItems
                .Where(i => batch.Contains(i.SerialCode))
                .Select(i => i.SerialCode)
                .ToListAsync();
            batch.ExceptWith(taken);
            codes.UnionWith(batch);
        }

        foreach (var code in codes)
        {
            _db.ProductItems.Add(new ProductItem
            {
                ProductId = id,
                SerialCode = code,
                Size = string.IsNullOrWhiteSpace(size) ? null : size.Trim(),
                Color = string.IsNullOrWhiteSpace(color) ? null : color.Trim()
            });
        }
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Генерирани са {count} артикула с уникални QR кодове.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task LoadBrands()
    {
        var brands = await _db.Brands.AsNoTracking().OrderBy(b => b.Name).ToListAsync();
        ViewBag.Brands = new SelectList(brands, "Id", "Name");
    }
}
