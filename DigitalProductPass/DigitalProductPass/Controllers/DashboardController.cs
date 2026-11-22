using DigitalProductPass.Data;
using DigitalProductPass.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalProductPass.Controllers;

// TODO (етап 3): [Authorize(Roles = "Brand,Admin")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _db;

    public DashboardController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var today = DateTime.UtcNow.Date;
        var since = today.AddDays(-6);

        var vm = new DashboardViewModel
        {
            Brands = await _db.Brands.CountAsync(),
            Products = await _db.Products.CountAsync(),
            Items = await _db.ProductItems.CountAsync(),
            TotalScans = await _db.ScanEvents.CountAsync(),
            ScansToday = await _db.ScanEvents.CountAsync(s => s.ScannedAt >= today)
        };

        var perDay = await _db.ScanEvents
            .Where(s => s.ScannedAt >= since)
            .GroupBy(s => s.ScannedAt.Date)
            .Select(g => new { Day = g.Key, Count = g.Count() })
            .ToListAsync();

        for (int i = 0; i < 7; i++)
        {
            var day = since.AddDays(i);
            vm.DayLabels.Add(day.ToString("dd.MM"));
            vm.DayCounts.Add(perDay.FirstOrDefault(x => x.Day == day)?.Count ?? 0);
        }

        vm.TopProducts = (await _db.ScanEvents
                .GroupBy(s => s.ProductItem.Product.Name)
                .Select(g => new { Name = g.Key, Scans = g.Count() })
                .OrderByDescending(x => x.Scans)
                .Take(5)
                .ToListAsync())
            .Select(x => new TopProductRow(x.Name, x.Scans))
            .ToList();

        vm.RecentScans = (await _db.ScanEvents
                .OrderByDescending(s => s.ScannedAt)
                .Take(10)
                .Select(s => new { s.ScannedAt, ProductName = s.ProductItem.Product.Name, s.ProductItem.SerialCode })
                .ToListAsync())
            .Select(x => new RecentScanRow(x.ScannedAt, x.ProductName, x.SerialCode))
            .ToList();

        return View(vm);
    }
}
