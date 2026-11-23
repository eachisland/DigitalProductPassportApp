using System.Text.RegularExpressions;
using DigitalProductPass.Data;
using DigitalProductPass.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalProductPass.Controllers;

[Route("scan")]
public class ScanController : Controller
{
    private static readonly Regex GuidRegex =
        new(@"[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}", RegexOptions.Compiled);

    // GS1 Digital Link: .../01/{gtin}[/10/{batch}]/21/{serial}
    private static readonly Regex Gs1Regex =
        new(@"/01/\d{14}(?:/10/[^/?#]+)?/21/(?<serial>[^/?#]+)", RegexOptions.Compiled);

    private readonly ApplicationDbContext _db;
    private readonly UiText _t;

    public ScanController(ApplicationDbContext db, UiText t)
    {
        _db = db;
        _t = t;
    }

    [HttpGet("")]
    public IActionResult Index() => View();

    // Manual entry of the code printed on the label.
    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Lookup(string? code) => ResolveInternal(code);

    // Called by the camera after a successful scan.
    [HttpGet("resolve")]
    public Task<IActionResult> Resolve(string? code) => ResolveInternal(code);

    private async Task<IActionResult> ResolveInternal(string? code)
    {
        code = code?.Trim();
        if (string.IsNullOrEmpty(code))
        {
            TempData["Error"] = _t["msg.enterCode"];
            return RedirectToAction(nameof(Index));
        }

        // 1) Resolver URL or bare GUID
        var match = GuidRegex.Match(code);
        if (match.Success && Guid.TryParse(match.Value, out var publicId))
            return Redirect($"/p/{publicId}");

        // 2) GS1 Digital Link or a readable serial code (e.g. TSHORG-WHI-M-7K3M9, legacy DPP-7K3M-9QXA)
        var gs1 = Gs1Regex.Match(code);
        var serial = (gs1.Success ? Uri.UnescapeDataString(gs1.Groups["serial"].Value) : code).ToUpperInvariant();
        var found = await _db.ProductItems
            .AsNoTracking()
            .Where(i => i.SerialCode == serial)
            .Select(i => (Guid?)i.PublicId)
            .FirstOrDefaultAsync();

        if (found is Guid id)
            return Redirect($"/p/{id}");

        TempData["Error"] = _t["msg.codeNotFound"];
        return RedirectToAction(nameof(Index));
    }
}
