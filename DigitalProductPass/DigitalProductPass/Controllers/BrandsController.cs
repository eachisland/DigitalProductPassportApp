using DigitalProductPass.Data;
using DigitalProductPass.Models;
using Microsoft.AspNetCore.Mvc;

namespace DigitalProductPass.Controllers;

// TODO (етап 3): само Admin
public class BrandsController : Controller
{
    private readonly ApplicationDbContext _db;

    public BrandsController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public IActionResult Create() => View(new Brand());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Country,Website")] Brand brand)
    {
        if (!ModelState.IsValid) return View(brand);

        _db.Brands.Add(brand);
        await _db.SaveChangesAsync();
        return RedirectToAction("Create", "Products");
    }
}
