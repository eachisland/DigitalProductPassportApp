using System.Text.RegularExpressions;
using DigitalProductPass.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalProductPass.Controllers;

[Route("scan")]
public class ScanController : Controller
{
    private static readonly Regex GuidRegex =
        new(@"[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}", RegexOptions.Compiled);

    private readonly ApplicationDbContext _db;

    public ScanController(ApplicationDbContext db) => _db = db;

    [HttpGet("")]
    public IActionResult Index() => View();

    // Ръчно въвеждане на код от етикета.
    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Lookup(string? code) => ResolveInternal(code);

    // Вика се от камерата след успешно сканиране.
    [HttpGet("resolve")]
    public Task<IActionResult> Resolve(string? code) => ResolveInternal(code);

    private async Task<IActionResult> ResolveInternal(string? code)
    {
        code = code?.Trim();
        if (string.IsNullOrEmpty(code))
        {
            TempData["Error"] = "Въведи или сканирай код.";
            return RedirectToAction(nameof(Index));
        }

        // 1) Пълен URL или чист GUID (съдържание на QR кода)
        var match = GuidRegex.Match(code);
        if (match.Success && Guid.TryParse(match.Value, out var publicId))
            return Redirect($"/p/{publicId}");

        // 2) Четим сериен код, напр. DPP-7K3M-9QXA
        var serial = code.ToUpperInvariant();
        var found = await _db.ProductItems
            .AsNoTracking()
            .Where(i => i.SerialCode == serial)
            .Select(i => (Guid?)i.PublicId)
            .FirstOrDefaultAsync();

        if (found is Guid id)
            return Redirect($"/p/{id}");

        TempData["Error"] = "Не намерихме продукт с този код.";
        return RedirectToAction(nameof(Index));
    }
}
