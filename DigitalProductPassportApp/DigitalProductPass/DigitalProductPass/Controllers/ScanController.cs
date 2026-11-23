using System.Linq.Expressions;
using System.Text.RegularExpressions;
using DigitalProductPass.Data;
using DigitalProductPass.Models;
using DigitalProductPass.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalProductPass.Controllers;

[Route("scan")]
public class ScanController : Controller
{
    private static readonly Regex ResolverPath =
        new(@"/p/(?<publicId>[^/]+)/?\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex Gs1Path =
        new(@"/01/(?<gtin>[0-9]{14})(?:/10/(?<batch>[^/]+))?/21/(?<serial>[^/]+)/?\z");

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
            return LookupError("msg.enterCode");

        var predicate = MatchCode(code);
        if (predicate is null)
            return LookupError("msg.codeNotFound");

        var found = await _db.ProductItems
            .AsNoTracking()
            .Where(predicate)
            .Select(i => (Guid?)i.PublicId)
            .FirstOrDefaultAsync(HttpContext.RequestAborted);

        if (found is Guid id)
            return RedirectToAction(nameof(PassportController.Details), "Passport", new { publicId = id });

        return LookupError("msg.codeNotFound");
    }

    private IActionResult LookupError(string message)
    {
        TempData["Error"] = _t[message];
        return RedirectToAction(nameof(Index));
    }

    private static Expression<Func<ProductItem, bool>>? MatchCode(string code)
    {
        if (Guid.TryParseExact(code, "D", out var publicId))
            return item => item.PublicId == publicId;

        var path = GetLinkPath(code);
        if (path is null)
        {
            // Handle both current and legacy label serials, including dashes copied from formatted text.
            var serialCode = Regex.Replace(code, @"[\u2010-\u2014\u2212]", "-").ToUpperInvariant();
            return item => item.SerialCode == serialCode;
        }

        var resolver = ResolverPath.Match(path);
        if (resolver.Success && Guid.TryParse(Uri.UnescapeDataString(resolver.Groups["publicId"].Value), out publicId))
            return item => item.PublicId == publicId;

        var gs1 = Gs1Path.Match(path);
        if (!gs1.Success)
            return null;

        var batch = gs1.Groups["batch"];
        return PassportLookup.MatchGs1(
            gs1.Groups["gtin"].Value,
            Uri.UnescapeDataString(gs1.Groups["serial"].Value),
            batch.Success ? Uri.UnescapeDataString(batch.Value) : null);
    }

    private static string? GetLinkPath(string code)
    {
        if (code.StartsWith('/') && !code.StartsWith("//", StringComparison.Ordinal))
            return code.Split(['?', '#'], 2)[0];

        return Uri.TryCreate(code, UriKind.Absolute, out var uri) && (uri.Scheme is "http" or "https")
            ? uri.AbsolutePath
            : null;
    }
}
