using System.Text.RegularExpressions;
using DigitalProductPass.Data;
using DigitalProductPass.Models;
using DigitalProductPass.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DigitalProductPass.Controllers;

/// <summary>Everything here (creating products, issuing variant SKUs, revoking items) is for the store owner only.</summary>
[Authorize(Roles = AccountController.StoreOwnerRole)]
public class ProductsController : Controller
{
    private static readonly Regex DiffRegex = new("^[A-Za-z0-9]{2,6}$", RegexOptions.Compiled);
    private static readonly Regex BatchRegex = new("^[A-Za-z0-9-]{1,20}$", RegexOptions.Compiled);

    private readonly ApplicationDbContext _db;
    private readonly UiText _t;

    public ProductsController(ApplicationDbContext db, UiText t)
    {
        _db = db;
        _t = t;
    }

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
        string? gtin = null;
        if (!string.IsNullOrWhiteSpace(form.Gtin))
        {
            gtin = Gtin.Normalize(form.Gtin);
            if (gtin is null) ModelState.AddModelError(nameof(form.Gtin), _t["msg.gtinInvalid"]);
        }

        if (await _db.Products.AnyAsync(p => p.BrandId == form.BrandId && p.Sku == form.Sku))
            ModelState.AddModelError(nameof(form.Sku), _t["msg.skuDup"]);

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
            Gtin = gtin,
            DifferentiationCode = form.DifferentiationCode.Trim().ToUpperInvariant(),
            Category = form.Category,
            Description = form.Description,
            ImageUrl = form.ImageUrl,
            WeightKg = form.WeightKg,
            CarbonFootprintKg = form.CarbonFootprintKg,
            WaterUsageLiters = form.WaterUsageLiters,
            RecyclabilityScore = form.RecyclabilityScore,
            RepairabilityIndex = form.RepairabilityIndex,
            WashInstructions = form.WashInstructions,
            EndOfLifeGuidance = form.EndOfLifeGuidance
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
    public async Task<IActionResult> AddMaterial(int id, string? name, decimal percentage, bool isRecycled,
        string? origin, decimal? carbonIndex, string? certificateCode)
    {
        if (!await _db.Products.AnyAsync(p => p.Id == id)) return NotFound();

        if (string.IsNullOrWhiteSpace(name) || percentage <= 0 || percentage > 100
            || (carbonIndex.HasValue && (carbonIndex < 0 || carbonIndex > 1000)))
        {
            TempData["Error"] = _t["msg.matInvalid"];
            return RedirectToAction(nameof(Details), new { id });
        }

        // Material shares of one product may never add up to more than 100%.
        var current = await _db.Materials.Where(m => m.ProductId == id).SumAsync(m => (decimal?)m.Percentage) ?? 0m;
        if (current + percentage > 100m + CarbonCalculator.Tolerance)
        {
            TempData["Error"] = _t.F("msg.matOver", current.ToString("0.##"));
            return RedirectToAction(nameof(Details), new { id });
        }

        _db.Materials.Add(new Material
        {
            ProductId = id,
            Name = name.Trim(),
            Percentage = percentage,
            IsRecycled = isRecycled,
            Origin = string.IsNullOrWhiteSpace(origin) ? null : origin.Trim(),
            CarbonIndexKgPerKg = carbonIndex,
            CertificateCode = string.IsNullOrWhiteSpace(certificateCode) ? null : certificateCode.Trim()
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = _t["msg.matAdded"];
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMaterial(int id, int materialId)
    {
        var material = await _db.Materials.FirstOrDefaultAsync(m => m.Id == materialId && m.ProductId == id);
        if (material is null) return NotFound();

        _db.Materials.Remove(material);
        await _db.SaveChangesAsync();
        TempData["Success"] = _t["msg.matRemoved"];
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddStep(int id, SupplyChainStage stage, string? facilityName, string? country, DateOnly? date)
    {
        if (!await _db.Products.AnyAsync(p => p.Id == id)) return NotFound();

        if (string.IsNullOrWhiteSpace(facilityName) || string.IsNullOrWhiteSpace(country))
        {
            TempData["Error"] = _t["msg.stepInvalid"];
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

    /// <summary>Issues variant-level items. Serial = DIFFERENTIATION-COLOR-SIZE-RANDOM, unique in the database.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateItems(int id, int count, string? size, string? color,
        string? differentiationCode, string? batchNumber)
    {
        var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();

        if (count < 1 || count > 500)
        {
            TempData["Error"] = _t["msg.countRange"];
            return RedirectToAction(nameof(Details), new { id });
        }

        var diff = string.IsNullOrWhiteSpace(differentiationCode) ? product.DifferentiationCode : differentiationCode.Trim();
        if (string.IsNullOrWhiteSpace(diff) || !DiffRegex.IsMatch(diff))
        {
            TempData["Error"] = _t["msg.diffInvalid"];
            return RedirectToAction(nameof(Details), new { id });
        }
        diff = diff.ToUpperInvariant();

        var batch = string.IsNullOrWhiteSpace(batchNumber) ? null : batchNumber.Trim();
        if (batch is not null && !BatchRegex.IsMatch(batch))
        {
            TempData["Error"] = _t["msg.batchInvalid"];
            return RedirectToAction(nameof(Details), new { id });
        }

        var cleanSize = string.IsNullOrWhiteSpace(size) ? null : size.Trim();
        var cleanColor = string.IsNullOrWhiteSpace(color) ? null : color.Trim();

        var codes = new HashSet<string>();
        while (codes.Count < count)
        {
            var candidates = new HashSet<string>();
            while (codes.Count + candidates.Count < count)
            {
                var c = SerialCodeGenerator.New(diff, cleanColor, cleanSize);
                if (!codes.Contains(c)) candidates.Add(c);
            }

            // Drop codes that already exist in the database (the unique index is the final safety net).
            var taken = await _db.ProductItems
                .Where(i => candidates.Contains(i.SerialCode))
                .Select(i => i.SerialCode)
                .ToListAsync();
            candidates.ExceptWith(taken);
            codes.UnionWith(candidates);
        }

        foreach (var code in codes)
        {
            _db.ProductItems.Add(new ProductItem
            {
                ProductId = id,
                SerialCode = code,
                DifferentiationCode = diff,
                Size = cleanSize,
                Color = cleanColor,
                BatchNumber = batch
            });
        }
        await _db.SaveChangesAsync();

        TempData["Success"] = _t.F("msg.issued", count);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Revoke(int itemId)
    {
        var item = await _db.ProductItems.FirstOrDefaultAsync(i => i.Id == itemId);
        if (item is null) return NotFound();

        if (!item.IsRevoked)
        {
            item.IsRevoked = true;
            item.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        TempData["Success"] = _t.F("msg.revoked", item.SerialCode);
        return RedirectToAction(nameof(Details), new { id = item.ProductId });
    }

    private async Task LoadBrands()
    {
        var brands = await _db.Brands.AsNoTracking().OrderBy(b => b.Name).ToListAsync();
        ViewBag.Brands = new SelectList(brands, "Id", "Name");
    }
}
