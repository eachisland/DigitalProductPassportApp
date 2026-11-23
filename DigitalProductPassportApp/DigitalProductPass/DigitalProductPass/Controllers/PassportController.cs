using System.Linq.Expressions;
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

    // Resolver link: /p/{guid}
    [HttpGet("{publicId:guid}")]
    public Task<IActionResult> Details(Guid publicId) =>
        Show(i => i.PublicId == publicId);

    // GS1 Digital Link: /01/{gtin}/21/{serial}
    [HttpGet("/01/{gtin:length(14)}/21/{serial}")]
    public Task<IActionResult> Gs1(string gtin, string serial)
    {
        var s = serial.ToUpperInvariant();
        return Show(i => i.SerialCode == s && i.Product.Gtin == gtin);
    }

    // GS1 Digital Link with batch/lot: /01/{gtin}/10/{batch}/21/{serial}
    [HttpGet("/01/{gtin:length(14)}/10/{batch}/21/{serial}")]
    public Task<IActionResult> Gs1WithBatch(string gtin, string batch, string serial)
    {
        var s = serial.ToUpperInvariant();
        return Show(i => i.SerialCode == s && i.Product.Gtin == gtin && i.BatchNumber == batch);
    }

    private async Task<IActionResult> Show(Expression<Func<ProductItem, bool>> predicate)
    {
        var item = await _db.ProductItems
            .AsNoTracking()
            .Include(i => i.Product).ThenInclude(p => p.Brand)
            .Include(i => i.Product).ThenInclude(p => p.Materials)
            .Include(i => i.Product).ThenInclude(p => p.SupplyChainSteps)
            .Include(i => i.Product).ThenInclude(p => p.Certificates)
            .AsSplitQuery()
            .FirstOrDefaultAsync(predicate);

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

        return View("Details", item);
    }
}
