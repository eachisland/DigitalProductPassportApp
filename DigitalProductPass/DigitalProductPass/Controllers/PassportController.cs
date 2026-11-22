using DigitalProductPass.Data;
using DigitalProductPass.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalProductPass.Controllers;

[Route("p")]
public class PassportController : Controller
{
    private readonly ApplicationDbContext _db;

    public PassportController(ApplicationDbContext db) => _db = db;

    // Публичната страница, към която сочи QR кодът.
    [HttpGet("{publicId:guid}")]
    public async Task<IActionResult> Details(Guid publicId)
    {
        var item = await _db.ProductItems
            .AsNoTracking()
            .Include(i => i.Product).ThenInclude(p => p.Brand)
            .Include(i => i.Product).ThenInclude(p => p.Materials)
            .Include(i => i.Product).ThenInclude(p => p.SupplyChainSteps)
            .Include(i => i.Product).ThenInclude(p => p.Certificates)
            .AsSplitQuery()
            .FirstOrDefaultAsync(i => i.PublicId == publicId);

        if (item is null)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return View("NotFound");
        }

        var ua = Request.Headers.UserAgent.ToString();
        _db.ScanEvents.Add(new ScanEvent
        {
            ProductItemId = item.Id,
            UserAgent = ua.Length > 300 ? ua[..300] : ua
        });
        await _db.SaveChangesAsync();

        return View(item);
    }
}
